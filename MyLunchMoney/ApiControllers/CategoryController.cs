using MyLunchMoney.Constants;
using MyLunchMoney.Models;
using MyLunchMoney.Services;
using System;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;

namespace MyLunchMoney.ApiControllers
{
	[RoutePrefix("api")]
	public class CategoryController : ApiBaseController
    {
		#region Declaration
		public readonly ICategoryService _categoryService;
		#endregion

		#region Constructor
		public CategoryController(ICategoryService categoryService)
		{
			_categoryService = categoryService;
		}
		#endregion

		#region Post Mothods
		[HttpPost]
		[Route("GetAllCategoryAsync")]
		public async Task<IHttpActionResult> GetAllCategoryAsync()
		{
			var data = await _categoryService.GetAllAsync();
			var result = new ApiHttpResponse(HttpStatusCode.OK, MessageCode.Ok, data);

			return Ok(result);
		}

		[HttpPost]
		[Route("AddCategoryAsync")]
		public async Task<IHttpActionResult> AddCategoryAsync()
		{
			var result = await _categoryService.AddCategoryAsync(CategoryRequest);

			return Ok(result);
		}

		[HttpPost]
		[Route("DeleteByIdCategoryAsync")]
		public async Task<IHttpActionResult> DeleteByIdCategoryAsync(Int64 id)
		{
			var result = await _categoryService.DeleteByIdAsync(id, "Api Testing");

			return Ok(result);
		}
		#endregion
	}
}
