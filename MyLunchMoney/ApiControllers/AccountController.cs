using MyLunchMoney.Models;
using MyLunchMoney.Repository;
using Microsoft.AspNet.Identity;
using System.Threading.Tasks;
using System.Web.Http;
using System;
using MyLunchMoney.Services;
using System.Web;
using System.Linq;
using MyLunchMoney.Helpers;
using System.Net;
using MyLunchMoney.Constants;
using Microsoft.AspNet.Identity.Owin;
using System.Configuration;
using MyLunchMoney.Models.Request;
using MyLunchMoney.Models.ViewModels;
using System.IO;

namespace MyLunchMoney.ApiControllers
{
    [RoutePrefix("api")]
    public class AccountController : ApiController
    {
        #region Declairation
        private readonly IEmailService _emailService;
        private readonly IAuthRepository _authRepository;
        private readonly IStateRepository _stateRepository;
        private readonly IAuthService _authService;
        private readonly IAWSService _awsService;
        private readonly ISchoolService _schoolService;
        private readonly IStoryService _storyService;
        private readonly ITransactionRepository _transactionRepository;
        private readonly ITransactionService _transactionService;
        private readonly IUserService _userService;
        private readonly string _defaultPassword = ConfigurationManager.AppSettings["DefaultPassword"].ToString();
        private readonly IMenuService _menuService;
        private readonly INotificationService _notificationService;
        #endregion

        #region Constructor
        public AccountController(
           IEmailService emailService,
           IAuthRepository authRepository,
           IAWSService awsService,
           IAuthService authService,
           IStateRepository stateRepository,
           ISchoolService schoolService,
           IStoryService storyService, ITransactionRepository transactionRepository, ITransactionService transactionService, IUserService userService, IMenuService menuService, INotificationService notificationService)
        {
            _emailService = emailService;
            _authRepository = authRepository;
            _awsService = awsService;
            _authService = authService;
            _stateRepository = stateRepository;
            _schoolService = schoolService;
            _storyService = storyService;
            _transactionRepository = transactionRepository;
            _transactionService = transactionService;
            _userService = userService;
            _menuService = menuService;
            _notificationService = notificationService;
        }
        #endregion

        #region Methods

        // POST api/Account/Register
        [HttpPost]
        [AllowAnonymous]
        [Route("Login")]
        public async Task<IHttpActionResult> Login([FromBody] LoginModel model)
        {
            var result = await _authRepository.LoginUser(model);

            return Ok(result);
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("GetPassword")]
        public string GetPassword(string password)
        {
            return new PasswordHelper().Decrypt(password);
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("GetUserTokenById")]
        public async Task<string> GetUserTokenById(string id)
        {
            return await _authRepository.GetUserToken(id);
        }

        // POST api/Account/Register
        [HttpPost]
        [AllowAnonymous]
        [Route("Register")]
        public async Task<IHttpActionResult> Register([FromBody] RegisterViewModel model)
        {
            var result = await _authService.RegisterUser(model, null);

            return Ok(result);
        }


        [HttpPost]
        [AllowAnonymous]
        [Route("RegisterStudent")]
        public async Task<IHttpActionResult> RegisterStudent()
        {
            var student = new RegisterViewModel()
            {
                Email = HttpContext.Current.Request.Form["Email"],
                Password = _defaultPassword,
                ConfirmPassword = _defaultPassword,
                FirstName = HttpContext.Current.Request.Form["FirstName"],
                LastName = HttpContext.Current.Request.Form["LastName"],
                DateOfBirth = Convert.ToDateTime(HttpContext.Current.Request.Form["DateOfBirth"]),
                GradeId = Convert.ToInt32(HttpContext.Current.Request.Form["GradeId"]),
                ClassId = HttpContext.Current.Request.Form["ClassId"],
                Balance = Convert.ToDouble(HttpContext.Current.Request.Form["Balance?"]),
                CurrencyType = HttpContext.Current.Request.Form["CurrencyType"],
                TransactionLimit = Convert.ToDouble(HttpContext.Current.Request.Form["TransactionLimit?"]),
                TransactionPin = HttpContext.Current.Request.Form["TransactionPin"],
                IsSponsored = Convert.ToInt32(HttpContext.Current.Request.Form["IsSponsored"]),
                IsLock = Convert.ToInt32(HttpContext.Current.Request.Form["IsLock"]),
                ParishId = Convert.ToInt32(HttpContext.Current.Request.Form["ParishId"]),
                SchoolId = HttpContext.Current.Request.Form["SchoolId"],
                RoleId = Convert.ToInt32(HttpContext.Current.Request.Form["RoleId"]),
                MLMID = HttpContext.Current.Request.Form["MLMID"],
                ParentUserId = HttpContext.Current.Request.Form["ParentUserId"],
                IsSponsorNeeded = Convert.ToInt32(HttpContext.Current.Request.Form["IsSponsorNeeded"])
            };

            var file = HttpContext.Current.Request.Files.Count > 0 ? HttpContext.Current.Request.Files[0] : null;
            var result = await _authService.RegisterUser(student, file);

            return Ok(result);
        }

