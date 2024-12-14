using MyLunchMoney.ActionFilter;
using MyLunchMoney.Constants;
using MyLunchMoney.Infrastructure.EnumType;
using MyLunchMoney.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace MyLunchMoney.Areas.Admin.Controllers
{
    //[CustomActionFilter]
    //[Authorize(Roles = "SchoolAdmin")]
    public class DashboardController : AdminBaseController
    {
        #region Declaration
        private readonly IDashboardService _dashboardService;
        private readonly ITransactionService _transactionService;
        #endregion

        #region Construction
        public DashboardController(IDashboardService dashboardService, ITransactionService transactionService)
        {
            _transactionService = transactionService;
            _dashboardService = dashboardService;
        }
        #endregion

        #region Methods
        public async Task<ActionResult> Index()
        {
            if (string.IsNullOrEmpty(CurrentRole))
            {
                return RedirectToAction("LogOff", "Account", new { areas = "Admin" });
            }

            if (CurrentRole.Equals(RoleName.SuperAdmin))
            {
                var model = await _dashboardService.DashboardDetailAsync(CurrentUserId);

                return View(model);
            }
            else
            {
                return View("~/Areas/Admin/Views/Dashboard/Dashboard.cshtml");
            }

        }
        public async Task<ActionResult> DashboardAsync()
        {
            var model = await _dashboardService.DashboardDetailAsync(CurrentUserId);

            return Json(new { data = model }, JsonRequestBehavior.AllowGet);
        }
        public async Task<ActionResult> GetMonthlyChartAmount()
        {
            var model = await _transactionService.GetMonthlyChartAmount();

            return Json(new { data = model }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public async Task<ActionResult> GetRecentTransactionAsync(string type)
        {
            var model = await _dashboardService.RecentTransactionAsync(type);

            return Json(new { data = model }, JsonRequestBehavior.AllowGet);

            //return PartialView("~/Areas/Admin/Views/Dashboard/_RecentTransactionList.cshtml", model);
        }
        [HttpGet]
        public async Task<ActionResult> GetRefundRequestsAsync()
        {
            var model = await _dashboardService.RecentTransactionAsync("");

            return Json(new { data = model }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public async Task<ActionResult> GetDashboardSchoolDetailAsync(string schoolId)
        {
            var model = await _dashboardService.DashboardSchoolDetailAsync(schoolId);
            return Json(new { data = model }, JsonRequestBehavior.AllowGet);
        }
        public async Task<ActionResult> GetMonthlySchoolChartAmount(string schoolId)
        {
            var model = await _transactionService.GetMonthlySchoolChartAmount(schoolId);
            return Json(new { data = model }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public async Task<ActionResult> GetAllSchoolTransactions(string schoolId)
        {
            var model = await _transactionService.GetAllSchoolTransactions(schoolId);
            return Json(new { data = model }, JsonRequestBehavior.AllowGet);
        }
        #endregion
    }

    internal struct NewStruct
    {
        public string Item1;
        public string Item2;

        public NewStruct(string item1, string item2)
        {
            Item1 = item1;
            Item2 = item2;
        }

        public override bool Equals(object obj)
        {
            return obj is NewStruct other &&
                   Item1 == other.Item1 &&
                   Item2 == other.Item2;
        }

        public override int GetHashCode()
        {
            int hashCode = -1030903623;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Item1);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Item2);
            return hashCode;
        }

        public void Deconstruct(out string item1, out string item2)
        {
            item1 = Item1;
            item2 = Item2;
        }

        public static implicit operator (string, string)(NewStruct value)
        {
            return (value.Item1, value.Item2);
        }

        public static implicit operator NewStruct((string, string) value)
        {
            return new NewStruct(value.Item1, value.Item2);
        }
    }
}