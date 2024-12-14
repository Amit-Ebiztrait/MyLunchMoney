using MyLunchMoney.Constants;
using MyLunchMoney.Helpers;
using MyLunchMoney.Models;
using Newtonsoft.Json;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace MyLunchMoney.ActionFilter
{
	public class ApiAuthorizeFilter : AuthorizationFilterAttribute
	{
		private ApplicationDbContext _context;
		public ApiAuthorizeFilter()
		{
			_context = new ApplicationDbContext();
		}

		public override void OnAuthorization(HttpActionContext filterContext)
		{
			var req = filterContext.Request;
			try
			{
				if (req.Headers.GetValues("Authorization") != null)
				{
					var tknString = req.Headers.GetValues("Authorization").FirstOrDefault();
					var _token = tknString.Substring("Bearer ".Length).Trim();
					var uid = GetTokenId(_token);

					var user = _context.Users.Where(t => t.Id == uid && t.IsActive == 1 && t.Token == _token).Any();
					if (!user)
					{
						var _apiRes = new ApiHttpResponse(HttpStatusCode.Unauthorized, MessageCode.InvalidToken);

						var result = JsonConvert.SerializeObject(_apiRes);
						filterContext.Response = req.CreateResponse(HttpStatusCode.OK);
						filterContext.Response.Content = new StringContent(result, Encoding.UTF8, "application/json");
						return;
					}
				}
				else
				{
					var _apiRes = new ApiHttpResponse(HttpStatusCode.Unauthorized, MessageCode.InvalidToken);

					var result = JsonConvert.SerializeObject(_apiRes);
					filterContext.Response = req.CreateResponse(HttpStatusCode.OK);
					filterContext.Response.Content = new StringContent(result, Encoding.UTF8, "application/json");
				}
			}
			catch (Exception ex)
			{
				var _apiRes = new ApiHttpResponse(HttpStatusCode.Forbidden, ex.Message.ToString());

				var result = JsonConvert.SerializeObject(_apiRes);
				filterContext.Response = req.CreateResponse(HttpStatusCode.OK);
				filterContext.Response.Content = new StringContent(result, Encoding.UTF8, "application/json");
			}
			base.OnAuthorization(filterContext);
		}

		private string GetTokenId(string token)
		{
			try
			{
				var stream = token;
				var handler = new JwtSecurityTokenHandler();
				var jsonToken = handler.ReadToken(stream);
				var tokenS = handler.ReadToken(stream) as JwtSecurityToken;

				return tokenS.Claims.First(claim => claim.Type == "sub").Value;
			}
			catch
			{
				return "0";
			}
		}
	}
}