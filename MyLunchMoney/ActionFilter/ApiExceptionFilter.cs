using MyLunchMoney.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Http.Filters;

namespace MyLunchMoney.ActionFilter
{
	public class ApiExceptionFilter : ExceptionFilterAttribute
	{
		public override void OnException(HttpActionExecutedContext _fc)
		{
			string error;

			if (_fc.Exception.InnerException == null)
			{
				error = _fc.Exception.Message;
			}
			else
			{
				error = _fc.Exception.InnerException.Message;
			}

			var _res = new ApiHttpResponse(HttpStatusCode.InternalServerError, error);

			var result = JsonConvert.SerializeObject(_res);
			_fc.Response = _fc.Request.CreateResponse(HttpStatusCode.OK);
			_fc.Response.Content = new StringContent(result, Encoding.UTF8, "application/json");

			return;
		}
	}
}