//using Microsoft.AspNetCore.Identity;
//using Microsoft.EntityFrameworkCore;

//namespace SusEquip.Services
//{
//    /// <summary>
//    /// To retrieve a list of all users, you would typically use the UserManager<IdentityUser> service, 
//    /// which is part of the ASP.NET Core Identity framework. 
//    /// The UserManager class provides methods for managing users in the identity system, 
//    /// including creating, updating, deleting users, and handling operations related to user accounts.
//    /// 
//    /// UserManagerService is a custom service that encapsulates the logic for retrieving all users. 
//    /// It uses dependency injection to receive an instance of UserManager<IdentityUser>, 
//    /// which is configured and provided by the ASP.NET Core Identity framework. 
//    /// </summary>
//    public class UserManagerService
//    {
//        private readonly UserManager<IdentityUser> _userManager;

//        public UserManagerService(UserManager<IdentityUser> userManager)
//        {
//            _userManager = userManager;
//        }

//        public async Task<List<IdentityUser>> GetAllUsersAsync()
//        {
//            return await _userManager.Users.ToListAsync();
//        }
//    }
//}
