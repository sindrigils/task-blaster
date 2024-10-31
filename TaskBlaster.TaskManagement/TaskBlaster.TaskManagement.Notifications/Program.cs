using Hangfire;
using Hangfire.PostgreSql;

using TaskBlaster.TaskManagement.Notifications.Services.Implementations;
using TaskBlaster.TaskManagement.Notifications.Services.Interfaces;
using TaskBlaster.TaskManagement.Notifications.Authorization;
using TaskBlaster.TaskManagement.Notifications.Services.Jobs;
using TaskBlaster.TaskManagement.DAL.Interfaces;
using TaskBlaster.TaskManagement.DAL.Implementations;
using TaskBlaster.TaskManagement.DAL.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.Authority = builder.Configuration["Auth0:Authority"];
    options.Audience = builder.Configuration["Auth0:Audience"];
});

builder.Services.AddDbContext<TaskBlasterDbContext>(options =>
{
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")
    );
}
);


// I got this setup from https://stackoverflow.com/questions/78518867/how-to-fix-usepostgresqlstorage-is-obsolete-will-be-removed-in-2-0-in-hangfir
// since the setup from the HangFire documentation always gave me a warning, because of using deprecated functions....
var hangfireConnection = builder.Configuration.GetConnectionString("HangFireConnection") ?? "";
builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UsePostgreSqlStorage(options => options.UseNpgsqlConnection(hangfireConnection)));
builder.Services.AddHangfireServer();


// Register custom services
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ITaskRepository, TaskRepository>();
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddTransient<IMailService, MailjetService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Enable Hangfire Dashboard with AllowAllAuthorizationFilter for anonymous access
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new AllowAllAuthorizationFilter() }
});

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

RecurringJob.AddOrUpdate<DueDateReminderJob>(
    "due-date-reminder-job",
    job => job.ExecuteAsync(),
    "*/30 * * * *"
);

app.Run();
