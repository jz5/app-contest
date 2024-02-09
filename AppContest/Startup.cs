using AppContest.Data;
using AppContest.Infrastructure;
using AppContest.Infrastructure.Tags;
using EFCoreSecondLevelCacheInterceptor;
using FluentValidation.AspNetCore;
using HtmlTags;
using MediatR;
using MediatR.Pipeline;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace AppContest
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMiniProfiler().AddEntityFramework();

            services.AddEFSecondLevelCache(options =>
            {
                options.UseMemoryCacheProvider(CacheExpirationMode.Absolute, TimeSpan.FromMinutes(30))
                    .DisableLogging(false)
                    // Don't cache null values. Remove this optional setting if it's not necessary.
                    .SkipCachingResults(result =>
                                result.Value == null || (result.Value is EFTableRows rows && rows.RowsCount == 0));
            });

            // SQL Server
            // services.AddDbContext<ApplicationDbContext>((serviceProvider, optionsBuilder) =>
            //     optionsBuilder
            //         .UseSqlServer(
            //             Configuration.GetConnectionString("DefaultConnection"))
            //         .AddInterceptors(serviceProvider.GetRequiredService<SecondLevelCacheInterceptor>()));

            // MySQL
            // Replace with your server version and type.
            // Use 'MariaDbServerVersion' for MariaDB.
            // Alternatively, use 'ServerVersion.AutoDetect(connectionString)'.
            // For common usages, see pull request #1233.
            var serverVersion = new MySqlServerVersion(new Version(5, 7));

            services.AddDbContext<ApplicationDbContext>(
                dbContextOptions => dbContextOptions
                    .UseMySql(Configuration.GetConnectionString("DefaultConnection"), serverVersion)
                    // // The following three options help with debugging, but should
                    // // be changed or removed for production.
                    // .LogTo(Console.WriteLine, LogLevel.Information)
                    // .EnableSensitiveDataLogging()
                    // .EnableDetailedErrors()
            );
            
            services.AddDatabaseDeveloperPageExceptionFilter();
            services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>();

            services.AddAutoMapper(typeof(Startup));
            services.AddMediatR(typeof(Startup));
            services.AddScoped(
                typeof(IPipelineBehavior<,>),
                typeof(RequestExceptionProcessorBehavior<,>));
            services.AddHtmlTags(new TagConventions());

            services.AddScoped<IEmailSender, Infrastructure.EmailSender>();

            services.AddSession();
            services.AddRazorPages(opt =>
                {
                    opt.Conventions.ConfigureFilter(new DbContextTransactionPageFilter());
                    opt.Conventions.ConfigureFilter(new ValidatorPageFilter());
                    opt.Conventions.ConfigureFilter(new ExceptionFilter());
                })
                .AddFluentValidation(cfg =>
                {
                    cfg.RegisterValidatorsFromAssemblyContaining<Startup>();
                    cfg.DisableDataAnnotationsValidation = true;
                })
                .AddSessionStateTempDataProvider();

            services.AddAuthorization(options =>
            {
                options.AddPolicy("RequireContestAdministratorRole",
                     policy => policy.RequireRole("ContestAdministrator"));
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseMiniProfiler();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseSession();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
            });
        }
    }
}
