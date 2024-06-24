using AutoMapper;
using DataNex.Data;
using DataNex.Model.Dtos;
using DataNex.Model.Dtos.Connector;
using DataNex.Model.Enums;
using DataNex.Model.Models;
using DataNexApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace DataNexApi.Controllers.Connector
{
    [Authorize]
    public class ConnectorJobsController:BaseController
    {
        private ApplicationDbContext _context;
        private IMapper _mapper;

        public ConnectorJobsController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }


        [HttpGet("getall")]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.ConnectorJobs.ToListAsync();

            return Ok(data);


        }       
        
        [HttpGet("getbyid/{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var data = await _context.ConnectorJobs.FirstOrDefaultAsync(x => x.Id == id);
            var dto = _mapper.Map<ConnectorJobDto>(data);

            return Ok(dto);
        }

        [HttpGet("getallbydatasourceid/{id}")]
        public async Task<IActionResult> GetAllByDataSourceId(Guid id)
        {
            var data = await _context.ConnectorJobs.Where(x => x.DataSourceId == id).Select(x=>new ConnectorJobDto()
            {
                Name = x.Name,
                Icon = x.Icon,
                Description = x.Description,
                JobType = x.JobType,
                DataSourceId = x.DataSourceId,
                WooConnectionDataSourceId = x.WooConnectionDataSourceId
            }).ToListAsync();

            return Ok(data);
        }

        [HttpPost("insertdto")]
        public async Task<IActionResult> InsertDto([FromBody] ConnectorJobDto connectorJobDto)
        {
            var actionUser = await GetActionUser();

            var data = new ConnectorJob();
            data.Name = connectorJobDto.Name;
            data.Icon = connectorJobDto.Icon;
            data.Description = connectorJobDto.Description;
            data.JobType = connectorJobDto.JobType;
            data.DataSourceId = connectorJobDto.DataSourceId;
            data.WooConnectionDataSourceId = connectorJobDto.WooConnectionDataSourceId;

            try
            {
                _context.ConnectorJobs.Add(data);
                await _context.SaveChangesAsync();
                LogService.CreateLog($"Connector job inserted by \"{actionUser.UserName}\"  Connector Job: {JsonConvert.SerializeObject(data)}.", LogTypeEnum.Information, LogOriginEnum.DataNexApp, actionUser.Id, _context);

            }
            catch (Exception ex)
            {
                LogService.CreateLog($"Connector Job could not be inserted by \"{actionUser.UserName}\"  Connector Job: {JsonConvert.SerializeObject(data)} Error: {ex.Message}.", LogTypeEnum.Error, LogOriginEnum.DataNexApp, actionUser.Id, _context);

            }

            var dto = _mapper.Map<ConnectorJobDto>(data);

            return Ok(dto);
        }

        [HttpPut("updatedto")]
        public async Task<IActionResult> UpdateDto([FromBody] ConnectorJobDto connectorJobDto)
        {
            var actionUser = await GetActionUser();

            var data = await _context.ConnectorJobs.FirstOrDefaultAsync(x => x.Id == connectorJobDto.Id);
            data.Name = connectorJobDto.Name;
            data.Icon = connectorJobDto.Icon;
            data.Description = connectorJobDto.Description;
            data.JobType = connectorJobDto.JobType;
            data.DataSourceId = connectorJobDto.DataSourceId;
            data.WooConnectionDataSourceId = connectorJobDto.WooConnectionDataSourceId;

            data.UserUpdated = actionUser.Id;
            data.DateUpdated = DateTime.Now;

            try
            {
                await _context.SaveChangesAsync();

                LogService.CreateLog($"Connector job updated by \"{actionUser.UserName}\"  Connector Job: {JsonConvert.SerializeObject(data)}.", LogTypeEnum.Information, LogOriginEnum.DataNexApp, actionUser.Id, _context);

            }
            catch (Exception ex)
            {
                LogService.CreateLog($"Connector Job could not be updated by \"{actionUser.UserName}\"  Connector Job: {JsonConvert.SerializeObject(data)} Error: {ex.Message}.", LogTypeEnum.Error, LogOriginEnum.DataNexApp, actionUser.Id, _context);
            }
            var dto = _mapper.Map<ConnectorJobDto>(data);

            return Ok(dto);
        }

        [HttpDelete("deletebyid/{id}")]
        public async Task<IActionResult> DeleteById(Guid id)
        {
            var actionUser = await GetActionUser();

            var data = await _context.ConnectorJobs.FirstOrDefaultAsync(x => x.Id == id);

            try
            {
                _context.ConnectorJobs.Remove(data);
                await _context.SaveChangesAsync();
                LogService.CreateLog($"Connector job deleted by \"{actionUser.UserName}\"  Connector Job: {JsonConvert.SerializeObject(data)}.", LogTypeEnum.Information, LogOriginEnum.DataNexApp, actionUser.Id, _context);

            }
            catch (Exception ex)
            {
                LogService.CreateLog($"Connector Job could not be deleted by \"{actionUser.UserName}\"  Connector Job: {JsonConvert.SerializeObject(data)} Error: {ex.Message}.", LogTypeEnum.Error, LogOriginEnum.DataNexApp, actionUser.Id, _context);

            }
            return Ok(data);
        }
    }
}