        [HttpPost]
        [Route("RegisterSchoolStaff")]
        public async Task<IHttpActionResult> RegisterSchoolStaff()
        {
            var staffUser = new RegisterViewModel()
            {
                Email = HttpContext.Current.Request.Form["Email"],
                FirstName = HttpContext.Current.Request.Form["FirstName"],
                LastName = HttpContext.Current.Request.Form["LastName"],
                PhoneNumber = HttpContext.Current.Request.Form["PhoneNumber"],
                SchoolId = HttpContext.Current.Request.Form["SchoolId"],
                StaffRoleName = HttpContext.Current.Request.Form["StaffRoleName"],
                Password = _defaultPassword,
                RoleId = Convert.ToInt32(HttpContext.Current.Request.Form["RoleId"])
            };

            var file = HttpContext.Current.Request.Files.Count > 0 ? HttpContext.Current.Request.Files[0] : null;
            var result = await _authService.RegisterUser(staffUser, file);

            return Ok(result);
        }

        [HttpPost]
        [Route("StudentIsLockAsync")]
        public async Task<IHttpActionResult> StudentIsLockAsync()
        {
            var result = await _authRepository.IsLockAsync(new StudentLockRequest());

            return Ok(result);
        }

        //[HttpPost]
        //[AllowAnonymous]
        //[Route("ImageUpload")]
        //public IHttpActionResult ImageUpload()
        //{
        //	var file = HttpContext.Current.Request.Files.Count > 0 ? HttpContext.Current.Request.Files[0] : null as HttpPostedFile;
        //	var guid = Guid.NewGuid().ToString();
        //	var folder = "users";

        //	var result = _awsService.AwsSaveImage(file, folder);

        //	return Ok(result);
        //}

        [HttpPost]
        [Route("AddSecoundaryAccount")]
        public async Task<IHttpActionResult> AddSecoundaryAccount()
        {
            var student = new RegisterViewModel()
            {
                Email = HttpContext.Current.Request.Form["Email"],
                PhoneNumber = HttpContext.Current.Request.Form["PhoneNumber"],
                Password = "1234567890",
                ConfirmPassword = "1234567890",
                FullName = HttpContext.Current.Request.Form["FullName"],
                RoleId = Convert.ToInt32(HttpContext.Current.Request.Form["RoleId"]),
                ParentUserId = HttpContext.Current.Request.Form["ParentUserId"]
            };

            var file = HttpContext.Current.Request.Files.Count > 0 ? HttpContext.Current.Request.Files[0] : null;
            var result = await _authService.RegisterUser(student, file);

            return Ok(result);
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("ForgotPassword")]
        public async Task<IHttpActionResult> ForgotPasswordAsync([FromBody] ForgotPasswordModel model)
        {
            var IdentityManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();

            try
            {
                var user = await IdentityManager.FindByNameAsync(model.Email);
                if (user == null || !(await IdentityManager.IsEmailConfirmedAsync(user.Id)))
                {
                    return Ok(new ApiHttpResponse(HttpStatusCode.BadRequest, MessageCode.UserNotExist));
                }

                // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                // Send an email with this link
                string cod = await IdentityManager.GeneratePasswordResetTokenAsync(user.Id);
                //var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);

                var callbackUrl = Url.Link("Default", new
                {
                    Controller = "Account",
                    Action = "ResetPassword",
                    code = cod
                });

                string body = string.Empty;
                using (StreamReader reader = new StreamReader(System.Web.Hosting.HostingEnvironment.MapPath("~/Template/ForgotPassword.html")))
                {
                    body = reader.ReadToEnd();
                }
                body = body.Replace("{resetLink}", callbackUrl);

                bool IsSendEmail = _emailService.EmailSend(model.Email, "Recover your account password", body, true);
                if (IsSendEmail)
                {
                    var result = new ApiHttpResponse(HttpStatusCode.OK, MessageCode.ResetPasswordLink);
                    return Ok(result);
                }
                else
                {
                    var result = new ApiHttpResponse(HttpStatusCode.BadRequest, MessageCode.InvalidInput);
                    return Ok(result);
                }
            }
            catch (Exception ex)
            {
                var error = new ApiHttpResponse(HttpStatusCode.InternalServerError, ex.Message);
                return Ok(error);
            }
        }

