using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using MyLunchMoney.Dapper;
using MyLunchMoney.Models;
using MyLunchMoney.Repository;
using MyLunchMoney.Services;

namespace MyLunchMoney.ApiControllers
{
    [RoutePrefix("api")]
    public class StudentController : ApiController
    {
        #region Declaration
        private readonly IDapperService _dapperService;
        private readonly ApplicationDbContext _context;
        private readonly IRepository<ApplicationUser> _appUserRepository;
        private readonly IUserService _userService;
        #endregion

        #region Constructor
        public StudentController(
        IDapperService dapperService
        , ApplicationDbContext context
        , IRepository<ApplicationUser> appUserRepository
        , IAuthRepository authRepository,
        IUserService userService)
        {
            _dapperService = dapperService;
            _context = context;
            _appUserRepository = appUserRepository;
            _userService = userService;
        }
        #endregion
        [HttpPost]
        [Route("GetStudentDetailByMLMIDWithoutTokenAsync")]
        public async Task<IHttpActionResult> GetStudentDetailByMLMIDWithoutTokenAsync(string id)
        {
            var result= await _userService.GetStudentDetailByMLMIDWithoutTokenAsync(id);
            return Ok(result);
        }
        [HttpPost]
        [Route("GetStudentListByMLMIDWithoutTokenAsync")]
        public async Task<IHttpActionResult> GetStudentListByMLMIDWithoutTokenAsync(string id,string userName)
        {
            var result = await _userService.GetStudentListByMLMIDWithoutTokenAsync(id,userName);
            return Ok(result);
        }
    }
}