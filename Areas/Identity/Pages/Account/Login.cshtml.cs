using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.Security.Principal;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace SusEquip.Areas.Identity.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        public LoginModel(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        /// <summary>
        /// This property is used to hold the data (like the email and password) that will be input on the login page.
        /// </summary>
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

                if (await _userManager.FindByEmailAsync(Input.Email) == null)
                {
                    // Email is not registered
                    ModelState.AddModelError("Email", "This email is not registered.");
                    Input.Email = string.Empty;
                    Input.Password = string.Empty;
                    return Page();

                }
                // wrong password
                else if (!await _userManager.CheckPasswordAsync(await _userManager.FindByEmailAsync(Input.Email), Input.Password))
                {
                    ModelState.AddModelError("Password", "Invalid password.");
                    Input.Email = string.Empty;
                    Input.Password = string.Empty;
                    return Page();

                }

                var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, isPersistent: false, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    return LocalRedirect("~/");
                }
            }

            // If we got this far, something failed, nspect the ModelState dictionary for errors
            if (!ModelState.IsValid)
            {
                foreach (var modelStateKey in ModelState.Keys)
                {
                    var modelStateVal = ModelState[modelStateKey];
                    foreach (var error in modelStateVal.Errors)
                    {
                        var key = modelStateKey;
                        var errorMessage = error.ErrorMessage;
                        // Log or examine the key and errorMessage
                    }
                }
                return Page();
            }


            return Page();
        }
    }

    public class InputModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
