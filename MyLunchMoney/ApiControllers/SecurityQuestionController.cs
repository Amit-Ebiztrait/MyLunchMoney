using MyLunchMoney.Services;
using System.Threading.Tasks;
using System.Web.Http;

namespace MyLunchMoney.ApiControllers
{
	[RoutePrefix("api")]
	public class SecurityQuestionController : ApiController
	{
		#region Declaration
		private readonly ISecurityQuestionService _service;
		#endregion

		#region Constructor
		public SecurityQuestionController(ISecurityQuestionService service)
		{
			_service = service;
		}
		#endregion

		#region Post Mothods
		[HttpPost]
		[Route("GetAllSecurityQuestionAsync")]
		public async Task<IHttpActionResult> GetAllSecurityQuestionAsync()
		{
			var result = await _service.GetAllQuestionAsync();

			return Ok(result);
		}
        [HttpPost]
        [Route("GetAllFaqQuestionAsync")]
        public async Task<IHttpActionResult> GetAllFaqQuestionAsync()
        {
            var result = await _service.GetAllFaqQuestionAsync();

            return Ok(result);
        }
        [HttpPost]
        [Route("GetAllContactsAsync")]
        public async Task<IHttpActionResult> GetAllContactsAsync()
        {
            var result = await _service.GetAllContactsAsync();

            return Ok(result);
        }
        #endregion
    }
}
