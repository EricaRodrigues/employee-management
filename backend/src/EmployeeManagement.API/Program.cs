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

// --------------------
// // HTTP pipeline
// // --------------------

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();