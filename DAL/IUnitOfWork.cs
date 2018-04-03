using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OTS.DAL.Repositories.Interfaces;

namespace OTS.DAL
{
    public interface IUnitOfWork
    {
        ICustomerRepository Customers { get; }
        IProductRepository Products { get; }
        IOrdersRepository Orders { get; }


        int SaveChanges();
    }
}
