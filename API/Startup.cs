using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using API.BusinessLogic;
using DataAccess.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Polly;

namespace API
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(sp => StartupExtensions.GetSqlServerConnectionString());
            services.AddSingleton((IConfigurationRoot)Configuration);

            services.AddTransient<IGeneralDataRepository, GeneralDataRepository>();
            services.AddTransient<IUserRepository, UserRepository>();

            services.AddCors(opt =>
            {
                opt.AddPolicy("CorsPolicy", policy =>
                {
                    policy
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .WithExposedHeaders("WWW-Authenticate", "content-disposition")
                        .WithOrigins(
                            "http://localhost:3000",
                            "http://localhost:5000",
                            "https://localhost:5001",
                            "http://localhost:8080",
                            "https://doublee-sample-20210405-ei3he32sea-uc.a.run.app/*",
                            "http://doublee-sample-20210405-ei3he32sea-uc.a.run.app/*"
                         )
                        .SetIsOriginAllowedToAllowWildcardSubdomains()
                        .AllowCredentials();
                });
            });

            // var server = "35.242.156.191"; // public ip
            // var name = "doublee-sample-20210405:europe-west2:doublee-sql"; // not using

            // var database = "sample";
            // var username = "sqlserver";
            // var password = "sogismhI0P2aM6kl";

            // var connString = $@"Server={server};Initial Catalog={database};Integrated Security=false;uid={username};password={password};";

            // // this was just to test passing in from the configuration file. appsettings.json
            // var host = Configuration["DB_HOST"];
            // var usr = Configuration["DB_USER"];
            // var pwd = Configuration["DB_PASS"];
            // var db = Configuration["DB_NAME"];

            // params should not be needed when the enviroment variables are working. if the are needed at all.
            var connectionString = StartupExtensions.GetSqlServerConnectionString();

            var connString = connectionString.ConnectionString;

            Configuration["ConnectionStrings:DefaultConnection"] = connString;

            services.AddControllers();

            // services.AddMvc();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "API", Version = "v1" });
            });

        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1"));
            }

            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseCors("CorsPolicy");

            app.UseHttpsRedirection();
            app.UseAuthorization();

            // app.UseMvc();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapFallbackToController("Index", "Fallback");
            });
        }
    }

    static class StartupExtensions
    {
        public static void OpenWithRetry(this DbConnection connection) =>
            // [START cloud_sql_sqlserver_dotnet_ado_backoff]
            Policy
                .Handle<SqlException>()
                .WaitAndRetry(new[]
                {
                    TimeSpan.FromSeconds(1),
                    TimeSpan.FromSeconds(2),
                    TimeSpan.FromSeconds(5)
                })
                .Execute(() => connection.Open());
        // [END cloud_sql_sqlserver_dotnet_ado_backoff]

        public static void InitializeDatabase()
        {
            var connectionString = GetSqlServerConnectionString();
            Console.WriteLine("create table");

            using (DbConnection connection = new SqlConnection(connectionString.ConnectionString))
            {
                connection.OpenWithRetry();
                using (var createTableCommand = connection.CreateCommand())
                {
                    // Create the 'votes' table if it does not already exist.
                    createTableCommand.CommandText = @"
                    IF OBJECT_ID(N'dbo.votes', N'U') IS NULL
                    BEGIN
                        CREATE TABLE dbo.votes(
                        vote_id INT NOT NULL IDENTITY(1, 1) PRIMARY KEY,
                        time_cast datetime NOT NULL,
                        candidate CHAR(6) NOT NULL)
                    END";
                    createTableCommand.ExecuteNonQuery();
                }
            }
        }

        public static SqlConnectionStringBuilder GetSqlServerConnectionString()
        {
            // [START cloud_sql_sqlserver_dotnet_ado_connection_tcp]
            // Equivalent connection string:
            // "User Id=<DB_USER>;Password=<DB_PASS>;Server=<DB_HOST>;Database=<DB_NAME>;"

            //var aSource = Environment.GetEnvironmentVariable("DB_HOST");
            Console.WriteLine("DB_HOST", aSource);

            // var connectionString = new SqlConnectionStringBuilder()
            // {
            //     // Remember - storing secrets in plain text is potentially unsafe. Consider using
            //     // something like https://cloud.google.com/secret-manager/docs/overview to help keep
            //     // secrets secret.
            //     DataSource = Environment.GetEnvironmentVariable("DB_HOST"),     // e.g. '127.0.0.1'
            //     // Set Host to 'cloudsql' when deploying to App Engine Flexible environment
            //     UserID = Environment.GetEnvironmentVariable("DB_USER"),         // e.g. 'my-db-user'
            //     Password = Environment.GetEnvironmentVariable("DB_PASS"),       // e.g. 'my-db-password'
            //     InitialCatalog = Environment.GetEnvironmentVariable("DB_NAME"), // e.g. 'my-database'

            //     // The Cloud SQL proxy provides encryption between the proxy and instance
            //     Encrypt = false,
            // };

            var connectionString = new SqlConnectionStringBuilder()
            {
                // Remember - storing secrets in plain text is potentially unsafe. Consider using
                // something like https://cloud.google.com/secret-manager/docs/overview to help keep
                // secrets secret.
                DataSource = "127.0.0.1",     // e.g. '127.0.0.1'
                // Set Host to 'cloudsql' when deploying to App Engine Flexible environment
                UserID = "sqlserver",         // e.g. 'my-db-user'
                Password = "sogismhI0P2aM6kl",       // e.g. 'my-db-password'
                InitialCatalog = "sample", // e.g. 'my-database'

                // The Cloud SQL proxy provides encryption between the proxy and instance
                Encrypt = false,
            };
            connectionString.Pooling = true;
            // [START_EXCLUDE]
            // The values set here are for demonstration purposes only. You 
            // should set these values to what works best for your application.
            // [START cloud_sql_sqlserver_dotnet_ado_limit]
            // MaximumPoolSize sets maximum number of connections allowed in the pool.
            connectionString.MaxPoolSize = 5;
            // MinimumPoolSize sets the minimum number of connections in the pool.
            connectionString.MinPoolSize = 0;
            // [END cloud_sql_sqlserver_dotnet_ado_limit]
            // [START cloud_sql_sqlserver_dotnet_ado_timeout]
            // ConnectionTimeout sets the time to wait (in seconds) while
            // trying to establish a connection before terminating the attempt.
            connectionString.ConnectTimeout = 15;
            // [END cloud_sql_sqlserver_dotnet_ado_timeout]
            // [START cloud_sql_sqlserver_dotnet_ado_lifetime]
            // ADO.NET connection pooler removes a connection
            // from the pool after it's been idle for approximately
            // 4-8 minutes, or if the pooler detects that the
            // connection with the server no longer exists.
            // [END cloud_sql_sqlserver_dotnet_ado_lifetime]
            // [END_EXCLUDE]
            return connectionString;
            // [END cloud_sql_sqlserver_dotnet_ado_connection_tcp]
        }
    }
}