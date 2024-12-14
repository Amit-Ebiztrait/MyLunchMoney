using MyLunchMoney.Models;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using System.Linq;
using System;

namespace MyLunchMoney.ActionFilter
{
	public class CustomActionFilter : ActionFilterAttribute
	{
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // Log Action Filter call
            using (ApplicationDbContext context = new ApplicationDbContext())
            {
                string userName = string.Empty;
                string paramString = string.Empty;
                paramString = filterContext.ActionParameters.Aggregate(paramString, (current, p) => current + (p.Key + ": " + p.Value + Environment.NewLine));
                if (filterContext.HttpContext.User.Identity.IsAuthenticated)
                {
                    userName = filterContext.HttpContext.User.Identity.Name;

                }
                ActionLog log = new ActionLog()
                {
                    Area = filterContext.RouteData.DataTokens["area"].ToString() ?? string.Empty,
                    Controller = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName,
                    Action = filterContext.ActionDescriptor.ActionName,
                    Params = paramString,
                    IP = filterContext.HttpContext.Request.UserHostAddress,
                    DateTime = filterContext.HttpContext.Timestamp,
                    UserName = userName
                };
                context.actionlog.Add(log);
                context.SaveChanges();
            }
        }
    }
}