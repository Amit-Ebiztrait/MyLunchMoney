using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;

namespace MyLunchMoney
{
	public class RouteConfig
	{
		public static void RegisterRoutes(RouteCollection routes)
		{
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");            

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Login", action = "Index", id = UrlParameter.Optional },
                new[] { "MyLunchMoney.Controllers" }
            );

            //routes.MapRoute(
            //    name: "AdminDefault",
            //    url: "Admin/{controller}/{action}/{id}",
            //    defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional },
            //    new[] { "MyLunchMoney.Areas.Admin.Controllers" }
            //);
        }

        //public static void Register(HttpConfiguration config)
        //{
        //    // Web API configuration and services 
        //    // Web API routes
        //    config.MapHttpAttributeRoutes();
        //    config.Routes.MapHttpRoute(
        //        name: "DefaultApi",
        //        routeTemplate: "api/{controller}/{id}",
        //        defaults: new { id = RouteParameter.Optional }
        //    );
        //    config.Filters.Add(new System.Web.Http.AuthorizeAttribute());
        //}
    }
}
