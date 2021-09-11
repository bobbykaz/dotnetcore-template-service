using bk_dotnet_template.Helpers;
using Certes;
using FluffySpoon.AspNet.EncryptWeMust;
using FluffySpoon.AspNet.EncryptWeMust.Certes;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Serilog;
using System;
using System.Linq;

namespace bk_dotnet_template
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console()
                .CreateLogger();

            Log.Logger.Information($"App Startup");

            services.AddMvc(options => {
                options.EnableEndpointRouting = false;
                options.Filters.Add(typeof(LoggingActionFilter));
            });
            services.AddControllers(options => {
                options.Filters.Add(typeof(LoggingActionFilter));
            });
            if (bool.Parse(Configuration["Swagger:Enabled"]))
            {
                services.AddSwaggerGen(c =>
                {
                    c.SwaggerDoc("v1", new OpenApiInfo { Title = "TBT API", Version = "v1" });
                });
            }
            //LetsEncrypt
            if (bool.Parse(Configuration["LetsEncrypt:Enabled"]))
            {
                var certEmail = Configuration["LetsEncrypt:Email"];
                var certCountry = Configuration["LetsEncrypt:CountryName"];
                var certLocality = Configuration["LetsEncrypt:Locality"];
                var certState = Configuration["LetsEncrypt:State"];
                var certOrg = Configuration["LetsEncrypt:Organization"];
                var certOrgUnit = Configuration["LetsEncrypt:OrganizationUnit"];
                var certUseStaging = bool.Parse(Configuration["LetsEncrypt:UseStaging"]);
                var certDomains = Configuration.GetSection("LetsEncrypt:Domains").GetChildren().Select(s => s.Value).ToArray();
                Log.Logger.Information($"App Startup - Cert info: {certEmail} {certCountry} / {certLocality} / {certState} [{certOrg} {certOrgUnit}] UseStage: {certUseStaging} - Domains: {certDomains}");

                services.AddFluffySpoonLetsEncrypt(new LetsEncryptOptions()
                {
                    Email = certEmail, //LetsEncrypt will send you an e-mail here when the certificate is about to expire
                    UseStaging = certUseStaging, //switch to true for testing
                    Domains = certDomains,
                    TimeUntilExpiryBeforeRenewal = TimeSpan.FromDays(30), //renew automatically 30 days before expiry
                    TimeAfterIssueDateBeforeRenewal = TimeSpan.FromDays(7), //renew automatically 7 days after the last certificate was issued
                    CertificateSigningRequest = new CsrInfo() //these are your certificate details
                    {
                        CountryName = certCountry,
                        Locality = certLocality,
                        Organization = certOrg,
                        OrganizationUnit = certOrgUnit,
                        State = certState
                    },
                    RenewalFailMode = RenewalFailMode.LogAndRetry
                });

                //the following line tells the library to persist challenges in-memory. challenges are the "/.well-known" URL codes that LetsEncrypt will call.
                services.AddFluffySpoonLetsEncryptMemoryChallengePersistence();
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            if (bool.Parse(Configuration["LetsEncrypt:Enabled"]))
            {
                app.UseFluffySpoonLetsEncrypt();
            }

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            if (bool.Parse(Configuration["Swagger:Enabled"]))
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
                });
            }
        }
    }
}
