using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNet.Identity;
using MyLunchMoney.Constants;
using MyLunchMoney.Helpers;
using MyLunchMoney.Hubs;
using MyLunchMoney.Models;
using MyLunchMoney.Models.Request;
using MyLunchMoney.Repository;
using MyLunchMoney.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace MyLunchMoney.Areas.Admin.Controllers
{
    public class OrderController : AdminBaseController
    {
        #region Declaration
        private readonly ITransactionService _tranService;
        private readonly IUserService _userService;
        private readonly IGradeRepository _grade;
        private readonly IItemRepository _item;
        private readonly IMenuService _menu;
        private readonly IPushNotificationService _pushnotification;
        private readonly INotificationService _notification;
        #endregion

        #region Constructor
        public OrderController
        (
            IUserService userService,
            ITransactionService tranService,
            IGradeRepository grade,
            IItemRepository item,
            IMenuService menu,
            IPushNotificationService pushnotification,
            INotificationService notification
        )
        {
            _tranService = tranService;
            _userService = userService;
            _grade = grade;
            _item = item;
            _menu = menu;
            _pushnotification = pushnotification;
            _notification = notification;
        }
        #endregion

        #region Get Methods
        public ActionResult Index()
        {
            return View();
        }
        #endregion

        [HttpGet]
        public async Task<ActionResult> GetMLMIDSearchAsync()
        {
            string searchString = Convert.ToString(Request.Params["searchString"]);
            var result = await _item.GetMLMIDSearchAsync(searchString, SchoolId);
            return Json(new object[] { result, result.Count() }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public async Task<ActionResult> GetStudentDetailByMLMIDAsync(string mlmid, string type)
        {
            ViewBag.Type = type;
            var model = await _tranService.GetManageOrderStudentDetailAsync(mlmid, type);
            return PartialView("_ManageTransactionHistory", model);
        }
        [HttpGet]
        public async Task<ActionResult> GetLastOrderDetailsAsync(string transactionhistoryId)
        {
            var result = await _tranService.GetLastOrderDetailsAsync(transactionhistoryId);
            return Json(new { data = result }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public async Task<ActionResult> ItemSearch()
        {
            string searchString = Convert.ToString(Request.Params["searchString"]);
            string type = Convert.ToString(Request.Params["type"]);
            var result = await _item.ItemSearchAsync(searchString, type, SchoolId);
            return Json(new object[] { result, result.Count() }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public async Task<ActionResult> GetGradeList()
        {
            var list = new List<KeyValuePair<string, Int64>>();
            var result = await _grade.GetGradeBySchoolIdAsync(SchoolId);
            foreach (var e in result)
            {
                list.Add(new KeyValuePair<string, Int64>(e.GradeName, e.GradeId));
            }

            return Json(new { data = list }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public async Task<ActionResult> GetClassNameAsync(int gradeName)
        {
            var list = new List<KeyValuePair<string, string>>();
            var result = await _grade.GetClassNameByGradeIdAsync(gradeName, SchoolId);
            foreach (var e in result)
            {
                list.Add(new KeyValuePair<string, string>(e, e));
            }

            return Json(new { data = list }, JsonRequestBehavior.AllowGet);
        }
        #region Post Mothods
        [HttpGet]
        public async Task<ActionResult> GetManageOrderListAsync(string type, string grade, string className, int days)
        {
            if(grade.ToLower() == "Grade".ToLower())
            {
                grade = string.Empty;
            }
            var result = await _tranService.GetManageOrderListAsync(SchoolId, type, grade, className, days);

            return Json(new { data = result }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public async Task<ActionResult> ExportManageOrderListAsync(string type, string grade, string className, int days)
        {
            string filename = string.Format("{0}-{1}.{2}", type, DateTime.Now.ToString("dd-MM-yyyy-hh:mm:ss"), "pdf");

            IEnumerable<CafeteriaManageOrderDTO> result = await _tranService.GetManageOrderListAsync(SchoolId, type, grade, className, days);

            Font titleFont = new Font(Font.FontFamily.HELVETICA, 14, Font.BOLD, BaseColor.BLACK);
            Font normalFont = new Font(Font.FontFamily.HELVETICA, 9, Font.NORMAL, BaseColor.BLACK);
            Font boldFont = new Font(Font.FontFamily.HELVETICA, 10, Font.BOLD, BaseColor.WHITE);

            #region Initialize PDF File
            MemoryStream stream = new MemoryStream();
            Document document = new Document(PageSize.A4, 25f, 25f, 25f, 25f);
            PdfWriter.GetInstance(document, stream);

            document.Open();
            #endregion

            try
            {
                //Header Title
                List<float> titleWidth = new List<float>() { 5f };//Title
                PdfPTable headingBody = new PdfPTable(1)
                {
                    WidthPercentage = 100,
                    KeepTogether = false
                };
                headingBody.SetWidths(titleWidth.ToArray());
                AddTitleCell((type + " List"), titleFont, headingBody);

                //Table
                List<float> colWidth = new List<float>() { 5f, 5f, 5f, 5f, 5f };//Item,Amount,Date				

                PdfPTable tablebody = new PdfPTable(5)
                {
                    WidthPercentage = 100,
                    KeepTogether = false
                };

                tablebody.SetWidths(colWidth.ToArray());
                BaseColor _color1 = new BaseColor(249, 249, 249);
                BaseColor _color2 = new BaseColor(236, 237, 240);

                if (result != null && result.Count() > 0)
                {
                    AddHeaderCell("MLM ID", boldFont, tablebody);
                    AddHeaderCell("Grade", boldFont, tablebody);
                    AddHeaderCell("Date", boldFont, tablebody);
                    AddHeaderCell("Total", boldFont, tablebody);
                    AddHeaderCell("Status", boldFont, tablebody);

                    int i = 0;
                    foreach (var item in result)
                    {
                        BaseColor _color = i % 2 == 0 ? _color2 : _color1;
                        AddColumnValues(item.MLMID, normalFont, _color, tablebody, Element.ALIGN_LEFT);
                        AddColumnValues(item.GradeName, normalFont, _color, tablebody, Element.ALIGN_LEFT);
                        AddColumnValues(item.CreatedOn.ToString(), normalFont, _color, tablebody, Element.ALIGN_RIGHT);
                        AddColumnValues(item.TotalAmount.ToString(), normalFont, _color, tablebody, Element.ALIGN_RIGHT);
                        AddColumnValues(Enum.GetName(typeof(EnumType.TransactionStatus), item.TransactionStatus), normalFont, _color, tablebody, Element.ALIGN_CENTER);
                        i++;
                    }
                    document.Add(headingBody);
                    document.Add(tablebody);
                }
                else
                {
                    AddHeaderCell("MLMID", boldFont, tablebody);
                    AddHeaderCell("GradeName", boldFont, tablebody);
                    AddHeaderCell("Date", boldFont, tablebody);
                    AddHeaderCell("Amount", boldFont, tablebody);
                    AddHeaderCell("Status", boldFont, tablebody);

                    NoRecordRow("No Records", normalFont, new BaseColor(249, 249, 249), tablebody, Element.ALIGN_CENTER, 5);

                    document.Add(headingBody);
                    document.Add(tablebody);
                }
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index", "Error");
            }
            finally
            {
                document.Close();
            }

            return File(stream.ToArray(), "application/pdf", filename);
        }


        [HttpGet]
        public async Task<ActionResult> RepeatOrderAsync(string tid)
        {
            var userId = User.Identity.GetUserId();
            var result = await _tranService.RepeatOrderAsync(tid, userId);

            return Json(new { data = result }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public async Task<ActionResult> ReversalOrderAsync(string tid, string MLMID)
        {

            var result = await _tranService.ReversalOrderAmountAsync(tid, User.Identity.GetUserId());
            if (result.code == HttpStatusCode.OK)
            {
                var userData = await _userService.GetStudentByMLMIDAsync(MLMID);
                if (userData.code == HttpStatusCode.OK && userData.data != null)
                {
                    UserDeviceToken deviceToken = await _userService.GetUserDeviceToken(userData.data[0].StudentUserId);
                    if (deviceToken != null && !string.IsNullOrEmpty(deviceToken.DeviceToken) && !string.IsNullOrEmpty(deviceToken.ParentId))
                    {
                        PushNotificationRequest notificationModel = new PushNotificationRequest
                        {
                            token = deviceToken.DeviceToken,
                            body = NotificationMessageCode.ReversalOrderBody,
                            title = NotificationMessageCode.ReversalOrderTitle
                        };

                        bool isSent = await _pushnotification.SendToDeviceAsync(notificationModel);
                        if (isSent)
                        {
                            NotificationRequest notificationrequest = new NotificationRequest
                            {
                                UserId = deviceToken.ParentId,
                                SenderId = CurrentUserId,
                                SchoolId = userData.data[0].SchoolId,
                                Subject = NotificationMessageCode.ReversalOrderTitle,
                                Message = NotificationMessageCode.ReversalOrderBody,
                                IsRead = 0,
                                CreatedBy = CurrentUserId
                            };
                            await _notification.AddNotificationAsync(notificationrequest);

                            var webnotificationModel = new AddNotificationRequest
                            {

                                CurrentUserId = CurrentUserId,
                                SchoolId = userData.data[0].SchoolId,
                                Subject = NotificationMessageCode.ReversalOrderTitle,
                                Message = NotificationMessageCode.ReversalOrderBody,
                            };
                            var addnotification = await _notification.AddNotifications("RO", webnotificationModel);
                            MessagesHub objHub = new MessagesHub();
                            objHub.GetNotification();
                        }
                    }
                }
            }
            return Json(new { data = result }, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public async Task<ActionResult> AddTransactionAsync(AddTransactionRequest request)
        {

            if (!string.IsNullOrEmpty(CurrentUserId))
            {
                request.AdminStaffUserId = CurrentUserId;
                request.UserId = CurrentUserId;
                var result = await _tranService.AddTransactionAsync(request);
                if (result.code == HttpStatusCode.OK)
                {

                    UserDeviceToken deviceToken = await _userService.GetUserDeviceToken(request.StudentUserId);
                    if (deviceToken != null && !string.IsNullOrEmpty(deviceToken.DeviceToken) && !string.IsNullOrEmpty(deviceToken.ParentId))
                    {
                        PushNotificationRequest notificationModel = new PushNotificationRequest
                        {
                            token = deviceToken.DeviceToken,
                            body = NotificationMessageCode.OrderProcessedBody,
                            title = NotificationMessageCode.OrderProcessedTitle
                        };

                        bool isSent = await _pushnotification.SendToDeviceAsync(notificationModel);
                        if (isSent)
                        {
                            NotificationRequest notificationrequest = new NotificationRequest
                            {
                                UserId = deviceToken.ParentId,
                                SenderId = CurrentUserId,
                                SchoolId = request.SchoolId,
                                Subject = NotificationMessageCode.OrderProcessedTitle,
                                Message = NotificationMessageCode.OrderProcessedBody,
                                IsRead = 0,
                                CreatedBy = CurrentUserId
                            };
                            await _notification.AddNotificationAsync(notificationrequest);
                        }
                    }
                    else
                    {
                        LoggerLibrary.WriteLog("AddTransactionAsync > OrderProcessedBody deviceToken not found  " + request.StudentUserId, true);
                    }
                }
                else
                {
                    if (result.message == MessageCode.InsufficientBalance)
                    {
                        UserDeviceToken deviceToken = await _userService.GetUserDeviceToken(request.StudentUserId);
                        if (deviceToken != null && !string.IsNullOrEmpty(deviceToken.DeviceToken) && !string.IsNullOrEmpty(deviceToken.ParentId))
                        {
                            PushNotificationRequest notificationModel = new PushNotificationRequest
                            {
                                token = deviceToken.DeviceToken,
                                body = NotificationMessageCode.InsufficientBalanceBody,
                                title = NotificationMessageCode.InsufficientBalanceTitle
                            };

                            bool isSent = await _pushnotification.SendToDeviceAsync(notificationModel);
                            if (isSent)
                            {
                                NotificationRequest notificationrequest = new NotificationRequest
                                {
                                    UserId = deviceToken.ParentId,
                                    SenderId = CurrentUserId,
                                    SchoolId = request.SchoolId,
                                    Subject = NotificationMessageCode.InsufficientBalanceTitle,
                                    Message = NotificationMessageCode.InsufficientBalanceBody,
                                    IsRead = 0,
                                    CreatedBy = CurrentUserId
                                };
                                await _notification.AddNotificationAsync(notificationrequest);
                            }
                        }
                        else
                        {
                            LoggerLibrary.WriteLog("AddTransactionAsync > InsufficientBalance deviceToken not found  " + request.StudentUserId, true);
                        }
                    }
                    LoggerLibrary.WriteLog("AddTransactionAsync : " + result.message, true);
                }
                return Json(new { data = result }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var result = new Dictionary<string, string>
                {
                    { "code", "400"},
                    { "message", MessageCode.SessionExpire}
                };

                return Json(new { data = result }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region Export Methods
        private static void AddTitleCell(string headerText, Font font, PdfPTable tabledays)
        {
            PdfPCell pdfCell = new PdfPCell()
            {
                Padding = 6,
                Colspan = 1,
                UseVariableBorders = true,
                UseBorderPadding = true,
                UseAscender = true,
                UseDescender = true,
                BorderColor = BaseColor.WHITE,
                BackgroundColor = BaseColor.WHITE
            };

            Paragraph paragraph = new Paragraph(headerText, font)
            {
                SpacingAfter = 4,
                Alignment = Element.ALIGN_CENTER
            };

            pdfCell.AddElement(paragraph);
            tabledays.AddCell(pdfCell);
        }
        private static void AddHeaderCell(string headerText, Font font, PdfPTable tabledays)
        {
            PdfPCell pdfCell = new PdfPCell()
            {
                Padding = 6,
                Colspan = 1,
                UseVariableBorders = true,
                UseBorderPadding = true,
                UseAscender = true,
                UseDescender = true,
                BorderColor = BaseColor.WHITE,
                BackgroundColor = BaseColor.BLUE
            };

            Paragraph paragraph = new Paragraph(headerText, font)
            {
                SpacingAfter = 4,
                Alignment = Element.ALIGN_CENTER
            };

            pdfCell.AddElement(paragraph);
            tabledays.AddCell(pdfCell);
        }
        private static void AddColumnValues(string text, Font font, BaseColor bgcolor, PdfPTable tabledays, int align = 1)
        {
            if (string.IsNullOrEmpty(text)) text = " ";

            PdfPCell pdfCell = new PdfPCell()
            {
                Padding = 5,
                UseVariableBorders = true,
                UseBorderPadding = true,
                UseAscender = true,
                UseDescender = true,
                BorderColor = BaseColor.WHITE,
                BackgroundColor = bgcolor
            };

            Paragraph paragraph = new Paragraph(text, font)
            {
                Alignment = align
            };

            pdfCell.AddElement(paragraph);
            tabledays.AddCell(pdfCell);
        }
        private static void NoRecordRow(string text, Font font, BaseColor bgcolor, PdfPTable tabledays, int align = 1, int colspan = 0)
        {
            if (string.IsNullOrEmpty(text)) text = " ";

            PdfPCell pdfCell = new PdfPCell()
            {
                Padding = 5,
                Colspan = colspan,
                UseVariableBorders = true,
                UseBorderPadding = true,
                UseAscender = true,
                UseDescender = true,
                BorderColor = BaseColor.WHITE,
                BackgroundColor = bgcolor
            };

            Paragraph paragraph = new Paragraph(text, font)
            {
                Alignment = align
            };

            pdfCell.AddElement(paragraph);
            tabledays.AddCell(pdfCell);
        }
        #endregion

        #region Pre Order
        [HttpPost]
        public async Task<ActionResult> RejectPreOrder(string preOrderId)
        {
            var result = await _menu.ManagePreOrderStatusAsync(preOrderId, EnumType.PreOrderStatus.Rejected);
            var preOrderResult = await _menu.GetPreorderByIdAsync(preOrderId);
            UserDeviceToken deviceToken = await _userService.GetUserDeviceToken(preOrderResult.StudentUserId);
            if (deviceToken != null && !string.IsNullOrEmpty(deviceToken.DeviceToken) && !string.IsNullOrEmpty(deviceToken.ParentId))
            {
                PushNotificationRequest notificationModel = new PushNotificationRequest
                {
                    token = deviceToken.DeviceToken,
                    body = NotificationMessageCode.PreOrderRejectedBody,
                    title = NotificationMessageCode.PreOrderRejectedTitle
                };

                bool isSent = await _pushnotification.SendToDeviceAsync(notificationModel);
                if (isSent)
                {
                    NotificationRequest notificationrequest = new NotificationRequest
                    {
                        UserId = deviceToken.ParentId,
                        SenderId = CurrentUserId,
                        SchoolId = preOrderResult.SchoolId,
                        Subject = NotificationMessageCode.PreOrderRejectedTitle,
                        Message = NotificationMessageCode.PreOrderRejectedBody,
                        IsRead = 0,
                        CreatedBy = CurrentUserId
                    };
                    await _notification.AddNotificationAsync(notificationrequest);
                }
            }

            return Json(new { data = result }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public async Task<ActionResult> ManagePreOrderAsync(string preOrderId, string transactionPin)
        {
            if (Session["CurrentUserId"] != null)
            {
                string UserId = Session["CurrentUserId"].ToString();
                var preOrderResult = await _menu.GetPreorderByIdAsync(preOrderId);
                var preOrderItemResult = await _menu.GetPreorderItemByIdAsync(preOrderId);
                string MLMID = await _userService.GetMLMIDbyStudentUserIdasync(preOrderResult.StudentUserId);
                AddTransactionRequest request = new AddTransactionRequest
                {
                    AdminStaffUserId = UserId,
                    UserId = UserId,
                    ClassName = preOrderResult.ClassName,
                    Grade = preOrderResult.Grade,
                    MLMID = MLMID,
                    NoOfItem = preOrderResult.NoOfItem,
                    SchoolId = preOrderResult.SchoolId,
                    StudentUserId = preOrderResult.StudentUserId,
                    TotalAmount = preOrderResult.TotalAmount,
                    TransactionPin = transactionPin,
                    Items = new List<TransactionItemHistory>(),
                    OrderType = 1
                };
                foreach (var item in preOrderItemResult)
                {
                    TransactionItemHistory addItem = new TransactionItemHistory
                    {
                        ItemId = item.ItemId,
                        CreatedBy = item.CreatedBy,
                        ItemName = item.ItemName,
                        Quantity = item.Quantity,
                        TotalPrice = item.TotalPrice,
                        UnitPrice = item.UnitPrice
                    };
                    request.Items.Add(addItem);
                }

                var result = await _tranService.AddTransactionAsync(request, false);
                if (result.code == HttpStatusCode.OK)
                {
                    var resultstatus = await _menu.ManagePreOrderStatusAsync(preOrderId, EnumType.PreOrderStatus.Processed);

                    if (resultstatus.code == HttpStatusCode.OK)
                    {
                        UserDeviceToken deviceToken = await _userService.GetUserDeviceToken(preOrderResult.StudentUserId);
                        if (deviceToken != null && !string.IsNullOrEmpty(deviceToken.DeviceToken) && !string.IsNullOrEmpty(deviceToken.ParentId))
                        {
                            PushNotificationRequest notificationModel = new PushNotificationRequest
                            {
                                token = deviceToken.DeviceToken,
                                body = NotificationMessageCode.PreOrderProcessedBody,
                                title = NotificationMessageCode.PreOrderProcessedTitle
                            };

                            bool isSent = await _pushnotification.SendToDeviceAsync(notificationModel);
                            if (isSent)
                            {
                                NotificationRequest notificationrequest = new NotificationRequest
                                {
                                    UserId = deviceToken.ParentId,
                                    SenderId = CurrentUserId,
                                    SchoolId = preOrderResult.SchoolId,
                                    Subject = NotificationMessageCode.PreOrderProcessedTitle,
                                    Message = NotificationMessageCode.PreOrderProcessedBody,
                                    IsRead = 0,
                                    CreatedBy = CurrentUserId
                                };
                                await _notification.AddNotificationAsync(notificationrequest);

                                var webnotificationModel = new AddNotificationRequest
                                {

                                    CurrentUserId = CurrentUserId,
                                    SchoolId = preOrderResult.SchoolId,
                                    Subject = NotificationMessageCode.PreOrderProcessedTitle,
                                    Message = NotificationMessageCode.PreOrderProcessedBody,
                                };
                                var addnotification = await _notification.AddNotifications("PO", webnotificationModel);
                                MessagesHub objHub = new MessagesHub();
                                objHub.GetNotification();
                            }
                        }
                        else
                        {
                            LoggerLibrary.WriteLog("ManagePreOrderAsync > PreOrderProcessedBody deviceToken not found  " + preOrderResult.StudentUserId, true);
                        }
                    }
                    else
                    {
                        LoggerLibrary.WriteLog("ManagePreOrderAsync > ManagePreOrderStatusAsync: " + resultstatus.message, true);
                    }
                }
                else
                {
                    if (result.message == MessageCode.InsufficientBalance)
                    {
                        UserDeviceToken deviceToken = await _userService.GetUserDeviceToken(preOrderResult.StudentUserId);
                        if (deviceToken != null && !string.IsNullOrEmpty(deviceToken.DeviceToken) && !string.IsNullOrEmpty(deviceToken.ParentId))
                        {
                            PushNotificationRequest notificationModel = new PushNotificationRequest
                            {
                                token = deviceToken.DeviceToken,
                                body = NotificationMessageCode.InsufficientBalanceBody,
                                title = NotificationMessageCode.InsufficientBalanceTitle
                            };

                            bool isSent = await _pushnotification.SendToDeviceAsync(notificationModel);
                            if (isSent)
                            {
                                NotificationRequest notificationrequest = new NotificationRequest
                                {
                                    UserId = deviceToken.ParentId,
                                    SenderId = CurrentUserId,
                                    SchoolId = preOrderResult.SchoolId,
                                    Subject = NotificationMessageCode.InsufficientBalanceTitle,
                                    Message = NotificationMessageCode.InsufficientBalanceBody,
                                    IsRead = 0,
                                    CreatedBy = CurrentUserId
                                };
                                await _notification.AddNotificationAsync(notificationrequest);
                            }
                        }
                        else
                        {
                            LoggerLibrary.WriteLog("ManagePreOrderAsync > InsufficientBalance deviceToken not found  " + preOrderResult.StudentUserId, true);
                        }
                    }
                    LoggerLibrary.WriteLog("ManagePreOrderAsync > AddTransactionAsync: " + result.message, true);
                }
                return Json(new { data = result }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var result = new Dictionary<string, string>
                {
                    { "code", "400"},
                    { "message", MessageCode.SessionExpire}
                };

                return Json(new { data = result }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion
    }
}