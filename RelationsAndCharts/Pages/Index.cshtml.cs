using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore; 
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace RelationsAndCharts.Pages
{ 

    public class IndexModel : PageModel
    {
        #region view
        public struct UserView
        {
            public string Id { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string Email { get; set; }
        }

        public struct ProjectView
        {
            public string Id { get; set; }
            public string Name { get; set; } 
        }

        public struct UserLogsSummedView 
        { 
            public Guid UserId { get; set; }
            public double TotalHoursWorked { get; set; }
        }

        public struct UserLogTotalView
        {
            public string UserId { get; set; }

            [AllowNull]
            public Dictionary<string, List<UserLogEntity>> ProjectLogs { get; set; }
            [AllowNull]
            public Dictionary<string, double> ProjectLogsSummed { get; set; }
        }
        #endregion

        #region db_interactions
        private const int PAGE_QUANTITY = 10;
        private readonly ILogger<IndexModel> _logger;
        private readonly DatabaseModelsContext _db_context;

        [AllowNull]
        public List<UserView> Users { get; set; } 
        [AllowNull]
        public List<UserLogTotalEntity>  { get; set; } 
        [AllowNull]
        public List<UserLogsSummedEntity> UserLogsSummedSet { get; set; } 
        [AllowNull]
        public Dictionary<Guid, string> ProjectsSet { get; set; }


        public IndexModel(ILogger<IndexModel> logger, DatabaseModelsContext context)
        { 
            _logger = logger;
            _db_context = context;
        }

        public async Task<IActionResult> OnPostRenegerateTables()
        {
            await _db_context.RecreateDB(); 
            UsersSet = _db_context.RetrieveUsers(0, PAGE_QUANTITY);
            return Page();
        }
         
        public IActionResult OnPostNextPage()
        {
            var page_offset = PageContext.HttpContext.Session.GetInt32("page_offset"); 
            page_offset += PAGE_QUANTITY;
            PageContext.HttpContext.Session.SetInt32(
                "page_offset", 
                page_offset.Value);
            UsersSet = _db_context.RetrieveUsers(
                page_offset.Value, 
                PAGE_QUANTITY); 
            return Page();
        }

        public IActionResult OnPostPreviousPage()
        {
            var page_offset = PageContext.HttpContext.Session.GetInt32(
                "page_offset");
            page_offset = Math.Max(0, page_offset.Value - PAGE_QUANTITY);
            PageContext.HttpContext.Session.SetInt32(
                "page_offset",
                page_offset.Value); 
            UsersSet = _db_context.RetrieveUsers(
                page_offset.Value,
                PAGE_QUANTITY); 
            return Page();
        }

        public void OnGet()
        { 
            if (PageContext.HttpContext.Session.GetString("page_offset") == null)
            {
                PageContext.HttpContext.Session.SetInt32("page_offset", 0);
            } 
            UsersSet = _db_context.RetrieveUsers(
                PageContext.HttpContext.Session.GetInt32("page_offset").Value,
                PAGE_QUANTITY);

            UserLogsSummedSet = _db_context.RetrieveTop10UserLogs();
            UserLogsTotalSet = _db_context.RetrieveTop10UserLogsDetailed();
            UserLogsTotalSet.Sort((lhs, rhs) => (int)(
                UserLogsSummedSet.Find((el) => el.user_id == rhs.user_id).total_hours_worked -
                UserLogsSummedSet.Find((el) => el.user_id == lhs.user_id).total_hours_worked));
            ProjectsSet = _db_context.RetrieveAllProjects();
        }
        #endregion

        #region ui_interactions 
        [BindProperty]
        public bool IsTop10Summed { get; set; }

        #endregion
    }
}