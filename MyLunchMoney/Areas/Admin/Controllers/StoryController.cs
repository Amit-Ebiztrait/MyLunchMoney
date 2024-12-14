using Microsoft.AspNet.Identity;
using MyLunchMoney.Models;
using MyLunchMoney.Services;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace MyLunchMoney.Areas.Admin.Controllers
{
	public class StoryController : AdminBaseController
	{
		#region Declaration
		private readonly IStoryService _storyService;
		#endregion

		#region Constructor
		public StoryController(IStoryService storyService)
		{
			_storyService = storyService;

		}
		#endregion

		#region Get Methods
		public ActionResult Index()
		{
			return View();
		}

		[HttpGet]
		public async Task<ActionResult> GetAllStoryAsync()
		{
			var result = await _storyService.GetAllStoryAsync(SchoolId);

			return Json(new { data = result }, JsonRequestBehavior.AllowGet);
		}

		[HttpGet]
		public async Task<ActionResult> GetStoryByIdAsync(string sid)
		{
			var result = await _storyService.GetStoryByIdAsync(SchoolId, sid);

			return Json(new { data = result }, JsonRequestBehavior.AllowGet);
		}
        #endregion

        #region Post Mothods
        [HttpPost]
        public async Task<ActionResult> SaveAsync()
        {
            try
            {
                var model = JsonConvert.DeserializeObject<StoryViewModel>(HttpContext.Request.Form["model"]);
                model.SchoolUserId = SchoolId;

                if (!string.IsNullOrEmpty(model.StoryId))
                {
                    var result = await _storyService.UpdateAsync(model);

                    return Json(result, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var result = await _storyService.AddAsync(model);

                    return Json(result, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message.ToString() }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public async Task<ActionResult> DeleteAsync(string id)
        {
            var userid = User.Identity.GetUserId();
            var result = await _storyService.DeleteAsync(id, userid);

            return Json(result, JsonRequestBehavior.AllowGet);
        } 

		[HttpPost]
		public async Task<ActionResult> ActiveDeactiveAsync(string id, int status)
		{
			var userid = User.Identity.GetUserId();
			var result = await _storyService.ActiveDeactiveAsync(userid, id, status);

			return Json(result, JsonRequestBehavior.AllowGet);
		}
		#endregion
	}
}