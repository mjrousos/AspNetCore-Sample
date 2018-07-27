// Licensed under the MIT license. See LICENSE file in the samples root for full license information.

using Autofac;
using Autofac.Extensions.DependencyInjection;
using CustomersAPI.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RequestCorrelation;
using Swashbuckle.AspNetCore.Swagger;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading.Tasks;

namespace CustomersAPI
{
    public class Startup
    {
        private IContainer _autofacContainer;

        // Gets the application's IConfiguration object from the
        // dependency injection container
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime.
        // Use this method to add services to the dependency injection container.
        //
        // If using the built-in ASP.NET Core dependency injection container, this
        // method can return void. Otherwise, it should return an IServiceProvider containing
        // the non-default container used.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            // Localization: Here we are adding in the localizaton service which will enable using IStringLocalizer in the CustomersController
            services.AddLocalization(options => options.ResourcesPath = "Resources");

            // Add framework services.
            services.AddMvc();

            // Add Swashbuckle service
            services.AddSwaggerGen(c =>
            {
                // Swagger options go here
                c.SwaggerDoc("v1", new Info { Title = "My API", Version = "v1" });
                c.DescribeAllEnumsAsStrings();
            });

            // Add Entity Framework Customers data provider
            services.AddDbContext<EFCustomersDataProvider>(options =>
                options.UseInMemoryDatabase("CustomerDataDb"));

            // Dependency Injection: This could be added directly to services (via AddScoped),
            // but in this sample we're adding it to the Autofac container in order to
            // demonstrate that interaction.
            // services.AddScoped<ICustomersDataProvider, EFCustomersDataProvider>();

            // Setup Autofac integration
            var builder = new ContainerBuilder();

            // Autofac registration calls can go here.
            builder.RegisterType<EFCustomersDataProvider>().As<ICustomersDataProvider>().InstancePerLifetimeScope();

            // If the container requires many registrations or registrations that are shared with other
            // containers, builder.RegisterModule is a useful API.
            // builder.RegisterModule(new MyAutofacModule);

            // Dependency Injection: Adds ASP.NET Core-registered services to the Autofac container
            builder.Populate(services);

            // Dependency Injection: Storing the container in a field so that other components can make use of it.
            // In many scenarios, this isn't necessary. builder.Build() can often be returned directly.
            _autofacContainer = builder.Build();

            // Dependency Injection: Return the DI container to be used by this web application.
            return new AutofacServiceProvider(_autofacContainer);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            // Logging: There is a logger created here that will log messages from the timing middleware to
            //          all the loggers contained in the LoggerFactory.
            // Middleware: Trivial middleware registration to demonstrate how
            //             to add a step to the app's HTTP request processing pipeline.
            // Middleware: Note that middleware will execute in the pipeline in the order it is registered.
            //             Since this trivial middleware is meant to time how long the entire processing
            //             of request takes, it is included very early in the middleware pipeline.
            var timingLogger = loggerFactory.CreateLogger("CustomersAPI.Startup.TimingMiddleware");

            app.Use(async (HttpContext context, Func<Task> next) =>
            {
                // Middleware: Logic to run regarding the request prior to invoking more middleware.
                //             This logic should be followed by either a call to next() (to invoke the next
                //             piece of middleware) or to context.Response.WriteAsync/SendFileAsync
                //             without calling next() (which will end the middleware pipeline and begin
                //             travelling back up the middleware stack.
                var timer = new Stopwatch();
                timer.Start();

                // Middleware: Calling the next delegate will invoke the next piece of middleware
                await next();

                // Middleware: Code after 'next' will usually run after another piece of middleware
                //             has written a response, so context.Response should not be written to here.
                timer.Stop();
                timingLogger.LogInformation("Request to {RequestMethod}:{RequestPath} processed in {ElapsedMilliseconds} ms", context.Request.Method, context.Request.Path, timer.ElapsedMilliseconds);
            });

            // Localization: Here we are building a list of supported cultures which will be used in the
            //               RequestLocalizationOptions in the app.UseRequestLocalization call below.
            var supportedCultures = new[]
              {
                    // Localization: Notice that neutral cultures (like 'es') are
                    //               listed after specific cultures. This best practice
                    //               ensures that if a particular culture request could
                    //               be satisifed by either a supported specific culture
                    //               or a supported neutral culture, the specific culture
                    //               will be preferred.
                    new CultureInfo("en-US"),
                    new CultureInfo("fr-FR"),
                    new CultureInfo("es"),
              };

            // Localization: Here we are configuring the RequstLocalization including setting the supported cultures from above
            //               in the RequestLocalizationOptions. We are also setting the default request culture to be used
            //               for current culture. These options will be used wherever we request localized strings.
            //               For more information see https://docs.asp.net/en/latest/fundamentals/localization.html
            //
            //               Request locale will be read from an Accept-Language header, a culture query string, or
            //               an ASP.NET Core culture cookie. Other options can be supported with custom RequestCultureProvider
            var requestLocalizationOptions = new RequestLocalizationOptions
            {
                DefaultRequestCulture = new RequestCulture("en-US"),

                // Formatting numbers, dates, etc.
                SupportedCultures = supportedCultures,

                // UI strings that we have localized.
                SupportedUICultures = supportedCultures
            };

            // Localization: If needed, it's straight-forward to add custom request culture providers which can
            //               extract requested culture from an HTTP context using arbitrary business logic.
            // requestLocalizationOptions.RequestCultureProviders.Insert(0, new CustomRequestCultureProvider(async httpContext =>
            // {
            //    return new ProviderCultureResult("en");
            // }));

            app.UseRequestLocalization(requestLocalizationOptions);

            app.UseStaticFiles();

            // Middleware: More complex middleware can be added with the app.UseMiddleware method
            app.UseMiddleware<RequestCorrelationMiddleware>();

            // Routing: Convention-based routes can be defined on an IRouteBuilder
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1"));
        }
    }
}
