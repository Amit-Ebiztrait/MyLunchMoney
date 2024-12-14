using MyLunchMoney.Models;
using MyLunchMoney.Services;
using System;
using System.Threading.Tasks;
using System.Web.Http;

namespace MyLunchMoney.ApiControllers
{
	[RoutePrefix("api")]
    public class PaymentMethodController : ApiBaseController
    {
        #region Declaration
        private readonly IPaymentMethodService _service;
        private readonly ITransactionService _tranService;

        #endregion

        #region Constructor
        public PaymentMethodController(IPaymentMethodService service, ITransactionService tranService)
        {
            _service = service;
            _tranService = tranService;
        }
        #endregion

        #region Post Methods
        [HttpPost]
        [Route("GetAllPaymentMethodAsync")]
        public async Task<IHttpActionResult> GetAllPaymentMethodAsync(string id)
        {
            var result = await _service.GetAllPaymentMethodAsync(id);

            return Ok(result);
        }

        [HttpPost]
        [Route("GetPaymentMethodAsync")]
        public async Task<IHttpActionResult> GetPaymentMethodAsync(int id)
        {
            var result = await _service.GetByIdAsync(id);

            return Ok(result);
        }

        [HttpPost]
        [Route("AddPaymentMethodAsync")]
        public async Task<IHttpActionResult> AddPaymentMethodAsync([FromBody] PaymentMethods model)
        {
            var result = await _service.AddPaymentMethodAsync(model);

            return Ok(result);
        }

        [HttpPost]
        [Route("UpdatePaymentMethodAsync")]
        public async Task<IHttpActionResult> UpdatePaymentMethodAsync([FromBody] PaymentMethods model)
        {
            var result = await _service.UpdatePaymentMethodAsync(model);

            return Ok(result);
        }

        [HttpPost]
        [Route("IsDefaultSet")]
        public async Task<IHttpActionResult> IsDefaultSet([FromBody] PaymentMethods model)
        {
            var result = await _service.IsDefaultSetAsync(model);

            return Ok(result);
        }

        [HttpPost]
        [Route("DeleteByIdPaymentMethodAsync")]
        public async Task<IHttpActionResult> DeleteByIdPaymentMethodAsync(Int64 id)
        {
            var result = await _service.DeleteByIdAsync(id);

            return Ok(result);
        }

        [HttpPost]
        [Route("AddStudentTransactionAsync")]
        public async Task<IHttpActionResult> AddStudentTransactionAsync()
        {
            var result = await _tranService.AddStudentTransactionAsync(AddStudentFundRequest);

            return Ok(result);
        }
        #endregion
    }
}
