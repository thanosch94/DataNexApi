using AutoMapper;
using DataNex.Data;
using DataNex.Model.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Extensions;

namespace DataNexApi.Controllers
{
    [Authorize]
    public class LogsController:BaseController
    {
        private ApplicationDbContext _context;
        public LogsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("getall")]
        public async Task<IActionResult> GetAll()
        {
            Guid companyId = GetCompanyFromHeader();

            var data = await _context.Logs.Select(x=> new LogDto()
            {
                Id = x.Id,
                LogName = x.LogName,
                DateAdded = x.DateAdded,
                AddedDateTimeFormatted = x.DateAdded.ToString("yyyy/MM/dd HH:mm:ss"),
                LogTypeName = x.LogType.GetDisplayName(),
                LogOriginName = x.LogOrigin.GetDisplayName()
                

            }).Where(x=>x.CompanyId==companyId).ToListAsync();

            return Ok(data.OrderByDescending(x => x.DateAdded));
        }
    }
}
