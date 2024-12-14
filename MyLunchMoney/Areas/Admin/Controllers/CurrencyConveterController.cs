using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using MyLunchMoney.Models.ViewModels;
using MyLunchMoney.Services;
using MyLunchMoney.EnumType;

namespace MyLunchMoney.Areas.Admin.Controllers
{
    public class CurrencyConveterController : Controller
    {
        #region Declaration
        private readonly IUserService _userService;
        #endregion

        #region Constructor
        public CurrencyConveterController(IUserService userService)
        {
            _userService = userService;
        }
        #endregion

        public async Task<ActionResult> Index()
        {
            var model = await _userService.GetCurrencyByIdAsync(1);
            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> SaveAsync()
        {
            try
            {
                var model = JsonConvert.DeserializeObject<CurrencyViewModel>(HttpContext.Request.Form["model"]);
                model.ConverterType = EnumType.ExchangeRateType.USDtoJMD.ToString();
                var result = await _userService.UpdateAsync(model);
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message.ToString() }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}