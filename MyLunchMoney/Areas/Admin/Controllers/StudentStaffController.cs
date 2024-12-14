using iTextSharp.text;
using iTextSharp.text.pdf;
using MyLunchMoney.Constants;
using MyLunchMoney.Models;
using MyLunchMoney.Models.ViewModels;
using MyLunchMoney.Repository;
using MyLunchMoney.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace MyLunchMoney.Areas.Admin.Controllers
{
    public class StudentStaffController : AdminBaseController
    {
        #region Declaration
        private readonly IUserService _userService;
        private readonly IUserRepository _userRepository;
        private readonly IAuthService _authService;
        private readonly ApplicationDbContext _context;
        private readonly IGradeRepository _gradeRepository;
        private readonly IPushNotificationService _pushnotification;
        private readonly INotificationService _notification;
        #endregion

        #region Constructor
        public StudentStaffController
        (
            IUserService userService,
            ApplicationDbContext context,
            IAuthService authService,
            IGradeRepository gradeRepository,
            IUserRepository userRepository,
             IPushNotificationService pushnotification,
            INotificationService notification
        )
        {
            _userService = userService;
            _authService = authService;
            _context = context;
            _gradeRepository = gradeRepository;
            _userRepository = userRepository;
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

        #region Post Mothods
        [HttpPost]
        public async Task<ActionResult> GetStudentListAsync(string schoolId)
        {
            var result = await _userService.GetStudentListAsync(schoolId);

            return Json(new { data = result }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public async Task<ActionResult> GetManageStudentList(int status, int days)
        {
            var result = await _userService.GetManageStudentList(SchoolId, status, days);

            return Json(new { data = result.data }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public async Task<ActionResult> GetTransactionDetailAsync(string id)
        {
            string StudentUserId = id;
            var result = await _userService.GetTransactionDetailAsync(StudentUserId);

            return Json(new { data = result.data }, JsonRequestBehavior.AllowGet);
        }
        //[HttpPost]
        //public async Task<ActionResult> GetTransactionDetailAsync(string id)
        //{
        //    var result = await _userService.GetTransactionDetailAsync(id);

        //    return Json(new { data = result.data }, JsonRequestBehavior.AllowGet);
        //}
        [HttpPost]
        public async Task<ActionResult> SaveAsync()
        {
            try
            {
                var model = JsonConvert.DeserializeObject<RegisterViewModel>(HttpContext.Request.Form["model"]);
                HttpPostedFile fileObj = null;
                model.SchoolId = SchoolId;
                var files = Request.Files;

                for (int i = 0; i < files.Count; i++)
                {
                    var file = files.Get(i);
                    var constructorInfo = typeof(HttpPostedFile).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance)[0];
                    fileObj = (HttpPostedFile)constructorInfo
                              .Invoke(new object[] { file.FileName, file.ContentType, file.InputStream });
                }

                var result = await _authService.RegisterUser(model, fileObj);
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message.ToString() }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public async Task<ActionResult> GetStudentDetailAsync(string sid)
        {
            var result = await _userService.GetStudentDetailAsync(sid);

            return Json(new { data = result }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public async Task<ActionResult> RemoveStudentImageAsync(string id)
        {
            var result = await _userService.RemoveStudentImageAsync(id);

            return Json(new { data = result }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public async Task<ActionResult> IsSponsoredCheckedAsync(string id, int isChecked)
        {
            var result = await _userService.IsSponsoredCheckedAsync(id, isChecked);

            return Json(new { data = result }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<ActionResult> IsActiveUser(string userId, int isActive)
        {
            var result = await _userService.IsActiveUser(userId, isActive);

            return Json(new { data = result }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public async Task<ActionResult> IsSponsorNededUser(string userId, int sponsor)
        {
            var result = await _userService.IsSponsorNededUser(userId, sponsor);

            return Json(new { data = result }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public async Task<ActionResult> DeleteStudentAsync(string id)
        {
            var result = await _userService.DeleteStudentAsync(id);

            return Json(new { data = result }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<ActionResult> StudentApproveDisapproveAsync(string id, int status, string MLMId)
        {
            var result = await _userService.StudentApproveDisapproveAsync(id, status);
            if (result.code == HttpStatusCode.OK)
            {
                var userData = await _userService.GetStudentByMLMIDAsync(MLMId);
                if (userData.code == HttpStatusCode.OK && userData.data != null)
                {
                    UserDeviceToken deviceToken = await _userService.GetUserDeviceToken(userData.data[0].StudentUserId);
                    if (deviceToken != null && !string.IsNullOrEmpty(deviceToken.DeviceToken) && !string.IsNullOrEmpty(deviceToken.ParentId))
                    {
                        if (status == 2)
                        {
                            PushNotificationRequest notificationModel = new PushNotificationRequest
                            {
                                token = deviceToken.DeviceToken,
                                body = NotificationMessageCode.ChildrenApprovalRequestBody,
                                title = NotificationMessageCode.ChildrenApprovalRequestTitle
                            };

                            bool isSent = await _pushnotification.SendToDeviceAsync(notificationModel);
                            if (isSent)
                            {
                                NotificationRequest notificationrequest = new NotificationRequest
                                {
                                    UserId = deviceToken.ParentId,
                                    SenderId = CurrentUserId,
                                    SchoolId = userData.data[0].SchoolId,
                                    Subject = NotificationMessageCode.ChildrenApprovalRequestTitle,
                                    Message = NotificationMessageCode.ChildrenApprovalRequestBody,
                                    IsRead = 0,
                                    CreatedBy = CurrentUserId
                                };
                                await _notification.AddNotificationAsync(notificationrequest);
                            }
                        }
                        if (status == 5)
                        {
                            PushNotificationRequest notificationModel = new PushNotificationRequest
                            {
                                token = deviceToken.DeviceToken,
                                body = NotificationMessageCode.ChildrenRejectRequestBody,
                                title = NotificationMessageCode.ChildrenRejectRequestTitle
                            };

                            bool isSent = await _pushnotification.SendToDeviceAsync(notificationModel);
                            if (isSent)
                            {
                                NotificationRequest notificationrequest = new NotificationRequest
                                {
                                    UserId = deviceToken.ParentId,
                                    SenderId = CurrentUserId,
                                    SchoolId = userData.data[0].SchoolId,
                                    Subject = NotificationMessageCode.ChildrenRejectRequestTitle,
                                    Message = NotificationMessageCode.ChildrenRejectRequestBody,
                                    IsRead = 0,
                                    CreatedBy = CurrentUserId
                                };
                                await _notification.AddNotificationAsync(notificationrequest);
                            }
                        }
                    }

                }
            }
            return Json(new { data = result }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public async Task<ActionResult> GetGradeBySchoolIdAsync()
        {
            var result = await _gradeRepository.GetGradeBySchoolIdAsync(SchoolId);

            return Json(new { data = result }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetGradeList()
        {
            var grades = _context.Grades.Where(x => x.SchoolId == SchoolId)
                .Select(x => new
                {
                    GradeId = x.GradeId,
                    GradeName = x.GradeName
                }).ToList();
            return Json(grades, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult GetClassList(int gradeId)
        {
            var classes = _context.Grades.Where(x => x.GradeId == gradeId && x.SchoolId == SchoolId)
                .Select(x => new
                {
                    ClassName = x.ClassName
                }).ToList();
            return Json(classes, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public async Task<ActionResult> GetClassNameAsync(string gradeName)
        {
            var list = new List<KeyValuePair<string, string>>();
            var result = await _gradeRepository.GetClassNameAsync(gradeName, SchoolId);
            foreach (var e in result)
            {
                list.Add(new KeyValuePair<string, string>(e, e));
            }
            return Json(new { data = list }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public async Task<ActionResult> ExportManageStudentListAsync(int status, int days)
        {
            string filename = string.Format("{0}-{1}.{2}", status, DateTime.Now.ToString("dd-MM-yyyy-hh:mm:ss"), "pdf");

            IEnumerable<ManageStudentViewModel> result = await _userRepository.GetManageStudentList(SchoolId, status, days);

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
                AddTitleCell((days + " List"), titleFont, headingBody);

                //Table
                List<float> colWidth = new List<float>() { 5f, 5f, 5f, 5f, 5f, 5f, 5f };//Item,Amount,Date				

                PdfPTable tablebody = new PdfPTable(7)
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
                    AddHeaderCell("Name", boldFont, tablebody);
                    AddHeaderCell("Total Amount", boldFont, tablebody);
                    AddHeaderCell("Grade", boldFont, tablebody);
                    AddHeaderCell("Date", boldFont, tablebody);
                    AddHeaderCell("Status", boldFont, tablebody);
                    AddHeaderCell("Sponsor", boldFont, tablebody);

                    int i = 0;
                    foreach (var item in result)
                    {
                        BaseColor _color = i % 2 == 0 ? _color2 : _color1;
                        AddColumnValues(item.MLMID, normalFont, _color, tablebody, Element.ALIGN_LEFT);
                        AddColumnValues(item.StudentName, normalFont, _color, tablebody, Element.ALIGN_LEFT);
                        AddColumnValues(item.Balance.ToString(), normalFont, _color, tablebody, Element.ALIGN_RIGHT);
                        AddColumnValues(item.Grade.ToString(), normalFont, _color, tablebody, Element.ALIGN_RIGHT);
                        AddColumnValues(item.CreatedOn.ToString(), normalFont, _color, tablebody, Element.ALIGN_RIGHT);
                        AddColumnValues(item.UserStatus.ToString(), normalFont, _color, tablebody, Element.ALIGN_RIGHT);
                        AddColumnValues(Enum.GetName(typeof(EnumType.StudentStatus), item.Status), normalFont, _color, tablebody, Element.ALIGN_RIGHT);
                        i++;
                    }
                    document.Add(headingBody);
                    document.Add(tablebody);
                }
                else
                {
                    AddHeaderCell("MLM ID", boldFont, tablebody);
                    AddHeaderCell("Name", boldFont, tablebody);
                    AddHeaderCell("Total Amount", boldFont, tablebody);
                    AddHeaderCell("Grade", boldFont, tablebody);
                    AddHeaderCell("Date", boldFont, tablebody);
                    AddHeaderCell("Status", boldFont, tablebody);
                    AddHeaderCell("Sponsor", boldFont, tablebody);

                    NoRecordRow("No Records", normalFont, new BaseColor(249, 249, 249), tablebody, Element.ALIGN_CENTER, 7);

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
    }
}