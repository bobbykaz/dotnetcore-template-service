using BK.DotNet.Api.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BK.DotNet.Template.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Weather()
        {
            //URL copied from launchSettings.json
            var client = new BK.DotNet.Api.Client.Client("https://localhost:5001", null, Log.Logger);
            var weathers = await client.GetWeatherForecastsAsync();

            return View(weathers);
        }
    }
}
