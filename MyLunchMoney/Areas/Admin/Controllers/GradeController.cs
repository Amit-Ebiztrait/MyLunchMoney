using MyLunchMoney.Models;
using MyLunchMoney.Repository;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace MyLunchMoney.Areas.Admin.Controllers
{
	public class GradeController : AdminBaseController
    {
		#region Declaration
		private readonly IGradeRepository _gradeRepository;
		#endregion

		#region Constructor
		public GradeController(IGradeRepository gradeRepository)
		{
			_gradeRepository = gradeRepository;
		}
		#endregion

		#region Get Methods
		[HttpGet]
		public async Task<ActionResult> GetAllAsync()
		{
			var result = await _gradeRepository.GetAllAsync();

			return Json(new { data = result });
		}
		#endregion

		#region Post Methods
		[HttpPost]
		public async Task<ActionResult> GetGradeBySchooIdAsync(string id)
		{
			var result = await _gradeRepository.GetGradeBySchoolIdAsync(id);

			return Json(new { data = result });
		}

		[HttpPost]
		public async Task<ActionResult> GetClassNameAsync(string gradeName, string schoolId)
		{
			var result = await _gradeRepository.GetClassNameAsync(gradeName, schoolId);

			return Json(new { data = result });
		}

		[HttpPost]
		[Route("AddGradeAsync")]
		public async Task<ActionResult> AddGradeAsync()
		{
			var model = JsonConvert.DeserializeObject<Grade>(HttpContext.Request.Form["model"]);
            model.SchoolId = SchoolId;

            var result = await _gradeRepository.AddAsync(model);

			return Json(new { data = result }, JsonRequestBehavior.AllowGet);
		}

		[HttpPost]
		[Route("DeleteByIdGradeAsync")]
		public async Task<ActionResult> DeleteByIdGradeAsync(Int64 id)
		{
			var result = await _gradeRepository.DeleteByIdAsync(id);

			return Json(new { data = result }, JsonRequestBehavior.AllowGet);
		} 
		#endregion
	}
}