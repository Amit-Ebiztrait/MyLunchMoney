using MyLunchMoney.Constants;
using MyLunchMoney.Models;
using MyLunchMoney.Repository;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;

namespace MyLunchMoney.ApiControllers
{
	[RoutePrefix("api")]
    public class GradeController : ApiBaseController
    {
        #region Declaration
        private readonly IGradeRepository _repo;
        #endregion

        #region Constructor
        public GradeController(IGradeRepository repo)
		{
			_repo = repo;
		}
		#endregion

		#region Post Methods
		[HttpPost]
		[Route("GetAllGradeAsync")]
		public async Task<IHttpActionResult> GetAllAsync()
		{
			var data = await _repo.GetAllAsync();
			var result = new ApiHttpResponse(HttpStatusCode.OK, MessageCode.Ok, data);

			return Ok(result);
		}

		[HttpPost]
		[Route("GetGradeBySchooIdAsync")]
		public async Task<IHttpActionResult> GetGradeBySchooIdAsync(string id)
		{
			var data = await _repo.GetGradeBySchoolIdAsync(id);
			var result = new ApiHttpResponse(HttpStatusCode.OK, MessageCode.Ok, data);

			return Ok(result);
		}

		[HttpPost]
		[Route("GetClassNameAsync")]
		public async Task<IHttpActionResult> GetClassNameAsync(string gradeName, string schoolId)
		{
			var list = new List<KeyValuePair<string, string>>();
			var result = await _repo.GetClassNameAsync(gradeName, schoolId);
			foreach (var e in result)
			{
				list.Add(new KeyValuePair<string, string>(e, e));
			}	

			return Json(new { data = list });
		}

		[HttpPost]
		[Route("AddGradeAsync")]
		public async Task<IHttpActionResult> AddGradeAsync([FromBody] Grade model)
		{
			var result = await _repo.AddAsync(model);

			return Ok(result);
		}

		[HttpPost]
		[Route("DeleteByIdGradeAsync")]
		public async Task<IHttpActionResult> DeleteByIdGradeAsync(Int64 id)
		{
			var result = await _repo.DeleteByIdAsync(id);

			return Ok(result);
		} 
		#endregion
	}
}
