using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNet.Identity;
using MyLunchMoney.Constants;
using MyLunchMoney.Models;
using MyLunchMoney.Models.DTOs;
using MyLunchMoney.Repository;
using MyLunchMoney.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;


namespace MyLunchMoney.Areas.Admin.Controllers
{
    public class MenuController : AdminBaseController
    {
        private readonly ITransactionService _tranService;
        private readonly IUserService _userService;
        private readonly IGradeRepository _grade;
        private readonly IMenuRepository _menu;

        public MenuController(IUserService userService, ITransactionService tranService, IGradeRepository grade, IMenuRepository menu)
        {
            _tranService = tranService;
            _userService = userService;
            _grade = grade;
            _menu = menu;
        }

        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<ActionResult> GetAllItemsAsync(string dayName, string categoryName, int days)
        {
            if (string.IsNullOrEmpty(categoryName) || categoryName == "Category")
            {
                categoryName = null;
            }
            var result = await _menu.GetMenuItems(dayName, SchoolId, categoryName, days);
            return Json(new { data = result }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult AddItems(string currentTab)
        {
            ViewBag.CurrentTab = currentTab;
            return PartialView("_ManageItem");
        }

        [HttpGet]
        public async Task<ActionResult> GetItemsAsync(string categoryName, string days)
        {
            if (string.IsNullOrEmpty(categoryName) || categoryName == "Category")
            {
                categoryName = null;
            }
            var result = await _menu.ItemListbySchoolId(SchoolId, categoryName, Convert.ToInt32(days));
            return Json(new { data = result }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<ActionResult> AddTransactionAsync(ManageMenuItemModel model)
        {
            if (SchoolId != null)
            {
                if (model.Items != null)
                {
                    string ids = string.Join(",", model.Items.Select(x => x.value));
                    string schoolId = SchoolId;
                    var result = await _menu.ManageMenuItems(ids, schoolId, model.Day);
                    return Json(new { data = result }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var result = new Dictionary<string, string>
                    {
                        { "code", "400"},
                        { "message", MessageCode.MenuItemSelect}
                    };
                    return Json(new { data = result }, JsonRequestBehavior.AllowGet);
                }
                
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

        [HttpPost]
        public async Task<ActionResult> DeleteAsync(int id)
        {
            var result = await _menu.DeleteMenuItemDetailAsync(id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public async Task<ActionResult> ExportMenuListAsync(string dayName, string categoryName, int days)
        {
            string filename = string.Format("{0}-{1}.{2}", dayName, DateTime.Now.ToString("dd-MM-yyyy-hh:mm:ss"), "pdf");
            if (string.IsNullOrEmpty(categoryName) || categoryName == "Category")
            {
                categoryName = null;
            }
            IEnumerable<MenuItemDetailDTO> result = await _menu.GetMenuItems(dayName, SchoolId, categoryName, days);

            Font titleFont = new Font(Font.FontFamily.HELVETICA, 14, Font.BOLD, BaseColor.BLACK);
            Font normalFont = new Font(Font.FontFamily.HELVETICA, 9, Font.NORMAL, BaseColor.BLACK);
            Font boldFont = new Font(Font.FontFamily.HELVETICA, 10, Font.BOLD, BaseColor.WHITE);

            #region Initialize PDF File
            MemoryStream stream = new MemoryStream();
            Document document = new Document(PageSize.A4, 25f, 25f, 25f, 25f);
            PdfWriter.GetInstance(document, stream);

            document.Open();
            #endregion
            if (dayName == "Monday")
            {
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
                    AddTitleCell((dayName + " List"), titleFont, headingBody);

                    //Table
                    List<float> colWidth = new List<float>() { 5f, 5f, 5f };//Item,Amount,Date				

                    PdfPTable tablebody = new PdfPTable(3)
                    {
                        WidthPercentage = 100,
                        KeepTogether = false
                    };

                    tablebody.SetWidths(colWidth.ToArray());
                    BaseColor _color1 = new BaseColor(249, 249, 249);
                    BaseColor _color2 = new BaseColor(236, 237, 240);

                    if (result != null && result.Count() > 0)
                    {
                        AddHeaderCell("Item Name", boldFont, tablebody);
                        AddHeaderCell("Price", boldFont, tablebody);
                        AddHeaderCell("Category", boldFont, tablebody);

                        int i = 0;
                        foreach (var item in result)
                        {
                            BaseColor _color = i % 2 == 0 ? _color2 : _color1;
                            AddColumnValues(item.ItemName, normalFont, _color, tablebody, Element.ALIGN_CENTER);
                            AddColumnValues(item.ItemPrice.ToString(), normalFont, _color, tablebody, Element.ALIGN_CENTER);
                            AddColumnValues(item.CategoryName.ToString(), normalFont, _color, tablebody, Element.ALIGN_CENTER);
                            i++;
                        }
                        document.Add(headingBody);
                        document.Add(tablebody);
                    }
                    else
                    {

                        AddHeaderCell("Item Name", boldFont, tablebody);
                        AddHeaderCell("Price", boldFont, tablebody);
                        AddHeaderCell("Category", boldFont, tablebody);

                        NoRecordRow("No Records", normalFont, new BaseColor(249, 249, 249), tablebody, Element.ALIGN_CENTER, 3);

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
            else if(dayName == "Tuesday")
            {
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
                    AddTitleCell((dayName + " List"), titleFont, headingBody);

                    //Table
                    List<float> colWidth = new List<float>() { 5f, 5f, 5f };//Item,Amount,Date				

                    PdfPTable tablebody = new PdfPTable(3)
                    {
                        WidthPercentage = 100,
                        KeepTogether = false
                    };

                    tablebody.SetWidths(colWidth.ToArray());
                    BaseColor _color1 = new BaseColor(249, 249, 249);
                    BaseColor _color2 = new BaseColor(236, 237, 240);

                    if (result != null && result.Count() > 0)
                    {
                        AddHeaderCell("Item Name", boldFont, tablebody);
                        AddHeaderCell("Price", boldFont, tablebody);
                        AddHeaderCell("Category", boldFont, tablebody);

                        int i = 0;
                        foreach (var item in result)
                        {
                            BaseColor _color = i % 2 == 0 ? _color2 : _color1;
                            AddColumnValues(item.ItemName, normalFont, _color, tablebody, Element.ALIGN_CENTER);
                            AddColumnValues(item.ItemPrice.ToString(), normalFont, _color, tablebody, Element.ALIGN_CENTER);
                            AddColumnValues(item.CategoryName.ToString(), normalFont, _color, tablebody, Element.ALIGN_CENTER);
                            i++;
                        }
                        document.Add(headingBody);
                        document.Add(tablebody);
                    }
                    else
                    {

                        AddHeaderCell("Item Name", boldFont, tablebody);
                        AddHeaderCell("Price", boldFont, tablebody);
                        AddHeaderCell("Category", boldFont, tablebody);

                        NoRecordRow("No Records", normalFont, new BaseColor(249, 249, 249), tablebody, Element.ALIGN_CENTER, 3);

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
            else if (dayName == "Wednesday")
            {
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
                    AddTitleCell((dayName + " List"), titleFont, headingBody);

                    //Table
                    List<float> colWidth = new List<float>() { 5f, 5f, 5f };//Item,Amount,Date				

                    PdfPTable tablebody = new PdfPTable(3)
                    {
                        WidthPercentage = 100,
                        KeepTogether = false
                    };

                    tablebody.SetWidths(colWidth.ToArray());
                    BaseColor _color1 = new BaseColor(249, 249, 249);
                    BaseColor _color2 = new BaseColor(236, 237, 240);

                    if (result != null && result.Count() > 0)
                    {
                        AddHeaderCell("Item Name", boldFont, tablebody);
                        AddHeaderCell("Price", boldFont, tablebody);
                        AddHeaderCell("Category", boldFont, tablebody);

                        int i = 0;
                        foreach (var item in result)
                        {
                            BaseColor _color = i % 2 == 0 ? _color2 : _color1;
                            AddColumnValues(item.ItemName, normalFont, _color, tablebody, Element.ALIGN_CENTER);
                            AddColumnValues(item.ItemPrice.ToString(), normalFont, _color, tablebody, Element.ALIGN_CENTER);
                            AddColumnValues(item.CategoryName.ToString(), normalFont, _color, tablebody, Element.ALIGN_CENTER);
                            i++;
                        }
                        document.Add(headingBody);
                        document.Add(tablebody);
                    }
                    else
                    {

                        AddHeaderCell("Item Name", boldFont, tablebody);
                        AddHeaderCell("Price", boldFont, tablebody);
                        AddHeaderCell("Category", boldFont, tablebody);

                        NoRecordRow("No Records", normalFont, new BaseColor(249, 249, 249), tablebody, Element.ALIGN_CENTER, 3);

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
            else if (dayName == "Thursday")
            {
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
                    AddTitleCell((dayName + " List"), titleFont, headingBody);

                    //Table
                    List<float> colWidth = new List<float>() { 5f, 5f, 5f };//Item,Amount,Date				

                    PdfPTable tablebody = new PdfPTable(3)
                    {
                        WidthPercentage = 100,
                        KeepTogether = false
                    };

                    tablebody.SetWidths(colWidth.ToArray());
                    BaseColor _color1 = new BaseColor(249, 249, 249);
                    BaseColor _color2 = new BaseColor(236, 237, 240);

                    if (result != null && result.Count() > 0)
                    {
                        AddHeaderCell("Item Name", boldFont, tablebody);
                        AddHeaderCell("Price", boldFont, tablebody);
                        AddHeaderCell("Category", boldFont, tablebody);

                        int i = 0;
                        foreach (var item in result)
                        {
                            BaseColor _color = i % 2 == 0 ? _color2 : _color1;
                            AddColumnValues(item.ItemName, normalFont, _color, tablebody, Element.ALIGN_CENTER);
                            AddColumnValues(item.ItemPrice.ToString(), normalFont, _color, tablebody, Element.ALIGN_CENTER);
                            AddColumnValues(item.CategoryName.ToString(), normalFont, _color, tablebody, Element.ALIGN_CENTER);
                            i++;
                        }
                        document.Add(headingBody);
                        document.Add(tablebody);
                    }
                    else
                    {

                        AddHeaderCell("Item Name", boldFont, tablebody);
                        AddHeaderCell("Price", boldFont, tablebody);
                        AddHeaderCell("Category", boldFont, tablebody);

                        NoRecordRow("No Records", normalFont, new BaseColor(249, 249, 249), tablebody, Element.ALIGN_CENTER, 3);

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
            else if (dayName == "Friday")
            {
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
                    AddTitleCell((dayName + " List"), titleFont, headingBody);

                    //Table
                    List<float> colWidth = new List<float>() { 5f, 5f, 5f };//Item,Amount,Date				

                    PdfPTable tablebody = new PdfPTable(3)
                    {
                        WidthPercentage = 100,
                        KeepTogether = false
                    };

                    tablebody.SetWidths(colWidth.ToArray());
                    BaseColor _color1 = new BaseColor(249, 249, 249);
                    BaseColor _color2 = new BaseColor(236, 237, 240);

                    if (result != null && result.Count() > 0)
                    {
                        AddHeaderCell("Item Name", boldFont, tablebody);
                        AddHeaderCell("Price", boldFont, tablebody);
                        AddHeaderCell("Category", boldFont, tablebody);

                        int i = 0;
                        foreach (var item in result)
                        {
                            BaseColor _color = i % 2 == 0 ? _color2 : _color1;
                            AddColumnValues(item.ItemName, normalFont, _color, tablebody, Element.ALIGN_CENTER);
                            AddColumnValues(item.ItemPrice.ToString(), normalFont, _color, tablebody, Element.ALIGN_CENTER);
                            AddColumnValues(item.CategoryName.ToString(), normalFont, _color, tablebody, Element.ALIGN_CENTER);
                            i++;
                        }
                        document.Add(headingBody);
                        document.Add(tablebody);
                    }
                    else
                    {

                        AddHeaderCell("Item Name", boldFont, tablebody);
                        AddHeaderCell("Price", boldFont, tablebody);
                        AddHeaderCell("Category", boldFont, tablebody);

                        NoRecordRow("No Records", normalFont, new BaseColor(249, 249, 249), tablebody, Element.ALIGN_CENTER, 3);

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
            else if (dayName == "Saturday")
            {
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
                    AddTitleCell((dayName + " List"), titleFont, headingBody);

                    //Table
                    List<float> colWidth = new List<float>() { 5f, 5f, 5f };//Item,Amount,Date				

                    PdfPTable tablebody = new PdfPTable(3)
                    {
                        WidthPercentage = 100,
                        KeepTogether = false
                    };

                    tablebody.SetWidths(colWidth.ToArray());
                    BaseColor _color1 = new BaseColor(249, 249, 249);
                    BaseColor _color2 = new BaseColor(236, 237, 240);

                    if (result != null && result.Count() > 0)
                    {
                        AddHeaderCell("Item Name", boldFont, tablebody);
                        AddHeaderCell("Price", boldFont, tablebody);
                        AddHeaderCell("Category", boldFont, tablebody);

                        int i = 0;
                        foreach (var item in result)
                        {
                            BaseColor _color = i % 2 == 0 ? _color2 : _color1;
                            AddColumnValues(item.ItemName, normalFont, _color, tablebody, Element.ALIGN_CENTER);
                            AddColumnValues(item.ItemPrice.ToString(), normalFont, _color, tablebody, Element.ALIGN_CENTER);
                            AddColumnValues(item.CategoryName.ToString(), normalFont, _color, tablebody, Element.ALIGN_CENTER);
                            i++;
                        }
                        document.Add(headingBody);
                        document.Add(tablebody);
                    }
                    else
                    {

                        AddHeaderCell("Item Name", boldFont, tablebody);
                        AddHeaderCell("Price", boldFont, tablebody);
                        AddHeaderCell("Category", boldFont, tablebody);

                        NoRecordRow("No Records", normalFont, new BaseColor(249, 249, 249), tablebody, Element.ALIGN_CENTER, 3);

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
            else if (dayName == "Sunday")
            {
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
                    AddTitleCell((dayName + " List"), titleFont, headingBody);

                    //Table
                    List<float> colWidth = new List<float>() { 5f, 5f, 5f };//Item,Amount,Date				

                    PdfPTable tablebody = new PdfPTable(3)
                    {
                        WidthPercentage = 100,
                        KeepTogether = false
                    };

                    tablebody.SetWidths(colWidth.ToArray());
                    BaseColor _color1 = new BaseColor(249, 249, 249);
                    BaseColor _color2 = new BaseColor(236, 237, 240);

                    if (result != null && result.Count() > 0)
                    {
                        AddHeaderCell("Item Name", boldFont, tablebody);
                        AddHeaderCell("Price", boldFont, tablebody);
                        AddHeaderCell("Category", boldFont, tablebody);

                        int i = 0;
                        foreach (var item in result)
                        {
                            BaseColor _color = i % 2 == 0 ? _color2 : _color1;
                            AddColumnValues(item.ItemName, normalFont, _color, tablebody, Element.ALIGN_CENTER);
                            AddColumnValues(item.ItemPrice.ToString(), normalFont, _color, tablebody, Element.ALIGN_CENTER);
                            AddColumnValues(item.CategoryName.ToString(), normalFont, _color, tablebody, Element.ALIGN_CENTER);
                            i++;
                        }
                        document.Add(headingBody);
                        document.Add(tablebody);
                    }
                    else
                    {

                        AddHeaderCell("Item Name", boldFont, tablebody);
                        AddHeaderCell("Price", boldFont, tablebody);
                        AddHeaderCell("Category", boldFont, tablebody);

                        NoRecordRow("No Records", normalFont, new BaseColor(249, 249, 249), tablebody, Element.ALIGN_CENTER, 3);

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