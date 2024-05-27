using DataNex.Model.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Security.Claims;

namespace DataNexApi.Controllers
{
    [Route("api/[controller]")]

    public class BaseController : Controller
    {

        public async Task<User> GetActionUser()
        {
            string userData = User.Claims.FirstOrDefault().Value;
            var actionUser = new User();
            JsonConvert.PopulateObject(userData, actionUser);

            return actionUser;
        }

    }
}
