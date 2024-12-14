using Microsoft.AspNet.Identity.Owin;
using MyLunchMoney.ActionFilter;
using MyLunchMoney.Models;
using MyLunchMoney.Models.Request;
using MyLunchMoney.Services;
using Newtonsoft.Json;
using System.Web;
using System.Web.Http;

namespace MyLunchMoney.ApiControllers
{
	//[ApiAuthorizeFilter, ApiExceptionFilter]
    public class ApiBaseController : ApiController
    {
        #region Identitys
        public ApplicationUserManager IdentityManager { get { return HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>(); } }
        #endregion

        #region Request Models
        public CategoryRequest CategoryRequest { get { return new CategoryRequest(); } }
        public ItemRequest ItemRequest { get { return new ItemRequest(); } }
        public ParentProfileRequest ParentProfileRequest { get { return new ParentProfileRequest(); } }
        public SponsorProfileRequest SponsorProfileRequest { get { return new SponsorProfileRequest(); } }
        public CafeteriaHistoryRequest CafeteriaHistoryRequest { get { return new CafeteriaHistoryRequest(); } }
        public StudentLockRequest StudentLockRequest { get { return new StudentLockRequest(); } }
        public TransactionHistoryRequest TransactionHistoryRequest { get { return new TransactionHistoryRequest(); } }
        public AddFundRequest AddFundRequest { get { return JsonConvert.DeserializeObject<AddFundRequest>(HttpContext.Current.Request.Form["model"]); } }
        public SponsorUserRequest SponsorUserRequest { get { return new SponsorUserRequest(); } }
        public StoryUserRequest StoryUserRequest { get { return new StoryUserRequest(); } }
        public AddStoryFundRequest AddStoryFundRequest { get { return JsonConvert.DeserializeObject<AddStoryFundRequest>(HttpContext.Current.Request.Form["model"]); } }
        public AddSchoolFundRequest AddSchoolFundRequest { get { return JsonConvert.DeserializeObject<AddSchoolFundRequest>(HttpContext.Current.Request.Form["model"]); } }
        public AddStudentFundRequest AddStudentFundRequest { get { return JsonConvert.DeserializeObject<AddStudentFundRequest>(HttpContext.Current.Request.Form["model"]); } }
        public AddTransactionRequest AddTransactionRequest { get { return JsonConvert.DeserializeObject<AddTransactionRequest>(HttpContext.Current.Request.Form["model"]); } }
        public AddPreOrderRequest AddPreOrderRequest { get { return JsonConvert.DeserializeObject<AddPreOrderRequest>(HttpContext.Current.Request.Form["model"]); } }
        public UpdatePreOrderRequest UpdatePreOrderRequest { get { return JsonConvert.DeserializeObject<UpdatePreOrderRequest>(HttpContext.Current.Request.Form["model"]); } }
        #endregion
    }
}
