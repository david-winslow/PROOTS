using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OTS.DAL.Core;
using OTS.DAL.Core.Interfaces;
using OTS.DAL.Models;

namespace OTS.DAL
{
  public interface IDatabaseInitializer
  {
    Task SeedAsync();
  }


  public class DatabaseInitializer : IDatabaseInitializer
  {
    private readonly IAccountManager _accountManager;
    private readonly ApplicationDbContext _context;
    private readonly ILogger _logger;

    public DatabaseInitializer(ApplicationDbContext context, IAccountManager accountManager,
      ILogger<DatabaseInitializer> logger)
    {
      _accountManager = accountManager;
      _context = context;
      _logger = logger;
    }

      public virtual async Task SeedAsync()
      {
          await _context.Database.MigrateAsync().ConfigureAwait(false);

          if (!await _context.Users.AnyAsync())
          {
              _logger.LogInformation("Generating default accounts");

              const string adminRoleName = "administrator";
              const string userRoleName = "ot";

              await EnsureRoleAsync(adminRoleName, "Manage system", ApplicationPermissions.GetAllPermissionValues());
              await EnsureRoleAsync(userRoleName, "Occupational Therapist", new string[] { });

              await CreateUserAsync("admin", "q", "David Winslow", "david@winslow.co.za",
                  "+27 (83) 823-0738", new[] {adminRoleName});
              await CreateUserAsync("aislinn", "q", "Aislinn Winslow", "aislinn@ot-services.co.za",
                  "+27 (72) 441-1941", new[] {userRoleName});

              _logger.LogInformation("Setup of default accounts completed");
          }
      }




      private async Task EnsureRoleAsync(string roleName, string description, string[] claims)
    {
      if ((await _accountManager.GetRoleByNameAsync(roleName)) == null)
      {
        var applicationRole = new ApplicationRole(roleName, description);

        var result = await _accountManager.CreateRoleAsync(applicationRole, claims);

        if (!result.Item1)
          throw new Exception(
            $"Seeding \"{description}\" role failed. Errors: {string.Join(Environment.NewLine, result.Item2)}");
      }
    }

    private async Task<ApplicationUser> CreateUserAsync(string userName, string password, string fullName, string email,
      string phoneNumber, string[] roles)
    {
      var applicationUser = new ApplicationUser
      {
        UserName = userName,
        FullName = fullName,
        Email = email,
        PhoneNumber = phoneNumber,
        EmailConfirmed = true,
        IsEnabled = true
      };

      var result = await _accountManager.CreateUserAsync(applicationUser, roles, password);

      if (!result.Item1)
        throw new Exception(
          $"Seeding \"{userName}\" user failed. Errors: {string.Join(Environment.NewLine, result.Item2)}");


      return applicationUser;
    }
  }
}
