namespace AuthServer
{

    using IdentityModel;
    using IdentityServer4;
    using IdentityServer4.Models;
    using IdentityServer4.Test;

    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.DependencyInjection;

    using System;
    using System.Security.Claims;
    using System.Collections.Generic;
    using System.Security.Cryptography.X509Certificates;


    public class Startup
    {


        // https://localhost:44301/.well-known/openid-configuration
        // https://localhost:44301/.well-known/openid-configuration/jwks


        private const string ApiName = "basket";

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddIdentityServer().AddInMemoryApiResources(new[]
                {
                    new ApiResource()
                    {
                        Name = ApiName,
                        Description = $"description for {ApiName}",
                        DisplayName = ApiName,
                        Scopes = new[]
                        {
                            new Scope(ApiName, $"scope 1 for {ApiName}", new[] {"myclaim", "anothercustomclaim"}),
                        }
                    }
                })
                .AddInMemoryIdentityResources(new List<IdentityResource>()
                {
                    new IdentityResources.OpenId(),
                    new IdentityResources.Profile(),
                    new IdentityResources.Email()
                })
                .AddInMemoryClients(new[]
                {
                    new Client()
                    {
                        ClientId = "Hybrid",
                        ClientSecrets = {new Secret("secret".Sha256())},
                        ClientName = "My Own Hybrid Client",
                        AllowedScopes =
                        {
                            ApiName,
                            IdentityServerConstants.StandardScopes.Profile,
                            IdentityServerConstants.StandardScopes.OpenId,
                            IdentityServerConstants.StandardScopes.Email
                        },
                        AllowedGrantTypes = new []{
                            GrantType.Hybrid
                        },
                        RedirectUris = new [] { "https://localhost:44317/signin-oidc" }
                        
                    }
                })
                .AddTestUsers(new List<TestUser>()
                {
                    new TestUser()
                    {
                        Username = "alice1",
                        Password = "workshop01*",
                        SubjectId = Guid.NewGuid().ToString(),
                        IsActive = true,
                        Claims = new[]
                        {
                            new Claim(JwtClaimTypes.Name, "alice1"),
                            new Claim(JwtClaimTypes.WebSite, "mywebsite.com"),
                            new Claim(JwtClaimTypes.Email, "alice1@mywebsite.com"),
                            new Claim("myclaim", "John Doe"),
                            new Claim("anothercustomclaim", "Manager"),
                        }
                    }
                })
                //    .AddDeveloperSigningCredential();    // this is needed only if we run with no cert
                .AddSigningCredential(OpenCertFromStore());


            services.AddMvc();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseIdentityServer();
            app.UseStaticFiles();
            // always last middleware and set up with default route scheme 
            app.UseMvcWithDefaultRoute();

        }

        private X509Certificate2 OpenCertFromStore()
        {
            using (var store = new X509Store(StoreName.My, StoreLocation.LocalMachine))
            {
                store.Open(OpenFlags.ReadOnly);
                var cert = store.Certificates.Find(X509FindType.FindByThumbprint,
                    "F9069BB4B703DC352D5639517D71BA12D3FEB136", false);
                return cert[0];
            }
        }
    }
}
