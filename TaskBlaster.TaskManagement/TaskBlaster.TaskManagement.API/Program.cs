using Microsoft.EntityFrameworkCore;
using TaskBlaster.TaskManagement.API.Services.Implementations;
using TaskBlaster.TaskManagement.API.Services.Interfaces;
using TaskBlaster.TaskManagement.DAL.Data;
using TaskBlaster.TaskManagement.DAL.Implementations;
using TaskBlaster.TaskManagement.DAL.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<TaskBlasterDbContext>(options =>
{
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"), b => b.MigrationsAssembly("TaskBlaster.TaskManagement.API")
    );
}
);

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

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();