        [HttpPost]
        [Route("ChangePasswordAsync")]
        public async Task<IHttpActionResult> ChangePasswordAsync([FromBody] ChangePasswordModel model)
        {
            var IdentityManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
            if (model.UserId == null || model.UserId == "")
            {
                ApplicationUser users = System.Web.HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>().FindById(System.Web.HttpContext.Current.User.Identity.GetUserId());
                model.UserId = users.Id;
                model.UserName = users.UserName;
            }
            try
            {
                if (String.Compare(model.NewPassword, model.ConfirmPassword) != 0)
                    return Ok(new ApiHttpResponse(HttpStatusCode.OK, MessageCode.PasswordNotMatch));

                var result = await IdentityManager.ChangePasswordAsync(model.UserId, model.OldPassword, model.NewPassword);

                if (result.Succeeded)
                    return Ok(new ApiHttpResponse(HttpStatusCode.OK, MessageCode.PasswordChanged));
                else
                    return Ok(new ApiHttpResponse(HttpStatusCode.BadRequest, result.Errors.FirstOrDefault()));
            }
            catch (Exception ex)
            {
                return Ok(new ApiHttpResponse(HttpStatusCode.OK, ex.Message));
            }
        }

        [HttpPost]
        [Route("GetAllStateForDropDownAsync")]
        public async Task<IHttpActionResult> GetAllStateForDropDownAsync()
        {
            var result = await _stateRepository.GetAllAsync();

            return Ok(result);
        }

        [NonAction]
        private IHttpActionResult GetErrorResult(IdentityResult result)
        {
            if (result == null)
            {
                return InternalServerError();
            }

            if (!result.Succeeded)
            {
                if (result.Errors != null)
                {
                    foreach (string error in result.Errors)
                    {
                        ModelState.AddModelError("", error);
                    }
                }

                if (ModelState.IsValid)
                {
                    // No ModelState errors are available to send, so just return an empty BadRequest.
                    return BadRequest();
                }

                return BadRequest(ModelState);
            }

            return null;
        }
        [HttpPost]
        [Route("GetSchoolsByStateIdWithoutTokenAsync")]
        public async Task<IHttpActionResult> GetSchoolsByStateIdWithoutTokenAsync(int pageNumber, int pageSize, Int64 stateId)
        {
            var result = await _schoolService.GetSchoolsByStateIdWithoutTokenAsync(pageNumber, pageSize, stateId);
            return Ok(result);
        }
        [HttpPost]
        [Route("GetSchoolDetailBySchoolIdWithoutTokenAsync")]
        public async Task<IHttpActionResult> GetSchoolDetailBySchoolIdWithoutTokenAsync(string schoolid)
        {
            var result = await _schoolService.GetSchoolDetailBySchoolIdWithoutTokenAsync(schoolid);

            return Ok(result);
        }
        [HttpPost]
        [Route("GetCafeteriaHistoryWithoutTokenAsync")]
        public async Task<IHttpActionResult> GetCafeteriaHistoryWithoutTokenAsync(string studentId, int days)
        {
            var startDate = DateTime.Now.AddDays(-days);
            var endDate = DateTime.Now;
            var result = await _transactionRepository.GetCafeteriaHistory(studentId, startDate, endDate);

            return Ok(result);
        }
        [HttpPost]
        [Route("GetSchoolDropdownListWithoutTokenAsync")]
        public async Task<IHttpActionResult> GetSchoolDropdownListWithoutTokenAsync()
        {
            var result = await _schoolService.GetSchoolDropdownListAsync();

            return Ok(result);
        }
        [HttpPost]
        [Route("GuestStorySponsorPaymentAsync")]
        public async Task<IHttpActionResult> GuestStorySponsorPaymentAsync([FromBody] AddStorySponsorViewModel model)
        {
            var result = await _transactionService.GuestStorySponsorPaymentAsync(model);

            return Ok(result);
        }
        [HttpPost]
        [Route("GuestSchoolSponsorPaymentAsync")]
        public async Task<IHttpActionResult> GuestSchoolSponsorPaymentAsync([FromBody] AddSchoolSponsorViewModel model)
        {
            var result = await _transactionService.GuestSchoolSponsorPaymentAsync(model);

            return Ok(result);
        }
        [HttpPost]
        [Route("GuestStudentSponsorPaymentAsync")]
        public async Task<IHttpActionResult> GuestStudentSponsorPaymentAsync([FromBody] AddStudentSponsorViewModel model)
        {
            var result = await _transactionService.GuestStudentSponsorPaymentAsync(model);

            return Ok(result);
        }
        [HttpPost]
        [Route("CreateSponsorStoryTransactionAsync")]
        public async Task<IHttpActionResult> CreateSponsorStoryTransactionAsync([FromBody] SponsorStoryTransactionViewModel userModel)
        {
            var result = await _transactionService.CreateSponsorStoryTransactionAsync(userModel);


            return Ok(result);
        }
        [HttpPost]
        [Route("CreateSponsorSchoolTransactionAsync")]
        public async Task<IHttpActionResult> CreateSponsorSchoolTransactionAsync([FromBody] SponsorSchoolTransactionVIewModel userModel)
        {
            var result = await _transactionService.CreateSponsorSchoolTransactionAsync(userModel);
            
            return Ok(result);
        }
        [HttpPost]
        [Route("CreateSponsorStudentTransactionAsync")]
        public async Task<IHttpActionResult> CreateSponsorStudentTransactionAsync([FromBody] SponsorStudentTransactionViewModel userModel)
        {
            var result = await _transactionService.CreateSponsorStudentTransactionAsync(userModel);
            
            return Ok(result);
        }

