using System;
using Autofac;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using PDFBuilder.Config;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Serilog;
using ILogger = Serilog.ILogger;

namespace PDFBuilder
{
    /// <summary>
    /// Application startup processes
    /// </summary>
    public class Startup
    {
        private static readonly ILogger Logger = Log.ForContext<Startup>();

        private IConfiguration Configuration { get; }

        /// <summary>
        /// DI Constructor
        /// </summary>
        /// <param name="configuration">Instance of configuration to load</param>
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        /// <summary>
        /// Method to configure services on application startup
        /// This method gets called by the runtime
        /// </summary>
        /// <param name="services">ServicesCollection object</param>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAntiforgery(options => options.Cookie.Name = "Antiforgery.PDFBuilder");

            services.AddMvc()
                .AddNewtonsoftJson(options =>
                    {
                        options.SerializerSettings.Formatting = Newtonsoft.Json.Formatting.Indented;
                    })
                .SetCompatibilityVersion(CompatibilityVersion.Version_3_0)
                .ConfigureApiBehaviorOptions(options =>
                    {
                        options.InvalidModelStateResponseFactory = context =>
                        {   
                            var requestPath = context.HttpContext.Request.Path;

                                var problemDetails = new ValidationProblemDetails(context.ModelState)
                                {
                                    Title = "One or more model validation errors occurred.",
                                    Status = StatusCodes.Status400BadRequest,
                                    Detail = "See the errors property for details.",
                                    Instance = requestPath
                                };

                                var errorData = JsonConvert.SerializeObject(problemDetails.Errors);
                                Logger.Error($"Error occurred validating model state on {requestPath}: {errorData}");

                                return new BadRequestObjectResult(problemDetails)
                                {
                                    ContentTypes = {"application/problem+json"}
                                };
                            };
                    });

            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Contact = new OpenApiContact()
                    {
                        Email = "benjaminpausnell@gmail.com",
                        Name = "Ben Snell",
                        Url = new Uri("https://www.linkedin.com/in/bensnell/")
                    },
                    Description = "PDFBuilder is a web API used to generate PDF documents",
                    Title = "PDFBuilder"
                });
                options.IncludeXmlComments($"{Program.WorkingDirectory}/PDFBuilder.xml", true);
            });

            Logger.Debug("ConfigureServices complete");
        }

        /// <summary>
        /// Configure Autofac conforming dependency injection container
        /// </summary>
        /// <param name="builder">Autofac ContainerBuilder</param>
        public void ConfigureContainer(ContainerBuilder builder)
        {
            builder.RegisterModule(new PDFBuilderModule(Configuration));

            builder.RegisterInstance(new LoggerFactory().AddSerilog())
                .As<ILoggerFactory>();

            Logger.Debug("Autofac configuration complete");
        }

        /// <summary>
        /// Configure the HTTP request pipeline.
        /// This method gets called by the runtime
        /// </summary>
        /// <param name="app">ApplicationBuilder object</param>
        /// <param name="env">HostingEnvironment object</param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.Use(async (context, next) =>
            {
                context.Response.Headers.Add("X-Xss-Protection", "1");
                context.Response.Headers.Add("X-Frame-Options", "SAMEORIGIN");
                context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
                await next();
            });

            app.UseRouting();
            app.UseCors(builder =>
            {
                builder.AllowAnyOrigin();
                builder.AllowAnyHeader();
                builder.AllowAnyMethod();
            });

            app.UseSwagger();
            app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "PDFBuilder"); });

            app.UseHttpsRedirection();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            }); 

            Logger.Debug("Request pipeline configuration complete");
        }
    }
}
