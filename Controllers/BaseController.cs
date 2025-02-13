﻿using DataNex.Data;
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

        public Guid GetCompanyFromHeader()
        {
            if (Request.Headers.TryGetValue("CompanyId", out var id))
            {
                // Use the header value here
                //var company = new { Header = companyId.ToString() };
                var companyId = id.ToString();
                return Guid.Parse(companyId);
            }
            else
            {
                return Guid.Empty;
            }

        }

        public int GetUserTimeZone()
        {
            if (Request.Headers.TryGetValue("TimeZoneOffset", out var offset))//Offset received in minutes
            {
                int.TryParse(offset.ToString(), out var tz);
                // Use the header value here
                int timeZoneOffset = tz;
                return timeZoneOffset;
            }
            else
            {
                return 0;
            }

        }

        public async Task<User> GetActionUser()
        {
            string userData = User.Claims.FirstOrDefault().Value;
            var actionUser = new User();
            JsonConvert.PopulateObject(userData, actionUser);

            return actionUser;
        }

        protected void LogMessage(string message, LogTypeEnum logType, LogOriginEnum logOrigin, Guid? userId)
        {
           using(var context = new ApplicationDbContext(AppBase.ConnectionString))
            {
                LogService.CreateLog(message, logType, logOrigin, userId, context);

            }


        }
    }
}
