using OTS.DAL.Repositories.Interfaces;

namespace OTS.DAL
{
  public interface IUnitOfWork
  {
  

    int SaveChanges();
  }
}
