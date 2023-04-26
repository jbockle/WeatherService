using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace WeatherService.Controllers
{
    /*
     * openweathermap api docs https://openweathermap.org/current
     */

    [Route("api/[controller]")]
    [ApiController]
    public class WeatherController : ControllerBase
    {
        [HttpGet("{zipCode}")]
        public async Task<ActionResult> GetWeather(string zipCode)
        {
            string apiKey = "eda95fa59c62ef331e1c65e296563af6";
            string apiBaseUrl = "http://api.openweathermap.org/data/2.5/weather?zip={0}&appid={1}&units=imperial";

            using (HttpClient client = new HttpClient())
            {
                string apiUrl = string.Format(apiBaseUrl, zipCode, apiKey);

                HttpResponseMessage response = await client.GetAsync(apiUrl);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    JObject jsonResult = JObject.Parse(content);
                    return Ok(new
                    {
                        City = jsonResult["name"],
                        Temperature = jsonResult["main"]["temp"].Value<double>(),
                        Weather = jsonResult["weather"][0]["main"].Value<string>()
                    });
                }
                else
                {
                    return BadRequest("Invalid ZIP code or unable to fetch weather data.");
                }
            }
        }

        [HttpGet("forecast/{zipCode}")]
        public async Task<ActionResult> GetForecast(string zipCode)
        {
            string apiKey = "eda95fa59c62ef331e1c65e296563af6";
            string apiBaseUrl = "http://api.openweathermap.org/data/2.5/forecast?zip={0}&appid={1}&units=imperial";

            using (HttpClient client = new HttpClient())
            {
                string apiUrl = string.Format(apiBaseUrl, zipCode, apiKey);

                HttpResponseMessage response = await client.GetAsync(apiUrl);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    JObject jsonResult = JObject.Parse(content);
                    List<object> forecasts = new List<object>();

                    for (int i = 0; i < jsonResult["list"].Count(); i += 2)
                    {
                        JObject forecast = (JObject)jsonResult["list"][i];
                        DateTime dt = DateTimeOffset.FromUnixTimeSeconds(forecast["dt"].Value<long>()).DateTime;
                        forecasts.Add(new
                        {
                            Date = dt.ToString("yyyy-MM-dd HH:mm:ss"),
                            Temperature = forecast["main"]["temp"].Value<double>(),
                            Weather = forecast["weather"][0]["main"].Value<string>()
                        });
                        i++;
                    }

                    return Ok(new { Forecasts = forecasts });
                }
                else
                {
                    return BadRequest("Invalid ZIP code or unable to fetch weather forecast data.");
                }
            }
        }
    }
}