using MyLunchMoney.Constants;
using MyLunchMoney.Models;
using MyLunchMoney.Services;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;

namespace MyLunchMoney.ApiControllers
{
    [RoutePrefix("api")]
    public class StoryController : ApiController
    {
        #region Declaration
        private readonly IStoryService _storyService;
        private readonly IAuthService _authService;
        #endregion

        #region Constructor
        public StoryController(IStoryService storyService, IAuthService authService)
        {
            _storyService = storyService;
            _authService = authService;
        }
        #endregion

        #region Post Mothods
        [HttpPost]
        [Route("GetAllStoryAsync")]
        public async Task<IHttpActionResult> GetAllStoryAsync(int pageNumber, int pageSize)
        {
            var result = await _storyService.GetAllStoryAsync(pageNumber, pageSize);

            return Ok(result);
        }
        [HttpPost]
        [Route("GetStoryByIdAsync")]
        public async Task<IHttpActionResult> GetStoryByIdAsync(string schoolId, string id)
        {
            var data = await _storyService.GetStoryByIdAsync(schoolId, id);
            var result = new ApiHttpResponse(HttpStatusCode.OK, MessageCode.Ok, data);

            return Ok(result);
        }
        [HttpPost]
        [Route("GetSponserStoryListAsync")]
        public async Task<IHttpActionResult> GetSponserStoryListAsync(int pageNumber, int pageSize, string sponserId)
        {
            var data = await _storyService.GetSponserStoryListAsync(pageNumber, pageSize, sponserId);
            return Ok(data);
        }
        [HttpPost]
        [Route("GetStoryDetailWithoutTokenAsync")]
        public async Task<IHttpActionResult> GetStoryDetailWithoutTokenAsync(string storyId)
        {
            var data = await _authService.GetStoryDetailAsync(storyId);
            return Ok(data);
        }
        #endregion
    }
}
