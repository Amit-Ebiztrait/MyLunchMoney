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
	public class ItemController : ApiBaseController
    {
		#region Declaration
		private readonly IItemService _itemService;
		#endregion

		#region Constructor
		public ItemController(IItemService itemService)
		{
			_itemService = itemService;
		}
		#endregion

		#region Post Mothods
		[HttpPost]
		[Route("GetAllItemAsync")]
		public async Task<IHttpActionResult> GetAllItemAsync()
		{
			var data = await _itemService.GetAllAsync();
			var result = new ApiHttpResponse(HttpStatusCode.OK, MessageCode.Ok, data);

			return Ok(result);
		}

		[HttpPost]
		[Route("AddItemAsync")]
		public async Task<IHttpActionResult> AddItemAsync()
		{
			var result = await _itemService.AddItemAsync(ItemRequest);

			return Ok(result);
		}

		[HttpPost]
		[Route("DeleteByIdItemAsync")]
		public async Task<IHttpActionResult> DeleteByIdItemAsync(Int64 id)
		{
			var result = await _itemService.DeleteByIdAsync(id);

			return Ok(result);
		}
		#endregion
	}
}
