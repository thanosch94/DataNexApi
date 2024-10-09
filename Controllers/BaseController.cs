using DataNex.Data;
using DataNex.Model.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Security.Claims;

namespace DataNexApi.Controllers
{
    [Route("api/[controller]")]

    public class BaseController : Controller
    {
        private ApplicationDbContext _context;
        public BaseController(ApplicationDbContext context)
        {
            _context = context;
        }

        public BaseController()
        {

        }

        public async Task<User> GetActionUser()
        {
            string userData = User.Claims.FirstOrDefault().Value;
            var actionUser = new User();
            JsonConvert.PopulateObject(userData, actionUser);

            return actionUser;
        }
        protected async Task ExecuteTransaction(Action action)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable))
            {
                try
                {
                    action();
                    await transaction.CommitAsync();
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }
    }
}
