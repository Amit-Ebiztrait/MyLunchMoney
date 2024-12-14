using iTextSharp.text.pdf;
using iTextSharp.text;
using MyLunchMoney.Models;
using MyLunchMoney.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using MyLunchMoney.Models.DTOs;

namespace MyLunchMoney.ApiControllers
{
	[RoutePrefix("api")]
	public class ExportPdfController : ApiController
    {
		#region Declaration
		private readonly ITransactionService _transactionService;
		private readonly IUserService _userService;
		#endregion

		#region Constructor
		public ExportPdfController(ITransactionService transactionService, IUserService userService)
		{
			_transactionService = transactionService;
			_userService = userService;
		}
		#endregion

		#region Post Mothods
		[HttpGet]
		[AllowAnonymous]
		[Route("ExportCafeteriaHistoryPDF")]
		public async Task<HttpResponseMessage> ExportCafeteriaHistoryPDF(string studentId,int days)
		{
			var startDate = DateTime.Now.AddDays(-days);
			var endDate = DateTime.Now;

			HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest);
			string filename = string.Format("{0}-{1}.{2}", "CafeteriaHistory", DateTime.Now.ToString("dd-MM-yyyy-hh:mm:ss"), "pdf");

			//Student Details
			var userDetail = await _userService.GetStudentDetailByIdAsync(studentId);
			StudentDetailDTO student = userDetail.data;

			//Cafeteria History
			var result = await _transactionService.GetCafeteriaHistoryPDF(studentId, startDate, endDate);
			List<StudentCafeteriaTransactionDTO> historyList = result.data;

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
				AddTitleCell("Cafeteria History", titleFont, headingBody);

                //Student Detail

                List<float> dWidth = new List<float>() { 5f, 5f };//Title
                PdfPTable detailBody = new PdfPTable(2)
                {
                    WidthPercentage = 100,
                    KeepTogether = false
                };
                detailBody.SetWidths(dWidth.ToArray());
                AddColumnValues("MLMID", normalFont, new BaseColor(249, 249, 249), detailBody, Element.ALIGN_LEFT);
                AddColumnValues(student.MLMID, normalFont, new BaseColor(249, 249, 249), detailBody, Element.ALIGN_LEFT);
                AddColumnValues("Student Name", normalFont, new BaseColor(249, 249, 249), detailBody, Element.ALIGN_LEFT);
                AddColumnValues(student.StudentName, normalFont, new BaseColor(249, 249, 249), detailBody, Element.ALIGN_LEFT);
                AddColumnValues("School Name", normalFont, new BaseColor(249, 249, 249), detailBody, Element.ALIGN_LEFT);
                AddColumnValues(student.SchoolName, normalFont, new BaseColor(249, 249, 249), detailBody, Element.ALIGN_LEFT);

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

				if (historyList != null && historyList.Count > 0)
				{
					AddHeaderCell("Date", boldFont, tablebody);
					AddHeaderCell("Item Name", boldFont, tablebody);
					AddHeaderCell("Quantity", boldFont, tablebody);
					AddHeaderCell("Unit Price", boldFont, tablebody);
					AddHeaderCell("Total Price", boldFont, tablebody);					

					int i = 0;
					foreach (var item in historyList)
					{
						BaseColor _color = i % 2 == 0 ? _color2 : _color1;
						AddColumnValues(item.CreatedOn.ToString(), normalFont, _color, tablebody, Element.ALIGN_LEFT);
						AddColumnValues(item.ItemName, normalFont, _color, tablebody, Element.ALIGN_LEFT);
						AddColumnValues(item.Quantity.ToString(), normalFont, _color, tablebody, Element.ALIGN_RIGHT);
						AddColumnValues(item.UnitPrice.ToString(), normalFont, _color, tablebody, Element.ALIGN_RIGHT);
						AddColumnValues(item.TotalAmount.ToString(), normalFont, _color, tablebody, Element.ALIGN_RIGHT);						
						i++;
					}
					document.Add(headingBody);
					document.Add(detailBody);
					document.Add(tablebody);
				}
				else
				{
					AddHeaderCell("Date", boldFont, tablebody);
					AddHeaderCell("Item Name", boldFont, tablebody);
					AddHeaderCell("Quantity", boldFont, tablebody);
					AddHeaderCell("Unit Price", boldFont, tablebody);
					AddHeaderCell("Total Price", boldFont, tablebody);

					NoRecordRow("No Records", normalFont, new BaseColor(249, 249, 249), tablebody, Element.ALIGN_CENTER, 5);

					document.Add(headingBody);
				    document.Add(detailBody);
					document.Add(tablebody);
				}
			}
			catch (Exception ex)
			{
				return Request.CreateResponse(HttpStatusCode.InternalServerError,HttpStatusCode.InternalServerError,ex.Message); ;
			}
			finally
			{
				document.Close();
			}

