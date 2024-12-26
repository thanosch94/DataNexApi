using AutoMapper;
using DataNex.Data;
using DataNex.Model.Dtos;
using DataNex.Model.Models;
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
                DateAdded = x.LocalDateAdded,
                AddedDateTimeFormatted = x.DateAdded.ToString("dd/MM/yyyy HH:mm:ss"),
                LogTypeName = x.LogType.GetDisplayName(),
                LogOriginName = x.LogOrigin.GetDisplayName(),
                CompanyId =x.CompanyId,
                

            }).Where(x=>x.CompanyId==companyId).ToListAsync();

            var dataToReturn = data.OrderByDescending(x => x.DateAdded).ToList();
            return Ok(dataToReturn);
        }
    }
}
