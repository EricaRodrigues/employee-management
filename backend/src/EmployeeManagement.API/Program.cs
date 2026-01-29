using EmployeeManagement.API.Middlewares;
using EmployeeManagement.Application.Services;
using EmployeeManagement.Application.Services.Interfaces;
using EmployeeManagement.Infrastructure.Context;
using EmployeeManagement.Infrastructure.Repositories;
using EmployeeManagement.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// --------------------
// Register services
// --------------------

// Controllers
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Application services
builder.Services.AddScoped<IEmployeeService, EmployeeService>();

// Infrastructure repositories
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();

// DbContext
builder.Services.AddDbContext<EmployeeDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);

// --------------------
// Build app
// --------------------

var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();

// --------------------
// // HTTP pipeline
// // --------------------

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();