        [HttpGet]
        [Route("GetAdminProfileAsync")]
        public async Task<IHttpActionResult> GetAdminProfileAsync()
        {
            var result = await _userService.GetAdminProfileAsync(User.Identity.GetUserId());

            return Ok(result);
        }

        [HttpPost]
        [Route("UpdateUserRecordsAsync")]
        public async Task<IHttpActionResult> UpdateUserRecordsAsync([FromBody] UserUpdateViewModel userModel)
        {
            var result = await _userService.UpdateUserRecordsAsync(userModel);

            return Ok(result);
        }
        [HttpPost]
        [Route("UploadUserProfilePicAsync")]
        public async Task<IHttpActionResult> UploadUserProfilePicAsync()
        {

            if (HttpContext.Current.Request.Files.Count > 0)
            {
                var httpPostedFile = HttpContext.Current.Request.Files["file"];
                var result = await _userService.UploadUserProfilePicAsync(httpPostedFile);
                return Ok(result);
            }
            return Ok(new ApiHttpResponse(HttpStatusCode.OK, MessageCode.SomethingWrong));


        }

        [HttpPost]
        [Route("RemoveUserProfilePicAsync")]
        public async Task<IHttpActionResult> RemoveUserProfilePicAsync()
        {
            var result = await _userService.RemoveUserProfilePicAsync();

            return Ok(result);
        }

        [HttpPost]
        [Route("GetPages")]
        public async Task<IHttpActionResult> GetPages(long pageId)
        {
            var result = await _menuService.GetPageByIdAsync(pageId);
            return Ok(result);
        }
        [HttpGet]
        [Route("GetUserNotificationAsync")]
        public async Task<IHttpActionResult> GetUserNotificationAsync()
        {
            var result = await _notificationService.GetUserNotificationsAsync(User.Identity.GetUserId());

            return Ok(result);
        }

        //Added  by nikunj -- 23/03/2022
        [HttpPost]
        [Route("sendGuestpaymentmail")]
        public async Task<IHttpActionResult> sendGuestpaymentmail([FromBody] sendGuestpaymentmail_Request model)
        {
            var result = await _transactionService.sendGuestpaymentmail(model);

            return Ok(result);
        }
        //Added  by nikunj -- 23/03/2022
        #endregion
    }
}
