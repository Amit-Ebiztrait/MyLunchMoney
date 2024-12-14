using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using iTextSharp.text;
using iTextSharp.text.pdf;
using MyLunchMoney.Models;
using MyLunchMoney.Models.ViewModels;
using MyLunchMoney.Repository;
using MyLunchMoney.Services;

namespace MyLunchMoney.Areas.Admin.Controllers
{
    public class UserTransactionController : Controller
    {
        #region Declaration
        private readonly IUserRepository _userRepo;
        private readonly IUserService _userService;
        private readonly ISchoolService _schoolService;
        private readonly IAuthRepository _authRepository;
        #endregion

        #region Constructor
        public UserTransactionController(
            IUserRepository userRepo
            , IUserService userService
            , ISchoolService schoolService
            , IAuthRepository authRepository
        )
        {
            _userRepo = userRepo;
            _userService = userService;
            _schoolService = schoolService;
            _authRepository = authRepository;
        }
        #endregion
        public ActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public async Task<ActionResult> GetUserTransactionListAsync(string type, int status, int days)
        {
            var result = await _userService.GetManageUserTransactionListAsync(type, status, days);

            return Json(new { data = result }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public async Task<ActionResult> GetSponsorBySchool(string schoolId, string type)
        {
            var model = await _schoolService.GetSponsorBySchool(schoolId, type);

            return Json(new { data = model }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public async Task<ActionResult> ExportUserTransactionListAsync(string type, int status, int days)
        {
            string filename = string.Format("{0}-{1}.{2}", type, DateTime.Now.ToString("dd-MM-yyyy-hh:mm:ss"), "pdf");

            IEnumerable<ManageUserTransactionViewModel> result = await _userService.GetManageUserTransactionListAsync(type, status, days);

            Font titleFont = new Font(Font.FontFamily.HELVETICA, 14, Font.BOLD, BaseColor.BLACK);
            Font normalFont = new Font(Font.FontFamily.HELVETICA, 9, Font.NORMAL, BaseColor.BLACK);
            Font boldFont = new Font(Font.FontFamily.HELVETICA, 10, Font.BOLD, BaseColor.WHITE);

            #region Initialize PDF File
            MemoryStream stream = new MemoryStream();
            Document document = new Document(PageSize.A4, 25f, 25f, 25f, 25f);
            PdfWriter.GetInstance(document, stream);

            document.Open();
            #endregion
            if (type == "Parent")
            {
                try
                {
                    //Header Title
                    List<float> titleWidth = new List<float>() { 5f };//Title
                    PdfPTable headingBody = new PdfPTable(1)
                    {
                        WidthPercentage = 110,
                        KeepTogether = false
                    };
                    headingBody.SetWidths(titleWidth.ToArray());
                    AddTitleCell((type + " List"), titleFont, headingBody);

                    //Table
                    List<float> colWidth = new List<float>() { 5f, 5f, 5f, 5f, 5f, 5f, 5f,5f };//Item,Amount,Date				

                    PdfPTable tablebody = new PdfPTable(8)
                    {
                        WidthPercentage = 110,
                        KeepTogether = false
                    };

                    tablebody.SetWidths(colWidth.ToArray());
                    BaseColor _color1 = new BaseColor(249, 249, 249);
                    BaseColor _color2 = new BaseColor(236, 237, 240);

                    if (result != null && result.Count() > 0)
                    {
                        AddHeaderCell("Id", boldFont, tablebody);
                        AddHeaderCell("ParentName", boldFont, tablebody);
                        AddHeaderCell("SchoolName", boldFont, tablebody);
                        AddHeaderCell("Email", boldFont, tablebody);
                        AddHeaderCell("Status", boldFont, tablebody);
                        AddHeaderCell("Date", boldFont, tablebody);
                        AddHeaderCell("Amount", boldFont, tablebody);
                        AddHeaderCell("Type", boldFont, tablebody);

                        int i = 0;
                        foreach (var item in result)
                        {
                            BaseColor _color = i % 2 == 0 ? _color2 : _color1;
                            AddColumnValues(item.TransactionId, normalFont, _color, tablebody, Element.ALIGN_LEFT);
                            AddColumnValues(item.FullName, normalFont, _color, tablebody, Element.ALIGN_LEFT);
                            AddColumnValues(item.SchoolName.ToString(), normalFont, _color, tablebody, Element.ALIGN_RIGHT);
                            AddColumnValues(item.Email.ToString(), normalFont, _color, tablebody, Element.ALIGN_RIGHT);
                            AddColumnValues(item.TransactionStatus.ToString(), normalFont, _color, tablebody, Element.ALIGN_CENTER);
                            AddColumnValues(item.TransactionDate.ToString(), normalFont, _color, tablebody, Element.ALIGN_RIGHT);
                            AddColumnValues(item.Amount.ToString(), normalFont, _color, tablebody, Element.ALIGN_RIGHT);
                            AddColumnValues(item.TransactionType.ToString(), normalFont, _color, tablebody, Element.ALIGN_RIGHT);
                            i++;
                        }
                        document.Add(headingBody);
                        document.Add(tablebody);
                    }
                    else
                    {
                        AddHeaderCell("Id", boldFont, tablebody);
                        AddHeaderCell("ParentName", boldFont, tablebody);
                        AddHeaderCell("SchoolName", boldFont, tablebody);
                        AddHeaderCell("Email", boldFont, tablebody);
                        AddHeaderCell("Status", boldFont, tablebody);
                        AddHeaderCell("Date", boldFont, tablebody);
                        AddHeaderCell("Amount", boldFont, tablebody);
                        AddHeaderCell("Type", boldFont, tablebody);

                        NoRecordRow("No Records", normalFont, new BaseColor(249, 249, 249), tablebody, Element.ALIGN_CENTER, 8);

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
            }
            else if (type == "Sponsor")
            {
                try
                {
                    //Header Title
                    List<float> titleWidth = new List<float>() { 5f };//Title
                    PdfPTable headingBody = new PdfPTable(1)
                    {
                        WidthPercentage = 110,
                        KeepTogether = false
                    };
                    headingBody.SetWidths(titleWidth.ToArray());
                    AddTitleCell((type + " List"), titleFont, headingBody);

                    //Table
                    List<float> colWidth = new List<float>() { 5f, 5f, 5f, 5f, 5f,5f,5f,5f };//Item,Amount,Date				

                    PdfPTable tablebody = new PdfPTable(8)
                    {
                        WidthPercentage = 110,
                        KeepTogether = false
                    };

                    tablebody.SetWidths(colWidth.ToArray());
                    BaseColor _color1 = new BaseColor(249, 249, 249);
                    BaseColor _color2 = new BaseColor(236, 237, 240);

                    if (result != null && result.Count() > 0)
                    {
                        AddHeaderCell("Id", boldFont, tablebody);
                        AddHeaderCell("SponsorName", boldFont, tablebody);
                        AddHeaderCell("SchoolName", boldFont, tablebody);
                        AddHeaderCell("Email", boldFont, tablebody);
                        AddHeaderCell("Status", boldFont, tablebody);
                        AddHeaderCell("Date", boldFont, tablebody);
                        AddHeaderCell("Amount", boldFont, tablebody);
                        AddHeaderCell("Type", boldFont, tablebody);

                        int i = 0;
                        foreach (var item in result)
                        {
                            BaseColor _color = i % 2 == 0 ? _color2 : _color1;
                            AddColumnValues(item.TransactionId, normalFont, _color, tablebody, Element.ALIGN_LEFT);
                            AddColumnValues(item.FullName, normalFont, _color, tablebody, Element.ALIGN_LEFT);
                            AddColumnValues(item.SchoolName.ToString(), normalFont, _color, tablebody, Element.ALIGN_RIGHT);
                            AddColumnValues(item.Email.ToString(), normalFont, _color, tablebody, Element.ALIGN_RIGHT);
                            AddColumnValues(item.TransactionStatus.ToString(), normalFont, _color, tablebody, Element.ALIGN_CENTER);
                            AddColumnValues(item.TransactionDate.ToString(), normalFont, _color, tablebody, Element.ALIGN_RIGHT);
                            AddColumnValues(item.Amount.ToString(), normalFont, _color, tablebody, Element.ALIGN_RIGHT);
                            AddColumnValues(item.TransactionType.ToString(), normalFont, _color, tablebody, Element.ALIGN_RIGHT);
                            i++;
                        }
                        document.Add(headingBody);
                        document.Add(tablebody);
                    }
                    else
                    {
                        AddHeaderCell("Id", boldFont, tablebody);
                        AddHeaderCell("SponsorName", boldFont, tablebody);
                        AddHeaderCell("SchoolName", boldFont, tablebody);
                        AddHeaderCell("Email", boldFont, tablebody);
                        AddHeaderCell("Status", boldFont, tablebody);
                        AddHeaderCell("Date", boldFont, tablebody);
                        AddHeaderCell("Amount", boldFont, tablebody);
                        AddHeaderCell("Type", boldFont, tablebody);

                        NoRecordRow("No Records", normalFont, new BaseColor(249, 249, 249), tablebody, Element.ALIGN_CENTER, 8);

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
            }
            else if (type == "School")
            {
                try
                {
                    //Header Title
                    List<float> titleWidth = new List<float>() { 5f };//Title
                    PdfPTable headingBody = new PdfPTable(1)
                    {
                        WidthPercentage = 110,
                        KeepTogether = false
                    };
                    headingBody.SetWidths(titleWidth.ToArray());
                    AddTitleCell((type + " List"), titleFont, headingBody);

                    //Table
                    List<float> colWidth = new List<float>() { 5f, 5f, 5f, 5f, 5f, 5f };//Item,Amount,Date				

                    PdfPTable tablebody = new PdfPTable(6)
                    {
                        WidthPercentage = 100,
                        KeepTogether = false
                    };

                    tablebody.SetWidths(colWidth.ToArray());
                    BaseColor _color1 = new BaseColor(249, 249, 249);
                    BaseColor _color2 = new BaseColor(236, 237, 240);

                    if (result != null && result.Count() > 0)
                    {
                        AddHeaderCell("Id", boldFont, tablebody);
                        AddHeaderCell("SchoolName", boldFont, tablebody);
                        AddHeaderCell("Email", boldFont, tablebody);
                        AddHeaderCell("Status", boldFont, tablebody);
                        AddHeaderCell("Date", boldFont, tablebody);
                        AddHeaderCell("Amount", boldFont, tablebody);

                        int i = 0;
                        foreach (var item in result)
                        {
                            BaseColor _color = i % 2 == 0 ? _color2 : _color1;
                            AddColumnValues(item.TransactionId, normalFont, _color, tablebody, Element.ALIGN_LEFT);
                            AddColumnValues(item.SchoolName.ToString(), normalFont, _color, tablebody, Element.ALIGN_RIGHT);
                            AddColumnValues(item.Email.ToString(), normalFont, _color, tablebody, Element.ALIGN_RIGHT);
                            AddColumnValues(item.TransactionStatus.ToString(), normalFont, _color, tablebody, Element.ALIGN_CENTER);
                            AddColumnValues(item.TransactionDate.ToString(), normalFont, _color, tablebody, Element.ALIGN_RIGHT);
                            AddColumnValues(item.Amount.ToString(), normalFont, _color, tablebody, Element.ALIGN_RIGHT);
                            i++;
                        }
                        document.Add(headingBody);
                        document.Add(tablebody);
                    }
                    else
                    {
                        AddHeaderCell("Id", boldFont, tablebody);
                        AddHeaderCell("SchoolName", boldFont, tablebody);
                        AddHeaderCell("Email", boldFont, tablebody);
                        AddHeaderCell("Status", boldFont, tablebody);
                        AddHeaderCell("Date", boldFont, tablebody);
                        AddHeaderCell("Amount", boldFont, tablebody);

                        NoRecordRow("No Records", normalFont, new BaseColor(249, 249, 249), tablebody, Element.ALIGN_CENTER, 6);

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
            }

            return File(stream.ToArray(), "application/pdf", filename);
        }

        [HttpGet]
        public async Task<ActionResult> RefundRequestApproveDisapproveAsync(int id, int status)
        {
            var result = await _userService.RefundRequestApproveDisapproveAsync(id, status);
            
            return Json(new { data = result }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public async Task<ActionResult> ConfirmRefundRequestAsync(int id, string RefNum)
        {
            var result = await _userService.ConfirmRefundRequestAsync(id, RefNum);

            return Json(new { data = result }, JsonRequestBehavior.AllowGet);
        }

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