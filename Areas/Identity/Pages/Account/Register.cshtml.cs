using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.DirectoryServices.AccountManagement;
using System.Security.Principal;

namespace SusEquip.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        // calling an asynchronous method (GetAccessEmailListAsync()) in a synchronous context 
        // forces the asynchronous method to run synchronously, which can cause blocking and negatively impact performance.
        private Lazy<Task<List<string>>> _accessEmailList = new Lazy<Task<List<string>>>(() => GetAccessEmailListAsync());

        public RegisterModel(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public void OnGet()
        {
            // Fetch the current user's information
            var fullName = WindowsIdentity.GetCurrent().Name;
            var parts = fullName.Split('\\');
            var username = parts[^1];
            var userEmail = $"{username}@dtu.dk";

            // Create an instance of the InputModel and set its Email property to the constructed email address,
            // which is then used to pre-populate the email field in the login form.
            Input = new InputModel
            {
                Email = userEmail
            };
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                // Get access EmailList
                List<string> accessEmailList = await _accessEmailList.Value;

                // Check if Email has been registered
                if (await _userManager.FindByEmailAsync(Input.Email) != null)
                {
                    ModelState.AddModelError("Email", "This email is already registered.");
                    Input.Email = string.Empty;
                    Input.Password = string.Empty;
                    return Page();
                }

                // Check if Email is in the access list
                else if (!accessEmailList.Contains(Input.Email))
                {
                    ModelState.AddModelError("Email", "This email is not authorized for registration.");
                    Input.Email = string.Empty;
                    Input.Password = string.Empty;
                    return Page();
                }

                // Add new user to db and shop index page
                var identity = new IdentityUser { UserName = Input.Email, Email = Input.Email };
                var result = await _userManager.CreateAsync(identity, Input.Password);

                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(identity, isPersistent: false);
                    return LocalRedirect("~/");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }
            return Page();
        }

        private static Task<List<string>> GetAccessEmailListAsync()
        {
            return Task.Run(async () =>
            {

                using (var context = new PrincipalContext(ContextType.Domain))
                {
                    using (var searcher = new PrincipalSearcher(new UserPrincipal(context)))
                    {
                        var group = GroupPrincipal.FindByIdentity(context, IdentityType.Name, "SUS-Administration-IT-46784");

                        return group.Members.Select(x => x.UserPrincipalName).ToList();
                    }
                }
            }
            );
        }
    }
}
