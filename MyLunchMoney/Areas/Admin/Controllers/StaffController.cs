using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using MyLunchMoney.Constants;
using MyLunchMoney.Hubs;
using MyLunchMoney.Models;
using MyLunchMoney.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace MyLunchMoney.Areas.Admin.Controllers
{
    public class StaffController : AdminBaseController
    {
        #region Declaration
        private readonly IUserService _userService;
        private readonly IAuthService _authService;
        private readonly IStaffService _staffService;
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private readonly IEmailService _emailService;
        private readonly INotificationService _notificationService;
        private readonly string _defaultPassword = ConfigurationManager.AppSettings["DefaultPassword"].ToString();
        #endregion

        #region Construction
        public StaffController(IStaffService staffService, IEmailService emailService, IUserService userService, IAuthService authService, INotificationService notificationService)
        {
            _userService = userService;
            _authService = authService;
            _staffService = staffService;
            _emailService = emailService;
            _notificationService = notificationService;
        }
        public StaffController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            _UserManager = userManager;
            SignInManager = signInManager;
        }
        #endregion

        #region Identity
        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        public ApplicationUserManager _UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        #endregion

        #region Get Methods
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public async Task<JsonResult> GetStaffListAsync(int status, int days)
        {
            var result = await _staffService.GetStaffListAsync(SchoolId, status, days);

            return Json(new { data = result }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Post Methods
        [HttpPost]
        public async Task<ActionResult> RegisterSchoolStaff()
        {
            String CurrentUserId = Session["CurrentUserId"].ToString();
            var model = JsonConvert.DeserializeObject<RegisterViewModel>(HttpContext.Request.Form["model"]);

            model.SchoolId = SchoolId;
            model.Password = _defaultPassword;
            model.RoleId = 4;

            var files = HttpContext.Request.Files.Count > 0 ? HttpContext.Request.Files : null;
            HttpPostedFile _file = null;

            if (files != null)
            {
                for (int i = 0; i < files.Count; i++)
                {
                    var file = files.Get(i);
                    var constructorInfo = typeof(HttpPostedFile).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance)[0];
                    _file = (HttpPostedFile)constructorInfo
                              .Invoke(new object[] { file.FileName, file.ContentType, file.InputStream });
                }
            }
            if (!string.IsNullOrEmpty(model.Id))
            {
                var result = await _authService.UpdateStaffDetail(model, _file);
                var notificationModel = new AddNotificationRequest
                {
                    SchoolId = SchoolId,
                    CurrentUserId = CurrentUserId,
                    Subject = "Staff Detail Updated",
                    Message = MessageCode.StaffUserUpdated
                };
                var addnotification = await _notificationService.AddNotifications("SN", notificationModel);
                MessagesHub objHub = new MessagesHub();
                objHub.GetNotification();
                return Json(new { data = result }, JsonRequestBehavior.AllowGet);

            }
            else
            {
                var result = await _authService.RegisterUser(model, _file);
                if (result.code == System.Net.HttpStatusCode.OK)
                {
                    var user = await _UserManager.FindByNameAsync(model.Email);
                    if (user == null)
                    {
                        // Don't reveal that the user does not exist or is not confirmed
                        var resultuser = new Dictionary<string, string>
                        {
                            { "code", "400"},
                            { "message", MessageCode.UserNotExist}
                        };
                        return Json(new { data = resultuser }, JsonRequestBehavior.AllowGet);
                    }

                    // Send an email with link
                    string code = await _UserManager.GeneratePasswordResetTokenAsync(user.Id);
                    var callbackUrl = Url.Action("ResetPassword", "Account", new { code = code }, protocol: Request.Url.Scheme);
                    string body = string.Empty;
                    using (StreamReader reader = new StreamReader(Server.MapPath("~/Template/ForgotPassword.html")))
                    {
                        body = reader.ReadToEnd();
                    }
                    body = body.Replace("{resetLink}", callbackUrl);
                    bool IsSendEmail = _emailService.EmailSend(model.Email, "Welcome to MILUNCHMONEY!", body, true);
                    if (IsSendEmail)
                    {
                        return Json(new { data = result }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        var resultemail = new Dictionary<string, string>
                        {
                            { "code", "400"},
                            { "message", "Email Sending Error"}
                        };
                        return Json(new { data = resultemail }, JsonRequestBehavior.AllowGet);
                    }
                    
                }

                return Json(new { data = result }, JsonRequestBehavior.AllowGet);
            }
        }
        
        [HttpGet]
        public async Task<ActionResult> GetStaffDetailAsync(string id)
        {
            var result = await _userService.GetStaffDetailAsync(id);

            return Json(new { data = result }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<ActionResult> StaffActiveDeactive(string userId, int isActive)
        {
            var result = await _userService.IsActiveUser(userId, isActive);

            return Json(new { data = result }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public async Task<ActionResult> DeleteStaffUserAsync(string id)
        {
            var result = await _staffService.DeleteStaffUserAsync(id);

            return Json(new { data = result }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public async Task<ActionResult> ExportManageStaffListAsync(int status, int days)
        {
            string filename = string.Format("{0}-{1}.{2}", status, DateTime.Now.ToString("dd-MM-yyyy-hh:mm:ss"), "pdf");

            IEnumerable<ManageUserViewModel> result = await _staffService.GetStaffListAsync(SchoolId, status, days);

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
                AddTitleCell((status + " List"), titleFont, headingBody);

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
                    AddHeaderCell("Name", boldFont, tablebody);
                    AddHeaderCell("Role", boldFont, tablebody);
                    AddHeaderCell("Email", boldFont, tablebody);
                    AddHeaderCell("Phone", boldFont, tablebody);
                    AddHeaderCell("Date", boldFont, tablebody);

                    int i = 0;
                    foreach (var item in result)
                    {
                        BaseColor _color = i % 2 == 0 ? _color2 : _color1;
                        AddColumnValues(item.Name, normalFont, _color, tablebody, Element.ALIGN_LEFT);
                        AddColumnValues(item.Role, normalFont, _color, tablebody, Element.ALIGN_LEFT);
                        AddColumnValues(item.Email.ToString(), normalFont, _color, tablebody, Element.ALIGN_RIGHT);
                        AddColumnValues(item.Phone.ToString(), normalFont, _color, tablebody, Element.ALIGN_RIGHT);
                        AddColumnValues(item.Date.ToString(), normalFont, _color, tablebody, Element.ALIGN_CENTER);
                        i++;
                    }
                    document.Add(headingBody);
                    document.Add(tablebody);
                }
                else
                {
                    AddHeaderCell("Name", boldFont, tablebody);
                    AddHeaderCell("Role", boldFont, tablebody);
                    AddHeaderCell("Email", boldFont, tablebody);
                    AddHeaderCell("Phone", boldFont, tablebody);
                    AddHeaderCell("Date", boldFont, tablebody);

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