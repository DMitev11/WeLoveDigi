using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Diagnostics.CodeAnalysis;

namespace RelationsAndCharts.Pages
{

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

    public struct UserLogView
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string ProjectId { get; set; }
        public double HoursWorked { get; set; }
        public string Date { get; set; }
    }

    public struct UserLogsSummedView
    {
        [DisallowNull]
        public string UserId { get; set; }
        public double TotalHoursWorked { get; set; }
    }

    public struct UserLogsTotalView
    {
        public string UserId { get; set; }

        [AllowNull]
        public Dictionary<string, List<UserLogView>> ProjectLogs { get; set; }
        [AllowNull]
        public Dictionary<string, double> ProjectLogsSummed { get; set; }
    }

    public class IndexModel : PageModel
    {
        public IndexModel(ILogger<IndexModel> logger, DatabaseModelsContext context)
        {
            _logger = logger;
            _db_context = context;
        }

        DateTime start { get; set; }
        DateTime end { get; set; }

        private readonly ILogger<IndexModel> _logger;
        #region db_interactions
        private const int PAGE_QUANTITY = 10;
        private readonly DatabaseModelsContext _db_context;
        #endregion

        #region view_data
        public List<UserView> Users { get; set; }
        [AllowNull]
        public UserLogsSummedView ComparisonView { get; set; }
        public UserLogsTotalView ComparisonTotalView { get; set; }
        public List<ProjectView> Projects { get; set; }
        public List<UserLogsSummedView> Top10UsersSummed { get; set; }
        public List<UserLogsTotalView> Top10Users { get; set; }
        #endregion

        #region getters
        public UserView GetUser(string userId)
        {
            var user = _db_context.GetUser(userId);
            return new UserView
            {
                Id = user.id.ToString(),
                Email = user.email,
                FirstName = user.first_name,
                LastName = user.last_name,
            };
        }

        private List<UserView> GetUsers(int offset, int page_size)
        {
            var ret = _db_context.GetUsers(offset, page_size);

            List<UserView> view = new List<UserView>();
            foreach (var user in ret)
            {
                view.Add(new UserView
                {
                    Id = user.id.ToString(),
                    Email = user.email,
                    FirstName = user.first_name,
                    LastName = user.last_name
                });
            }
            return view;
        }

        private List<ProjectView> GetProjects()
        {
            var ret = _db_context.GetAllProjects();
            List<ProjectView> view = new List<ProjectView>();
            foreach (var project in ret)
            {
                view.Add(new ProjectView
                {
                    Id = project.id.ToString(),
                    Name = project.name
                });
            }
            return view;
        }

        private List<UserLogsSummedView> GetTopLogsSummed()
        {
            var ret = _db_context.GetTop10UserLogs();
            List<UserLogsSummedView> view = new List<UserLogsSummedView>();
            foreach (var log in ret)
            {
                view.Add(new UserLogsSummedView
                {
                    UserId = log.user_id.ToString(),
                    TotalHoursWorked = log.total_hours_worked
                });
            }
            return view;
        }

        private List<UserLogsSummedView> GetTopLogsSummed(DateTime start, DateTime end)
        {
            var ret = _db_context.GetTop10UserLogs(start, end);
            List<UserLogsSummedView> view = new List<UserLogsSummedView>();
            foreach (var log in ret)
            {
                view.Add(new UserLogsSummedView
                {
                    UserId = log.user_id.ToString(),
                    TotalHoursWorked = log.total_hours_worked
                });
            }
            return view;
        }

        private UserLogsSummedView GetUserLogsSummed(string userId)
        {
            var ret = _db_context.GetUserLogsSummed(userId);
            return new UserLogsSummedView
            {
                UserId = ret.user_id.ToString(),
                TotalHoursWorked = ret.total_hours_worked
            };
        }

        private UserLogsTotalView GetUserLogsTotal(string userId)
        {
            var ret_topUser = GetTopLogsSummed();
            UserLogsTotalView user_total = new UserLogsTotalView
            {
                UserId = userId,
                ProjectLogs = new Dictionary<string, List<UserLogView>>(),
                ProjectLogsSummed = new Dictionary<string, double>()
            };

            List<UserLogEntity> all_logs = _db_context.GetUserLogs(userId);
            all_logs.ForEach((log) =>
            {
                if (!user_total.ProjectLogs.ContainsKey(log.project_id.ToString()))
                {
                    user_total.ProjectLogs[log.project_id.ToString()] = new List<UserLogView>();
                }
                if (!user_total.ProjectLogsSummed.ContainsKey(log.project_id.ToString()))
                {
                    user_total.ProjectLogsSummed[log.project_id.ToString()] = 0;
                }

                user_total.ProjectLogs[log.project_id.ToString()].Add(
                    new UserLogView
                    {
                        Id = log.id.ToString(),
                        UserId = log.user_id.ToString(),
                        ProjectId = log.project_id.ToString(),
                        HoursWorked = log.hours_worked,
                        Date = log.date.ToString()
                    });
                user_total.ProjectLogsSummed[log.project_id.ToString()] += log.hours_worked;
            });
            return user_total;
        }

        private List<UserLogsTotalView> GetTopLogsTotal()
        {
            var ret_topUser = GetTopLogsSummed();
            var list_userLogsTotal = new List<UserLogsTotalView>();

            foreach (var user in ret_topUser)
            {
                UserLogsTotalView user_total = new UserLogsTotalView
                {
                    UserId = user.UserId,
                    ProjectLogs = new Dictionary<string, List<UserLogView>>(),
                    ProjectLogsSummed = new Dictionary<string, double>()
                };

                List<UserLogEntity> all_logs = _db_context.GetUserLogs(user.UserId);

                all_logs.ForEach((log) =>
                {
                    if (!user_total.ProjectLogs.ContainsKey(log.project_id.ToString()))
                    {
                        user_total.ProjectLogs[log.project_id.ToString()] = new List<UserLogView>();
                    }
                    if (!user_total.ProjectLogsSummed.ContainsKey(log.project_id.ToString()))
                    {
                        user_total.ProjectLogsSummed[log.project_id.ToString()] = 0;
                    }

                    user_total.ProjectLogs[log.project_id.ToString()].Add(
                        new UserLogView
                        {
                            Id = log.id.ToString(),
                            UserId = log.user_id.ToString(),
                            ProjectId = log.project_id.ToString(),
                            HoursWorked = log.hours_worked,
                            Date = log.date.ToString()
                        });
                    user_total.ProjectLogsSummed[log.project_id.ToString()] += log.hours_worked;
                });
                list_userLogsTotal.Add(user_total);
            }
            return list_userLogsTotal;
        }

        private List<UserLogsTotalView> GetTopLogsTotal(DateTime start_date, DateTime end_date)
        {
            var ret_topUser = GetTopLogsSummed(start_date, end_date);
            var list_userLogsTotal = new List<UserLogsTotalView>();

            foreach (var user in ret_topUser)
            {
                UserLogsTotalView user_total = new UserLogsTotalView
                {
                    UserId = user.UserId,
                    ProjectLogs = new Dictionary<string, List<UserLogView>>(),
                    ProjectLogsSummed = new Dictionary<string, double>()
                };

                List<UserLogEntity> all_logs = _db_context.GetUserLogs(user.UserId);

                all_logs.ForEach((log) =>
                {
                    if (!user_total.ProjectLogs.ContainsKey(log.project_id.ToString()))
                    {
                        user_total.ProjectLogs[log.project_id.ToString()] = new List<UserLogView>();
                    }
                    if (!user_total.ProjectLogsSummed.ContainsKey(log.project_id.ToString()))
                    {
                        user_total.ProjectLogsSummed[log.project_id.ToString()] = 0;
                    }

                    user_total.ProjectLogs[log.project_id.ToString()].Add(
                        new UserLogView
                        {
                            Id = log.id.ToString(),
                            UserId = log.user_id.ToString(),
                            ProjectId = log.project_id.ToString(),
                            HoursWorked = log.hours_worked,
                            Date = log.date.ToString()
                        });
                    user_total.ProjectLogsSummed[log.project_id.ToString()] += log.hours_worked;
                });
                list_userLogsTotal.Add(user_total);
            }
            return list_userLogsTotal;
        }

        private int SetContextInt32(string name, int value)
        {
            PageContext.HttpContext.Session.SetInt32(
                name,
                value);
            return GetContextInt32(name);
        }

        private int GetContextInt32(string name)
        {

            var ret = PageContext.HttpContext.Session.GetInt32(name);
            if (ret.HasValue)
            {
                return ret.Value;
            }
            throw new Exception("Invalid saved context");
        }

        private int GetContextInt32Ensure(string name, int default_value = 0)
        {
            var ret = PageContext.HttpContext.Session.GetInt32(name);
            if (!ret.HasValue)
            {
                return SetContextInt32(name, default_value);
            }
            return ret.Value;
        }

        public async Task<IActionResult> OnPostResetDB()
        {
            await _db_context.ResetDB();
            return OnGet();
        }

        public IActionResult OnPostNextPage()
        {
            var page_offset = GetContextInt32("page_offset");
            page_offset += PAGE_QUANTITY;
            SetContextInt32("page_offset", page_offset);
            return OnGet();
        }

        public IActionResult OnPostPreviousPage()
        {
            var page_offset = GetContextInt32("page_offset");
            page_offset = Math.Max(0, page_offset - PAGE_QUANTITY);
            SetContextInt32("page_offset", page_offset);
            return OnGet();
        }

        public IActionResult Compare()
        {
            Users = GetUsers(GetContextInt32Ensure("page_offset"), PAGE_QUANTITY);
            Projects = GetProjects();
            Top10UsersSummed = GetTopLogsSummed();
            Top10Users = GetTopLogsTotal();
            Top10Users.Sort((lhs, rhs) =>
            (int)(Top10UsersSummed.Find((el) => el.UserId == lhs.UserId).TotalHoursWorked -
            (int)(Top10UsersSummed.Find((el) => el.UserId == rhs.UserId).TotalHoursWorked)));
            return Page();
        }

        public IActionResult OnGet()
        {
            Users = GetUsers(GetContextInt32Ensure("page_offset"), PAGE_QUANTITY);
            Projects = GetProjects();
            Top10UsersSummed = GetTopLogsSummed();
            Top10Users = GetTopLogsTotal();
            Top10Users.Sort((lhs, rhs) =>
                (int)(Top10UsersSummed.Find((el) => el.UserId == lhs.UserId).TotalHoursWorked -
                (int)(Top10UsersSummed.Find((el) => el.UserId == rhs.UserId).TotalHoursWorked)));
            return Page();
        }

        public IActionResult OnGetCompare(string index)
        {
            if (index == "-1") return OnGet();
            ComparisonView = GetUserLogsSummed(
                GetUsers(
                    GetContextInt32Ensure("page_offset"),
                    PAGE_QUANTITY)[System.Int32.Parse(index)].Id);
            ComparisonTotalView = GetUserLogsTotal(
                GetUsers(
                    GetContextInt32Ensure("page_offset"),
                    PAGE_QUANTITY)[System.Int32.Parse(index)].Id);
            return OnGet();
        }

        public IActionResult OnGetCalendarDates(string date_start = "2022-1-1", string date_end = "2025-1-1")
        {
            OnGet();
            DateTime start = DateTime.Parse(date_start);
            DateTime end = DateTime.Parse(date_end);
            Top10UsersSummed = GetTopLogsSummed(start, end);
            Top10Users = GetTopLogsTotal(start, end);
            return Page();
        }
        #endregion
    }
}