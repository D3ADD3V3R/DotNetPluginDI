using System.Reflection;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

namespace SchichtberichtInterface
{

    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            Env = env;
        }

        public IConfiguration Configuration;
        public IWebHostEnvironment Env;
        private IServiceCollection collection;

        private void SetupSerilog(object sender = null)
        {
            var builder = new ConfigurationBuilder();
            builder.SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{(!Env.IsDevelopment() ? "Production" : "Development")}.json", optional: true)
                .AddEnvironmentVariables();

            var newLogger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Build())
                .Enrich.FromLogContext()
                .Enrich.WithAssemblyName()
                .WriteTo;


            if (Env.IsDevelopment())
            {
                Log.Logger = newLogger.Console(theme: AnsiConsoleTheme.Literate, outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}][{AssemblyName}-{SourceContext:l}] {Message:lj}{NewLine}{Exception}").CreateLogger();
            }
            else
            {
                Log.Logger = newLogger.File("loggs/log.txt", outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}][{AssemblyName}-{SourceContext:l}] {Message:lj}{NewLine}{Exception}", rollingInterval: RollingInterval.Day).CreateLogger();
            }

            var reloadToken = Configuration.GetReloadToken();
            reloadToken.RegisterChangeCallback(SetupSerilog, null);
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            SetupSerilog(null);

            services.AddRouting();

            services
                .AddControllers()
                .AddNewtonsoftJson(option =>
                {
                    option.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                });

            services.AddHealthChecks();

#if DEBUG
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "Anna Barebone Service", Version = "v1" });

                var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
            });
#endif

        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)//, PluginRouterContainer routerContainer, PluginManager pluginManager)
        {
            if (env.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Anna Barebone Service v1"));
            }
            else
            {
                app.UseHsts();
                //app.UseHttpsRedirection();
                app.UseStaticFiles();
            }

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            if (env.IsDevelopment())
            {
                app.UseHttpLogging();
                app.UseHealthChecks("/healtz");
            }

            // Here is the Exception Thrown
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });


        }

    }
}
