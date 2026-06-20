using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Diagnostics;
using System.Linq;
using ksp.care.Models;
using ksp.care.Helpers;

namespace ksp.care.Controllers
{
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _config;

        public HomeController(ApplicationDbContext context, IConfiguration config)
        {
            _context = context;
            _config  = config;
        }

        // ── Public pages ──────────────────────────────────────────
        public IActionResult Index()
        {
            ViewBag.Doctors = _context.Doctors
                .Where(d => d.IsActive)
                .OrderBy(d => d.Name)
                .ToList();
            ViewBag.Packages = _context.Tests
                .Where(t => t.IsActive && t.Category == "Health Package")
                .ToList();
            return View();
        }
        public IActionResult Privacy() => View();
        public IActionResult About() => View();
        public IActionResult Contact() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SubmitContact(string Name, string Phone, string Message)
        {
            if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(Phone) || string.IsNullOrWhiteSpace(Message))
            {
                TempData["ContactError"] = "All fields are required.";
                return RedirectToAction("Contact");
            }
            _context.ContactMessages.Add(new ksp.care.Models.ContactMessage
            {
                Name = Name.Trim(),
                Phone = Phone.Trim(),
                Message = Message.Trim(),
                CreatedAt = DateTime.Now
            });
            _context.SaveChanges();
            TempData["ContactSuccess"] = "Message sent! We'll get back to you soon.";
            return RedirectToAction("Contact");
        }

        // ── Patient registration ──────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateProfile(PatientRecord newUser)
        {
            if (_context.PatientRecords.Any(p => p.Mobile == newUser.Mobile))
            {
                TempData["Error"] = "An account already exists with this mobile number.";
                return RedirectToAction("Index");
            }
            newUser.Password = PasswordHelper.HashPassword(newUser.Password);
            _context.PatientRecords.Add(newUser);
            _context.SaveChanges();
            TempData["Success"] = "Profile created! Please login with your password.";
            return RedirectToAction("UserLogin");
        }

        // ── Patient auth ──────────────────────────────────────────
        public IActionResult UserLogin()
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("UserMobile")))
                return RedirectToAction("Dashboard", "User");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ProcessUserLogin(string identifier, string password)
        {
            var user = _context.PatientRecords
                .FirstOrDefault(u => u.Mobile == identifier);

            if (user != null)
            {
                bool valid;
                if (PasswordHelper.IsBCryptHash(user.Password))
                {
                    valid = PasswordHelper.VerifyPassword(password, user.Password);
                }
                else
                {
                    // Legacy plain-text password — verify & auto-upgrade to hash
                    valid = user.Password == password;
                    if (valid)
                    {
                        user.Password = PasswordHelper.HashPassword(password);
                        _context.SaveChanges();
                    }
                }

                if (valid)
                {
                    HttpContext.Session.SetString("UserMobile", user.Mobile);
                    return RedirectToAction("Dashboard", "User");
                }
            }

            TempData["Error"] = "Invalid mobile number or password.";
            return RedirectToAction("UserLogin");
        }

        // ── Admin / Staff auth ────────────────────────────────────
        public IActionResult AdminLogin()
        {
            if (HttpContext.Session.GetString("IsAdmin") == "true")
                return RedirectToAction("Dashboard", "Admin");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ProcessAdminLogin(string staffId, string passkey)
        {
            var adminPasskey = _config["AdminSettings:Passkey"] ?? "admin123";
            if (!string.IsNullOrWhiteSpace(staffId) && passkey == adminPasskey)
            {
                HttpContext.Session.SetString("IsAdmin", "true");
                HttpContext.Session.SetString("CurrentStaffId", staffId);
                return RedirectToAction("Dashboard", "Admin");
            }
            TempData["Error"] = "Authentication Failed: Invalid Staff ID or Passkey.";
            return RedirectToAction("AdminLogin");
        }

        // ── Error ─────────────────────────────────────────────────
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult PageNotFound()
        {
            Response.StatusCode = StatusCodes.Status404NotFound;
            return View("NotFound");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}
