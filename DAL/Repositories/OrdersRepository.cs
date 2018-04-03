using Microsoft.EntityFrameworkCore;
using OTS.DAL.Models;
using OTS.DAL.Repositories.Interfaces;

namespace OTS.DAL.Repositories
{
  public class OrdersRepository : Repository<Order>, IOrdersRepository
  {
    public OrdersRepository(DbContext context) : base(context)
    {
    }


    private ApplicationDbContext _appContext => (ApplicationDbContext) _context;
  }
}
