using System;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Server.IIS;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Homie.Areas.Identity.Models;
using Homie.Areas.Finances.Models;
using Homie.Areas.Finances.Services;
using Homie.Data.Models;
using Homie.Models;
using SmartBreadcrumbs.Extensions;

namespace Homie
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Регистрация кодировки windows-1251 для API ЦБ РФ
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            var builder = WebApplication.CreateBuilder(args);
            
            // Настройка Kestrel для корректного запуска
            builder.WebHost.UseKestrel();

            // ============================================================
            // КОНФИГУРАЦИЯ СЕРВИСОВ (ConfigureServices)
            // ============================================================

            // Подключение к базе данных MariaDB
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseMySql(
                    builder.Configuration.GetConnectionString("DefaultConnection"),
                    new MySqlServerVersion(new Version()),
                    mySqlOptions => mySqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(10),
                        errorNumbersToAdd: null
                    )
                )
            );

            // SECURITY: Усиленная политика паролей и защита от brute-force атак
            builder.Services.AddIdentity<User, IdentityRole>(opts =>
            {
                // Требования к паролю
                opts.Password.RequiredLength = 12;                  // минимальная длина 12 символов
                opts.Password.RequireNonAlphanumeric = true;        // требуются специальные символы (!@#$%^&*)
                opts.Password.RequireLowercase = true;              // требуются символы в нижнем регистре
                opts.Password.RequireUppercase = true;              // требуются символы в верхнем регистре
                opts.Password.RequireDigit = true;                  // требуются цифры

                // Защита от brute-force атак: блокировка аккаунта после неудачных попыток
                opts.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15); // блокировка на 15 минут
                opts.Lockout.MaxFailedAccessAttempts = 5;                        // максимум 5 неудачных попыток
                opts.Lockout.AllowedForNewUsers = true;                          // включить для новых пользователей
            })
            .AddEntityFrameworkStores<ApplicationDbContext>();

            // Настройка cookie аутентификации
            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.HttpOnly = true;
                options.ExpireTimeSpan = TimeSpan.FromDays(2);
                options.LoginPath = "/Identity/Account/Login";
                options.AccessDeniedPath = "/Identity/Account/AccessDenied";
                options.SlidingExpiration = true;
            });

            // Кэширование и сессии
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession();

            // Регистрация настроек загрузки файлов
            builder.Services.Configure<FileUploadSettings>(builder.Configuration.GetSection("FileUpload"));
            builder.Services.AddSingleton(sp => sp.GetRequiredService<IOptions<FileUploadSettings>>().Value);

            // ── Finances module ──────────────────────────────────────────
            builder.Services.Configure<FinancesSettings>(builder.Configuration.GetSection("Finances"));
            builder.Services.Configure<CoinGeckoSettings>(builder.Configuration.GetSection("Finances:CoinGecko"));
            builder.Services.Configure<MoexIssSettings>(builder.Configuration.GetSection("Finances:MoexIss"));
            builder.Services.Configure<YahooFinanceSettings>(builder.Configuration.GetSection("Finances:YahooFinance"));

            builder.Services.AddHttpClient<ICbrExchangeRateService, CbrExchangeRateService>();
            builder.Services.AddHttpClient<IMoexPriceService, MoexPriceService>();
            builder.Services.AddHttpClient<IYahooFinancePriceService, YahooFinancePriceService>();
            builder.Services.AddHttpClient<ICoinGeckoPriceService, CoinGeckoPriceService>();

            builder.Services.AddScoped<IPriceUpdateOrchestrator, PriceUpdateOrchestrator>();
            builder.Services.AddScoped<IFinanceCalculationService, FinanceCalculationService>();
            builder.Services.AddHostedService<PriceUpdateHostedService>();

            // Контроллеры и Views с настройками безопасности
            builder.Services.AddControllersWithViews(options =>
            {
                // Увеличиваем лимит на размер запроса до 10 MB
                options.MaxModelBindingCollectionSize = 1024;

                // SECURITY: Глобальная защита от CSRF-атак для всех POST/PUT/DELETE операций
                options.Filters.Add(new Microsoft.AspNetCore.Mvc.AutoValidateAntiforgeryTokenAttribute());
            });

            // Настройка имени заголовка для AJAX-передачи antiforgery-токена
            builder.Services.AddAntiforgery(options =>
            {
                options.HeaderName = "RequestVerificationToken";
            });

            // Настройка лимитов для загрузки файлов
            builder.Services.Configure<IISServerOptions>(options =>
            {
                options.MaxRequestBodySize = 10 * 1024 * 1024; // 10 MB
            });

            builder.Services.Configure<KestrelServerOptions>(options =>
            {
                options.Limits.MaxRequestBodySize = 10 * 1024 * 1024; // 10 MB
            });

            // Razor Pages
            builder.Services.AddRazorPages();

            // SmartBreadcrumbs для навигации
            builder.Services.AddBreadcrumbs(Assembly.GetExecutingAssembly(), options =>
            {
                options.TagName = "nav";
                options.TagClasses = "";
                options.OlClasses = "breadcrumb";
                options.LiClasses = "breadcrumb-item";
                options.ActiveLiClasses = "breadcrumb-item active";
            });

            // ============================================================
            // ПОСТРОЕНИЕ ПРИЛОЖЕНИЯ
            // ============================================================
            var app = builder.Build();

            // ============================================================
            // КОНФИГУРАЦИЯ MIDDLEWARE (Configure)
            // ============================================================

            // Обработка ошибок
            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // HSTS для защищённых соединений (30 дней)
                app.UseHsts();
            }

            // Перенаправление на HTTPS
            app.UseHttpsRedirection();
            
            // Отображение страниц статусов (404, 500 и т.д.)
            app.UseStatusCodePages();
            
            // Статические файлы (CSS, JS, изображения)
            app.UseStaticFiles();

            // SECURITY: Добавление заголовков безопасности для защиты от различных атак
            app.Use(async (context, next) =>
            {
                // Защита от MIME-sniffing атак
                context.Response.Headers["X-Content-Type-Options"] = "nosniff";

                // Защита от clickjacking атак (запрет встраивания в iframe)
                context.Response.Headers["X-Frame-Options"] = "SAMEORIGIN";

                // Включение встроенной защиты браузера от XSS
                context.Response.Headers["X-XSS-Protection"] = "1; mode=block";

                // Content Security Policy для защиты от XSS и injection атак
                // 'unsafe-inline' и 'unsafe-eval' разрешены для совместимости с jQuery и inline скриптами
                // В продакшене рекомендуется использовать nonce или hash для inline скриптов
                context.Response.Headers["Content-Security-Policy"] =
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
                    "connect-src 'self'";

                // Контроль передачи Referer заголовка
                context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";

                // Отключение потенциально опасных возможностей браузера
                context.Response.Headers["Permissions-Policy"] = "geolocation=(), microphone=(), camera=()";

                await next();
            });

            // Маршрутизация
            app.UseRouting();

            // Аутентификация и авторизация
            app.UseAuthentication();
            app.UseAuthorization();

            // Сессии
            app.UseSession();

            // Настройка endpoints (маршрутов)
            app.MapRazorPages();
            app.MapControllerRoute(
                name: "areas",
                pattern: "{area:exists}/{controller=Home}/{action=Index}"
            );
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}"
            );

            // Вывод информации о запуске для VS Code serverReadyAction
            app.Lifetime.ApplicationStarted.Register(() =>
            {
                var addresses = app.Urls;
                foreach (var address in addresses)
                {
                    Console.WriteLine($"Now listening on: {address}");
                }
            });

            // Запуск приложения
            app.Run();
        }
    }
}
