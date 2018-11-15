using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ApiClient
{
    public class Startup
    {
        public Startup ( IConfiguration configuration )
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices ( IServiceCollection services )
        {

            services.AddAuthentication (setup =>
                 {
                    // be careful not to use the DefaultAuthenticationScheme or you get an error
                    setup.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    // no cookie here
                    setup.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
                 })
                .AddCookie ()
                .AddOpenIdConnect (setup =>
                 {

                    // clear everything if we want to make it really explicit
                    // otherwise "openid" and "profile" always come in as default scopes
                    setup.Scope.Clear ();

                    // url of server, check out port to make sure
                    setup.Authority = "https://localhost:44301";
                    // make sure it is the same client name as used on the server
                    setup.ClientId = "Hybrid";
                     setup.ClientSecret = "secret";
                    // response
                    setup.ResponseType = "code id_token";

                    // now we add the specific scopes
                    setup.Scope.Add ("openid");
                     setup.Scope.Add ("profile");
                     setup.Scope.Add ("basket");
                    // asking for the extended profile info that we also added via custom claims in the server
                    setup.Scope.Add ("extendedprofile");

                    // by default, there are a lot of claim actions that remove stuff from the returned claims
                    // so, in a bit counterintuitive move, you need to remove the claim action that deletes the amr info
                    setup.ClaimActions.Remove ("amr");

                     setup.ClaimActions.MapUniqueJsonKey ("myclaim", "myclaim");
                     setup.ClaimActions.MapUniqueJsonKey ("anothercustomclaim", "anothercustomclaim");
                     setup.ClaimActions.MapUniqueJsonKey ("userrole", "userrole");

                    // save them to the http context
                    setup.SaveTokens = true;

                     setup.GetClaimsFromUserInfoEndpoint = true;

                 });

            //services.Configure<CookiePolicyOptions>(options =>
            //{
            //    // This lambda determines whether user consent for non-essential cookies is needed for a given request.
            //    options.CheckConsentNeeded = context => true;
            //    options.MinimumSameSitePolicy = SameSiteMode.None;
            //});


            services.AddMvc ().SetCompatibilityVersion (CompatibilityVersion.Version_2_1);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure ( IApplicationBuilder app, IHostingEnvironment env )
        {
            if ( env.IsDevelopment () )
            {
                app.UseDeveloperExceptionPage ();
            }
            else
            {
                app.UseExceptionHandler ("/Home/Error");
                app.UseHsts ();
            }


            app.UseAuthentication ();
            // must absolutely add this so that the default /signin-oidc endpoint
            // gets set up and listening for requests callbacks
            app.UseHttpsRedirection ();
            app.UseStaticFiles ();
            app.UseCookiePolicy ();

            app.UseMvc (routes =>
             {
                 routes.MapRoute (
                     name: "default",
                     template: "{controller=Home}/{action=Index}/{id?}");
             });
        }
    }
}
