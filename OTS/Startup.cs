using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using OTS.DAL;
using OTS.DAL.Models;
using AutoMapper;
using OTS.DAL.Core;
using OTS.DAL.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Swashbuckle.AspNetCore.Swagger;
using AppPermissions = OTS.DAL.Core.ApplicationPermissions;
using IdentityServer4.AccessTokenValidation;
using System.Collections.Generic;
using Microsoft.IdentityModel.Logging;
using OTS.Authorization;
using OTS.Helpers;
using OTS.ViewModels;

namespace OTS
{
  public class Startup
  {
    private readonly IHostingEnvironment _hostingEnvironment;


    public Startup(IConfiguration configuration, IHostingEnvironment env)
    {
      Configuration = configuration;
      _hostingEnvironment = env;
    }

    public IConfiguration Configuration { get; }


    public void ConfigureServices(IServiceCollection services)
    {
      var assemblyName = "OTS";
      var connectionString = Configuration["ConnectionStrings:DefaultConnection"];
      services.AddDbContext<ApplicationDbContext>(options =>
      {
        
        options.UseSqlServer(connectionString, b => b.MigrationsAssembly(assemblyName));
      });

      // add identity
      services.AddIdentity<ApplicationUser, ApplicationRole>()
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

      // Configure Identity options and password complexity here
      services.Configure<IdentityOptions>(options =>
      {
        // User settings
        options.User.RequireUniqueEmail = true;
        options.Password.RequireDigit = false;
        options.Password.RequiredLength = 1;

        //    //options.Password.RequiredLength = 8;
        //    //options.Password.RequireNonAlphanumeric = false;
        //    //options.Password.RequireUppercase = true;
        //    //options.Password.RequireLowercase = false;

        //    //// Lockout settings
        //    //options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
        //    //options.Lockout.MaxFailedAccessAttempts = 10;
      });


      // Adds IdentityServer.
      services.AddIdentityServer()
        // The AddDeveloperSigningCredential extension creates temporary key material for signing tokens.
        // This might be useful to get started, but needs to be replaced by some persistent key material for production scenarios.
        // See http://docs.identityserver.io/en/release/topics/crypto.html#refcrypto for more information.
        .AddDeveloperSigningCredential()
        .AddConfigurationStore(options =>
        { 
          options.ConfigureDbContext = builder => builder.UseSqlServer(connectionString, sql => sql.MigrationsAssembly(assemblyName)); 
        })
        .AddOperationalStore(options =>
        { 
          options.ConfigureDbContext = builder => builder.UseSqlServer(connectionString, sql => sql.MigrationsAssembly(assemblyName)); 
     
          // this enables automatic token cleanup. this is optional. 
          options.EnableTokenCleanup = true; 
        })
        .AddAspNetIdentity<ApplicationUser>()
        .AddProfileService<ProfileService>();
         IdentityModelEventSource.ShowPII = true;
      var applicationUrl = Configuration["ApplicationUrl"].TrimEnd('/');

      services.AddAuthentication(IdentityServerAuthenticationDefaults.AuthenticationScheme)
        .AddIdentityServerAuthentication(options =>
        {
          options.Authority = applicationUrl;
          options.SupportedTokens = SupportedTokens.Jwt;
          options.RequireHttpsMetadata = false;
          options.ApiName = IdentityServerConfig.ApiName;
        });


      services.AddAuthorization(options =>
      {
        options.AddPolicy(Policies.ViewAllUsersPolicy,
          policy => policy.RequireClaim(ClaimConstants.Permission, AppPermissions.ViewUsers));
        options.AddPolicy(Policies.ManageAllUsersPolicy,
          policy => policy.RequireClaim(ClaimConstants.Permission, AppPermissions.ManageUsers));

        options.AddPolicy(Policies.ViewAllRolesPolicy,
          policy => policy.RequireClaim(ClaimConstants.Permission, AppPermissions.ViewRoles));
        options.AddPolicy(Policies.ViewRoleByRoleNamePolicy,
          policy => policy.Requirements.Add(new ViewRoleAuthorizationRequirement()));
        options.AddPolicy(Policies.ManageAllRolesPolicy,
          policy => policy.RequireClaim(ClaimConstants.Permission, AppPermissions.ManageRoles));

        options.AddPolicy(Policies.AssignAllowedRolesPolicy,
          policy => policy.Requirements.Add(new AssignRolesAuthorizationRequirement()));
      });


      // Add cors
      services.AddCors();

      // Add framework services.
      services.AddMvc();


      // In production, the Angular files will be served from this directory
      services.AddSpaStaticFiles(configuration =>
      {
        configuration.RootPath = "ClientApp/dist";
      });


      // Enforce https during production. To quickly enable ssl during development. Go to: Project Properties->Debug->Enable SSL
      //if (!_hostingEnvironment.IsDevelopment())
      //    services.Configure<MvcOptions>(options => options.Filters.Add(new RequireHttpsAttribute()));


      //Todo: ***Using DataAnnotations for validation until Swashbuckle supports FluentValidation***
      //services.AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<Startup>());


      //.AddJsonOptions(opts =>
      //{
      //    opts.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
      //});


      services.AddSwaggerGen(c =>
      {
        c.SwaggerDoc("v1", new Info {Title = IdentityServerConfig.ApiFriendlyName, Version = "v1"});

        c.OperationFilter<AuthorizeCheckOperationFilter>();

        c.AddSecurityDefinition("oauth2", new OAuth2Scheme
        {
          Type = "oauth2",
          Flow = "password",
          TokenUrl = $"{applicationUrl}/connect/token",
          Scopes = new Dictionary<string, string>
            {{IdentityServerConfig.ApiName, IdentityServerConfig.ApiFriendlyName}}
        });
      });


      Mapper.Initialize(cfg =>
      {
        cfg.AddProfile<AutoMapperProfile>();
      });


      // Configurations
      services.Configure<SmtpConfig>(Configuration.GetSection("SmtpConfig"));


      // Business Services
      services.AddScoped<IEmailer, Emailer>();


      // Repositories
      services.AddScoped<IUnitOfWork, HttpUnitOfWork>();
      services.AddScoped<IAccountManager, AccountManager>();

      // Auth Handlers
      services.AddSingleton<IAuthorizationHandler, ViewUserAuthorizationHandler>();
      services.AddSingleton<IAuthorizationHandler, ManageUserAuthorizationHandler>();
      services.AddSingleton<IAuthorizationHandler, ViewRoleAuthorizationHandler>();
      services.AddSingleton<IAuthorizationHandler, AssignRolesAuthorizationHandler>();

      // DB Creation and Seeding
      services.AddTransient<IDatabaseInitializer, IdentityServerDbInitializer>();
    }


    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
    {
      loggerFactory.AddConsole(Configuration.GetSection("Logging"));
      loggerFactory.AddDebug(LogLevel.Warning);
      loggerFactory.AddFile(Configuration.GetSection("Logging"));

      Utilities.ConfigureLogger(loggerFactory);
      EmailTemplates.Initialize(env);

      if (env.IsDevelopment())
      {
        app.UseDeveloperExceptionPage();
      }
      else
      {
        // Enforce https during production
        //var rewriteOptions = new RewriteOptions()
        //    .AddRedirectToHttps();
        //app.UseRewriter(rewriteOptions);

        app.UseExceptionHandler("/Home/Error");
      }


      //Configure Cors
      app.UseCors(builder => builder
        .AllowAnyOrigin()
        .AllowAnyHeader()
        .AllowAnyMethod());


      app.UseStaticFiles();
      app.UseSpaStaticFiles();
      app.UseIdentityServer();


      app.UseSwagger();
      app.UseSwaggerUI(c =>
      {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", $"{IdentityServerConfig.ApiFriendlyName} V1");
        c.OAuthClientId(IdentityServerConfig.SwaggerClientID);
        c.OAuthClientSecret("no_password"); //Leaving it blank doesn't work
        c.OAuthAppName("Swagger UI");
      });


      app.UseMvc(routes =>
      {
        routes.MapRoute(
          name: "default",
          template: "{controller}/{action=Index}/{id?}");
      });

      app.UseSpa(spa =>
      {
        // To learn more about options for serving an Angular SPA from ASP.NET Core,
        // see https://go.microsoft.com/fwlink/?linkid=864501

        spa.Options.SourcePath = "ClientApp";

        if (env.IsDevelopment())
        {
          spa.UseAngularCliServer(npmScript: "start");
        }
      });
    }
  }
}
