using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System;
using ksp.care.Models;
using ksp.care.Helpers;

namespace ksp.care.Controllers
{
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UserController(ApplicationDbContext context)
        {
            _context = context;
        }

        private PatientRecord? GetCurrentUser()
        {
            var mobile = HttpContext.Session.GetString("UserMobile");
            if (string.IsNullOrEmpty(mobile)) return null;
            return _context.PatientRecords.FirstOrDefault(u => u.Mobile == mobile);
        }

        public IActionResult Dashboard()
        {
            var user = GetCurrentUser();
            if (user == null) return RedirectToAction("UserLogin", "Home");
            ViewData["UserName"] = $"{user.FirstName} {user.LastName}";
            ViewData["UserPatientId"] = user.Id;
            ViewData["UserGender"] = user.Gender;
            ViewData["UserAge"] = user.Age;
            ViewData["UserPhoto"] = user.ProfilePhoto;
            var myAppointments = _context.Appointments
                .Where(a => a.PatientMobile == user.Mobile)
                .OrderByDescending(a => a.AppointmentDate)
                .ToList();
            ViewBag.Doctors = _context.Doctors.ToList();
            return View(myAppointments);
        }

        public IActionResult Package()
        {
            var user = GetCurrentUser();
            if (user == null) return RedirectToAction("UserLogin", "Home");
            ViewBag.Tests = _context.Tests.Where(t => t.IsActive).OrderBy(t => t.Category).ThenBy(t => t.TestName).ToList();
            ViewBag.Doctors = _context.Doctors.Where(d => d.IsActive).OrderBy(d => d.Name).ToList();
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult FinalizeBooking(List<string> SelectedTests, DateTime AppointmentDate, string AppointmentTime, int? DoctorId)
        {
            var user = GetCurrentUser();
            if (user == null) return RedirectToAction("UserLogin", "Home");

            if (SelectedTests == null || SelectedTests.Count == 0)
            {
                TempData["Error"] = "Please select at least one test or package.";
                return RedirectToAction("Package");
            }

            // FIX: PostgreSQL requires DateTimeKind.Utc
            var utcDate = DateTime.SpecifyKind(AppointmentDate, DateTimeKind.Utc);
            var patientName = $"{user.FirstName} {user.LastName}";
            var packageName = string.Join(", ", SelectedTests);

            // Calculate total from DB test prices (with fallback)
            decimal totalAmount = 0;
            var fallbackPrices = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase) {
                {"Basic Master Health", 1499m}, {"Comprehensive Body", 2999m},
                {"Vitamin Profile", 999m}, {"Cardiac Care", 1799m},
                {"Diabetes Screening", 1299m}, {"Thyroid Panel", 899m},
                {"Liver & Kidney Panel", 1599m}, {"Women's Health", 2499m}
            };

            foreach (var testName in SelectedTests)
            {
                var dbTest = _context.Tests.FirstOrDefault(t => t.TestName == testName && t.IsActive);
                if (dbTest != null)
                    totalAmount += dbTest.Price;
                else if (fallbackPrices.ContainsKey(testName))
                    totalAmount += fallbackPrices[testName];
                else
                    totalAmount += 500m; // Default fallback price
            }

            // Create appointment
            var appointment = new AppointmentRecord
            {
                PatientId       = user.Id,
                PatientMobile   = user.Mobile,
                PatientName     = patientName,
                PackageName     = packageName,
                AppointmentDate = utcDate,
                AppointmentTime = AppointmentTime,
                DoctorId        = DoctorId,
                Status          = "Pending"
            };
            _context.Appointments.Add(appointment);
            _context.SaveChanges(); // Save to get appointment.Id

            // Auto-create bill
            _context.Billings.Add(new BillingRecord
            {
                PatientId     = user.Id,
                PatientMobile = user.Mobile,
                PatientName   = patientName,
                AppointmentId = appointment.Id,
                PackageName   = packageName,
                TotalAmount   = totalAmount,
                PaidAmount    = 0,
                Status        = "Pending",
                CreatedAt     = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc)
            });
            _context.SaveChanges();

