using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using Microsoft.AspNetCore.HttpOverrides;
using System.Net;
using TaskBridge.Api.Tenancy;
using TaskBridge.Application.Abstractions;
using TaskBridge.Application.Notifications;
using TaskBridge.Application.Projects;
using TaskBridge.Infrastructure.Persistence;
using TaskBridge.Infrastructure.Repositories;
using TaskBridge.Infrastructure.Time;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
	options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
	foreach (var proxy in builder.Configuration.GetSection("Network:TrustedProxies").Get<string[]>() ?? [])
	{
		options.KnownProxies.Add(IPAddress.Parse(proxy));
	}
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
	options.SwaggerDoc("v1", new OpenApiInfo
	{
		Title = "TaskBridge API",
		Version = "v1"
	});
});
builder.Services.AddAuthentication().AddJwtBearer();
builder.Services.AddAuthorization(options =>
{
	options.AddPolicy("InternalService", policy =>
	{
		policy.RequireAuthenticatedUser();
		policy.RequireClaim("service_scope", "taskbridge.audit.write");
	});
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddDbContext<TaskBridgeDbContext>(options =>
	options.UseSqlite(builder.Configuration.GetConnectionString("TaskBridge")));
builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
builder.Services.AddScoped<IAuditEntryRepository, AuditEntryRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<IUnitOfWork>(serviceProvider =>
	serviceProvider.GetRequiredService<TaskBridgeDbContext>());
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<ICurrentUserContext, HttpCurrentTenant>();
builder.Services.AddSingleton<IClock, SystemClock>();
builder.Services.AddScoped<IValidator<CreateProjectRequest>, CreateProjectRequestValidator>();
builder.Services.AddScoped<IValidator<UpdateProjectStatusRequest>, UpdateProjectStatusRequestValidator>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
	var dbContext = scope.ServiceProvider.GetRequiredService<TaskBridgeDbContext>();
	await dbContext.Database.EnsureCreatedAsync();
}

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseForwardedHeaders();
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.MapControllers();

app.Run();
