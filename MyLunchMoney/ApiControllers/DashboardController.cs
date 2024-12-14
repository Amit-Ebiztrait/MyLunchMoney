using MyLunchMoney.Services;
using System.Threading.Tasks;
using System.Web.Http;

namespace MyLunchMoney.ApiControllers
{
	[RoutePrefix("api")]
    public class DashboardController : ApiBaseController
    {
		#region Declaration
		private readonly IUserService _userService;
		#endregion

		#region Constructor
		public DashboardController(IUserService userService)
		{
			_userService = userService;
		}
		#endregion

		#region Post Methods
		[HttpPost]
		[Route("GetDashboardListAsync")]
		public async Task<IHttpActionResult> GetDashboardListAsync(string id)
		{
			var result = await _userService.GetDashboardListAsync(id);

			return Ok(result);
		}
		#endregion
	}
}
