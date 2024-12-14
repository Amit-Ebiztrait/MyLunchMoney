using MyLunchMoney.Helpers;
using MyLunchMoney.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace MyLunchMoney.ApiControllers
{
	[RoutePrefix("api")]
	public class ApiDemoController : ApiController
    {
		#region Declaration
		private readonly ICategoryRepository _repository;
		#endregion

		#region Constructor
		public ApiDemoController(ICategoryRepository repository)
		{
			_repository = repository;
		}
		#endregion

		#region Get Methods
		public void test() { }
		public void test2() { }
		public void test3() { }

		[HttpPost]
		[Route("GenerateIds")]
		public IHttpActionResult GenerateIds()
		{
			var FundTransactionId = StringHelper.GenerateFundTransactionId();
			var FundTransactionHistoryId = StringHelper.GenerateFundTransactionHistoryId();
			var TransactionId = StringHelper.GenerateTransactionId();
			var TransactionHistoryId = StringHelper.GenerateTransactionHistoryId();
			var SchoolId = StringHelper.GenerateSchoolId();
			var StoryId = StringHelper.GenerateStoryId();

			return Json(new
			{
				FundTransactionId = FundTransactionId,
				FundTransactionHistoryId = FundTransactionHistoryId,
				TransactionId = TransactionId,
				TransactionHistoryId = TransactionHistoryId,
				SchoolId = SchoolId,
				StoryId = StoryId
			});
		}
		#endregion

		#region Post Mothods

		#endregion
	}
}
