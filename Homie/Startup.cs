using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Identity;
using Homie.Data.Models;
using Homie.Areas.Identity.Models;
using SmartBreadcrumbs.Extensions;
using System.Reflection;
using Homie.Models;
using Microsoft.AspNetCore.Server.IIS;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Options;

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
            services.AddDbContext<ApplicationDbContext>(options =>
            options.UseMySql(Configuration.GetConnectionString("DefaultConnection"), new MySqlServerVersion(new Version())));
           

            // SECURITY: Усиленная политика паролей для защиты от brute-force атак
            services.AddIdentity<User, IdentityRole>(opts =>
            {
                // Требования к паролю
                opts.Password.RequiredLength = 12;   // минимальная длина 12 символов
                opts.Password.RequireNonAlphanumeric = true;   // требуются специальные символы (!@#$%^&*)
                opts.Password.RequireLowercase = true; // требуются символы в нижнем регистре
                opts.Password.RequireUppercase = true; // требуются символы в верхнем регистре
                opts.Password.RequireDigit = true; // требуются цифры
                
                // Защита от brute-force атак: блокировка аккаунта после неудачных попыток
                opts.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15); // блокировка на 15 минут
                opts.Lockout.MaxFailedAccessAttempts = 5; // максимум 5 неудачных попыток
                opts.Lockout.AllowedForNewUsers = true; // включить для новых пользователей
            })
               .AddEntityFrameworkStores<ApplicationDbContext>();

            services.ConfigureApplicationCookie(options =>
            {
                // Cookie settings
                options.Cookie.HttpOnly = true;
                options.ExpireTimeSpan = TimeSpan.FromDays(2);

                options.LoginPath = "/Identity/Account/Login";
                options.AccessDeniedPath = "/Identity/Account/AccessDenied";
                options.SlidingExpiration = true;
            });

            services.AddDistributedMemoryCache();
            services.AddSession();

            // Регистрация настроек загрузки файлов
            services.Configure<FileUploadSettings>(Configuration.GetSection("FileUpload"));
            services.AddSingleton(sp => sp.GetRequiredService<IOptions<FileUploadSettings>>().Value);

            services.AddControllersWithViews(options =>
            {
                // Увеличиваем лимит на размер запроса до 10 MB
                options.MaxModelBindingCollectionSize = 1024;
                
                // Глобальная защита от CSRF-атак для всех POST/PUT/DELETE операций
                options.Filters.Add(new Microsoft.AspNetCore.Mvc.AutoValidateAntiforgeryTokenAttribute());
            });

            // Настройка лимитов для загрузки файлов
            services.Configure<IISServerOptions>(options =>
            {
                options.MaxRequestBodySize = 10 * 1024 * 1024; // 10 MB
            });

            services.Configure<KestrelServerOptions>(options =>
            {
                options.Limits.MaxRequestBodySize = 10 * 1024 * 1024; // 10 MB
            });

            services.AddRazorPages();

            services.AddBreadcrumbs(Assembly.GetExecutingAssembly(), options =>
            {
                options.TagName = "nav";
                options.TagClasses = "";
                options.OlClasses = "breadcrumb";
                options.LiClasses = "breadcrumb-item";
                options.ActiveLiClasses = "breadcrumb-item active";
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStatusCodePages();
            app.UseStaticFiles();

            // SECURITY: Добавление заголовков безопасности для защиты от различных атак
            app.Use(async (context, next) =>
            {
                // Защита от MIME-sniffing атак
                context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
                
                // Защита от clickjacking атак (запрет встраивания в iframe)
                context.Response.Headers.Add("X-Frame-Options", "SAMEORIGIN");
                
                // Включение встроенной защиты браузера от XSS
                context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
                
                // Content Security Policy для защиты от XSS и injection атак
                // 'unsafe-inline' и 'unsafe-eval' разрешены для совместимости с jQuery и inline скриптами
                // В продакшене рекомендуется использовать nonce или hash для inline скриптов
                context.Response.Headers.Add("Content-Security-Policy", 
                    "default-src 'self'; " +
                    "script-src 'self' 'unsafe-inline' 'unsafe-eval' " +
                        "https://ajax.googleapis.com https://cdnjs.cloudflare.com https://stackpath.bootstrapcdn.com " +
                        "https://ff.kis.v2.scr.kaspersky-labs.com https://cdn.jsdelivr.net https://ajax.aspnetcdn.com; " +
                    "style-src 'self' 'unsafe-inline' " +
                        "https://cdnjs.cloudflare.com https://www.w3schools.com " +
                        "https://cdn.jsdelivr.net https://fonts.googleapis.com; " +
                    "font-src 'self' data: " +
                        "https://cdnjs.cloudflare.com https://maxcdn.bootstrapcdn.com " +
                        "https://fonts.gstatic.com https://cdn.jsdelivr.net; " +
                    "img-src 'self' data: https:; " +
                    "connect-src 'self'");
                
                // Контроль передачи Referer заголовка
                context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");
                
                // Отключение потенциально опасных возможностей браузера
                context.Response.Headers.Add("Permissions-Policy", 
                    "geolocation=(), microphone=(), camera=()");
                
                await next();
            });

            app.UseRouting();

            app.UseAuthentication();    // подключение аутентификации
            app.UseAuthorization();

            app.UseSession();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllerRoute("areas","{area:exists}/{controller=Home}/{action=Index}");
                endpoints.MapControllerRoute("default","{controller=Home}/{action=Index}/{id?}");
            });            

            
        }
    }
}
