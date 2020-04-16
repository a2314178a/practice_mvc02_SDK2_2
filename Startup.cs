using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Hosting;
using practice_mvc02.Repositories;
using practice_mvc02.filters;
using practice_mvc02.Models;

namespace practice_mvc02
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
           // services.AddControllersWithViews();
            services.AddEntityFrameworkMySql()
                    .AddDbContext<DBContext>(options=>options.UseMySql(Configuration["ConnectionStrings:DefaultConnection"]));
            services.AddSession();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddTransient<AccountRepository>();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            
            services.AddDistributedMemoryCache();	// 將 Session 存在 ASP.NET Core 記憶體中
            services.AddTimedJob(); //Add TimedJob services
            services.AddTransient<MasterRepository>();
            services.AddTransient<PunchCardRepository>();
            services.AddTransient<SetRuleRepository>();
            services.AddTransient<ApplySignRepository>();
            services.AddTransient<MessageRepository>();
            services.AddTransient<AnnualLeaveRepository>();
            services.AddTransient<punchCardFunction>(); 
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {            
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseSession();
            app.UseTimedJob();  //使用TimedJob
           // app.UseRouting();
            //app.UseAuthorization();
            /*app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });*/
            app.UseMvc(routes =>
			{
				routes.MapRoute(
					name: "default",
					template: "{controller=Home}/{action=Index}/{id?}");
			});
        }
    }
}
