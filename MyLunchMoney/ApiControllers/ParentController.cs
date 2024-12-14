using MyLunchMoney.Constants;
using MyLunchMoney.Hubs;
using MyLunchMoney.Models;
using MyLunchMoney.Models.Request;
using MyLunchMoney.Repository;
using MyLunchMoney.Services;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace MyLunchMoney.ApiControllers
{
	[RoutePrefix("api")]
	public class ParentController : ApiBaseController
    {
		#region Declaration
		private readonly IUserService _userService;
		private readonly ITransactionService _tranService;
		private readonly IAuthRepository _authRepository;
		private readonly IAuthService _authService;
		private readonly INotificationService _notificationService;
		#endregion

		#region Constructor
		public ParentController(IUserService userService
			, IAuthRepository authRepository
			, IAuthService authService
			, ITransactionService tranService
			, INotificationService notificationService)
		{
			_tranService = tranService;
			_userService = userService;
			_authRepository = authRepository;
			_authService = authService;
			_notificationService = notificationService;
		}
		#endregion

		#region Post Mothods
		[HttpPost]
		[Route("GetParentProfileAsync")]
		public async Task<IHttpActionResult> GetParentProfileAsync(string id)
		{
			var data = await _authRepository.GetParentProfileByIdAsync(id);
			var result = new ApiHttpResponse(HttpStatusCode.OK, MessageCode.Ok, data);

			return Ok(result);
		}

		[HttpPost]
		[Route("AddFundAsync")]
		public async Task<IHttpActionResult> AddFundAsync()//[FromBody] AddFundRequest AddFundRequest)
		{
			var result = await _tranService.AddFundAsync(AddFundRequest);
			if(result.code == HttpStatusCode.OK)
            {
				var notificationModel = new AddNotificationRequest
				{
					
					CurrentUserId = AddFundRequest.CurrentUserId,					
					Subject = "Add Fund By Parent",
					Message = MessageCode.AllocatedAmountSuccess
				};
				var addnotification = await _notificationService.AddNotifications("FA", notificationModel);
				MessagesHub objHub = new MessagesHub();
				objHub.GetNotification();
			}
			return Ok(result);
		}

        [HttpPost]
        [Route("AddStoryFundAsync")]
        public async Task<IHttpActionResult> AddStoryFundAsync()
        {
            var result = await _tranService.AddStoryFundAsync(AddStoryFundRequest);
			if(result.code == HttpStatusCode.OK)
            {
				var notificationModel = new AddNotificationRequest
				{

					CurrentUserId = AddStoryFundRequest.ParentId,
					SchoolId=AddStoryFundRequest.SchoolId,
					Subject = "Add story Fund",
					Message = MessageCode.StoryFundPayment
				};
				var addnotification = await _notificationService.AddNotifications("SF", notificationModel);
				MessagesHub objHub = new MessagesHub();
				objHub.GetNotification();
			}
            return Ok(result);
        }

        [HttpPost]
        [Route("AddSchoolTransactionFundAsync")]
        public async Task<IHttpActionResult> AddSchoolTransactionFundAsync()
        {
            var result = await _tranService.AddSchoolTransactionFundAsync(AddSchoolFundRequest);

            return Ok(result);
        }

        [HttpPost]
		[Route("GetStudentDetailByMLMIDAsync")]
		public async Task<IHttpActionResult> GetStudentByMLMIDAsync(string id)
		{
			var result = await _userService.GetStudentByMLMIDAsync(id);

			return Ok(result);
		}
        [HttpPost]
        [Route("GetStudentListByMLMIDAsync")]
        public async Task<IHttpActionResult> GetStudentListByMLMIDAsync(string id, string userName)
        {
            var result = await _userService.GetStudentListByMLMIDAsync(id,userName);

            return Ok(result);
        }
        [HttpPost]
		[Route("GetFundChildListAsync")]
		public async Task<IHttpActionResult> GetFundChildListAsync(string id)
		{
			var result = await _authService.GetFundChildListAsync(id);

			return Ok(result);
		}

		[HttpPost]
		[Route("AddStudentByMLMID")]
		public async Task<IHttpActionResult> AddStudentByMLMID(string mlmid, string parentid)
		{
			var result = await _authRepository.AddStudentByMLMID(mlmid, parentid);

			return Ok(result);
		}

		[HttpPost]
		[Route("GetSecoundaryAccountId")]
		public async Task<IHttpActionResult> GetSecoundaryAccountId(string id)
		{
			var result = await _authRepository.GetSecoundaryAccountId(id);

			return Ok(result);
		}

		[HttpPost]
		[Route("TransferToSecondaryAccount")]
		public async Task<IHttpActionResult> TransferToSecondaryAccount(string pid,string sid)
		{
			var result = await _userService.TransferToSecondaryAccount(pid, sid);

			return Ok(result);
		}

		[HttpPost]
		[Route("DeleteSecondaryAccount")]
		public async Task<IHttpActionResult> DeleteSecondaryAccount(string pid, string sid)
		{
			var result = await _userService.DeleteSecondaryAccount(pid, sid);

			return Ok(result);
		}

		[HttpPost]
		[Route("RequestForCancellationAsync")]
		public async Task<IHttpActionResult> RequestForCancellationAsync(string id)
		{
			var result = await _userService.RequestForCancellationAsync(id);

			return Ok(result);
		}

		[HttpPost]
		[Route("DeleteBothAccountAsync")]
		public async Task<IHttpActionResult> DeleteBothAccountAsync(string id)
		{
			var result = await _authRepository.DeleteBothAccountAsync(id);

			return Ok(result);
		}

		[HttpPost]
		[Route("SaveParentProfileAsync")]
		public async Task<IHttpActionResult> SaveParentProfileAsync()
		{
			var result = await _authService.SaveParentProfileAsync(ParentProfileRequest);

			return Ok(result);

		}

		[HttpPost]
		[Route("UpdateProfileAsync")]
		public async Task<IHttpActionResult> UpdateProfileAsync()
		{
			var result = await _userService.UpdateParentProfileAsync(ParentProfileRequest);

			return Ok(result);

		}

        [HttpPost]
        [Route("ManageAccountAsync")]
        public async Task<IHttpActionResult> ManageAccountAsync(string pid)
        {
            var result = await _authService.ManageAccountAsync(pid);

            return Ok(result);
        }
        #endregion
    }
}
