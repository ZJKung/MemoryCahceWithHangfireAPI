using System;
using System.Collections.Generic;
using System.Text;
using Bogus;
using Bogus.Extensions;
using Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Core
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
            ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }
        public DbSet<Customer> Customers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //seed fake data
            var ids = 1;
            var stock = new Faker<Customer>()
                .RuleFor(m => m.Id, f => ids++)
                .RuleFor(m => m.FirstName, f => f.Person.FirstName)
                .RuleFor(m => m.LastName, f => f.Person.LastName)
                .RuleFor(m => m.Contact, f => f.Person.Address.City)
                .RuleFor(m => m.Email, f => f.Person.Email)
                .RuleFor(m => m.DateOfBirth, f => f.Person.DateOfBirth);

            modelBuilder.Entity<Customer>(e =>
            {
                e.HasData(stock.GenerateBetween(1000, 1000));
            });
        }


    }
}
