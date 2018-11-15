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
        public void ConfigureServices ( IServiceCollection services )
        {
            // keeps data in IDistributedCache, if we dont say anything, the default is InMemory cache, not distributed...
            services.AddOidcStateDataFormatterCache();

            // you can add additional authentication handlers
            //services.AddAuthentication().AddOpenIdConnect("aad", "Azure AD", setup =>
            //{
            //    setup.Authority = "https://login.windows.net/____tenantname____";
            //    setup.ClientSecret = "wha'ever";
            //    setup.ClientId = "wha'ever";
            //    setup.SingInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
            //    setup.SingOutScheme = IdentityServerConstants.SignoutScheme;
            //});
            // in AddIdentityServer we can configure lots of stuff for the server  via lambda
            services.AddIdentityServer (setup =>
                 {

                     setup.Authentication.CookieLifetime = new TimeSpan (0, 1, 0, 0);
                     setup.Authentication.CookieSlidingExpiration = true;
                     // lots of configuration options...
                     //setup.Discovery.ShowApiScopes

                 }).AddInMemoryApiResources (new []
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
                .AddInMemoryIdentityResources (new List<IdentityResource> ()
                {
                    new IdentityResources.OpenId(),
                    new IdentityResources.Profile(),
                    new IdentityResources.Email(),
                    // add custom claims 
                    new IdentityResource("extendedprofile", new []
                    {
                        "userrole", "anothercustomclaim", "myclaim"
                    })

                })
                .AddInMemoryClients (new []
                {
                    new Client()
                    {
                        // make sure it is the same client name as used on the client
                        ClientId = "Hybrid",
                        ClientSecrets = {new Secret("secret".Sha256())},
                        ClientName = "My Own Hybrid Client",
                        AccessTokenType = AccessTokenType.Jwt,
                        AccessTokenLifetime =  6000,
                        IdentityTokenLifetime =  6000,
                        AllowOfflineAccess = true,
                        AuthorizationCodeLifetime = 100,
                        PostLogoutRedirectUris =
                        {
                            "https://localhost:44317/signout-callback-oidc"
                        },
                        FrontChannelLogoutUri = "https://localhost:44317/home/FrontLogout",
                        AllowedScopes =
                        {
                            ApiName,
                            IdentityServerConstants.StandardScopes.Profile,
                            IdentityServerConstants.StandardScopes.OpenId,
                            IdentityServerConstants.StandardScopes.Email,
                            // include it here or you get an unauthorized client error message
                            "extendedprofile"
                        },
                        AllowedGrantTypes = new []{
                            GrantType.Hybrid
                        },
                        // dont forget this and make sure the port is that of the client
                        // so that the server can call the client back
                        // the signin-oidc is up because we included RedirectUris on the client
                         RedirectUris = new [] { "https://localhost:44317/signin-oidc" },
                        AlwaysIncludeUserClaimsInIdToken = true

                    }
                })
                .AddTestUsers (new List<TestUser> ()
                {
                    new TestUser()
                    {
                        Username = "alice",
                        Password = "alice",
                        SubjectId = Guid.NewGuid().ToString(),
                        IsActive = true,
                        Claims = new[]
                        {
                            new Claim(JwtClaimTypes.Name, "alice"),
                            new Claim(JwtClaimTypes.WebSite, "mywebsite.com"),
                            new Claim(JwtClaimTypes.Email, "alice@mywebsite.com"),
                            // add any claims you want
                            new Claim("myclaim", "John Doe"),
                            new Claim("anothercustomclaim", "Manager"),
                            new Claim("userrole", "admin"),
                        }
                    }
                })
                //    .AddDeveloperSigningCredential();    // this is needed only if we run with no cert
                // if we install a cert (in my, in localmachine), then we can sign
                .AddSigningCredential (OpenCertFromStore ());


            services.AddMvc ();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure ( IApplicationBuilder app, IHostingEnvironment env )
        {
            app.UseIdentityServer ();
            app.UseStaticFiles ();
            // always last middleware and set up with default route scheme 
            app.UseMvcWithDefaultRoute ();

        }

        private X509Certificate2 OpenCertFromStore ()
        {
            /*
             *
             * to install this server run this line from powershell admin
             * 
             * $todaydt = Get-Date
             * $3years = $todaydt.AddYears(3)
             * $CertPassword = ConvertTo-SecureString -String “YourPassword” -Force –AsPlainText
             * New-SelfSignedCertificate -dnsname workshop.plainconcepts.com -notafter $3years -CertStoreLocation cert:\LocalMachine\My -KeyUsage DigitalSignature
             *
             * change 'workshop.plainconcepts.com' to whatever you want
             *
             * then copy the thumbprint here from the cert once installed
             *
             * the other argument is false since this is a selfsigned cert, not one actually
             * signed from a trusted entity
             *
             */


            using ( var store = new X509Store (StoreName.My, StoreLocation.LocalMachine) )
            {
                store.Open (OpenFlags.ReadOnly);
                var cert = store.Certificates.Find (X509FindType.FindByThumbprint,
                    "F9069BB4B703DC352D5639517D71BA12D3FEB136", false);
                return cert [0];
            }
        }
    }
}
