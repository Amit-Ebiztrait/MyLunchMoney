using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using MyLunchMoney.Models;
using MyLunchMoney.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace MyLunchMoney.Areas.Admin.Controllers
{
	public class ForgotPasswordController : Controller
	{
		#region Declaration
		private ApplicationSignInManager _signInManager;
		private ApplicationUserManager _userManager;
		private readonly IEmailService _emailService;
		#endregion

		#region Constructor
		public ForgotPasswordController(
			//ApplicationUserManager userManager, 
			//ApplicationSignInManager signInManager, 
			IEmailService emailService)
		{
			//UserManager = userManager;
			//SignInManager = signInManager;
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
		#endregion

		#region Methods
		public ActionResult Index()
		{
			return View();
		}

		[HttpPost]
		[AllowAnonymous]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> Index(ForgotPasswordViewModel model, string returnUrl)
		{
			if (ModelState.IsValid)
			{
				var user = await UserManager.FindByNameAsync(model.Email);
				if (user == null)
				{
					// Don't reveal that the user does not exist or is not confirmed
					TempData["Message"] = "Invalid email attempt.";
					return View();
				}

				// For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=320771
				// Send an email with link
				string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
				var callbackUrl = Url.Action("ResetPassword", "Account", new { code = code }, protocol: Request.Url.Scheme);
				string body = string.Empty;
				using (StreamReader reader = new StreamReader(Server.MapPath("~/Template/ForgotPassword.html")))
				{
					body = reader.ReadToEnd();
				}
				body = body.Replace("{resetLink}", callbackUrl);
				bool IsSendEmail = _emailService.EmailSend(model.Email, "Recover your account password", body, true);
				if (IsSendEmail)
				{
					TempData["Message"] = "Recovery password link has been sent please check mail.Thank you.";
					return RedirectToAction("Index", "ForgotPassword", new { area = "Admin" });
				}
			}

			// If we got this far, something failed, redisplay form
			return View(model);
		}
		#endregion
	}
}