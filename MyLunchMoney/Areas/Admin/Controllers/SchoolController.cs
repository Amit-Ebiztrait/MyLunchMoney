using Microsoft.AspNet.Identity.Owin;
using MyLunchMoney.Constants;
using MyLunchMoney.Repository;
using MyLunchMoney.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace MyLunchMoney.Areas.Admin.Controllers
{
    public class SchoolController : AdminBaseController
    {
        #region Declaration
        private readonly ISchoolService _schoolService;
        private readonly IStateRepository _stateRepository;
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private readonly IEmailService _emailService;
        #endregion

        #region Constructor
        public SchoolController(ISchoolService schoolService, IEmailService emailService, IStateRepository stateRepository)
        {
            _schoolService = schoolService;
            _stateRepository = stateRepository;
            _emailService = emailService;
        }
        public SchoolController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            _UserManager = userManager;
            SignInManager = signInManager;
        }
        #endregion

        #region Get Methods
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<ActionResult> GetGeneralSchoolDetailAsync(string id)
        {
            var model = await _schoolService.GetGeneralSchoolDetailAsync(id);

            return Json(new { data = model }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public async Task<ActionResult> GetSchoolTypeDropDownListAsync()
        {
            var list = new List<KeyValuePair<string, Int64>>();
            var result = await _schoolService.GetSchoolTypeDropDownListAsync();
            foreach (var e in result)
            {
                list.Add(new KeyValuePair<string, Int64>(e.SchoolTypeName, e.SchoolTypeId));
            }

            return Json(new { data = list }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public async Task<ActionResult> GetStatesDropDownListAsync()
        {
            var list = new List<KeyValuePair<string, Int64>>();
            var result = await _stateRepository.GetAllAsync();
            foreach (var e in result)
            {
                list.Add(new KeyValuePair<string, Int64>(e.StateName, e.StateId));
            }

            return Json(new { data = list }, JsonRequestBehavior.AllowGet);
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

        public ApplicationUserManager _UserManager
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

        #endregion

        #region Post Mothods
        [HttpPost]
        public async Task<ActionResult> AddSchoolAsync()
        {
            var result = await _schoolService.AddSchoolAsync(SchoolRequest);
            if (result.code == System.Net.HttpStatusCode.OK)
            {
                var user = await _UserManager.FindByNameAsync(SchoolRequest.School.SchoolEmail);
                if (user == null)
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    var resultuser = new Dictionary<string, string>
                        {
                            { "code", "400"},
                            { "message", MessageCode.UserNotExist}
                        };
                    return Json(new { data = resultuser }, JsonRequestBehavior.AllowGet);
                }

                // Send an email with link
                string code = await _UserManager.GeneratePasswordResetTokenAsync(user.Id);
                var callbackUrl = Url.Action("ResetPassword", "Account", new { code = code }, protocol: Request.Url.Scheme);
                string body = string.Empty;
                using (StreamReader reader = new StreamReader(Server.MapPath("~/Template/ForgotPassword.html")))
                {
                    body = reader.ReadToEnd();
                }
                body = body.Replace("{resetLink}", callbackUrl);
                bool IsSendEmail = _emailService.EmailSend(SchoolRequest.School.SchoolEmail, "Welcome to MILUNCHMONEY!", body, true);
                if (IsSendEmail)
                {
                    return Json(new { data = result }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var resultemail = new Dictionary<string, string>
                        {
                            { "code", "400"},
                            { "message", "Email Sending Error"}
                        };
                    return Json(new { data = resultemail }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { data = result }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<ActionResult> SchoolActiveDeactiveAsync(string id, int active)
        {
            
            var result = await _schoolService.SchoolActiveDeactiveAsync(id, active);
            //if (result.code == System.Net.HttpStatusCode.OK && active == 1)
            //{
            //    var schoolData = await _schoolService.GetGeneralSchoolDetailAsync(id);
            //    var user = await _UserManager.FindByNameAsync(schoolData.SchoolEmail);
            //    if (user == null)
            //    {
            //        // Don't reveal that the user does not exist or is not confirmed
            //        var resultuser = new Dictionary<string, string>
            //            {
            //                { "code", "400"},
            //                { "message", MessageCode.UserNotExist}
            //            };
            //        return Json(new { data = resultuser }, JsonRequestBehavior.AllowGet);
            //    }

            //    // Send an email with link
            //    string code = await _UserManager.GeneratePasswordResetTokenAsync(user.Id);
            //    var callbackUrl = Url.Action("ResetPassword", "Account", new { code = code }, protocol: Request.Url.Scheme);
            //    string body = string.Empty;
            //    using (StreamReader reader = new StreamReader(Server.MapPath("~/Template/ForgotPassword.html")))
            //    {
            //        body = reader.ReadToEnd();
            //    }
            //    body = body.Replace("{resetLink}", callbackUrl);
            //    bool IsSendEmail = _emailService.EmailSend(schoolData.SchoolEmail, "Welcome to MILUNCHMONEY!", body, true);
            //    if (IsSendEmail)
            //    {
            //        return Json(new { data = result }, JsonRequestBehavior.AllowGet);
            //    }
            //    else
            //    {
            //        var resultemail = new Dictionary<string, string>
            //            {
            //                { "code", "400"},
            //                { "message", "Email Sending Error"}
            //            };
            //        return Json(new { data = resultemail }, JsonRequestBehavior.AllowGet);
            //    }
            //}
            return Json(new { data = result }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<ActionResult> SchoolDeleteAsync(string id)
        {

            var result = await _schoolService.SchoolDeleteAsync(id);
            return Json(new { data = result }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<ActionResult> SchoolApproveDisapproveAsync(string id, int status)
        {
            var result = await _schoolService.SchoolApproveDisapproveAsync(id, status);
            if (result.code == System.Net.HttpStatusCode.OK && status == 3)
            {
                var schoolData = await _schoolService.GetGeneralSchoolDetailAsync(id);
                var user = await _UserManager.FindByNameAsync(schoolData.SchoolEmail);
                if (user == null)
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    var resultuser = new Dictionary<string, string>
                        {
                            { "code", "400"},
                            { "message", MessageCode.UserNotExist}
                        };
                    return Json(new { data = resultuser }, JsonRequestBehavior.AllowGet);
                }

                // Send an email with link
                string code = await _UserManager.GeneratePasswordResetTokenAsync(user.Id);
                var callbackUrl = Url.Action("ResetPassword", "Account", new { code = code }, protocol: Request.Url.Scheme);
                string body = string.Empty;
                using (StreamReader reader = new StreamReader(Server.MapPath("~/Template/ForgotPassword.html")))
                {
                    body = reader.ReadToEnd();
                }
                body = body.Replace("{resetLink}", callbackUrl);
                bool IsSendEmail = _emailService.EmailSend(schoolData.SchoolEmail, "Welcome to MILUNCHMONEY!", body, true);
                if (IsSendEmail)
                {
                    return Json(new { data = result }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var resultemail = new Dictionary<string, string>
                        {
                            { "code", "400"},
                            { "message", "Email Sending Error"}
                        };
                    return Json(new { data = resultemail }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { data = result }, JsonRequestBehavior.AllowGet);
        }
        #endregion
    }
}