using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using stage1taskHng.API.Models;
using System.Net;
using System.Net.Http;


namespace stage1taskHng.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HelloController : ControllerBase
    {
        private readonly IpApiClient ipApiClient;
        private readonly HttpClient httpClient;

        public HelloController(IpApiClient ipApiClient, HttpClient httpClient)
        {
            this.ipApiClient = ipApiClient;
            this.httpClient = httpClient;
            
        }


        // GET Hello
        // GET: /api/hello?visitor_name=Mark
        [HttpGet]
        public async Task<IActionResult> Get(CancellationToken ct, [FromQuery] string visitor_name)
        {
            try
            {

                var clientIp = Response.HttpContext.Connection.RemoteIpAddress.ToString();
                if (clientIp == "::1")
                {
                    clientIp = Dns.GetHostEntry(Dns.GetHostName()).AddressList[1].ToString();
                }


                var ipAddress = HttpContext.GetServerVariable("HTTP_X_FORWARDED_FOR") ?? HttpContext.Connection.RemoteIpAddress?.ToString();
                var ipAddressWithoutPort = ipAddress?.Split(':')[0];

                var ipApiResponse = await ipApiClient.Get(ipAddressWithoutPort, ct);


                //var location = ipAddressWithoutPort;
                var City = ipApiResponse?.city;
                var Longitude = ipApiResponse?.lon.GetValueOrDefault();
                var Latitude = ipApiResponse?.lat.GetValueOrDefault();


                //var client = new HttpClient();
                //var apiKey = "b88e40b34150441e921153112240107";
                string apiUrl = $"https://api.weatherapi.com/v1/current.json?key=b88e40b34150441e921153112240107&q=nigeria&aqi=no";

                HttpResponseMessage response = await httpClient.GetAsync(apiUrl);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();

                JObject weatherData = JObject.Parse(responseBody);
                string temperature = weatherData["current"]["temp_c"].ToString();

                var greeting = $"Hello, {visitor_name}!, the temperature is {temperature} degrees Celsius in {City}";

                var visitorInfo = new VisitorInfo
                {
                    ClientIp = clientIp,
                    Location = City,
                    Greeting = greeting
                };

                return Ok(visitorInfo);

            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

    }
}
