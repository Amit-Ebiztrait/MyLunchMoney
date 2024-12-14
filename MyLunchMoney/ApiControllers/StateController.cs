using MyLunchMoney.Repository;
using System.Threading.Tasks;
using System.Web.Http;

namespace MyLunchMoney.ApiControllers
{
	[RoutePrefix("api")]
	public class StateController : ApiBaseController
    {
		#region Declaration
		private readonly IStateRepository _repository;
		#endregion

		#region Constructor
		public StateController(IStateRepository repository)
		{
			_repository = repository;
		}
		#endregion

		#region Post Methods
		[HttpPost]
		[Route("GetAllStateAsync")]
		public async Task<IHttpActionResult> GetAllStateAsync()
		{
			var result = await _repository.GetAllAsync();

			return Ok(result);
		}
		#endregion
	}
}
