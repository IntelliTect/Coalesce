using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Coalesce.Starter.Vue.Web.Pages
{
    [AllowAnonymous]
    public class SignInModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public string? ReturnUrl { get; set; }

        [BindProperty]
        public string? Provider { get; set; }

        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            if (!string.IsNullOrWhiteSpace(Provider))
            {
                // Request a redirect to the external login provider.
                return new ChallengeResult(Provider, new()
                {
                    RedirectUri = ReturnUrl
                });
            }

            return Page();
        }
    }
}
