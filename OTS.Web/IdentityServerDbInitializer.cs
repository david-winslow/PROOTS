using System.Threading.Tasks;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OTS.DAL;
using OTS.DAL.Core.Interfaces;

namespace OTS.Web
{
    public class IdentityServerDbInitializer : DatabaseInitializer
    {
        private readonly PersistedGrantDbContext _persistedGrantContext;
        private readonly ConfigurationDbContext _configurationContext;
        private readonly ILogger _logger;

        public IdentityServerDbInitializer(
            ApplicationDbContext context,
            PersistedGrantDbContext persistedGrantContext,
            ConfigurationDbContext configurationContext,
            IAccountManager accountManager,
            ILogger<IdentityServerDbInitializer> logger) : base(context, accountManager, logger)
        {
            _persistedGrantContext = persistedGrantContext;
            _configurationContext = configurationContext;
            _logger = logger;
        }


        public override async Task SeedAsync()
        {
            await base.SeedAsync().ConfigureAwait(false);

            await _persistedGrantContext.Database.MigrateAsync().ConfigureAwait(false);
            await _configurationContext.Database.MigrateAsync().ConfigureAwait(false);


            if (!await _configurationContext.Clients.AnyAsync())
            {
                _logger.LogInformation("Seeding IdentityServer Clients");

                foreach (var client in IdentityServerConfig.GetClients())
                {
                    _configurationContext.Clients.Add(client.ToEntity());
                }
                _configurationContext.SaveChanges();
            }

            if (!await _configurationContext.IdentityResources.AnyAsync())
            {
                _logger.LogInformation("Seeding IdentityServer Identity Resources");

                foreach (var resource in IdentityServerConfig.GetIdentityResources())
                {
                    _configurationContext.IdentityResources.Add(resource.ToEntity());
                }
                _configurationContext.SaveChanges();
            }

            if (!await _configurationContext.ApiResources.AnyAsync())
            {
                _logger.LogInformation("Seeding IdentityServer API Resources");

                foreach (var resource in IdentityServerConfig.GetApiResources())
                {
                    _configurationContext.ApiResources.Add(resource.ToEntity());
                }
                _configurationContext.SaveChanges();
            }
        }
    }
}
