using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

#region entities
public class UserEntity
{
    [Key]
    public Guid id { get; set; } // Represents the uniqueidentifier field
    public string first_name { get; set; } // Represents the char(255) field
    public string last_name { get; set; } // Represents the char(255) field
    public string email { get; set; } // Represents the nchar(255) field
}

public class ProjectEntity
{
    [Key] 
    public Guid id { get; set; } // Represents the uniqueidentifier field
    public string name { get; set; } // Represents the char(255) field
}

[Keyless] 
public class UserLogsSummedEntity
{
    public Guid user_id { get; set; } 
    public double total_hours_worked { get; set; } 
}

public class UserLogEntity
{
    [Key] 
    public Guid id { get; set; } 
    public Guid user_id { get; set; } 
    public Guid project_id { get; set; }  
    public double hours_worked { get; set; }  
    public DateTime date { get; set; }
} 

public class UserLogTotalEntity
{ 
    public Guid user_id { get; set; }

    [AllowNull]
    public Dictionary<Guid, List<UserLogEntity>> project_logs { get; set; }
    [AllowNull]
    public Dictionary<Guid, double> project_logs_summed { get; set; }
}
#endregion

public class DatabaseModelsContext : DbContext
{
    #region data_sets
    private DbSet<ProjectEntity> ProjectsSet { get; set; }
    private DbSet<UserEntity> UsersSet { get; set; }
    private DbSet<UserLogsSummedEntity> Top10UserLogsSummed { get; set; }
    private DbSet<UserLogEntity> UserLogsSet { get; set; }
    private DatabaseModelsContext(DbContextOptions<DatabaseModelsContext> options) : base(options) { }
    #endregion

    #region getters
    public List<UserEntity> GetUsers(int offset, int quantity)
    {
        // Use FromSqlRaw to call the stored function
        return UsersSet.FromSqlRaw("SELECT * FROM RetrieveUsers(@offset, @quantity)",
            new[] {
                new SqlParameter("@offset", offset),
                new SqlParameter("@quantity", quantity)}).ToList();
    }

    public async Task<bool> ResetDB()
    {
        try { await Database.ExecuteSqlRawAsync("EXEC RecreateDB"); }
        catch (Exception e) {
            Console.WriteLine(e);
            return false;
        }
        return true;
    }

    public List<UserLogsSummedEntity> GetTop10UserLogs()
    {
        var list = Top10UserLogsSummed.FromSqlRaw(
                  "SELECT TOP 10 user_id, SUM(hours_worked) AS total_hours_worked " +
                  "FROM TimeLogs " +
                  "GROUP BY user_id " +
                  "ORDER BY total_hours_worked DESC;")
                  .ToList();

        return list;
    }

    public List<UserLogEntity> GetUserLogs(string userId)
    {
        return UserLogsSet.FromSqlRaw(
                "SELECT id, user_id, hours_worked, project_id, date " +
                "FROM TimeLogs " +
                "WHERE user_id = @userId",
               new[] { new SqlParameter("@userId", userId) }).ToList();
    }
    public List<UserLogTotalEntity> GetTop10UserLogsDetailed()
    {
        List<UserLogTotalEntity> users_logs_total = new List<UserLogTotalEntity>();

        List<UserLogsSummedEntity> topLogs = TopUserLogsTotal.FromSqlRaw(
                  "SELECT TOP 10 user_id, SUM(hours_worked) AS total_hours_worked " +
                  "FROM TimeLogs " +
                  "GROUP BY user_id " +
                  "ORDER BY total_hours_worked DESC;")
                  .ToList(); 

        for(int i = 0; i < topLogs.Count; i++)
        {
            UserLogsSummedEntity topLog = topLogs[i];
            UserLogTotalEntity user_total = new UserLogTotalEntity
            { 
                user_id = topLog.user_id,
                project_logs = new Dictionary<Guid, List<UserLogEntity>>(),
                project_logs_summed = new Dictionary<Guid, double>() 
            };

            List<UserLogEntity> all_logs = 

            all_logs.ForEach((log) =>
            {
                if (!user_total.project_logs.ContainsKey(log.project_id))
                {
                    user_total.project_logs[log.project_id] = new List<UserLogEntity>();
                }
                if (!user_total.project_logs_summed.ContainsKey(log.project_id))
                {
                    user_total.project_logs_summed[log.project_id] = 0;
                }
                user_total.project_logs[log.project_id].Add(log);
                user_total.project_logs_summed[log.project_id] += log.hours_worked;
            });
            users_logs_total.Add(user_total);
        }

        return users_logs_total;
    }

    public List<ProjectEntity> GetAllProjects()
    {
        return ProjectsSet.FromSqlRaw("SELECT * FROM Projects").ToList();
    }
    #endregion
}