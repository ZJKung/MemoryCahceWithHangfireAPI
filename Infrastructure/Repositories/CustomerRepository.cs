using System;
using Core;
using Core.Entities;
using Core.Enums;
using Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class CustomerRepository : GenericReposirory<Customer>, ICustomerRepository
    {
        private readonly DbSet<Customer> _customer;
        public CustomerRepository(ApplicationDbContext dbContext, Func<CacheTech, ICacheService> cacheService) : base(dbContext, cacheService)
        {
            dbContext.Database.EnsureCreated();
            _customer = dbContext.Set<Customer>();
        }
    }
}
