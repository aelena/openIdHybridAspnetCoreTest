namespace ProtectedApi
{
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc;

    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;


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

            //services.AddAuthorization(options =>
            //{
            //    options.AddPolicy("myclaim", policy =>
            //        policy.Requirements.Add(new DefaultRequirement()));
            //});

            //services.AddSingleton<IAuthorizationHandler, DefaultAuthorizationHandler>()

            services.AddAuthentication(setup =>
                {
                    setup.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    setup.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(setup =>
                {
                    setup.Authority = "https://localhost:44301/";
                    setup.Audience = "basket";
                    setup.RequireHttpsMetadata = true;
                    var validators = setup.SecurityTokenValidators;
                });

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseMvc();
        }
    }
}
