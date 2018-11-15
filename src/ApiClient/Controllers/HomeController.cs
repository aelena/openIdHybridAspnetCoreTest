using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ApiClient.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;

namespace ApiClient.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [Authorize]
        public async Task<IActionResult> Tokens()
        {

            var claims = HttpContext.User.Claims.ToList();
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {await HttpContext.GetTokenAsync("access_token")}");
                var response = await client.GetAsync("http://localhost:44391/api/basket");
                if (!response.IsSuccessStatusCode)
                    ViewData["Message"] = $"Error calling API {response.StatusCode}";
                else
                    ViewData["Message"] = $"Success!";
            }

            return View(new TokenModel()
            {
                AccessToken = await HttpContext.GetTokenAsync("access_token"),
                IdToken = await HttpContext.GetTokenAsync("id_token")
            });
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
