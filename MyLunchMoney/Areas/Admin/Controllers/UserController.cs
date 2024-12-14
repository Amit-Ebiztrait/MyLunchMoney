using iTextSharp.text;
using iTextSharp.text.pdf;
using MyLunchMoney.Helpers;
using MyLunchMoney.Models;
using MyLunchMoney.Repository;
using MyLunchMoney.Services;
//using Newtonsoft.Json;
//using OfficeOpenXml;
//using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace MyLunchMoney.Areas.Admin.Controllers
{
    public class UserController : AdminBaseController
    {
        #region Declaration
        private readonly IUserRepository _userRepo;
        private readonly IUserService _userService;
        private readonly ISchoolService _schoolService;
        private readonly IAuthRepository _authRepository;
        private readonly IStaffRepository _staffRepository;
        #endregion

        #region Constructor
        public UserController(
            IUserRepository userRepo
            , IUserService userService
            , ISchoolService schoolService
            , IAuthRepository authRepository
            , IStaffRepository staffRepository
        )
        {
            _userRepo = userRepo;
            _userService = userService;
            _schoolService = schoolService;
            _authRepository = authRepository;
            _staffRepository = staffRepository;
        }
        #endregion

        #region Methods
        public ActionResult Index() => View();

        [HttpPost]
        public async Task<JsonResult> GetUserListAsync(string type, int status, int days)
        {
            var result = await _userService.GetManageUserListAsync(type, status, days);
            return Json(new { data = result.data }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public async Task<ActionResult> ExportManageUserPDF(string type, int status, int days)
        {
            string filename = string.Format("{0}-{1}.{2}", type, DateTime.Now.ToString("dd-MM-yyyy-hh:mm:ss"), "pdf");
            //Cafeteria History
            var result = await _userService.GetManageUserListAsync(type, status, days);
            List<ManageUserViewModel> userList = result.data;

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

                if (userList != null && userList.Count > 0)
                {
                    AddHeaderCell("Name", boldFont, tablebody);
                    AddHeaderCell("Phone", boldFont, tablebody);
                    AddHeaderCell("Email", boldFont, tablebody);
                    AddHeaderCell("Date", boldFont, tablebody);
                    AddHeaderCell("Status", boldFont, tablebody);

                    int i = 0;
                    foreach (var item in userList)
                    {
                        BaseColor _color = i % 2 == 0 ? _color2 : _color1;
                        AddColumnValues(item.Name, normalFont, _color, tablebody, Element.ALIGN_LEFT);
                        AddColumnValues(item.Phone, normalFont, _color, tablebody, Element.ALIGN_RIGHT);
                        AddColumnValues(item.Email.ToString(), normalFont, _color, tablebody, Element.ALIGN_LEFT);
                        AddColumnValues(item.Date.ToString(), normalFont, _color, tablebody, Element.ALIGN_RIGHT);
                        AddColumnValues(item.Status.ToString(), normalFont, _color, tablebody, Element.ALIGN_CENTER);
                        i++;
                    }
                    document.Add(headingBody);
                    document.Add(tablebody);
                }
                else
                {
                    AddHeaderCell("Name", boldFont, tablebody);
                    AddHeaderCell("Phone", boldFont, tablebody);
                    AddHeaderCell("Email", boldFont, tablebody);
                    AddHeaderCell("Date", boldFont, tablebody);
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

        [HttpPost]
        public async Task<ActionResult> IsActiveUser(string userId, int isActive)
        {
            var result = await _userService.IsActiveUser(userId, isActive);

            return Json(new { data = result }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public async Task<ActionResult> UserDetail(string id)
        {
            var model = await _userService.GetManageUserDetail(id);
            return PartialView("_UserDetailPartialView", model);
            //return Json(new { data = model }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public async Task<ActionResult> GetSponsorProfileAsync(string id)
        {
            var model = await _authRepository.GetSponsorProfileAsync(id);
            var response = new SponsorProfileViewModel();
            PropertyCopier<SponsorProfile, SponsorProfileViewModel>.Copy(model, response);

            var child = await _authRepository.GetSponserStudentListAsync(id);
            response.ChildDetails = child;
            return PartialView("_SponsorDetailPartialView", response);
            //return Json(new { data = model, Child = child }, JsonRequestBehavior.AllowGet);

        }
        [HttpGet]
        public async Task<ActionResult> GetGeneralSchoolDetailAsync(string id)
        {
            var model = await _schoolService.GetGeneralSchoolDetailAsync(id);
            return Json(new { data = model }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public async Task<ActionResult> GetStudentListAsync(string id)
        {
            var model = await _userService.GetStudentListAsync(id);
            return Json(new { data = model }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public async Task<ActionResult> GetStudentDetailAsync(string id)
        {
            var model = await _userService.GetStudentDetailAsync(id);
            return Json(new { data = model }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public async Task<ActionResult> GetStaffListAsync(string id)
        {
            var model = await _staffRepository.GetStaffListAsync(id, 1, 1);
            return Json(new { data = model }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public async Task<ActionResult> GetStaffDetailAsync(string id)
        {
            var model = await _userService.GetStaffDetailAsync(id);
            return Json(new { data = model }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public async Task<ActionResult> GetChildCafeteriaHistory(string studentUserId)
        {
            var model = await _userService.GetChildCafeteriaHistory(studentUserId);

            return Json(new { data = model }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public async Task<ActionResult> GetChildTransactionHistory(string studentId)
        {
            var model = await _userService.GetChildTransactionHistory(studentId);

            return Json(new { data = model }, JsonRequestBehavior.AllowGet);
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


        #region School Students 
        [HttpGet]
        public ActionResult SchoolStudents(string schoolId)
        {
            TempData["schoolId"] = schoolId;
            return View();
        }
        [HttpGet]
        public async Task<ActionResult> GetSchoolStudentList(int status, int days)
        {
            string schoolid = Convert.ToString(TempData.Peek("schoolId"));
            var result = await _userService.GetManageStudentList(schoolid, status, days);

            return Json(new { data = result.data }, JsonRequestBehavior.AllowGet);
        }
        #endregion


        #region School Staff 
        [HttpGet]
        public ActionResult SchoolStaff(string schoolId)
        {
            TempData["schoolIdforStaff"] = schoolId;
            return View();
        }
        [HttpGet]
        public async Task<ActionResult> GetSchoolStaffList(int status, int days)
        {
            string schoolid = Convert.ToString(TempData.Peek("schoolIdforStaff"));
            var result = await _staffRepository.GetStaffListAsync(schoolid, status, days);

            return Json(new { data = result }, JsonRequestBehavior.AllowGet);
        }
        #endregion
    }
}