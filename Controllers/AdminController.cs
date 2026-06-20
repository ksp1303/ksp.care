using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using ksp.care.Models;
using ksp.care.Helpers;

namespace ksp.care.Controllers
{
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment  _env;
        private const int PageSize = 12;

        public AdminController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env     = env;
        }

        private bool IsAdmin() =>
            HttpContext.Session.GetString("IsAdmin") == "true";

        // ── Dashboard ─────────────────────────────────────────────
        public IActionResult Dashboard(string? search, int page = 1)
        {
            if (!IsAdmin()) return RedirectToAction("AdminLogin", "Home");

            ViewBag.TotalPatients = _context.PatientRecords.Count();
            ViewBag.TotalReports  = _context.Reports.Count();
            ViewBag.PendingCount  = _context.Appointments.Count(a => a.Status == "Pending");
            ViewBag.TotalDoctors  = _context.Doctors.Count(d => d.IsActive);

            try
            {
                ViewBag.TotalBillDue = _context.Billings
                    .Where(b => b.Status != "Paid")
                    .Sum(b => (decimal?)(b.TotalAmount - b.PaidAmount)) ?? 0m;
            }
            catch { ViewBag.TotalBillDue = 0m; }

            ViewBag.Doctors = _context.Doctors.ToList();

            var query = _context.Appointments.AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(a =>
                    a.PatientName.Contains(search) ||
                    a.PatientMobile.Contains(search));
            }
            ViewBag.Search = search;

            var paged = PaginatedList<AppointmentRecord>.Create(
                query.OrderByDescending(a => a.CreatedAt), page, PageSize);

            return View(paged);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateStatus(int id, string newStatus)
        {
            if (!IsAdmin()) return RedirectToAction("AdminLogin", "Home");
            var apt = _context.Appointments.FirstOrDefault(a => a.Id == id);
            if (apt != null)
            {
                apt.Status = newStatus;

                // Auto-generate SampleId when sample is collected
                if (newStatus == "Sample Collected" && string.IsNullOrEmpty(apt.SampleId))
                {
                    var today = DateTime.UtcNow.ToString("yyyyMMdd");
                    var count = _context.Appointments
                        .Count(a => a.SampleId != null && a.SampleId.Contains(today)) + 1;
                    apt.SampleId = $"KSP-{today}-{count:D4}";
                    apt.SampleCollectedAt = DateTime.UtcNow;
                }

                _context.SaveChanges();
                TempData["Success"] = $"Ref #{id} updated to '{newStatus}'.";
            }
            return RedirectToAction("Dashboard");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AssignPhlebotomist(int id, string phlebotomistName)
        {
            if (!IsAdmin()) return RedirectToAction("AdminLogin", "Home");
            var apt = _context.Appointments.FirstOrDefault(a => a.Id == id);
            if (apt != null)
            {
                apt.PhlebotomistName = phlebotomistName;
                _context.SaveChanges();
                TempData["Success"] = $"Phlebotomist '{phlebotomistName}' assigned to Ref #{id}.";
            }
            return RedirectToAction("Dashboard");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AssignDoctor(int id, int doctorId)
        {
            if (!IsAdmin()) return RedirectToAction("AdminLogin", "Home");
            var apt = _context.Appointments.FirstOrDefault(a => a.Id == id);
            if (apt != null)
            {
                apt.DoctorId = doctorId;
                _context.SaveChanges();
                var doc = _context.Doctors.FirstOrDefault(d => d.Id == doctorId);
                TempData["Success"] = $"Doctor '{(doc?.Name ?? "Unknown")}' assigned to Ref #{id}.";
            }
            return RedirectToAction("Dashboard");
        }

        // ── Patients ──────────────────────────────────────────────
        public IActionResult Patient(string? search, int page = 1)
        {
            if (!IsAdmin()) return RedirectToAction("AdminLogin", "Home");

            var query = _context.PatientRecords.AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(p =>
                    p.FirstName.Contains(search) ||
                    p.LastName.Contains(search) ||
                    p.Mobile.Contains(search));
            }
            ViewBag.Search = search;

            var paged = PaginatedList<PatientRecord>.Create(
                query.OrderByDescending(p => p.Id), page, PageSize);

            return View(paged);
        }

        public IActionResult EditPatient(int id)
        {
            if (!IsAdmin()) return RedirectToAction("AdminLogin", "Home");
            var patient = _context.PatientRecords.FirstOrDefault(p => p.Id == id);
            if (patient == null)
            {
                TempData["Error"] = "Patient not found.";
                return RedirectToAction("Patient");
            }
            return View(patient);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditPatient(PatientRecord model)
        {
            if (!IsAdmin()) return RedirectToAction("AdminLogin", "Home");

            var patient = _context.PatientRecords.FirstOrDefault(p => p.Id == model.Id);
            if (patient != null)
            {
                patient.FirstName = model.FirstName;
                patient.LastName  = model.LastName;
                patient.Mobile    = model.Mobile;
                patient.Age       = model.Age;
                patient.Gender    = model.Gender;
                patient.City      = model.City;
                patient.Pincode   = model.Pincode;
                patient.Place     = model.Place;
                patient.State     = model.State;
                patient.Ward      = model.Ward;
                patient.Email     = model.Email;

                // Only update password if a new plain-text one is provided.
                // Skip if empty, matches existing hash, or is already a BCrypt hash (prevents double-hashing).
                if (!string.IsNullOrWhiteSpace(model.Password)
                    && model.Password != patient.Password
                    && !PasswordHelper.IsBCryptHash(model.Password))
                {
                    patient.Password = PasswordHelper.HashPassword(model.Password);
                }

                _context.SaveChanges();
                TempData["Success"] = $"Patient {patient.FirstName} updated successfully.";
            }
            return RedirectToAction("Patient");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePatient(int id)
        {
            if (!IsAdmin()) return RedirectToAction("AdminLogin", "Home");
            var patient = _context.PatientRecords.FirstOrDefault(p => p.Id == id);
            if (patient != null)
            {
                // Note: Consider foreign key constraints (Appointments, Reports, Billings)
                // For a clinic system, a soft delete is usually better, but here we hard delete.
                _context.PatientRecords.Remove(patient);
                _context.SaveChanges();
                TempData["Success"] = $"Patient {patient.FirstName} deleted.";
            }
            return RedirectToAction("Patient");
        }

        // ── Register New Patient (Admin Portal) ───────────────────
        public IActionResult RegisterPatient()
        {
            if (!IsAdmin()) return RedirectToAction("AdminLogin", "Home");
            ViewBag.Doctors = _context.Doctors.Where(d => d.IsActive).OrderBy(d => d.Name).ToList();
            
            // Auto-generate a dummy Patient ID for UI purposes
            var nextId = _context.PatientRecords.Any() ? _context.PatientRecords.Max(p => p.Id) + 1 : 1;
            ViewBag.NextPatientId = nextId;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RegisterPatient(string designation, string firstName, string lastName, string mobile, string gender, int age, string ageType, string email, string address, int? doctorId, string hospital, string laboratory)
        {
            if (!IsAdmin()) return RedirectToAction("AdminLogin", "Home");
            
            var patient = _context.PatientRecords.FirstOrDefault(p => p.Mobile == mobile);
            if (patient != null)
            {
                TempData["Error"] = $"Patient with mobile {mobile} already exists.";
                return RedirectToAction("RegisterPatient");
            }

            patient = new PatientRecord
            {
                FirstName = firstName ?? string.Empty,
                LastName  = lastName ?? string.Empty,
                Mobile    = mobile ?? string.Empty,
                Password  = PasswordHelper.HashPassword(mobile ?? "1234"), // Default password
                Email     = email,
                Gender    = gender ?? "Other",
                Age       = age,
                Place     = address, // Store address in Place field for now
                City      = string.Empty,
                Pincode   = string.Empty,
                State     = string.Empty,
                Ward      = string.Empty
            };
            
            _context.PatientRecords.Add(patient);
            _context.SaveChanges();

            // "Go To Billing" intent: Normally we'd redirect to create an appointment/bill.
            TempData["Success"] = $"Patient {firstName} registered successfully! You can now book tests for them.";
            return RedirectToAction("Patient");
        }

        // ── Doctors ───────────────────────────────────────────────
        public IActionResult Doctors()
        {
            if (!IsAdmin()) return RedirectToAction("AdminLogin", "Home");
            return View(_context.Doctors.OrderByDescending(d => d.CreatedAt).ToList());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddDoctor(string name, string specialization, string? mobile)
        {
            if (!IsAdmin()) return RedirectToAction("AdminLogin", "Home");
            _context.Doctors.Add(new DoctorRecord
            {
                Name           = name,
                Specialization = specialization,
                Mobile         = mobile ?? string.Empty
            });
            _context.SaveChanges();
            TempData["Success"] = $"Dr. {name} added successfully.";
            return RedirectToAction("Doctors");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleDoctorStatus(int id)
        {
            if (!IsAdmin()) return RedirectToAction("AdminLogin", "Home");
            var doc = _context.Doctors.FirstOrDefault(d => d.Id == id);
            if (doc != null)
            {
                doc.IsActive = !doc.IsActive;
                _context.SaveChanges();
                TempData["Success"] = $"Dr. {doc.Name} is now {(doc.IsActive ? "active" : "inactive")}.";
            }
            return RedirectToAction("Doctors");
        }

        // ── Tests / Package Catalog ──────────────────────────────
        public IActionResult Tests(string? search, int page = 1)
        {
            if (!IsAdmin()) return RedirectToAction("AdminLogin", "Home");

            var query = _context.Tests.AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(t =>
                    t.TestName.Contains(search) ||
                    t.Category.Contains(search));
            }
            ViewBag.Search = search;

            var paged = PaginatedList<TestRecord>.Create(
                query.OrderByDescending(t => t.CreatedAt), page, PageSize);

            return View(paged);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddTest(string testName, string category, decimal price, string? description)
        {
            if (!IsAdmin()) return RedirectToAction("AdminLogin", "Home");

            if (_context.Tests.Any(t => t.TestName == testName))
            {
                TempData["Error"] = $"Test '{testName}' already exists.";
                return RedirectToAction("Tests");
            }

            _context.Tests.Add(new TestRecord
            {
                TestName    = testName,
                Category    = category,
                Price       = price,
                Description = description
            });
            _context.SaveChanges();
            TempData["Success"] = $"Test '{testName}' added at ₹{price:N2}.";
            return RedirectToAction("Tests");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditTest(int id, string testName, string category, decimal price, string? description)
        {
            if (!IsAdmin()) return RedirectToAction("AdminLogin", "Home");
            var test = _context.Tests.FirstOrDefault(t => t.Id == id);
            if (test != null)
            {
                test.TestName    = testName;
                test.Category    = category;
                test.Price       = price;
                test.Description = description;
                _context.SaveChanges();
                TempData["Success"] = $"Test '{testName}' updated.";
            }
            return RedirectToAction("Tests");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleTestStatus(int id)
        {
            if (!IsAdmin()) return RedirectToAction("AdminLogin", "Home");
            var test = _context.Tests.FirstOrDefault(t => t.Id == id);
            if (test != null)
            {
                test.IsActive = !test.IsActive;
                _context.SaveChanges();
                TempData["Success"] = $"'{test.TestName}' is now {(test.IsActive ? "active" : "inactive")}.";
            }
            return RedirectToAction("Tests");
        }

        // ── Billing ───────────────────────────────────────────────
        public IActionResult PendingBill(string? search, int page = 1)
        {
            if (!IsAdmin()) return RedirectToAction("AdminLogin", "Home");

            try
            {
                var query = _context.Billings.Where(b => b.Status != "Paid").AsQueryable();
                if (!string.IsNullOrWhiteSpace(search))
                {
                    query = query.Where(b =>
                        b.PatientName.Contains(search) ||
                        b.PatientMobile.Contains(search));
                }
                ViewBag.Search = search;

                var orderedQuery = query.OrderByDescending(b => b.CreatedAt);

                // Compute total due from the DB without loading all rows into memory
                ViewBag.TotalDue = query.Sum(b => (decimal?)(b.TotalAmount - b.PaidAmount)) ?? 0m;

                var billedIds = _context.Billings.Select(b => b.AppointmentId).ToHashSet();
                ViewBag.UnbilledApts = _context.Appointments
                    .Where(a => a.Status == "Confirmed" && !billedIds.Contains(a.Id))
                    .OrderByDescending(a => a.CreatedAt).ToList();

                var paged = PaginatedList<BillingRecord>.Create(orderedQuery, page, PageSize);
                return View(paged);
            }
            catch
            {
                TempData["Error"] = "Billings table not ready. Run migrations.";
                ViewBag.TotalDue = 0m;
                ViewBag.Search = search;
                ViewBag.UnbilledApts = new List<AppointmentRecord>();
                return View(PaginatedList<BillingRecord>.Create(new List<BillingRecord>(), 1, PageSize));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateBill(int appointmentId, decimal amount)
        {
            if (!IsAdmin()) return RedirectToAction("AdminLogin", "Home");
            var apt = _context.Appointments.FirstOrDefault(a => a.Id == appointmentId);
            if (apt == null) { TempData["Error"] = "Appointment not found."; return RedirectToAction("PendingBill"); }
            if (_context.Billings.Any(b => b.AppointmentId == appointmentId))
            { TempData["Error"] = "Bill already exists for this appointment."; return RedirectToAction("PendingBill"); }

            var patient = _context.PatientRecords.FirstOrDefault(p => p.Mobile == apt.PatientMobile);

            _context.Billings.Add(new BillingRecord
            {
                PatientId     = patient?.Id,
                PatientMobile = apt.PatientMobile,
                PatientName   = apt.PatientName,
                AppointmentId = apt.Id,
                PackageName   = apt.PackageName,
                TotalAmount   = amount,
                Status        = "Pending"
            });
            _context.SaveChanges();
            TempData["Success"] = $"Bill of ₹{amount:N2} created for {apt.PatientName}.";
            return RedirectToAction("PendingBill");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RecordPayment(int id, decimal amount)
        {
            if (!IsAdmin()) return RedirectToAction("AdminLogin", "Home");
            var bill = _context.Billings.FirstOrDefault(b => b.Id == id);
            if (bill != null && amount > 0)
            {
                bill.PaidAmount += amount;
                if (bill.PaidAmount >= bill.TotalAmount)
                {
                    bill.PaidAmount = bill.TotalAmount;
                    bill.Status     = "Paid";
                    TempData["Success"] = $"Bill #{id} fully paid.";
                }
                else
                {
                    TempData["Success"] = $"₹{amount:N2} recorded for Bill #{id}. Remaining: ₹{bill.PendingAmount:N2}";
                }
                _context.SaveChanges();
            }
            return RedirectToAction("PendingBill");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult MarkAsPaid(int id)
        {
            if (!IsAdmin()) return RedirectToAction("AdminLogin", "Home");
            var bill = _context.Billings.FirstOrDefault(b => b.Id == id);
            if (bill != null)
            {
                bill.PaidAmount = bill.TotalAmount;
                bill.Status     = "Paid";
                _context.SaveChanges();
                TempData["Success"] = $"Bill #{id} marked as paid.";
            }
            return RedirectToAction("PendingBill");
        }

        // ── Invoice ──────────────────────────────────────────────
        public IActionResult ViewInvoice(int id)
        {
            if (!IsAdmin()) return RedirectToAction("AdminLogin", "Home");
            var bill = _context.Billings.FirstOrDefault(b => b.Id == id);
            if (bill == null) { TempData["Error"] = "Bill not found."; return RedirectToAction("PendingBill"); }
            return View("~/Views/Shared/Invoice.cshtml", bill);
        }

        // ── Bill Edit (Manage Bills) ──────────────────────────────
        public IActionResult BillEdit(string? search, int page = 1)
        {
            if (!IsAdmin()) return RedirectToAction("AdminLogin", "Home");

            var query = _context.Billings.AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(b => b.PatientName.Contains(search) || b.PatientMobile.Contains(search));
            }
            ViewBag.Search = search;

            var paged = PaginatedList<BillingRecord>.Create(query.OrderByDescending(b => b.CreatedAt), page, PageSize);
            return View(paged);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteBill(int id)
        {
            if (!IsAdmin()) return RedirectToAction("AdminLogin", "Home");
            var bill = _context.Billings.FirstOrDefault(b => b.Id == id);
            if (bill != null)
            {
                _context.Billings.Remove(bill);
                _context.SaveChanges();
                TempData["Success"] = $"Bill #{id} deleted successfully.";
            }
            return RedirectToAction("BillEdit");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult GenerateBarcode(int id)
        {
            if (!IsAdmin()) return RedirectToAction("AdminLogin", "Home");
            // Placeholder for actual barcode generation logic
            TempData["Success"] = $"Barcode generated for Bill #{id} (Simulated).";
            return RedirectToAction("BillEdit");
        }

        // ── Prescriptions ─────────────────────────────────────────
        public IActionResult Prescriptions(string? search, int page = 1)
        {
            if (!IsAdmin()) return RedirectToAction("AdminLogin", "Home");

            var query = _context.Prescriptions.AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(p =>
                    p.PatientName.Contains(search) ||
                    p.PatientMobile.Contains(search) ||
                    p.DoctorName.Contains(search));
            }
            ViewBag.Search = search;
            ViewBag.Doctors = _context.Doctors.Where(d => d.IsActive).OrderBy(d => d.Name).ToList();
            ViewBag.Patients = _context.PatientRecords.OrderBy(p => p.FirstName).ThenBy(p => p.LastName).ToList();

            // Get confirmed appointments for the dropdown
            ViewBag.Appointments = _context.Appointments
                .Where(a => a.Status == "Confirmed")
                .OrderByDescending(a => a.CreatedAt).ToList();

            var paged = PaginatedList<PrescriptionRecord>.Create(
                query.OrderByDescending(p => p.CreatedAt), page, PageSize);

            return View(paged);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddPrescription(int? appointmentId, string patientMobile,
            string doctorName, string diagnosis, string medicines, string? notes)
        {
            if (!IsAdmin()) return RedirectToAction("AdminLogin", "Home");

            var patient = _context.PatientRecords.FirstOrDefault(p => p.Mobile == patientMobile);
            if (patient == null)
            { TempData["Error"] = "Patient not found."; return RedirectToAction("Prescriptions"); }

            _context.Prescriptions.Add(new PrescriptionRecord
            {
                AppointmentId = appointmentId,
                PatientMobile = patient.Mobile,
                PatientName   = $"{patient.FirstName} {patient.LastName}",
                DoctorName    = doctorName,
                Diagnosis     = diagnosis,
                Medicines     = medicines,
                Notes         = notes
            });
            _context.SaveChanges();
            TempData["Success"] = $"Prescription added for {patient.FirstName} {patient.LastName}.";
            return RedirectToAction("Prescriptions");
        }

        // ── Analytics ─────────────────────────────────────────────
        public IActionResult Analytics()
        {
            if (!IsAdmin()) return RedirectToAction("AdminLogin", "Home");

            // Monthly revenue (last 6 months)
            var sixMonthsAgo = DateTime.UtcNow.AddMonths(-6);
            var billings = _context.Billings.Where(b => b.CreatedAt >= sixMonthsAgo).ToList();

            var monthlyRevenue = billings
                .GroupBy(b => b.CreatedAt.ToString("yyyy-MM"))
                .OrderBy(g => g.Key)
                .Select(g => new { Month = g.Key, Revenue = g.Sum(b => b.PaidAmount), Billed = g.Sum(b => b.TotalAmount) })
                .ToList();

            ViewBag.RevenueLabels = System.Text.Json.JsonSerializer.Serialize(monthlyRevenue.Select(m => m.Month));
            ViewBag.RevenueData   = System.Text.Json.JsonSerializer.Serialize(monthlyRevenue.Select(m => m.Revenue));
            ViewBag.BilledData    = System.Text.Json.JsonSerializer.Serialize(monthlyRevenue.Select(m => m.Billed));

            // Popular tests (top 10)
            var popularTests = _context.Appointments
                .GroupBy(a => a.PackageName)
                .Select(g => new { Name = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(10).ToList();

            ViewBag.TestLabels = System.Text.Json.JsonSerializer.Serialize(popularTests.Select(t => t.Name));
            ViewBag.TestData   = System.Text.Json.JsonSerializer.Serialize(popularTests.Select(t => t.Count));

            // Patient registrations (last 6 months)
            var patients = _context.PatientRecords.ToList(); // no CreatedAt on PatientRecord, use count
            ViewBag.TotalPatients = patients.Count;

            // Appointment status distribution
            var statusDist = _context.Appointments
                .GroupBy(a => a.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToList();

            ViewBag.StatusLabels = System.Text.Json.JsonSerializer.Serialize(statusDist.Select(s => s.Status));
            ViewBag.StatusData   = System.Text.Json.JsonSerializer.Serialize(statusDist.Select(s => s.Count));

            // Summary stats
            ViewBag.TotalRevenue     = billings.Sum(b => b.PaidAmount);
            ViewBag.TotalBilled      = billings.Sum(b => b.TotalAmount);
            ViewBag.TotalAppointments = _context.Appointments.Count();
            ViewBag.TestsThisMonth   = _context.Appointments
                .Count(a => a.CreatedAt.Month == DateTime.UtcNow.Month && a.CreatedAt.Year == DateTime.UtcNow.Year);
            ViewBag.AvgBillValue     = billings.Any() ? billings.Average(b => b.TotalAmount) : 0m;

            return View();
        }

        // ── Report upload ─────────────────────────────────────────
        public IActionResult Report()
        {
            if (!IsAdmin()) return RedirectToAction("AdminLogin", "Home");
            ViewBag.Patients = _context.PatientRecords.OrderBy(p => p.FirstName).ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessUpload(
            string patientMobile, string testName, IFormFile reportFile)
        {
            if (!IsAdmin()) return RedirectToAction("AdminLogin", "Home");

            if (reportFile != null && reportFile.Length > 0)
            {
                var patient = _context.PatientRecords.FirstOrDefault(p => p.Mobile == patientMobile);
                if (patient == null)
                { TempData["Error"] = "Patient not found."; return RedirectToAction("Report"); }

                var uploadsDir = Path.Combine(_env.WebRootPath, "uploads");
                if (!Directory.Exists(uploadsDir)) Directory.CreateDirectory(uploadsDir);

                var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(reportFile.FileName)}";
                using var fs = new FileStream(Path.Combine(uploadsDir, fileName), FileMode.Create);
                await reportFile.CopyToAsync(fs);

                _context.Reports.Add(new ReportRecord
                {
                    PatientMobile = patient.Mobile,
                    PatientName   = $"{patient.FirstName} {patient.LastName}",
                    TestName      = testName,
                    FilePath      = "/uploads/" + fileName
                });
                await _context.SaveChangesAsync();

                TempData["Success"] = $"'{testName}' uploaded for {patient.FirstName} {patient.LastName}.";
                return RedirectToAction("Dashboard");
            }

            TempData["Error"] = "Please select a valid PDF file.";
            return RedirectToAction("Report");
        }

        // ── Pending Tests (Awaiting Reports) ──────────────────────
        public IActionResult PendingTests(string? search, int page = 1)
        {
            if (!IsAdmin()) return RedirectToAction("AdminLogin", "Home");

            // Get appointments that don't have a corresponding report uploaded yet (matching by mobile and test name)
            var query = _context.Appointments
                .Where(a => a.Status != "Cancelled" && !_context.Reports.Any(r => r.PatientMobile == a.PatientMobile && r.TestName == a.PackageName))
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(a => a.PatientName.Contains(search) || a.PatientMobile.Contains(search));
            }

            ViewBag.Search = search;
            var paged = PaginatedList<AppointmentRecord>.Create(query.OrderByDescending(a => a.CreatedAt), page, PageSize);
            return View(paged);
        }

        // ── Financial Report ──────────────────────────────────────
        public IActionResult FinancialReport(DateTime? fromDate, DateTime? toDate, string paymentMode)
        {
            if (!IsAdmin()) return RedirectToAction("AdminLogin", "Home");

            var query = _context.Billings.AsQueryable();
            
            if (fromDate.HasValue)
                query = query.Where(b => b.CreatedAt >= fromDate.Value.ToUniversalTime());
            
            if (toDate.HasValue)
            {
                // Add one day to include the whole toDate
                var endOfToDate = toDate.Value.ToUniversalTime().AddDays(1);
                query = query.Where(b => b.CreatedAt < endOfToDate);
            }

            // We don't have PaymentMode in BillingRecord, so we'll just ignore the filter for now or mock it
            ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
            ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");
            ViewBag.PaymentMode = paymentMode;

            var results = query.OrderByDescending(b => b.CreatedAt).ToList();
            
            ViewBag.GrandTotal = results.Sum(b => b.TotalAmount);
            ViewBag.GrandPaid = results.Sum(b => b.PaidAmount);
            ViewBag.GrandDue = results.Sum(b => b.PendingAmount);

            return View(results);
        }

        // ── Seed Doctors (one-time) ────────────────────────────────
        public IActionResult SeedDoctors()
        {
            if (!IsAdmin()) return RedirectToAction("AdminLogin", "Home");
            if (_context.Doctors.Any())
            {
                TempData["Success"] = "Doctors already exist. No seeding needed.";
                return RedirectToAction("Doctors");
            }

            var seedDoctors = new List<DoctorRecord>
            {
                new DoctorRecord { Name = "Rajesh Kumar",    Specialization = "General Medicine", Mobile = "9876543210" },
                new DoctorRecord { Name = "Priya Sharma",    Specialization = "Pathology",        Mobile = "9876543211" },
                new DoctorRecord { Name = "Anand Rao",       Specialization = "Cardiology",       Mobile = "9876543212" },
                new DoctorRecord { Name = "Meera Nair",      Specialization = "Endocrinology",    Mobile = "9876543213" },
                new DoctorRecord { Name = "Suresh Bhat",     Specialization = "Orthopedics",      Mobile = "9876543214" },
                new DoctorRecord { Name = "Kavitha Hegde",   Specialization = "Gynecology",       Mobile = "9876543215" },
                new DoctorRecord { Name = "Vikram Shetty",   Specialization = "Dermatology",      Mobile = "9876543216" },
                new DoctorRecord { Name = "Lakshmi Pai",     Specialization = "Pediatrics",       Mobile = "9876543217" },
            };
            _context.Doctors.AddRange(seedDoctors);
            _context.SaveChanges();
            TempData["Success"] = $"{seedDoctors.Count} doctors seeded successfully!";
            return RedirectToAction("Doctors");
        }

        // ── Contact Messages ──────────────────────────────────────
        public IActionResult ContactMessages()
        {
            if (!IsAdmin()) return RedirectToAction("AdminLogin", "Home");
            var messages = _context.ContactMessages
                .OrderByDescending(m => m.CreatedAt)
                .ToList();
            return View(messages);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult MarkMessageRead(int id)
        {
            if (!IsAdmin()) return RedirectToAction("AdminLogin", "Home");
            var msg = _context.ContactMessages.Find(id);
            if (msg != null) { msg.IsRead = true; _context.SaveChanges(); }
            return RedirectToAction("ContactMessages");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteMessage(int id)
        {
            if (!IsAdmin()) return RedirectToAction("AdminLogin", "Home");
            var msg = _context.ContactMessages.Find(id);
            if (msg != null) { _context.ContactMessages.Remove(msg); _context.SaveChanges(); }
            return RedirectToAction("ContactMessages");
        }

        // ── Logout ────────────────────────────────────────────────
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}