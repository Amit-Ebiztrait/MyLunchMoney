using MyLunchMoney.Constants;
using MyLunchMoney.Models;
using MyLunchMoney.Models.DTOs;
using MyLunchMoney.Models.Request;
using MyLunchMoney.Repository;
using MyLunchMoney.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace MyLunchMoney.ApiControllers
{
	[RoutePrefix("api")]
	public class SponsorController : ApiBaseController
    {
		#region Declaration
		private readonly IUserService _userService;
		private readonly IAuthService _authService;
        private readonly IAuthRepository _authRepository;
        private readonly ITransactionService _tranService;
        #endregion

        #region Constructor
        public SponsorController(IUserService userService, IAuthService authService, IAuthRepository authRepository, ITransactionService tranService)
		{
			_userService = userService;
			_authService = authService;
            _authRepository = authRepository;
            _tranService = tranService;
        }
        #endregion

        #region Post Mothods
        [HttpPost]
		[Route("SaveSponsorProfileAsync")]
		public async Task<IHttpActionResult> SaveSponsorProfileAsync()
		{
			var result = await _authService.SaveSponsorProfileAsync(SponsorProfileRequest);

            return Ok(result);
		}
        [HttpPost]
        [Route("GetSponsorProfileAsync")]
        public async Task<IHttpActionResult> GetSponsorProfileAsync(string id)
        {
            var data = await _authRepository.GetSponsorProfileAsync(id);
            var result = new ApiHttpResponse(HttpStatusCode.OK, MessageCode.Ok, data);

            return Ok(result);
        }

        [HttpPost]
        [Route("GetSponserStudentListAsync")]
        public async Task<IHttpActionResult> GetSponserStudentListAsync(string sponsorId)
        {
            var result = await _authService.GetSponserStudentListAsync(sponsorId);
            return Ok(result);
        }

        [HttpPost]
        [Route("AddSponsorRegisterDetailAsync")]
        public async Task<IHttpActionResult> AddSponsorRegisterDetailAsync()
        {
            var result = await _authService.UpdateSponsorRegisterDetailAsync(SponsorUserRequest);
            return Ok(result);
        }
        [HttpPost]
        [Route("GetStoryDetailAsync")]
        public async Task<IHttpActionResult> GetStoryDetailAsync(string storyId)
        {
            var data = await _authService.GetStoryDetailAsync(storyId);
            return Ok(data);
        }

        [HttpPost]
        [Route("AddStoryPaymentDetailAsync")]
        public async Task<IHttpActionResult> AddStoryPaymentDetailAsync()
        {
            var result = await _authService.AddStoryPaymentDetailAsync(StoryUserRequest);
            return Ok(result);
        }
        //[HttpPost]
        //[Route("GetSponsorSponsorShipHistoryAsync")]
        //public async Task<IHttpActionResult> GetSponsorSponsorShipHistoryAsync(string id,string schoolId)
        //{
        //    var data = await _authRepository.GetSponsorSponsorShipHistoryAsync(id,schoolId);
        //    var result = new ApiHttpResponse(HttpStatusCode.OK, MessageCode.Ok, data);

        //    return Ok(result);
        //}
        #endregion
    }
}
