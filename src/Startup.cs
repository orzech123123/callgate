using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using CallGate.Data;
using CallGate.DependencyInjection;
using CallGate.Extensions;
using CallGate.Repositories;
using CallGate.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using CallGate.Filters;
using CallGate.Services.Email;
using CallGate.Services.Helper;
using CallGate.Services.Seed;
using CallGate.Services.Socket;
using Microsoft.AspNetCore.SpaServices.Webpack;
using Microsoft.Extensions.Hosting;

namespace CallGate
{
    public class Startup
    {
        private IConfiguration Configuration { get; }
        private IWebHostEnvironment Env { get; set; }
        
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            Env = env;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            RegisterServices(services);

            IMvcBuilder mvc = services.AddControllersWithViews(options =>
            {
                options.Filters.Add(typeof(ApiExceptionFilter));
                options.EnableEndpointRouting = false;
            });
            
            IndentJsonForDevelopment(mvc);

            services.Configure<ConfigurationManager>(Configuration.GetSection("AppSettings"));
            services.Configure<RethinkDbOptions>(Configuration.GetSection("RethinkDb"));
            services.Resolve<IJwtHelper>().AddJwtAuthentication(services);
        }
        
        private void RegisterServices(IServiceCollection services)
        {
            services.AddDbContext<DatabaseContext>(options => options.UseSqlServer(Configuration.GetConnectionString("CallGateConnectionString")));
            services.AddAutoMapper(typeof(Startup));
            services.AddTransient(typeof(IRepository<>), typeof(Repository<>));
            DependencyInjectionRegisterer.RegisterAssemblies(services);

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();

            services.AddScoped<IUrlHelper>(provider =>
            {
                var actionContext = provider.GetService<IActionContextAccessor>().ActionContext;
                return new UrlHelper(actionContext);
            });
            
            services.AddTransient(typeof(IMailService), Env.IsDevelopment() ? typeof(SandboxConsoleMailService) : typeof(EmailLabsMailService));
            
            if (Env.IsDevelopment())
            {
                services.AddCors(options =>
                {
                    options.AddPolicy("CorsPolicy",
                        builder => builder.AllowAnyOrigin()
                            .AllowAnyMethod()
                            .AllowAnyHeader());
                });
            }
        }
        
        private void IndentJsonForDevelopment(IMvcBuilder mvc)
        {
            if(Env.IsDevelopment())
            {
                mvc.AddNewtonsoftJson(jsonFormat => jsonFormat.SerializerSettings.Formatting = Formatting.Indented);
            }
        }

        public void Configure(
            IApplicationBuilder app,
            IDatabaseManager databaseManager,
            ISeedManager seedManager,
            ILoggerFactory loggerFactory,
            IRethinkDbManager rethinkDbManager,
            ISocketOptions socketOptions,
            ISocketMiddleware socketMiddleware)
        {
            if (Env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseWebpackDevMiddleware(new WebpackDevMiddlewareOptions
                {
                    HotModuleReplacement = true
                });

                app.UseCors("CorsPolicy");
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            loggerFactory.AddFile("Logs/logs-{Date}.txt", LogLevel.Error);

            app.UseStaticFiles();
            
            app.UseAuthentication();
            
            app.UseWebSockets(socketOptions.GetOptions());
            app.Use(async (context, next) => await socketMiddleware.Invoke(context, next));

            //app.UseMvc(routes =>
            //{
            //    routes.MapRoute(
            //        name: "default",
            //        template: "{controller=Home}/{action=Index}/{id?}");

            //    routes.MapSpaFallbackRoute(
            //        name: "spa-fallback",
            //        defaults: new { controller = "Home", action = "Index" });
            //});

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "Default",
                    pattern: "{controller=default}/{action=Index}/{id?}");

                endpoints.MapFallbackToController("Index", "Home");
            });

            rethinkDbManager.EnsureDatabaseCreated();
            databaseManager.EnsureDatabaseCreated();

            seedManager.Seed();

            app.UseMvcWithDefaultRoute();
        }
    }
}