            TempData["BookingSuccess"] = $"{SelectedTests.Count} test(s) booked for {AppointmentDate:dd MMM yyyy} at {AppointmentTime}. Bill of ₹{totalAmount:N0} generated.";
            return RedirectToAction("Dashboard");
        }

        // ── Cancel Appointment ────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CancelAppointment(int id)
        {
            var user = GetCurrentUser();
            if (user == null) return RedirectToAction("UserLogin", "Home");

            var apt = _context.Appointments.FirstOrDefault(a =>
                a.Id == id && a.PatientMobile == user.Mobile);

            if (apt != null && apt.Status == "Pending")
            {
                apt.Status = "Cancelled";
                _context.SaveChanges();
                TempData["BookingSuccess"] = $"Appointment #{id} has been cancelled.";
            }
            else
            {
                TempData["Error"] = "Cannot cancel this appointment. Only pending appointments can be cancelled.";
            }
            return RedirectToAction("Dashboard");
        }




        public IActionResult Profile()
        {
            var user = GetCurrentUser();
            if (user == null) return RedirectToAction("UserLogin", "Home");
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateProfile(string FirstName, string LastName, int Age, string Gender,
                                           string City, string? State, string? Pincode, string? Place, string? Email)
        {
            var user = GetCurrentUser();
            if (user == null) return RedirectToAction("UserLogin", "Home");
            user.FirstName = FirstName;
            user.LastName  = LastName;
            user.Age       = Age;
            user.Gender    = Gender;
            user.City      = City;
            user.State     = State;
            user.Pincode   = Pincode ?? string.Empty;
            user.Place     = Place;
            user.Email     = Email;
            _context.SaveChanges();
            TempData["ProfileSuccess"] = "Profile updated successfully.";
            return RedirectToAction("Profile");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UploadPhoto(IFormFile photo)
        {
            var user = GetCurrentUser();
            if (user == null) return RedirectToAction("UserLogin", "Home");

            if (photo == null || photo.Length == 0)
            {
                TempData["ProfileError"] = "Please select a photo to upload.";
                return RedirectToAction("Profile");
            }

            // Validate file type
            var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var ext = Path.GetExtension(photo.FileName).ToLowerInvariant();
            if (!allowed.Contains(ext))
            {
                TempData["ProfileError"] = "Only JPG, PNG, and WebP images are allowed.";
                return RedirectToAction("Profile");
            }

            // Validate size (max 5MB)
            if (photo.Length > 5 * 1024 * 1024)
            {
                TempData["ProfileError"] = "Photo must be less than 5 MB.";
                return RedirectToAction("Profile");
            }

            // Delete old photo if exists
            if (!string.IsNullOrEmpty(user.ProfilePhoto))
            {
                var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", user.ProfilePhoto.TrimStart('/'));
                if (System.IO.File.Exists(oldPath))
                    System.IO.File.Delete(oldPath);
            }

            // Save new photo
            var fileName = $"{user.Id}_{Guid.NewGuid():N}{ext}";
            var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "photos");
            Directory.CreateDirectory(uploadsDir);
            var filePath = Path.Combine(uploadsDir, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                photo.CopyTo(stream);
            }

            user.ProfilePhoto = $"/uploads/photos/{fileName}";
            _context.SaveChanges();
            TempData["ProfileSuccess"] = "Profile photo updated!";
            return RedirectToAction("Profile");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RemovePhoto()
        {
            var user = GetCurrentUser();
            if (user == null) return RedirectToAction("UserLogin", "Home");

            if (!string.IsNullOrEmpty(user.ProfilePhoto))
            {
                var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", user.ProfilePhoto.TrimStart('/'));
                if (System.IO.File.Exists(oldPath))
                    System.IO.File.Delete(oldPath);
                user.ProfilePhoto = null;
                _context.SaveChanges();
                TempData["ProfileSuccess"] = "Profile photo removed.";
            }
            return RedirectToAction("Profile");
        }

        public IActionResult Reports()
        {
            var user = GetCurrentUser();
            if (user == null) return RedirectToAction("UserLogin", "Home");
            
            var myReports = _context.Reports
                .Where(r => r.PatientMobile == user.Mobile)
                .OrderByDescending(r => r.UploadedAt)
                .ToList();

            var pendingTests = _context.Appointments
                .Where(a => a.PatientMobile == user.Mobile && a.Status != "Cancelled" && !_context.Reports.Any(r => r.PatientMobile == a.PatientMobile && r.TestName == a.PackageName))
                .OrderByDescending(a => a.CreatedAt)
                .ToList();

            ViewBag.PendingTests = pendingTests;

            return View(myReports);
        }

        // ── My Bills ──────────────────────────────────────────────
        public IActionResult Bills()
        {
            var user = GetCurrentUser();
            if (user == null) return RedirectToAction("UserLogin", "Home");

            try
            {
                var myBills = _context.Billings
                    .Where(b => b.PatientMobile == user.Mobile)
                    .OrderByDescending(b => b.CreatedAt)
                    .ToList();
                return View(myBills);
            }
            catch
            {
                return View(new System.Collections.Generic.List<BillingRecord>());
            }
        }

        // ── My Prescriptions ─────────────────────────────────────
        public IActionResult Prescriptions()
        {
            var user = GetCurrentUser();
            if (user == null) return RedirectToAction("UserLogin", "Home");

            var myPrescriptions = _context.Prescriptions
                .Where(p => p.PatientMobile == user.Mobile)
                .OrderByDescending(p => p.CreatedAt)
                .ToList();
            return View(myPrescriptions);
        }

        // ── Pay Bill ──────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult PayBill(int id, decimal amount)
        {
            var user = GetCurrentUser();
            if (user == null) return RedirectToAction("UserLogin", "Home");

            var bill = _context.Billings.FirstOrDefault(b => b.Id == id && b.PatientMobile == user.Mobile);
            if (bill == null)
            {
                TempData["Error"] = "Bill not found.";
                return RedirectToAction("Bills");
            }

            if (amount <= 0)
            {
                TempData["Error"] = "Please enter a valid payment amount.";
                return RedirectToAction("Bills");
            }

            // Cap at remaining balance
            var remaining = bill.TotalAmount - bill.PaidAmount;
            if (amount > remaining) amount = remaining;

            bill.PaidAmount += amount;
            if (bill.PaidAmount >= bill.TotalAmount)
            {
                bill.PaidAmount = bill.TotalAmount;
                bill.Status = "Paid";
                TempData["Success"] = $"Bill #{id} fully paid! ₹{amount:N2} received.";
            }
            else
            {
                TempData["Success"] = $"₹{amount:N2} payment recorded for Bill #{id}. Remaining: ₹{bill.PendingAmount:N2}";
            }

            _context.SaveChanges();
            return RedirectToAction("Bills");
        }

        // ── Invoice ──────────────────────────────────────────────
        public IActionResult ViewInvoice(int id)
        {
            var user = GetCurrentUser();
            if (user == null) return RedirectToAction("UserLogin", "Home");

            var bill = _context.Billings.FirstOrDefault(b => b.Id == id && b.PatientMobile == user.Mobile);
            if (bill == null) { TempData["Error"] = "Invoice not found."; return RedirectToAction("Bills"); }
            return View("~/Views/Shared/Invoice.cshtml", bill);
        }

        // ── Change Password ──────────────────────────────────────
        public IActionResult ChangePassword()
        {
            var user = GetCurrentUser();
            if (user == null) return RedirectToAction("UserLogin", "Home");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdatePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            var user = GetCurrentUser();
            if (user == null) return RedirectToAction("UserLogin", "Home");

            if (newPassword != confirmPassword)
            {
                TempData["Error"] = "New password and confirmation do not match.";
                return RedirectToAction("ChangePassword");
            }

            if (newPassword.Length < 4)
            {
                TempData["Error"] = "Password must be at least 4 characters.";
                return RedirectToAction("ChangePassword");
            }

            // Verify current password (supports both hashed and legacy plain-text)
            bool validCurrent;
            if (PasswordHelper.IsBCryptHash(user.Password))
            {
                validCurrent = PasswordHelper.VerifyPassword(currentPassword, user.Password);
            }
            else
            {
                validCurrent = user.Password == currentPassword;
            }

            if (!validCurrent)
            {
                TempData["Error"] = "Current password is incorrect.";
                return RedirectToAction("ChangePassword");
            }

            user.Password = PasswordHelper.HashPassword(newPassword);
            _context.SaveChanges();
            TempData["ProfileSuccess"] = "Password changed successfully.";
            return RedirectToAction("Profile");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}