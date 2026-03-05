using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Repository.Api.Data;
using Repository.Api.Repositories;
using Repository.Api.Repositories.Interfaces;

namespace Repository.Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers();

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        //builder.Services.AddCors(options =>
        //{
        //    options.AddDefaultPolicy(builder =>
        //    {
        //        builder.AllowAnyOrigin();
        //        builder.AllowAnyHeader();
        //        builder.AllowAnyMethod();
        //    });
        //});

        builder.Services.AddDbContext<AppDbContext>(options =>
        {
            var sqlConnection = builder.Configuration.GetConnectionString("DefaultConnection");
            options.UseSqlServer(sqlConnection, sqlconfig =>
            {
                sqlconfig.EnableRetryOnFailure(3);
                sqlconfig.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);

                // Note: To customize the name of the migration history table and change the schema, uncomment the line below
                //sqlconfig.MigrationsHistoryTable("Migrations", "dbo"); // Useful if there are multiple DbContexts in the same database
            });
            options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);

            options.LogTo(Console.WriteLine, [RelationalEventId.CommandExecuted]);
            options.EnableSensitiveDataLogging();
        });

        // Dependency Injection for Repositories and Unit of Work
        builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(builder =>
            {
                builder.AllowAnyHeader().AllowAnyMethod()
                .SetIsOriginAllowed(_ => true)  // allow any origin
                //.AllowCredentials() // allow credentials
                //.WithExposedHeaders(HeaderNames.ContentDisposition) // to allow file downloads
                ;
            });
        });

        var app = builder.Build();

        // To enable working behind reverse proxies
        app.UseForwardedHeaders(new()
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
            KnownProxies = { }
        });

        app.UseHttpsRedirection();

        app.UseSwagger();
        app.UseSwaggerUI(options => options.SwaggerEndpoint("/swagger/v1/swagger.json", $"{app.Environment.ApplicationName} v1"));

        app.UseRouting();
        app.UseCors();

        app.MapControllers();
        app.Run();
    }
}