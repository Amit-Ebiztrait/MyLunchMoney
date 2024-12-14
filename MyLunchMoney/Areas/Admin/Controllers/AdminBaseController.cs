using MyLunchMoney.Models;
using MyLunchMoney.ActionFilter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;

namespace MyLunchMoney.Areas.Admin.Controllers
{
    //[CustomActionFilter]
    [RouteArea("Admin")]    
    public class AdminBaseController : Controller
    {
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext == null || filterContext.HttpContext == null) return;

            //Stores the Request in an Accessible object
            if (filterContext.HttpContext.Request == null) return;

            var returnPath = HttpContext.Request.Url;
            string returnUrl = returnPath.AbsolutePath + returnPath.Query;

            // Check user session if null then redirect to login page
            if (filterContext.HttpContext.User.Identity.IsAuthenticated) { }
            else if ("/Admin/Account/ResetPassword" == returnPath.AbsolutePath) { }
            else
            {
                if (string.IsNullOrEmpty(returnUrl) || returnUrl == "/")
                    filterContext.Result = new RedirectResult("~/Admin/Login", true);
                else
                    filterContext.Result = new RedirectResult("~/Admin/Login", true);

                Session.Abandon();
                //return;
            }
            base.OnActionExecuting(filterContext);
        }

        public string CurrentRole { get { return Session["CurrentRole"] as string; } }
        public string CurrentUserId { get { return Session["CurrentUserId"] as string; } }
        public string SchoolId { get { return Session["SchoolId"] as string; } }
        public string SchoolAdminId { get { return Session["SchoolAdminId"] as string; } }
        public SchoolRequest SchoolRequest { get { return new SchoolRequest(); } }
    }
}