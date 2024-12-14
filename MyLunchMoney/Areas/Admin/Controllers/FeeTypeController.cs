using Microsoft.AspNet.Identity;
using MyLunchMoney.Constants;
using MyLunchMoney.Hubs;
using MyLunchMoney.Models;
using MyLunchMoney.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace MyLunchMoney.Areas.Admin.Controllers
{
    public class FeeTypeController : AdminBaseController
    {
            
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;
        private readonly IFeeTypeService _feeTypeService;
        public FeeTypeController(ApplicationDbContext context, INotificationService notificationService, IFeeTypeService feeTypeService)
        {
            this._context = context;
            this._notificationService = notificationService;
            this._feeTypeService = feeTypeService;
        }
        // GET: Admin/FeeType
     
        public ActionResult  Index()
        {
            
            return View();
        }
        [HttpGet]
        public async Task<ActionResult> GetAllAsync()
        {
            var result = await _feeTypeService.GetAllAsync(SchoolId);

            return Json(new { data = result }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public async Task<ActionResult> SaveAsync()
        {
            try
            {
                var model = JsonConvert.DeserializeObject<FeesTypeDetailViewModel>(HttpContext.Request.Form["model"]);
                model.SchoolId = SchoolId;
                model.UserId = CurrentUserId;
              
                if (model.FeeTypeId > 0)
                {
                    var result = await _feeTypeService.UpdateDetailAsync(model);
                    if (result != null)
                    {
                        NotificationRequest notificationrequest = new NotificationRequest
                        {
                            UserId = SchoolAdminId,
                            SenderId = CurrentUserId,
                            SchoolId = SchoolId,
                            Subject = MessageCode.FeeTypeUpdated,
                            Message = MessageCode.FeeTypeUpdated,
                            IsRead = 0,
                            CreatedBy = CurrentUserId
                        };
                        await _notificationService.AddNotificationAsync(notificationrequest);

                        MessagesHub objmessagesHub = new MessagesHub();
                        objmessagesHub.GetNotification();
                    }
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var result = await _feeTypeService.AddDetailAsync(model);
                    if (result != null)
                    {

                        NotificationRequest notificationrequest = new NotificationRequest
                        {
                            UserId = SchoolAdminId,
                            SenderId = CurrentUserId,
                            SchoolId =SchoolId,
                            Subject = MessageCode.FeeTypeAdded,
                            Message = MessageCode.FeeTypeAdded,
                            IsRead = 0,
                            CreatedBy = CurrentUserId
                        };
                        await _notificationService.AddNotificationAsync(notificationrequest);
                    }
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message.ToString() }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public async Task<ActionResult> GetDetailById(int id)
        {
            try
            {
                var result = await _feeTypeService.GetByIdAsync(id);
                return Json(new { data = result }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message.ToString() }, JsonRequestBehavior.AllowGet);
            }
            //var result = _context.items.Where(x => x.ItemId == id).FirstOrDefault();
            //if (result != null)
            //{
            //    return Json(result, JsonRequestBehavior.AllowGet);
            //}
        }
        [HttpPost]
        public async Task<ActionResult> DeleteAsync(int id)
        {
            var userid = User.Identity.GetUserId();
            var result = await _feeTypeService.DeleteByIdAsync(id, userid);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}