﻿using OTS.DAL.Repositories;
using OTS.DAL.Repositories.Interfaces;

namespace OTS.DAL
{
  public class UnitOfWork : IUnitOfWork
  {
    readonly ApplicationDbContext _context;

   


    public UnitOfWork(ApplicationDbContext context)
    {
      _context = context;
    }


   


   


    public int SaveChanges()
    {
      return _context.SaveChanges();
    }
  }
}
