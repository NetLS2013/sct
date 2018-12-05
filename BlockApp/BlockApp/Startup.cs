using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using BlockApp.Data;
using BlockApp.Data.Entities;
using BlockApp.Data.Repositories;
using BlockApp.Interfaces;
using BlockApp.Models;
using BlockApp.Models.ManageViewModels;
using BlockApp.Services;
using NBitcoin;
using Swashbuckle.AspNetCore.Swagger;
using ITransactionRepository = BlockApp.Interfaces.ITransactionRepository;

namespace BlockApp
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            var config = new AutoMapper.MapperConfiguration(
                cfg =>
                    {
                        cfg.CreateMap<TransactionViewModel, Data.Entities.Transaction>();
                        cfg.CreateMap<Data.Entities.Transaction, TransactionViewModel>();

                        cfg.CreateMap<LogViewModel, Log>();
                        cfg.CreateMap<Log, LogViewModel>();

                        cfg.CreateMap<Data.Entities.Transaction, TransactionViewModel>()
                            .ForMember(f => f.StatusMessage, o => o.Ignore());
                    });
            
            var mapper = config.CreateMapper();
            services.AddSingleton(mapper);

            services.AddDbContext<ApplicationDbContext>(
                options => options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            var lockoutOptions = new LockoutOptions()
                                     {
                                         AllowedForNewUsers = true,
                                         DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5),
                                         MaxFailedAccessAttempts = 5
                                     };

            services.AddIdentity<ApplicationUser, IdentityRole>(options => { options.Lockout = lockoutOptions;  })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();
            services.ConfigureApplicationCookie(options =>
                {
                    options.SlidingExpiration = true;
                });

            // Add application services.
            services.AddTransient<IEmailSender, EmailSender>(provider => new EmailSender(Configuration));
            services.AddTransient<IRequestProvider, RequestProvider>();
            
            services.AddScoped<IMerchantRepository, MerchantRepository>();
            services.AddScoped<ITransactionRepository, TransactionRepository>();
            services.AddScoped<ILogRepository, LogRepository>();
            services.AddScoped<ISubscriptionsRepository, SubscriptionsRepository>();

            services.AddScoped<IBitcoinService, BitcoinService>(provider => new BitcoinService(Network.GetNetwork(Configuration.GetValue<string>("BitcoinNetwork"))));

            services.AddScoped<IEthereumService, EthereumService>(provider => new EthereumService(Configuration.GetValue<string>("EthereumNetwork"), Configuration.GetValue<string>("EthereumWords"), Configuration.GetValue<string>("EthereumPassword")));

            services.AddMvc();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "Safecryptotrades API", Version = "v1" });
                
                var basePath = AppContext.BaseDirectory;
                var xmlPath = Path.Combine(basePath, "payment.xml"); 
                
                c.IncludeXmlComments(xmlPath);
            });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Error/Index");
            }

            app.UseStatusCodePagesWithReExecute("/Error/Index");
            app.UseStaticFiles();
            app.UseAuthentication();

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Safecryptotrades API V1");
            });
            
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
