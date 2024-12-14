using MyLunchMoney.Models;
using MyLunchMoney.Repository;
using Microsoft.AspNet.Identity;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.Owin.Security;
using System.Web;
using MyLunchMoney.Services;
using Microsoft.AspNet.Identity.Owin;
using MyLunchMoney.Constants;
using System.Linq;

namespace MyLunchMoney.Areas.Admin.Controllers
{
	public class AccountController : AdminBaseController
	{
		private readonly IUserRepository _userRepository;
		private readonly IUserService _userService;
		private ApplicationSignInManager _signInManager;
		private ApplicationUserManager _userManager;
		public AccountController(IUserRepository userRepository, IUserService userService)
		{
			_userRepository = userRepository;
			_userService = userService;
		}

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

		[HttpGet]
		public async Task<ActionResult> GetAdminProfileAsync()
		{
			var model = await _userService.GetAdminProfileAsync(User.Identity.GetUserId());

			return Json(new { data = model }, JsonRequestBehavior.AllowGet);
		}

		[HttpGet]
		public ActionResult ChangePassword()
		{
			var model = new ChangePasswordModel()
			{
				UserName = Session["_userName"].ToString()
			};
			return View(model);
		}

		[HttpGet]
		public async Task<ActionResult> ChangePasswordAsync(ChangePasswordModel model)
		{
			var result = await _userManager.ChangePasswordAsync(model.UserId, model.OldPassword, model.NewPassword);
			if (!result.Succeeded)
			{
				var error = result.Errors.FirstOrDefault();
				return Json(new { code = HttpStatusCode.BadRequest, message = error });
			}
			return Json(new { code = HttpStatusCode.OK, message = MessageCode.PasswordChanged });
		}

		[HttpGet]
		[AllowAnonymous]
		public ActionResult ResetPassword(string code)
		{
			var model = new ResetPasswordViewModel() { Code = code };
			return code == null ? View("Error") : View(model);
		}

		[HttpPost]
		[AllowAnonymous]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
		{
			if (!ModelState.IsValid)
			{
				return View(model);
			}
			var user = await UserManager.FindByNameAsync(model.Email);
			if (user == null)
			{
				// Don't reveal that the user does not exist
				return RedirectToAction("ResetPassword", "Account", new { code = model.Code });
			}
			var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
			if (result.Succeeded)
			{
				return View("~/Areas/Admin/Views/Account/ForgotPasswordConfirmation.cshtml");
			}
			AddErrors(result);
			return View();
		}

		[HttpGet]
		//[ValidateAntiForgeryToken]
		public ActionResult LogOff()
		{
			Session.Clear();
			AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
			return RedirectToAction("Index", "Login", new { area = "Admin" });
		}

		private IAuthenticationManager AuthenticationManager
		{
			get
			{
				return HttpContext.GetOwinContext().Authentication;
			}
		}


		#region Helpers
		// Used for XSRF protection when adding external logins
		private const string XsrfKey = "XsrfId";

		private void AddErrors(IdentityResult result)
		{
			foreach (var error in result.Errors)
			{
				ModelState.AddModelError("", error);
			}
		}

		private ActionResult RedirectToLocal(string returnUrl)
		{
			if (Url.IsLocalUrl(returnUrl))
			{
				return Redirect(returnUrl);
			}
			return RedirectToAction("Index", "Home");
		}

		internal class ChallengeResult : HttpUnauthorizedResult
		{
			public ChallengeResult(string provider, string redirectUri)
				: this(provider, redirectUri, null)
			{
			}

			public ChallengeResult(string provider, string redirectUri, string userId)
			{
				LoginProvider = provider;
				RedirectUri = redirectUri;
				UserId = userId;
			}

			public string LoginProvider { get; set; }
			public string RedirectUri { get; set; }
			public string UserId { get; set; }

			public override void ExecuteResult(ControllerContext context)
			{
				var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
				if (UserId != null)
				{
					properties.Dictionary[XsrfKey] = UserId;
				}
				context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
			}
		}
		#endregion
	}
}