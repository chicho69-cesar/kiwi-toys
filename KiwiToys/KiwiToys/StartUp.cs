using KiwiToys.Data;
using KiwiToys.Data.Entities;
using KiwiToys.Helpers;
using KiwiToys.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Vereyon.Web;

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
            builder.Services.AddAutoMapper(typeof(Program));
            builder.Services.AddFlashMessage();

            builder.Services.AddScoped<IApiService, ApiService>();
            
            builder.Services.AddScoped<IBlobHelper, BlobHelper>();
            builder.Services.AddScoped<IBodyMailHelper, BodyMailHelper>();
            builder.Services.AddScoped<ICombosHelper, CombosHelper>();
            builder.Services.AddScoped<IGetLocation, GetLocation>();
            builder.Services.AddScoped<IMailHelper, MailHelper>();
            builder.Services.AddScoped<IOrdersHelper, OrdersHelper>();
            builder.Services.AddScoped<IUserHelper, UserHelper>();
            builder.Services.AddScoped<IContactService, ContactService>();
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