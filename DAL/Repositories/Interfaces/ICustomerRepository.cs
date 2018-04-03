using System;
using System.Collections.Generic;
using System.Linq;
using OTS.DAL.Models;

namespace OTS.DAL.Repositories.Interfaces
{
    public interface ICustomerRepository : IRepository<Customer>
    {
        IEnumerable<Customer> GetTopActiveCustomers(int count);
        IEnumerable<Customer> GetAllCustomersData();
    }
}
