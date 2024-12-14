using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNet.Identity;
using MyLunchMoney.Constants;
using MyLunchMoney.Hubs;
using MyLunchMoney.Models;
using MyLunchMoney.Models.DTOs;
using MyLunchMoney.Services;
using Newtonsoft.Json;

namespace MyLunchMoney.Areas.Admin.Controllers
{
	public class ItemController : AdminBaseController
    {
        private readonly IItemService _itemService;
        private readonly ICategoryService _categoryService;
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notification;

        public ItemController(IItemService itemService, ApplicationDbContext context, ICategoryService categoryService, INotificationService notification)
        {
            _context = context;
            _itemService = itemService;
            _categoryService = categoryService;
            _notification = notification;
        }
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<ActionResult> GetAllItemsAsync(int? cat, int days)
        {
            var result = await _itemService.GetItemDetailAsync(SchoolId, cat, days);

            return Json(new { data = result }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public async Task<ActionResult> GetAllCategoryBySchoolIdAsync()
        {
            var result = await _categoryService.GetAllCategoryBySchoolIdAsync(SchoolId);

            return Json(new { data = result.data }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<ActionResult> SaveAsync()
        {
            try
            {
                var model = JsonConvert.DeserializeObject<ItemDetailViewModel>(HttpContext.Request.Form["model"]);
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
                if (model.ItemId > 0)
                {
                    var result = await _itemService.UpdateItemDetailAsync(model, fileObj);
                    if(result!= null)
                    {
                        NotificationRequest notificationrequest = new NotificationRequest
                        {
                            UserId = SchoolAdminId,
                            SenderId = CurrentUserId,
                            SchoolId = SchoolId,
                            Subject = MessageCode.ItemUpdated,
                            Message = MessageCode.ItemUpdated,
                            IsRead = 0,
                            CreatedBy = CurrentUserId
                        };
                        await _notification.AddNotificationAsync(notificationrequest);

                        MessagesHub objmessagesHub = new MessagesHub();
                        objmessagesHub.GetNotification();
                    }
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var result = await _itemService.AddItemDetailAsync(model, fileObj);
                    if(result != null)
                    {

                        NotificationRequest notificationrequest = new NotificationRequest
                        {
                            UserId = SchoolAdminId,
                            SenderId = CurrentUserId,
                            SchoolId = SchoolId,
                            Subject = MessageCode.ItemAdded,
                            Message = MessageCode.ItemAdded,
                            IsRead = 0,
                            CreatedBy = CurrentUserId
                        };
                        await _notification.AddNotificationAsync(notificationrequest);
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
        public async Task<ActionResult> GetDetailById(int? id)
        {
            try
            {
                var result = await _itemService.GetItemDetailByIdAsync(id);
                return Json(new { data = result.data }, JsonRequestBehavior.AllowGet);
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
            var result = await _itemService.DeleteItemDetailAsync(id,userid);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetCategoryList()
        {
            var categories = _context.Categories.Where(x => x.SchoolId == SchoolId)
                .Where(t => t.IsActive == 1 && t.IsDelete == 0)
                .Select(x => new
                {
                    CategoryId = x.CategoryId,
                    CategoryName = x.CategoryName
                }).ToList();

            return Json(categories, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<JsonResult> ItemDelete(IEnumerable<int> itemids)
        {
            var result = await _itemService.MultipleDeleteAsync(itemids);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public async Task<ActionResult> ExportManageItemListAsync(int? cat, int days)
        {
            string filename = string.Format("{0}-{1}.{2}", cat, DateTime.Now.ToString("dd-MM-yyyy-hh:mm:ss"), "pdf");

            IEnumerable<ItemDetailDTO> result = await _itemService.GetItemDetailAsync(SchoolId, cat, days);

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
                AddTitleCell((cat + " List"), titleFont, headingBody);

                //Table
                List<float> colWidth = new List<float>() { 5f, 5f, 5f, 5f };//Item,Amount,Date				

                PdfPTable tablebody = new PdfPTable(4)
                {
                    WidthPercentage = 100,
                    KeepTogether = false
                };

                tablebody.SetWidths(colWidth.ToArray());
                BaseColor _color1 = new BaseColor(249, 249, 249);
                BaseColor _color2 = new BaseColor(236, 237, 240);

                if (result != null && result.Count() > 0)
                {
                    AddHeaderCell("ItemName", boldFont, tablebody);
                    AddHeaderCell("ItemPrice", boldFont, tablebody);
                    AddHeaderCell("CategoryName", boldFont, tablebody);
                    AddHeaderCell("CreatedOn", boldFont, tablebody);

                    int i = 0;
                    foreach (var item in result)
                    {
                        BaseColor _color = i % 2 == 0 ? _color2 : _color1;
                        AddColumnValues(item.ItemName, normalFont, _color, tablebody, Element.ALIGN_LEFT);
                        AddColumnValues(item.ItemPrice.ToString(), normalFont, _color, tablebody, Element.ALIGN_LEFT);
                        AddColumnValues(item.CategoryName.ToString(), normalFont, _color, tablebody, Element.ALIGN_RIGHT);
                        AddColumnValues(item.CreatedOn.ToString(), normalFont, _color, tablebody, Element.ALIGN_CENTER);
                        i++;
                    }
                    document.Add(headingBody);
                    document.Add(tablebody);
                }
                else
                {
                    AddHeaderCell("ItemName", boldFont, tablebody);
                    AddHeaderCell("ItemPrice", boldFont, tablebody);
                    AddHeaderCell("CategoryName", boldFont, tablebody);
                    AddHeaderCell("CreatedOn", boldFont, tablebody);

                    NoRecordRow("No Records", normalFont, new BaseColor(249, 249, 249), tablebody, Element.ALIGN_CENTER, 4);

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