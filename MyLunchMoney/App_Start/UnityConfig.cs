using MyLunchMoney.Dapper;
using MyLunchMoney.Repository;
using MyLunchMoney.Services;
using MyLunchMoney.UnitOfWork;
using System.Web.Http;
using System.Web.Mvc;
using Unity;

namespace MyLunchMoney
{
	public static class UnityConfig
	{
		public static void RegisterComponents()
		{
			var container = new UnityContainer();

			// register all your components with the container here
			// it is NOT necessary to register your controllers

			// e.g. container.RegisterType<ITestService, TestService>();  
			//Register Services
			container.RegisterType<IDapperService, DapperService>();
			container.RegisterType<IPushNotificationService, PushNotificationService>();
			container.RegisterType<IUnitOfWorkManager, UnitOfWorkManager>();
			container.RegisterType<IEmailService, EmailService>();
			container.RegisterType<ITokenService, TokenService>();
			container.RegisterType<IUserService, UserService>();
			container.RegisterType<ICategoryService, CategoryService>();
			container.RegisterType<IItemService, ItemService>();
			container.RegisterType<ISchoolService, SchoolService>();
			container.RegisterType<INotificationService, NotificationService>();
			container.RegisterType<ITransactionService, TransactionService>();
			container.RegisterType<IPaymentMethodService, PaymentMethodService>();
			container.RegisterType<ISecurityQuestionService, SecurityQuestionService>();
			container.RegisterType<IAuthService, AuthService>();
			container.RegisterType<IAWSService, AWSService>();
			container.RegisterType<IFACPGService, FACPGService>();
			container.RegisterType<IDashboardService, DashboardService>();
			container.RegisterType<IStaffService, StaffService>();
			container.RegisterType<IStoryService, StoryService>();
			container.RegisterType<IMenuService, MenuService>();
			container.RegisterType<IFeeTypeService, FeeTypeService>();

			//Register Repositories
			container.RegisterType(typeof(IRepository<>), typeof(Repository<>));
			container.RegisterType<IUserRepository, UserRepository>();
			container.RegisterType<IAuthRepository, AuthRepository>();
			container.RegisterType<IGradeRepository, GradeRepository>();
			container.RegisterType<IPaymentMethodRepository, PaymentMethodRepository>();
			container.RegisterType<ICategoryRepository, CategoryRepository>();
			container.RegisterType<IItemRepository, ItemRepository>();
			container.RegisterType<IStateRepository, StateRepository>();
			container.RegisterType<ISchoolRepository, SchoolRepository>();
			container.RegisterType<INotificationRepository, NotificationRepository>();
			container.RegisterType<ITransactionRepository, TransactionRepository>();
			container.RegisterType<ISecurityQuestionRepository, SecurityQuestionRepository>();
			container.RegisterType<IDashboardRepository, DashboardRepository>();
			container.RegisterType<IStaffRepository, StaffRepository>();
			container.RegisterType<IStoryRepository, StoryRepository>();
			container.RegisterType<ISchoolTypeRepository, SchoolTypeRepository>();
			container.RegisterType<IMenuRepository, MenuRepository>();
			container.RegisterType<IFeeTypeRepository, FeeTypeRepository>();

			DependencyResolver.SetResolver(new Unity.Mvc5.UnityDependencyResolver(container));
			GlobalConfiguration.Configuration.DependencyResolver = new Unity.WebApi.UnityDependencyResolver(container);
		}
	}
}