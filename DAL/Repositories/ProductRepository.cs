using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OTS.DAL.Models;
using OTS.DAL.Repositories.Interfaces;

namespace OTS.DAL.Repositories
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        public ProductRepository(DbContext context) : base(context)
        { }




        private ApplicationDbContext _appContext => (ApplicationDbContext)_context;
    }
}
