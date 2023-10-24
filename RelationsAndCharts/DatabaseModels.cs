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

    [AllowNull]
    public string first_name { get; set; } // Represents the char(255) field
    [AllowNull]
    public string last_name { get; set; } // Represents the char(255) field
    [AllowNull]
    public string email { get; set; } // Represents the nchar(255) field
}

public class ProjectEntity
{
    [Key]
    public Guid id { get; set; } // Represents the uniqueidentifier field
    [AllowNull]
    public string name { get; set; } // Represents the char(255) field
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


[Keyless]
public class UserLogsSummedEntity
{
    public Guid user_id { get; set; }
    public double total_hours_worked { get; set; }
}

#endregion

public class DatabaseModelsContext : DbContext
{
    public DatabaseModelsContext(DbContextOptions<DatabaseModelsContext> options) : base(options) { }

    #region data_sets
    private DbSet<UserEntity> UsersSet { get; set; }
    private DbSet<ProjectEntity> ProjectsSet { get; set; }
    private DbSet<UserLogEntity> UserLogsSet { get; set; }
    private DbSet<UserLogsSummedEntity> Top10UserLogsSummed { get; set; }
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

    public UserEntity GetUser(string userId)
    {
        // Use FromSqlRaw to call the stored function
        return UsersSet.FromSqlRaw("SELECT * FROM Users WHERE id = @user_id",
            new[] {
                new SqlParameter("@user_id", userId)
            }).ToList()[0];
    }

    public async Task<bool> ResetDB()
    {
        try { await Database.ExecuteSqlRawAsync("EXEC ResetDB"); }
        catch (Exception e)
        {
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

    public List<UserLogsSummedEntity> GetTop10UserLogs(DateTime start, DateTime end)
    {
        var list = Top10UserLogsSummed.FromSqlRaw(
                  "SELECT TOP 10 user_id, SUM(hours_worked) AS total_hours_worked " +
                  "FROM TimeLogs " +
                  "WHERE date >=  @start AND date <= @end " +
                  "GROUP BY user_id " +
                  "ORDER BY total_hours_worked DESC;",
            new[] {
                new SqlParameter("@start", String.Format("{0:d}", start)),
                new SqlParameter("@end", String.Format("{0:d}", end))})
                  .ToList();

        return list;
    }

    public UserLogsSummedEntity GetUserLogsSummed(string userId)
    {
        return Top10UserLogsSummed.FromSqlRaw(
                  "SELECT user_id, SUM(hours_worked) AS total_hours_worked " +
                  "FROM TimeLogs " +
                  "WHERE user_id = @userId " +
                  "GROUP BY user_id " +
                  "ORDER BY total_hours_worked DESC; ",
                  new[] {
                    new SqlParameter("@userId", userId)
                 }).ToList()[0];
    }

    public List<UserLogEntity> GetUserLogs(string userId)
    {
        return UserLogsSet.FromSqlRaw(
                "SELECT id, user_id, hours_worked, project_id, date " +
                "FROM TimeLogs " +
                "WHERE user_id = @userId",
               new[] { new SqlParameter("@userId", userId) }).ToList();
    }

    public List<ProjectEntity> GetAllProjects()
    {
        return ProjectsSet.FromSqlRaw("SELECT * FROM Projects").ToList();
    }
    #endregion
}