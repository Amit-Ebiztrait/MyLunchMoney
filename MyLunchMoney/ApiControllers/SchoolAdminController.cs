using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using MyLunchMoney.Models;
using MyLunchMoney.Models.Request;
using MyLunchMoney.Services;

namespace MyLunchMoney.ApiControllers
{
    [RoutePrefix("api")]
    public class SchoolAdminController : ApiBaseController
    {
        #region Declaration
        private readonly ITransactionService _tranService;
        private readonly IItemService _itemService;
        private readonly IMenuService _menuService;
        #endregion

        #region Constructor
        public SchoolAdminController(ITransactionService tranService, IItemService itemService, IMenuService menuService)
        {
            _tranService = tranService;
            _itemService = itemService;
            _menuService = menuService;
        }
        #endregion
        [HttpPost]
        [Route("AddTransactionAsync")]
        public async Task<IHttpActionResult> AddTransactionAsync()
        {
            var result = await _tranService.AddTransactionAsync(AddTransactionRequest);

            return Ok(result);
        }

        [HttpPost]
        [Route("MenuItemList")]
        public async Task<IHttpActionResult> GetMenuItemsAsync(string dayName, string oSchoolId, int pageIndex, int pageSize)
        {

            var result = await _menuService.GetMenuItems(dayName, oSchoolId, pageIndex, pageSize);

            return Ok(result);
        }
        [HttpPost]
        [Route("AddPreOrderAsync")]
        public async Task<IHttpActionResult> AddPreOrderAsync()
        {
            var result = await _menuService.AddPreOrderAsync(AddPreOrderRequest);
            return Ok(result);
        }

        [HttpPost]
        [Route("PreOrderHistoryAsync")]
        public async Task<IHttpActionResult> GetPreOrderHistoryAsync( string studentId, int pageIndex, int pageSize)
        {
            var result = await _menuService.GetPreOrderHistory(studentId, pageIndex, pageSize);
            return Ok(result);
        }

        [HttpPost]
        [Route("PreOrderDetailAsync")]
        public async Task<IHttpActionResult> GetPreOrderDetailAsync(string preOrderId)
        {
            var result = await _menuService.GetPreOrderDetailAsync(preOrderId);
            return Ok(result);
        }
        [HttpPost]
        [Route("UpdatePreOrderAsync")]
        public async Task<IHttpActionResult> UpdatePreOrderAsync()
        {
            var result = await _menuService.UpdatePreOrderAsync(UpdatePreOrderRequest);
            return Ok(result);
        }
    }
}