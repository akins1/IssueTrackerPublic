using IssueTracker.Data;
using IssueTracker.Models;
using IssueTracker.Services;
using IssueTracker.Services.Factories;
using IssueTracker.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = DataUtility.GetConnectionString(builder.Configuration);
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString, o => 
    o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentity<IssueTrackerUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddClaimsPrincipalFactory<ITUserClaimsPrincipalFactory>()
    .AddDefaultUI()
    .AddDefaultTokenProviders();


// Issue Tracker Services
builder.Services.AddScoped<IITRolesService, ITRolesService>();
builder.Services.AddScoped<IITCompanyInfoService, ITCompanyInfoService>();
builder.Services.AddScoped<IITProjectService, ITProjectService>();
builder.Services.AddScoped<IITTicketService, ITTicketService>();
builder.Services.AddScoped<IITTicketHistoryService, ITTicketHistoryService>();
builder.Services.AddScoped<IITTicketHistoryService, ITTicketHistoryService>();
builder.Services.AddScoped<IITNotificationService, ITNotificationService>();
builder.Services.AddScoped<IITInviteService, ITInviteService>();
builder.Services.AddScoped<IITFileService, ITFileService>();
builder.Services.AddScoped<IITLookupService, ITLookupService>();

//builder.Services.AddScoped<IEmailSender, ITEmailService>();
//builder.Services.Configure(builder.Configuration.GetSection("MailSettings"));


builder.Services.AddControllersWithViews();

var app = builder.Build();

await DataUtility.ManageDataAsync(app);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
