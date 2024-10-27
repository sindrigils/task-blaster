using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

using TaskBlaster.TaskManagement.API.ExceptionHandlerExtension;
using TaskBlaster.TaskManagement.API.Services.Implementations;
using TaskBlaster.TaskManagement.API.Services.Interfaces;
using TaskBlaster.TaskManagement.DAL.Data;
using TaskBlaster.TaskManagement.DAL.Implementations;
using TaskBlaster.TaskManagement.DAL.Interfaces;
using TaskBlaster.TaskManagement.Models.InputModels;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.Authority = builder.Configuration.GetValue<string>("Auth0:Authority");
    options.Audience = builder.Configuration.GetValue<string>("Auth0:Audience");

    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = async context =>
        {
            var userService = context.HttpContext.RequestServices.GetRequiredService<IUserService>();

            var name = context.Principal?.Claims.FirstOrDefault(c => c.Type == "name")?.Value;
            var email = context.Principal?.Claims.FirstOrDefault(c => c.Type == "email_address")?.Value;

            if (email != null && name != null)
            {
                await userService.CreateUserIfNotExistsAsync(new UserInputModel
                {
                    FullName = name,
                    EmailAddress = email,
                });
            }

        }
    };
});

builder.Services.AddDbContext<TaskBlasterDbContext>(options =>
{
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"), b => b.MigrationsAssembly("TaskBlaster.TaskManagement.API")
    );
}
);

builder.Services.AddHttpContextAccessor();


builder.Services.AddTransient<ICommentRepository, CommentRepository>();
builder.Services.AddTransient<IPriorityRepository, PriorityRepository>();
builder.Services.AddTransient<IStatusRepository, StatusRepository>();
builder.Services.AddTransient<ITagRepository, TagRepository>();
builder.Services.AddTransient<ITaskRepository, TaskRepository>();
builder.Services.AddTransient<IUserRepository, UserRepository>();

builder.Services.AddTransient<ICommentService, CommentService>();
builder.Services.AddTransient<INotificationService, NotificationService>();
builder.Services.AddTransient<IPriorityService, PriorityService>();
builder.Services.AddTransient<IStatusService, StatusService>();
builder.Services.AddTransient<ITagService, TagService>();
builder.Services.AddTransient<ITaskService, TaskService>();
builder.Services.AddTransient<IUserService, UserService>();

builder.Services.AddAuthorization();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
// if (app.Environment.IsDevelopment())
// {
//     app.UseSwagger();
//     app.UseSwaggerUI();
// }

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.UseGlobalExceptionHandler();

app.Run();