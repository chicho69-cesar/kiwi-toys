using KiwiToys.Data;
using KiwiToys.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace KiwiToys {
    public class StartUp {
        public static WebApplication InitializeApplication(string[] args) {
            var builder = WebApplication.CreateBuilder(args);
            ConfigureServices(builder);
            var app = builder.Build();
            SeedData(app);
            ConfigureMiddlewares(app);
            return app;
        }

        private static void ConfigureServices(WebApplicationBuilder builder) {
            builder.Services.AddDbContext<DataContext>(options => {
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
            });

            builder.Services.AddIdentity<User, IdentityRole>(config => {
                config.Tokens.AuthenticatorTokenProvider = TokenOptions.DefaultAuthenticatorProvider;
                config.SignIn.RequireConfirmedEmail = true;
                config.User.RequireUniqueEmail = true;
                config.Password.RequireDigit = true;
                config.Password.RequiredUniqueChars = 0;
                config.Password.RequireLowercase = true;
                config.Password.RequireNonAlphanumeric = true;
                config.Password.RequireUppercase = false;
                config.Password.RequiredLength = 8;
                config.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromSeconds(10); //TimeSpan.FromMinutes(5);
                config.Lockout.MaxFailedAccessAttempts = 5;
                config.Lockout.AllowedForNewUsers = true;
            })
                .AddDefaultTokenProviders()
                .AddEntityFrameworkStores<DataContext>();

            builder.Services.ConfigureApplicationCookie(options => {
                options.LoginPath = "/Accounts/NotAuthorized";
                options.AccessDeniedPath = "/Accounts/NotAuthorized";
            });

            builder.Services.AddControllersWithViews();
            builder.Services.AddRazorPages().AddRazorRuntimeCompilation();

            builder.Services.AddTransient<SeedDb>();
        }

        private static void ConfigureMiddlewares(WebApplication app) {
            if (!app.Environment.IsDevelopment()) {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseStatusCodePagesWithReExecute("/error/{0}");
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}"
            );
        }

        private static void SeedData(WebApplication app) {
            IServiceScopeFactory scopeFactory = app.Services.GetService<IServiceScopeFactory>();
            using IServiceScope scope = scopeFactory.CreateScope();
            SeedDb service = scope.ServiceProvider.GetService<SeedDb>();
            service.SeedAsync().Wait();
        }
    }
}