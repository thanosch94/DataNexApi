using DataNex.Data;
using DataNex.Model.Enums;
using DataNex.Model.Models;
using DataNexApi.Services;
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

            var transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
            
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
        protected void LogMessage(string message, LogTypeEnum logType, LogOriginEnum logOrigin, Guid? userId)
        {
           
                LogService.CreateLog(message, logType,logOrigin, userId, _context);
            

        }
    }
}
