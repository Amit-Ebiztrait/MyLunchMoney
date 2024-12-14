using Microsoft.AspNet.Identity;
using MyLunchMoney.Models;
using MyLunchMoney.Repository;
using MyLunchMoney.Services;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace MyLunchMoney.Areas.Admin.Controllers
{
	public class SchoolTypeController : AdminBaseController
	{
		#region Declaration
		private readonly ISchoolTypeRepository _schoolTypeRepository;
		#endregion

		#region Constructor
		public SchoolTypeController(ISchoolTypeRepository schoolTypeRepository)
		{
			_schoolTypeRepository = schoolTypeRepository;

		}
		#endregion

		#region Get Methods
		public ActionResult Index()
		{
			return View();
		}

		[HttpGet]
		public async Task<ActionResult> GetAllAsync()
		{
			var result = await _schoolTypeRepository.GetSchoolTypeListAsync();

			return Json(new { data = result }, JsonRequestBehavior.AllowGet);
		}

		[HttpGet]
		public async Task<ActionResult> GetSchoolTypeByIdAsync(Int64 id)
		{
			var result = await _schoolTypeRepository.GetStoryByIdAsync(id);

			return Json(new { data = result }, JsonRequestBehavior.AllowGet);
		}
        #endregion

        #region Post Mothods
        [HttpPost]
        public async Task<ActionResult> SaveAsync()
        {
			var model = JsonConvert.DeserializeObject<SchoolType>(HttpContext.Request.Form["model"]);

			if (model.SchoolTypeId > 0)
			{
				var result = await _schoolTypeRepository.UpdateAsync(model);

				return Json(result, JsonRequestBehavior.AllowGet);
			}
			else
			{
				var result = await _schoolTypeRepository.AddAsync(model);

				return Json(result, JsonRequestBehavior.AllowGet);
			}
		}

        [HttpPost]
        public async Task<ActionResult> DeleteAsync(Int64 id)
        {
            var userid = User.Identity.GetUserId();
            var result = await _schoolTypeRepository.DeleteAsync(id, userid);

            return Json(result, JsonRequestBehavior.AllowGet);
        } 

		[HttpPost]
		public async Task<ActionResult> ActiveDeactiveAsync(Int64 id, int status)
		{
			var userid = User.Identity.GetUserId();
			var result = await _schoolTypeRepository.ActiveDeactiveAsync(userid, id, status);

			return Json(result, JsonRequestBehavior.AllowGet);
		}
		#endregion
	}
}