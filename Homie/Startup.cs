using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Homies.Models;
using Homie.Areas.Series.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Pomelo.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Pomelo.EntityFrameworkCore.MySql.Storage.Internal;



namespace Homie
{
    public class Startup
    {
        public Startup(IConfiguration confiquration) =>
        Configuration = confiquration;
        public IConfiguration Configuration { get; }
    

    // This method gets called by the runtime. Use this method to add services to the container.
    // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
    public void ConfigureServices(IServiceCollection services)
        {
            //services.AddDbContext<ApplicationDbContext>(options =>
            //options.UseMySql("Server=localhost;User Id=root;Password=123456;Database=homiedb"));

            services.AddDbContext<ApplicationDbContext>(options =>
            options.UseMySql(Configuration.GetConnectionString("DefaultConnection")));

            services.AddTransient<IMovieRepository, EFMoviesRepository>();            

            services.AddRazorPages();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostEnvironment env)
        {           

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllerRoute("areas","{area:exists}/{controller=Home}/{action=Index}");
                endpoints.MapControllerRoute("default","{controller=Home}/{action=Index}");
            });
            
            app.UseDeveloperExceptionPage();
            app.UseStatusCodePages();
            app.UseStaticFiles();            
        
        }
    }
}
