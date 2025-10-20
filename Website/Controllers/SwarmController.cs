using Microsoft.AspNetCore.Mvc;
using RestSharp;
using Newtonsoft.Json;
using APIClasses;
namespace Website.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SwarmController : Controller
    {
        private RestClient client = new RestClient("http://localhost:5199");//for Server

        [HttpGet]
        [Route("GetAllClientsInSwarm")]
        public async Task<IActionResult> GetAllClientsInSwarm()
        {
            RestRequest restRequest = new RestRequest("api/clients/GetAllClients", Method.Get);
            RestResponse restResponse = await client.ExecuteAsync(restRequest);

            if (!restResponse.IsSuccessful)
            {
                return BadRequest("Error, Could not find clients");
            }

            var list = JsonConvert.DeserializeObject<List<Client>>(restResponse.Content);

            return Ok(list);
        }
    }
}