			byte[] buffer = stream.ToArray();
			response = Request.CreateResponse(HttpStatusCode.OK);
			response.Content = new StreamContent(new MemoryStream(buffer));
			response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
			response.Content.Headers.ContentLength = buffer.Length;
			if (ContentDispositionHeaderValue.TryParse("inline; filename=" + filename, out ContentDispositionHeaderValue contentDisposition))
			{
				response.Content.Headers.ContentDisposition = contentDisposition;
			}

			return response;
		}

        [HttpGet]
        [AllowAnonymous]
        [Route("ExportTransactionPDF")]
        public async Task<HttpResponseMessage> ExportTransactionPDF(string studentId, int days)
        {
            var startDate = DateTime.Now.AddDays(-days);
            var endDate = DateTime.Now;

            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest);
            string filename = string.Format("{0}-{1}.{2}", "TransactionHistory", DateTime.Now.ToString("dd-MM-yyyy-hh:mm:ss"), "pdf");

            //Student Details
            //var userDetail = await _transactionService.GetTransactionListAsync(studentId, startDate, endDate);
            //StudentTransactionDTO student = userDetail.data;

            //Student Details
            var userDetail = await _userService.GetStudentDetailByIdAsync(studentId);
            StudentDetailDTO student = userDetail.data;

            //Cafeteria History
            var result = await _transactionService.GetTransactionPDF(studentId, startDate, endDate);
            List<StudentTransactionDTO> historyList = result.data;

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
                AddTitleCell("Transaction History", titleFont, headingBody);

                //Student Detail

                List<float> dWidth = new List<float>() { 5f, 5f };//Title
                PdfPTable detailBody = new PdfPTable(2)
                {
                    WidthPercentage = 100,
                    KeepTogether = false
                };
                detailBody.SetWidths(dWidth.ToArray());
                AddColumnValues("MLMID", normalFont, new BaseColor(249, 249, 249), detailBody, Element.ALIGN_LEFT);
                AddColumnValues(student.MLMID, normalFont, new BaseColor(249, 249, 249), detailBody, Element.ALIGN_LEFT);
                AddColumnValues("Student Name", normalFont, new BaseColor(249, 249, 249), detailBody, Element.ALIGN_LEFT);
                AddColumnValues(student.StudentName, normalFont, new BaseColor(249, 249, 249), detailBody, Element.ALIGN_LEFT);
                AddColumnValues("School Name", normalFont, new BaseColor(249, 249, 249), detailBody, Element.ALIGN_LEFT);
                AddColumnValues(student.SchoolName, normalFont, new BaseColor(249, 249, 249), detailBody, Element.ALIGN_LEFT);
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

                if (historyList != null && historyList.Count > 0)
                {
                    AddHeaderCell("Date", boldFont, tablebody);
                    AddHeaderCell("Item Name", boldFont, tablebody);
                    AddHeaderCell("Quantity", boldFont, tablebody);
                    AddHeaderCell("Status", boldFont, tablebody);
                    AddHeaderCell("Total Price", boldFont, tablebody);

                    int i = 0;
                    foreach (var item in historyList)
                    {
                        BaseColor _color = i % 2 == 0 ? _color2 : _color1;
                        AddColumnValues(item.CreatedOn.ToString(), normalFont, _color, tablebody, Element.ALIGN_LEFT);
                        AddColumnValues(item.ItemName, normalFont, _color, tablebody, Element.ALIGN_LEFT);
                        AddColumnValues(item.Quantity.ToString(), normalFont, _color, tablebody, Element.ALIGN_RIGHT);
                        AddColumnValues(item.TransactionStatus.ToString(), normalFont, _color, tablebody, Element.ALIGN_RIGHT);
                        AddColumnValues(item.TotalPrice.ToString(), normalFont, _color, tablebody, Element.ALIGN_RIGHT);
                        i++;
                    }
                    document.Add(headingBody);
                    document.Add(detailBody);
                    document.Add(tablebody);
                }
                else
                {
                    AddHeaderCell("Date", boldFont, tablebody);
                    AddHeaderCell("Item Name", boldFont, tablebody);
                    AddHeaderCell("Quantity", boldFont, tablebody);
                    AddHeaderCell("Status", boldFont, tablebody);
                    AddHeaderCell("Total Price", boldFont, tablebody);

                    NoRecordRow("No Records", normalFont, new BaseColor(249, 249, 249), tablebody, Element.ALIGN_CENTER, 5);

                    document.Add(headingBody);
                    document.Add(detailBody);
                    document.Add(tablebody);
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, HttpStatusCode.InternalServerError, ex.Message); ;
            }
            finally
            {
                document.Close();
            }

            byte[] buffer = stream.ToArray();
            response = Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StreamContent(new MemoryStream(buffer));
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
            response.Content.Headers.ContentLength = buffer.Length;
            if (ContentDispositionHeaderValue.TryParse("inline; filename=" + filename, out ContentDispositionHeaderValue contentDisposition))
            {
                response.Content.Headers.ContentDisposition = contentDisposition;
            }

            return response;
        }

        [HttpGet]
		[AllowAnonymous]
		[Route("ExportTransactionHistoryPDF")]
		public async Task<HttpResponseMessage> ExportTransactionHistoryPDF(string parentid, string studentid, int days)
		{
			var startDate = DateTime.Now.AddDays(-days);
			var endDate = DateTime.Now;

			HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest);
			string filename = string.Format("{0}-{1}.{2}", "TransactionHistory", DateTime.Now.ToString("dd-MM-yyyy-hh:mm:ss"), "pdf");

			//Student Details
			var userDetail = await _userService.GetStudentDetailByIdAsync(studentid);
			StudentDetailDTO student = userDetail.data;

			//Transaction List
			var result = await _transactionService.GetParentStudentTransactionPDF(parentid, studentid, startDate, endDate);
			List<ParentStudentTransactionDTO> historyList = result.data;

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
				AddTitleCell("Transaction History", titleFont, headingBody);

				//Student Detail
				List<float> dWidth = new List<float>() { 5f, 5f };//Title
				PdfPTable detailBody = new PdfPTable(2)
				{
					WidthPercentage = 100,
					KeepTogether = false
				};
				detailBody.SetWidths(dWidth.ToArray());
				AddColumnValues("MLMID", normalFont, new BaseColor(249, 249, 249), detailBody, Element.ALIGN_LEFT);
				AddColumnValues(student.MLMID, normalFont, new BaseColor(249, 249, 249), detailBody, Element.ALIGN_LEFT);
				AddColumnValues("Student Name", normalFont, new BaseColor(249, 249, 249), detailBody, Element.ALIGN_LEFT);
				AddColumnValues(student.StudentName, normalFont, new BaseColor(249, 249, 249), detailBody, Element.ALIGN_LEFT);
				AddColumnValues("School Name", normalFont, new BaseColor(249, 249, 249), detailBody, Element.ALIGN_LEFT);
				AddColumnValues(student.SchoolName, normalFont, new BaseColor(249, 249, 249), detailBody, Element.ALIGN_LEFT);


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

				if (historyList != null && historyList.Count > 0)
				{
					AddHeaderCell("Date", boldFont, tablebody);
					AddHeaderCell("Card Number", boldFont, tablebody);
					AddHeaderCell("Amount", boldFont, tablebody);

					int i = 0;
					foreach (var item in historyList)
					{
						BaseColor _color = i % 2 == 0 ? _color2 : _color1;
						AddColumnValues(item.CreatedOn.ToString(), normalFont, _color, tablebody, Element.ALIGN_LEFT);
						AddColumnValues(item.CardNumber, normalFont, _color, tablebody, Element.ALIGN_CENTER);
						AddColumnValues(item.Amount.ToString(), normalFont, _color, tablebody, Element.ALIGN_RIGHT);
						i++;
					}
					document.Add(headingBody);
					document.Add(detailBody);
					document.Add(tablebody);
				}
				else
				{
					AddHeaderCell("Date", boldFont, tablebody);
					AddHeaderCell("Card Number", boldFont, tablebody);
					AddHeaderCell("Amount", boldFont, tablebody);

					NoRecordRow("No Records", normalFont, new BaseColor(249, 249, 249), tablebody, Element.ALIGN_CENTER, 3);

					document.Add(headingBody);
					document.Add(detailBody);
					document.Add(tablebody);
				}
			}
			catch (Exception ex)
			{
				return Request.CreateResponse(HttpStatusCode.InternalServerError, HttpStatusCode.InternalServerError, ex.Message); ;
			}
			finally
			{
				document.Close();
			}

			byte[] buffer = stream.ToArray();
			response = Request.CreateResponse(HttpStatusCode.OK);
			response.Content = new StreamContent(new MemoryStream(buffer));
			response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
			response.Content.Headers.ContentLength = buffer.Length;
			if (ContentDispositionHeaderValue.TryParse("inline; filename=" + filename, out ContentDispositionHeaderValue contentDisposition))
			{
				response.Content.Headers.ContentDisposition = contentDisposition;
			}

			return response;
		}

        [HttpGet]
        [AllowAnonymous]
        [Route("ExportSchoolTransactionHistoryPDF")]
        public async Task<HttpResponseMessage> ExportSchoolTransactionHistoryPDF(string schoolId,string parentid,DateTime? date)
        {
            var startDate = date;
            var endDate = date;

            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest);
            string filename = string.Format("{0}-{1}.{2}", "SchoolTransactionHistory", DateTime.Now.ToString("dd-MM-yyyy-hh:mm:ss"), "pdf");

            //Transaction List
            var result = await _transactionService.GetParentSchoolTransactionPDF(schoolId,parentid,startDate, endDate);
            List<ParentSchoolTransactionDTO> historyList = result.data;

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
                AddTitleCell("School Transaction History", titleFont, headingBody);

                //Table
                List<float> colWidth = new List<float>() { 5f, 5f, 5f, 5f };				

                PdfPTable tablebody = new PdfPTable(4)
                {
                    WidthPercentage = 100,
                    KeepTogether = false
                };

                tablebody.SetWidths(colWidth.ToArray());
                BaseColor _color1 = new BaseColor(249, 249, 249,249);
                BaseColor _color2 = new BaseColor(236, 237, 240,241);

                if (historyList != null && historyList.Count > 0)
                {
                    AddHeaderCell("Date", boldFont, tablebody);
                    AddHeaderCell("Card Number", boldFont, tablebody);
                    AddHeaderCell("School Name", boldFont, tablebody);
                    AddHeaderCell("Amount", boldFont, tablebody);

                    int i = 0;
                    foreach (var item in historyList)
                    {
                        BaseColor _color = i % 2 == 0 ? _color2 : _color1;
                        AddColumnValues(item.CreatedOn.ToString(), normalFont, _color, tablebody, Element.ALIGN_LEFT);
                        AddColumnValues(item.CardNumber, normalFont, _color, tablebody, Element.ALIGN_CENTER);
                        AddColumnValues(item.SchoolName, normalFont, _color, tablebody, Element.ALIGN_CENTER);
                        AddColumnValues(item.Amount.ToString(), normalFont, _color, tablebody, Element.ALIGN_RIGHT);
                        i++;
                    }
                    document.Add(headingBody);
                    document.Add(tablebody);
                }
                else
                {
                    AddHeaderCell("Date", boldFont, tablebody);
                    AddHeaderCell("Card Number", boldFont, tablebody);
                    AddHeaderCell("School Name", boldFont, tablebody);
                    AddHeaderCell("Amount", boldFont, tablebody);

                    NoRecordRow("No Records", normalFont, new BaseColor(249, 249, 249,249), tablebody, Element.ALIGN_CENTER, 4);

                    document.Add(headingBody);
                    document.Add(tablebody);
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, HttpStatusCode.InternalServerError, ex.Message); ;
            }
            finally
            {
                document.Close();
            }

            byte[] buffer = stream.ToArray();
            response = Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StreamContent(new MemoryStream(buffer));
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
            response.Content.Headers.ContentLength = buffer.Length;
            if (ContentDispositionHeaderValue.TryParse("inline; filename=" + filename, out ContentDispositionHeaderValue contentDisposition))
            {
                response.Content.Headers.ContentDisposition = contentDisposition;
            }

            return response;
        }
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

		private static void NoRecordRow(string text, Font font, BaseColor bgcolor, PdfPTable tabledays, int align = 1, int colspan=0)
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
