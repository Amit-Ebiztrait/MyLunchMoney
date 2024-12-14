using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNet.Identity;
using MyLunchMoney.Constants;
using MyLunchMoney.Helpers;
using MyLunchMoney.Hubs;
using MyLunchMoney.Models;
using MyLunchMoney.Repository;
using MyLunchMoney.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace MyLunchMoney.Areas.Admin.Controllers
{
    public class TransactionsController : AdminBaseController
    {
        #region Declaration
        private readonly ITransactionService _tranService;
        private readonly IUserService _userService;
        private readonly ISchoolService _schoolService;
        private readonly INotificationService _notificationservice;

        #endregion

        #region Constructor
        public TransactionsController(IUserService userService, ITransactionService tranService, ISchoolService schoolService, INotificationService notificationservice)
        {
            _tranService = tranService;
            _userService = userService;
            _schoolService = schoolService;
            _notificationservice = notificationservice;
        }
        #endregion

        #region Get Methods
        public ActionResult Index()
        {
            return View();
        }
        #endregion

        #region Post Mothods
        [HttpGet]
        public async Task<ActionResult> GetTransactionListAsync(string type, int days, string schoolIds)
        {
            if(schoolIds != "" && schoolIds != null && schoolIds != "undefined")
			{
                Session["SchoolId"] = schoolIds;
            }
            var result = await _tranService.GetManageTransactionListAsync(SchoolId, type, days);

            return Json(new { data = result }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public async Task<ActionResult> GetSchoolDetailAllocateAsync()
        {
            var result = await _schoolService.GetSchoolAllocationAsync(SchoolId);

            return Json(new { data = result.data }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public async Task<ActionResult> ExportManageTransactionListAsync(string type, int days)
        {
            string filename = string.Format("{0}-{1}.{2}", type, DateTime.Now.ToString("dd-MM-yyyy-hh:mm:ss"), "pdf");

            IEnumerable<ManageTransactionViewModel> result = await _tranService.GetManageTransactionListAsync(SchoolId, type, days);

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
                    List<float> colWidth = new List<float>() { 5f, 5f, 5f, 5f, 5f, 5f, 7f };//Item,Amount,Date				

                    PdfPTable tablebody = new PdfPTable(7)
                    {
                        WidthPercentage = 110,
                        KeepTogether = false
                    };

                    tablebody.SetWidths(colWidth.ToArray());
                    BaseColor _color1 = new BaseColor(249, 249, 249);
                    BaseColor _color2 = new BaseColor(236, 237, 240);

                    if (result != null && result.Count() > 0)
                    {
                        AddHeaderCell("Transaction History Id", boldFont, tablebody);
                        AddHeaderCell("Sender Name", boldFont, tablebody);
                        AddHeaderCell("CreatedOn", boldFont, tablebody);
                        AddHeaderCell("Amount", boldFont, tablebody);
                        AddHeaderCell("Email", boldFont, tablebody);
                        AddHeaderCell("MLMID", boldFont, tablebody);
                        AddHeaderCell("Student Name", boldFont, tablebody);

                        int i = 0;
                        foreach (var item in result)
                        {
                            BaseColor _color = i % 2 == 0 ? _color2 : _color1;
                            AddColumnValues(item.TransactionHistoryId, normalFont, _color, tablebody, Element.ALIGN_LEFT);
                            AddColumnValues(item.SenderName, normalFont, _color, tablebody, Element.ALIGN_LEFT);
                            AddColumnValues(item.CreatedOn.ToString(), normalFont, _color, tablebody, Element.ALIGN_RIGHT);
                            AddColumnValues(item.TotalAmount.ToString(), normalFont, _color, tablebody, Element.ALIGN_RIGHT);
                            AddColumnValues(item.Email.ToString(), normalFont, _color, tablebody, Element.ALIGN_CENTER);
                            AddColumnValues(item.MLMID.ToString(), normalFont, _color, tablebody, Element.ALIGN_RIGHT);
                            AddColumnValues(item.StudentName.ToString(), normalFont, _color, tablebody, Element.ALIGN_RIGHT);
                            i++;
                        }
                        document.Add(headingBody);
                        document.Add(tablebody);
                    }
                    else
                    {
                        AddHeaderCell("Transaction History Id", boldFont, tablebody);
                        AddHeaderCell("Sender Name", boldFont, tablebody);
                        AddHeaderCell("CreatedOn", boldFont, tablebody);
                        AddHeaderCell("Amount", boldFont, tablebody);
                        AddHeaderCell("Email", boldFont, tablebody);
                        AddHeaderCell("MLMID", boldFont, tablebody);
                        AddHeaderCell("Student Name", boldFont, tablebody);

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
                    List<float> colWidth = new List<float>() { 5f, 5f, 5f, 5f, 5f };//Item,Amount,Date				

                    PdfPTable tablebody = new PdfPTable(5)
                    {
                        WidthPercentage = 110,
                        KeepTogether = false
                    };

                    tablebody.SetWidths(colWidth.ToArray());
                    BaseColor _color1 = new BaseColor(249, 249, 249);
                    BaseColor _color2 = new BaseColor(236, 237, 240);

                    if (result != null && result.Count() > 0)
                    {
                        AddHeaderCell("TransactionHistory Id", boldFont, tablebody);
                        AddHeaderCell("CreatedOn", boldFont, tablebody);
                        AddHeaderCell("Amount", boldFont, tablebody);
                        AddHeaderCell("SenderName", boldFont, tablebody);
                        AddHeaderCell("Email", boldFont, tablebody);

                        int i = 0;
                        foreach (var item in result)
                        {
                            BaseColor _color = i % 2 == 0 ? _color2 : _color1;
                            AddColumnValues(item.TransactionHistoryId, normalFont, _color, tablebody, Element.ALIGN_LEFT);
                            AddColumnValues(item.CreatedOn.ToString(), normalFont, _color, tablebody, Element.ALIGN_LEFT);
                            AddColumnValues(item.TotalAmount.ToString(), normalFont, _color, tablebody, Element.ALIGN_CENTER);
                            AddColumnValues(item.SenderName, normalFont, _color, tablebody, Element.ALIGN_RIGHT);
                            AddColumnValues(item.Email, normalFont, _color, tablebody, Element.ALIGN_CENTER);
                            i++;
                        }
                        document.Add(headingBody);
                        document.Add(tablebody);
                    }
                    else
                    {
                        AddHeaderCell("TransactionHistory Id", boldFont, tablebody);
                        AddHeaderCell("CreatedOn", boldFont, tablebody);
                        AddHeaderCell("Amount", boldFont, tablebody);
                        AddHeaderCell("SenderName", boldFont, tablebody);
                        AddHeaderCell("Email", boldFont, tablebody);

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
            }
            else if (type == "Reversal")
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
                    List<float> colWidth = new List<float>() { 5f, 5f, 5f, 5f, 5f,5f };//Item,Amount,Date				

                    PdfPTable tablebody = new PdfPTable(6)
                    {
                        WidthPercentage = 110,
                        KeepTogether = false
                    };

                    tablebody.SetWidths(colWidth.ToArray());
                    BaseColor _color1 = new BaseColor(249, 249, 249);
                    BaseColor _color2 = new BaseColor(236, 237, 240);

                    if (result != null && result.Count() > 0)
                    {
                        AddHeaderCell("TransactionHistory Id", boldFont, tablebody);
                        AddHeaderCell("CreatedOn", boldFont, tablebody);
                        AddHeaderCell("Amount", boldFont, tablebody);
                        AddHeaderCell("MLM ID", boldFont, tablebody);
                        AddHeaderCell("Student Name", boldFont, tablebody);
                        AddHeaderCell("Staff Name", boldFont, tablebody);

                        int i = 0;
                        foreach (var item in result)
                        {
                            BaseColor _color = i % 2 == 0 ? _color2 : _color1;
                            AddColumnValues(item.TransactionHistoryId, normalFont, _color, tablebody, Element.ALIGN_LEFT);
                            AddColumnValues(item.CreatedOn.ToString(), normalFont, _color, tablebody, Element.ALIGN_LEFT);
                            AddColumnValues(item.TotalAmount.ToString(), normalFont, _color, tablebody, Element.ALIGN_CENTER);
                            AddColumnValues(item.MLMID, normalFont, _color, tablebody, Element.ALIGN_RIGHT);
                            AddColumnValues(item.StudentName, normalFont, _color, tablebody, Element.ALIGN_RIGHT);
                            AddColumnValues(item.StaffName, normalFont, _color, tablebody, Element.ALIGN_CENTER);
                            i++;
                        }
                        document.Add(headingBody);
                        document.Add(tablebody);
                    }
                    else
                    {
                        AddHeaderCell("TransactionHistory Id", boldFont, tablebody);
                        AddHeaderCell("CreatedOn", boldFont, tablebody);
                        AddHeaderCell("Amount", boldFont, tablebody);
                        AddHeaderCell("MLM ID", boldFont, tablebody);
                        AddHeaderCell("Student Name", boldFont, tablebody);
                        AddHeaderCell("Staff Name", boldFont, tablebody);

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
            else if (type == "Refund")
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
                    List<float> colWidth = new List<float>() { 5f, 5f, 5f, 5f, 5f };//Item,Amount,Date				

                    PdfPTable tablebody = new PdfPTable(5)
                    {
                        WidthPercentage = 110,
                        KeepTogether = false
                    };

                    tablebody.SetWidths(colWidth.ToArray());
                    BaseColor _color1 = new BaseColor(249, 249, 249);
                    BaseColor _color2 = new BaseColor(236, 237, 240);

                    if (result != null && result.Count() > 0)
                    {
                        AddHeaderCell("TransactionHistory Id", boldFont, tablebody);
                        AddHeaderCell("CreatedOn", boldFont, tablebody);
                        AddHeaderCell("Amount", boldFont, tablebody);
                        AddHeaderCell("MLM ID", boldFont, tablebody);
                        AddHeaderCell("Student Name", boldFont, tablebody);

                        int i = 0;
                        foreach (var item in result)
                        {
                            BaseColor _color = i % 2 == 0 ? _color2 : _color1;
                            AddColumnValues(item.TransactionHistoryId, normalFont, _color, tablebody, Element.ALIGN_LEFT);
                            AddColumnValues(item.CreatedOn.ToString(), normalFont, _color, tablebody, Element.ALIGN_LEFT);
                            AddColumnValues(item.TotalAmount.ToString(), normalFont, _color, tablebody, Element.ALIGN_CENTER);
                            AddColumnValues(item.MLMID, normalFont, _color, tablebody, Element.ALIGN_RIGHT);
                            AddColumnValues(item.StudentName, normalFont, _color, tablebody, Element.ALIGN_RIGHT);
                            i++;
                        }
                        document.Add(headingBody);
                        document.Add(tablebody);
                    }
                    else
                    {
                        AddHeaderCell("TransactionHistory Id", boldFont, tablebody);
                        AddHeaderCell("CreatedOn", boldFont, tablebody);
                        AddHeaderCell("Amount", boldFont, tablebody);
                        AddHeaderCell("MLM ID", boldFont, tablebody);
                        AddHeaderCell("Student Name", boldFont, tablebody);

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
            }

            return File(stream.ToArray(), "application/pdf", filename);
        }

        [HttpPost]
        public async Task<ActionResult> AddAmountAsync(string studentUserId, string transactionId, string amount)
        {
            var result = await _tranService.AddAmountAsync(studentUserId, transactionId, Convert.ToInt32(amount));
            if(result.code == HttpStatusCode.OK)
            {
                var notificationModel = new AddNotificationRequest
                {

                    CurrentUserId = CurrentUserId,
                    SchoolId = SchoolId,
                    Subject = "Sponser Amount",
                    Message = MessageCode.AllocatedAmountSuccess
                };
                var addnotification = await _notificationservice.AddNotifications("SA", notificationModel);
                MessagesHub objHub = new MessagesHub();
                objHub.GetNotification();
            }
            return Json(new { data = result }, JsonRequestBehavior.AllowGet);

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