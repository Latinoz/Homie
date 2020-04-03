﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Homie.Areas.Identity.Models;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Homie
{
    public class Program
    {
        //public static async Task Main(string[] args)
        //{
            //var host = CreateWebHostBuilder(args).Build();

            //using (var scope = host.Services.CreateScope())
            //{
            //    var services = scope.ServiceProvider;
            //    try
            //    {
            //        var userManager = services.GetRequiredService<UserManager<User>>();
            //        var rolesManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            //        await RoleInitializer.InitializeAsync(userManager, rolesManager);
            //    }
            //    catch (Exception ex)
            //    {
            //        var logger = services.GetRequiredService<ILogger<Program>>();
            //        logger.LogError(ex, "An error occurred while seeding the database.");
            //    }
            //}

            //host.Run();

            
        //}

        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseDefaultServiceProvider(options =>
                    options.ValidateScopes = false);
                
    }
}
