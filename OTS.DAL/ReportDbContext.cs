using Microsoft.EntityFrameworkCore;

namespace OTS.DAL
{
    public class ReportDbContext :DbContext
  {
        public ReportDbContext( DbContextOptions<ReportDbContext> options) : base(options)
        {
          
        }

        public DbSet<Report> Reports {get;set;}
  }
}
