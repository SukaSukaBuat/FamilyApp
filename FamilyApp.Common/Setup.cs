using FamilyApp.Common.Databases.FamilyDb;
using FamilyApp.Common.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace FamilyApp.Common
{
    public static class Setup
    {
        public static void ConfigureCommonServices(this WebApplicationBuilder builder)
        {
            //register dbcontext
            var dataSourceBuilder = new NpgsqlDataSourceBuilder(builder.Configuration.GetConnectionString("FamilyDbConnStr"));
            dataSourceBuilder.MapEnum<Gender>();
            dataSourceBuilder.MapEnum<OuthProvider>();
            var dataSource = dataSourceBuilder.Build();

            builder.Services.AddDbContext<FamilyDbContext>(options =>
            {
                options.UseNpgsql(dataSource, x => x.MigrationsAssembly("FamilyApp.Common"));
                options.UseSnakeCaseNamingConvention();
            });

            //add identity api
            builder.Services.AddIdentityApiEndpoints<TblUser>(e =>
            {
            })
                .AddRoles<IdentityRole<Guid>>()
                .AddEntityFrameworkStores<FamilyDbContext>();


            builder.Services.Configure<IdentityOptions>(options =>
            {
                // Password settings.
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
                options.Password.RequiredLength = 6;
                options.Password.RequiredUniqueChars = 1;

                // Lockout settings.
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;

                // User settings.
                options.User.AllowedUserNameCharacters =
                "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedEmail = true;
                options.SignIn.RequireConfirmedAccount = true;
            });

            builder.Services.AddScoped<IGenericRepository, GenericRepository>();
        }
    }
}
