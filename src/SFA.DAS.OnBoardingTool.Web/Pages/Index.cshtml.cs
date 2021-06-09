using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using SFA.DAS.OnBoardingTool.Application;
using SFA.DAS.OnBoardingTool.Domain;
using SFA.DAS.OnBoardingTool.Web.Models;

namespace SFA.DAS.OnBoardingTool.Web.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IAtlassianService _atlassianService;

        public IndexModel(
            ILogger<IndexModel> logger,
            IAtlassianService atlassianService
        )
        {
            _logger = logger;
            _atlassianService = atlassianService;
        }

        [BindProperty]
        public UserDetails UserDetails { get; set; }

        // public async Task<IActionResult> OnPostAsync()
        // {
        //     if (!ModelState.IsValid)
        //     {
        //         return Page();
        //     }

            // await
            //     _atlassianService.CreateUser((UserDetails.Atlassian ? new User
            //         {
            //             Firstname = UserDetails.Firstname,
            //             Lastname = UserDetails.Lastname,
            //             Username = UserDetails.AtlassianUser.Email,
            //             Email = UserDetails.PersonalEmail
            //         } : null)
            //     );

        //     return RedirectToPage("./Success");
        // }
    }
}
