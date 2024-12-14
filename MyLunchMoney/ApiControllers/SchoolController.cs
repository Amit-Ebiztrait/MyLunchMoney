using MyLunchMoney.Repository;
using MyLunchMoney.Services;
using System;
using System.Threading.Tasks;
using System.Web.Http;

namespace MyLunchMoney.ApiControllers
{
	[RoutePrefix("api")]
	public class SchoolController : ApiBaseController
	{
		#region Declaration
		private readonly ISchoolService _service;
		private readonly IStateRepository _stateRepository;
		#endregion

		#region Constructor
		public SchoolController(ISchoolService service, IStateRepository stateRepository)
		{
			_service = service;
			_stateRepository = stateRepository;
		}
		#endregion

		#region Post Mothods
		[HttpPost]
		[Route("GetSchoolDropdownListAsync")]
		public async Task<IHttpActionResult> GetSchoolDropdownListAsync()
		{
			var result = await _service.GetSchoolDropdownListAsync();

			return Ok(result);
		}

		[HttpPost]
		[Route("GetAllSchoolsByIdAsync")]
		public async Task<IHttpActionResult> GetAllSchoolsByIdAsync(int pageNumber, int pageSize, string userid)
		{
			var result = await _service.GetAllSchoolsByIdAsync(pageNumber, pageSize, userid);

			return Ok(result);
		}

		[HttpPost]
		[Route("GetAllSchoolsAsync")]
		public async Task<IHttpActionResult> GetAllSchoolsAsync(int pageNumber, int pageSize)
		{
			var result = await _service.GetAllSchoolsAsync(pageNumber, pageSize);

			return Ok(result);
		}

		[HttpPost]
		[Route("GetSchoolsByStateIdAsync")]
		public async Task<IHttpActionResult> GetSchoolsByStateIdAsync(int pageNumber,int pageSize,Int64 stateId, string userId)
		{
			var result = await _service.GetAllSchoolsByStateIdAsync(pageNumber, pageSize, stateId, userId);

			return Ok(result);
		}

		[HttpPost]
		[Route("GetSchoolDetailBySchoolIdAsync")]
		public async Task<IHttpActionResult> GetSchoolDetailBySchoolIdAsync(string schoolId, string userId)
		{
			var result = await _service.GetSchoolDetailBySchoolIdAsync(schoolId, userId);

			return Ok(result);
		}
        [HttpPost]
        [Route("GetSponsorSchoolListAsync")]
        public async Task<IHttpActionResult> GetSponsorSchoolListAsync(int pageNumber, int pageSize,string sponsorId)
        {
            var result = await _service.GetSponsorSchoolListAsync(pageNumber, pageSize, sponsorId);
            return Ok(result);
        }
        [HttpPost]
        [Route("GetSchoolDetailDropDownListAsync")]
        public async Task<IHttpActionResult> GetSchoolDetailDropDownListAsync(Int64 stateId)
        {
            var result = await _service.GetSchoolDetailDropDownListAsync(stateId);

            return Ok(result);
        }
        #endregion
    }
}
