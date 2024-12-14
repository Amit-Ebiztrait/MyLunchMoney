

using MyLunchMoney.Models;
using MyLunchMoney.Services;
using Microsoft.AspNet.Identity.Owin;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using MyLunchMoney.Constants;
using MyLunchMoney.Infrastructure.EnumType;
using MyLunchMoney.Repository;
using MyLunchMoney.EnumType;
using System;
using System.IO;
using System.Net;

namespace MyLunchMoney.Areas.Admin.Controllers
{
    public class LoginController : Controller
    {
        #region Declaration
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private ApplicationDbContext _context;
        private ITokenService _tokenService;
        private IPushNotificationService _notification;
        private SchoolRepository _schoolRepository;
        private UserRepository _userRepository;
        private readonly IEmailService _emailService;
        #endregion

        #region Constructor
        public LoginController(
        ApplicationDbContext context
        , ITokenService tokenService
        , IPushNotificationService notification
        , SchoolRepository schoolRepository
        , UserRepository userrepository
        , IEmailService emailService
        )
        {
            _context = context;
            _tokenService = tokenService;
            _notification = notification;
            _schoolRepository = schoolRepository;
            _userRepository = userrepository;
            _emailService = emailService;
        }
        #endregion

        #region Identity
        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }
        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }
        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }
        #endregion

        #region Methods
        [AllowAnonymous]
        public async Task<ActionResult> Index(string returnUrl)
        {
            //var user = await UserManager.FindByEmailAsync(User.Identity.Name);
            //if (user != null)
            //{
            //    string rolename = UserManager.GetRoles(user.Id).FirstOrDefault();
            //    if (!string.IsNullOrEmpty(rolename))
            //    {
            //        Session.Timeout = 1140;
            //        if (User.Identity.IsAuthenticated && rolename == RoleName.SuperAdmin)
            //        {
            //            return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
            //        }
            //        else if (User.Identity.IsAuthenticated && rolename == RoleName.SchoolAdmin)
            //        {
            //            return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
            //        }
            //        else if (User.Identity.IsAuthenticated && rolename == RoleName.SchoolStaff)
            //        {
            //            return RedirectToAction("Index", "StudentStaff", new { area = "Admin" });
            //        }
            //        else
            //        {
            //            return RedirectToAction("LogOff", "Account", new { areas = "Admin" });
            //        }
            //    }
            //}
            //ViewBag.ReturnUrl = returnUrl;
            return View("~/Areas/Admin/views/Login/Index.cshtml");
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Index(LoginViewModel model, string returnUrl)
        {

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            //Check Account Inactive
            var user = await UserManager.FindByEmailAsync(model.Email);
            if (user != null)
            {
                
                string rolename = UserManager.GetRoles(user.Id).FirstOrDefault();
                if (rolename == RoleName.Parent
                    || rolename == RoleName.Spouse
                    || rolename == RoleName.Sponsor
                    || rolename == RoleName.Student
                    )
                {
                    ModelState.AddModelError("", MessageCode.InvalidEmailPassword);
                    return View();
                }
                if (user.IsActive == 0)
                {
                    ModelState.AddModelError("", MessageCode.AccountDeactivated);
                    return View();
                }
                if (rolename == RoleType.SchoolAdmin.ToString())
                {
                    var schoolStatus = _schoolRepository.GetSchoolStatus(user.Id);
                    if (schoolStatus == (int)SchoolStatus.Pending || schoolStatus == (int)SchoolStatus.Disapprove || schoolStatus == (int)SchoolStatus.Inactive)
                    {
                        ModelState.AddModelError("", MessageCode.AccountStatus.Replace("#Status", Enum.GetName(typeof(SchoolStatus), schoolStatus)));
                        return View();
                    }
                }
            }

            var result = await SignInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, shouldLockout: false);

            if (result == SignInStatus.Success)
            {
                if (user != null)
                {
                    Session.Timeout = 1140;
                    string rolename = UserManager.GetRoles(user.Id).FirstOrDefault();
                    Session["CurrentRole"] = rolename;
                    Session["CurrentUserImage"] = user.ImagePath;
                    Session["CurrentUserId"] = user.Id;
                    if (rolename == RoleType.SchoolAdmin.ToString())
                    {
                        var sid = _context.schools.Where(t => t.UserId == user.Id).Select(t => t.SchoolId).FirstOrDefault();
                        Session["SchoolId"] = sid;
                        Session["CurrentUserId"] = user.Id;
                    }
                    else if (rolename == RoleType.SchoolStaff.ToString())
                    {
                        var sid = _context.staffuser.Where(t => t.UserId == user.Id).Select(t => t.SchoolId).FirstOrDefault();
                        var said = _context.schools.Where(t => t.SchoolId == sid).Select(t => t.UserId).FirstOrDefault();
                        Session["SchoolId"] = sid;
                        Session["SchoolAdminId"] = said;
                    }
                }
            }

            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl,model.Email);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid login attempt.");
                    return View(model);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl,string Email)
        {
            if (!Session["CurrentRole"].Equals(RoleName.SuperAdmin))
            {
                return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
            }
            ViewBag.EmailId = ViewBag.EmailId;
            return RedirectToverification(Email);
        }
        private ActionResult RedirectToverification(string Email)
        {
           // Email = "adipatel819@gmail.com";
            Verification model = new Verification();
            string numbers = "0123456789";
            Random random = new Random();
            string otp = string.Empty;
            for (int i = 0; i <= 5; i++)
            {
                int tempval = random.Next(0, numbers.Length);
                otp += tempval;
            }
            model.Code = otp;
            model.IsVerified = false;
            model.UserId = Session["CurrentUserId"].ToString();
            var result = _context.verifications.Add(model);
            _context.SaveChanges();
            string body = string.Empty;
            using (StreamReader reader = new StreamReader(System.Web.Hosting.HostingEnvironment.MapPath("~/Template/VerificationOTP.html")))
            {
                body = reader.ReadToEnd();
            }
            body = body.Replace("{resetLink}", model.Code);

            bool IsSendEmail = _emailService.EmailSend(Email, "", body, true);
            if (IsSendEmail)
            {
                return View("~/Areas/Admin/Views/Login/Verification.cshtml");
            }
            else
            {
                return View("~/Areas/Admin/Views/Login/Verification.cshtml");
                //TempData["ErrorMessage"] = "Verification mail can not Be send please login again to get verification code";
                //return View("~/Areas/Admin/views/Login/Index.cshtml");
            }
           
        }
        [HttpPost]
        [AllowAnonymous]

        public async Task<ActionResult> Verifiyotp(Verification model)
        {
            string UserId = Session["CurrentUserId"].ToString();
            var result = await _userRepository.GetverificationcodeAsync(UserId);
            if (result.Code == model.Code)
            {
                return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
            }
            else
            {
                TempData["ErrorMessage"] = "Please Enter Correct OTP To Verify";
                return View("~/Areas/Admin/Views/Login/Verification.cshtml", model);
            }
        }
        #endregion
    }
}