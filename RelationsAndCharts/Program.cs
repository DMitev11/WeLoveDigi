using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

var builder = WebApplication.CreateBuilder(args); 

// Add services to the container. 
builder.Services.AddRazorPages();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{ 
    options.IdleTimeout = TimeSpan.FromMinutes(20);
    options.Cookie.Name = ".RelationsAndCharts.Session";
});
builder.Services.AddDbContext<DatabaseModelsContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DatabaseConnection"),
        options =>
        { 
            options.CommandTimeout(60);
            options.EnableRetryOnFailure(
                maxRetryCount: 5,   // Maximum number of retry attempts.
                maxRetryDelay: TimeSpan.FromSeconds(30),   // Maximum delay between retries.
                errorNumbersToAdd: new List<int> { 1205 });  // Error codes to treat as transient.
        }));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();
app.UseSession();
app.Run();
