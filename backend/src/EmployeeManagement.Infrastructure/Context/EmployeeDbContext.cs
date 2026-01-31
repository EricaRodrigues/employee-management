using EmployeeManagement.Domain.Entities;
using EmployeeManagement.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagement.Infrastructure.Context;

// DbContext responsible for database access
public class EmployeeDbContext(DbContextOptions<EmployeeDbContext> options) : DbContext(options)
{

    // Employees table
    public DbSet<Employee> Employees => Set<Employee>();

    // Phones table
    public DbSet<Phone> Phones => Set<Phone>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Employee mapping
        modelBuilder.Entity<Employee>(builder =>
        {
            // Primary key
            builder.HasKey(e => e.Id);

            // Required fields
            builder.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            builder.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            builder.Property(e => e.Email).IsRequired().HasMaxLength(200);
            builder.Property(e => e.DocNumber).IsRequired().HasMaxLength(50);
            builder.Property(e => e.PasswordHash).IsRequired();

            // Unique document number and Email
            builder.HasIndex(e => e.DocNumber).IsUnique();
            builder.HasIndex(e => e.Email).IsUnique();

            // One employee can have many phones
            // EmployeeId is a shadow property (not in domain)
            builder
                .HasMany(e => e.Phones)
                .WithOne()
                .HasForeignKey("EmployeeId");

            // Self reference for manager relationship
            builder
                .HasOne(e => e.Manager)
                .WithMany()
                .HasForeignKey(e => e.ManagerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Phone mapping
        modelBuilder.Entity<Phone>(builder =>
        {
            // Shadow primary key, used only by EF
            // Phone does not have identity in the domain
            builder.Property<Guid>("Id");
            builder.HasKey("Id");

            // Phone number is required
            builder.Property(p => p.Number).IsRequired().HasMaxLength(20);
        });

        // ------------------------------------------------------------
        // Seed Data
        // ------------------------------------------------------------

        // Seed admin user to bootstrap the application (demo purpose)
        var adminId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        modelBuilder.Entity<Employee>().HasData(
            new
            {
                Id = adminId,
                FirstName = "Admin",
                LastName = "Director",
                Email = "admin@company.com",
                DocNumber = "00000000001",
                BirthDate = DateTime.SpecifyKind(
                    new DateTime(1992, 4, 2),
                    DateTimeKind.Utc
                ),
                Role = EmployeeRoleEnum.Director,
                ManagerId = (Guid?)null,
                PasswordHash = "$2a$11$Ey8TKH0BmJnmnsg1ei30OuG0.N9CdgxGWaDiTtCwFzLN9p2fBMIh6" //Admin@123
            }
        );
    }
}