using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using MyLunchMoney.Models;
using MyLunchMoney.Services;
using Newtonsoft.Json;

namespace MyLunchMoney.Areas.Admin.Controllers
{
	public class CategoryController : AdminBaseController
    {
        private readonly ICategoryService _categoryService;
        private readonly ApplicationDbContext _context;
        private readonly IAWSService _awsService;

        public CategoryController(
            ICategoryService categoryService
            , ApplicationDbContext context, IAWSService awsService
            )
        {
            _categoryService = categoryService;
            _context = context;
            _awsService = awsService;
        }
        public ActionResult Index() => View();
        public ActionResult Add() => View();
        [HttpGet]
        public async Task<ActionResult> GetAllCategories()
        {
             var result = await _categoryService.GetAllCategoryAsync();
             return Json(new { data = result.Data }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public async Task<ActionResult> SaveAsync()
        {
            try
            {
                var model = JsonConvert.DeserializeObject<Category>(HttpContext.Request.Form["model"]);
                model.SchoolId = SchoolId;
                model.UserId = User.Identity.GetUserId();
                HttpPostedFile fileObj = null;

                var files = Request.Files;

                for (int i = 0; i < files.Count; i++)
                {
                    var file = files.Get(i);
                    var constructorInfo = typeof(HttpPostedFile).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance)[0];
                    fileObj = (HttpPostedFile)constructorInfo
                              .Invoke(new object[] { file.FileName, file.ContentType, file.InputStream });
                }
                if (model.CategoryId > 0)
                {
                    var result = await _categoryService.UpdateCategoryDetailAsync(model, fileObj);
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var result = await _categoryService.AddCategoryDetailAsync(model, fileObj);
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
            }
            catch(Exception ex)
            {
                return Json(new { error = ex.Message.ToString() }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public async Task<ActionResult> DeleteAsync(int id)
        {
            var userid = User.Identity.GetUserId();
            var result = await _categoryService.DeleteByIdAsync(id, userid);

            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}