//using Microsoft.AspNetCore.Identity;
//namespace SusEquip.Services
//{
//    public class SuperAdminService
//    {
//        private readonly UserManager<IdentityUser> _userManager;

//        public SuperAdminService(UserManager<IdentityUser> userManager)
//        {
//            _userManager = userManager;
//        }

//        public async Task EnsureSuperAdminAccountAsync()
//        {
//            var superAdminEmail = "superadmin@super.admin";
//            var superAdminUser = await _userManager.FindByEmailAsync(superAdminEmail);

//            if (superAdminUser == null)
//            {
//                var user = new IdentityUser
//                {
//                    UserName = superAdminEmail,
//                    Email = superAdminEmail,
//                };

//                var result = await _userManager.CreateAsync(user, "SuperSuper");

//                //Add userRole?

//                //if (result.Succeeded)
//                //{
//                //    await _userManager.AddToRoleAsync(user, "Admin");
//                //    // Optionally, assign additional claims or roles as needed
//                //}
//            }
//        }
//    }
//}
