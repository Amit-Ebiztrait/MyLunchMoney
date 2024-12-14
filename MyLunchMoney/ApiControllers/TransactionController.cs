using iTextSharp.text.pdf;
using MyLunchMoney.Models;
using MyLunchMoney.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;

namespace MyLunchMoney.ApiControllers
{
	[RoutePrefix("api")]
	public class TransactionController : ApiBaseController
    {
		#region Declaration
		private readonly ITransactionService _transactionService;
        private readonly IUserService _userService;
        #endregion

        #region Constructor
        public TransactionController(ITransactionService transactionService,IUserService userService)
		{
			_transactionService = transactionService;
            _userService = userService;
		}
		#endregion

		#region Post Mothods
		[HttpPost]
		[Route("GetAllTransactionHistory")]
		public async Task<IHttpActionResult> GetAllTransactionHistory()
		{
			var result = await _transactionService.GetParentTransactionHistory();

			return Ok(result);
		}
        [HttpPost]
        [Route("GetSponsorTransactionHistory")]
        public async Task<IHttpActionResult> GetSponsorTransactionHistory()
        {
            var result = await _transactionService.GetSponsorTransactionHistory();

            return Ok(result);
        }
        [HttpPost]
		[Route("GetCafeteriaHistory")]
		public async Task<IHttpActionResult> GetCafeteriaHistory()
		{
			var result = await _transactionService.GetCafeteriaHistory(CafeteriaHistoryRequest);

			return Ok(result);
		}

		[HttpPost]
		[Route("GetParentStudentTransaction")]
		public async Task<IHttpActionResult> GetParentStudentTransaction()
		{
			var pid = System.Web.HttpContext.Current.Request.Form["parentid"];
			var sid = System.Web.HttpContext.Current.Request.Form["studentid"];
			var days = Convert.ToInt32(System.Web.HttpContext.Current.Request.Form["days"]);
			var startDate = DateTime.Now.AddDays(-days);
			var endDate = DateTime.Now;

			var result = await _transactionService.GetParentStudentTransaction(pid, sid, startDate, endDate);

			return Ok(result);
		}
        [HttpPost]
        [Route("GetTransactionListAsync")]
        public async Task<IHttpActionResult> GetTransactionListAsync()
        {
            var sid = System.Web.HttpContext.Current.Request.Form["studentid"];
            var days = Convert.ToInt32(System.Web.HttpContext.Current.Request.Form["days"]);
            var startDate = DateTime.Now.AddDays(-days);
            var endDate = DateTime.Now;

            var result = await _transactionService.GetTransactionListAsync(sid, startDate, endDate);

            return Ok(result);
        }
        #endregion
    }
}
