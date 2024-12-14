using MyLunchMoney.Services;
using System.Threading.Tasks;
using System.Web.Http;

namespace MyLunchMoney.ApiControllers
{
    [RoutePrefix("api")]
    public class NotificationController : ApiBaseController
    {
        #region Declaration
        private readonly INotificationService _notificationService;
        #endregion

        #region Constructor
        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }
        #endregion

        #region Post Mothods
        [HttpPost]
        [Route("GetAllNotificationAsync")]
        public async Task<IHttpActionResult> GetAllNotificationAsync(string id, int pageNumber, int pageSize)
        {
            var result = await _notificationService.GetAllAsync(id, pageNumber, pageSize);

            return Ok(result);
        }

        [HttpPost]
        [Route("ManageNotificationAsync")]
        public async Task<IHttpActionResult> ManageNotificationAsync(string userId)
        {
            if (!string.IsNullOrWhiteSpace(userId))
            {
                var result = await _notificationService.ManageNotificationAsync(userId, 1);
                return Ok(result);
            }
            else
            {
                return BadRequest();
            }
        }
        #endregion
    }
}
