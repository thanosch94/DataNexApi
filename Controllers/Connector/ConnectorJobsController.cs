using AutoMapper;
using DataNex.Data;
using DataNex.Model.Dtos;
using DataNex.Model.Dtos.Connector;
using DataNex.Model.Dtos.Woocommerce;
using DataNex.Model.Enums;
using DataNex.Model.Models;
using DataNexApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mysqlx.Prepare;
using Newtonsoft.Json;
using WooCommerceService;

namespace DataNexApi.Controllers.Connector
{
    [Authorize]
    public class ConnectorJobsController : BaseController
    {
        private ApplicationDbContext _context;
        private IMapper _mapper;
        private static readonly object _lockObject = new object();

        public ConnectorJobsController(ApplicationDbContext context, IMapper mapper):base(context)
        {
            _context = context;
            _mapper = mapper;
        }


        [HttpGet("getall")]
        public async Task<IActionResult> GetAll()
        {
            Guid companyId = GetCompanyFromHeader();

            var data = await _context.ConnectorJobs.Where(x => x.CompanyId == companyId).ToListAsync();

            return Ok(data);


        }

        [HttpGet("getbyid/{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            Guid companyId = GetCompanyFromHeader();

            var data = await _context.ConnectorJobs.Where(x => x.Id == id && x.CompanyId == companyId).Select(x => new ConnectorJobDto()
            {
                Id = x.Id,
                SerialNumber = x.SerialNumber,
                Code = x.Code,
                Name = x.Name,
                Icon = x.Icon,
                Description = x.Description,
                JobType = x.JobType,
                DataSourceId = x.DataSourceId,
                WooConnectionDataSourceId = x.WooConnectionDataSourceId
            }).FirstOrDefaultAsync();
            var dto = _mapper.Map<ConnectorJobDto>(data);

            return Ok(dto);
        }

        [HttpGet("getallbyjobtype/{jobType}")]
        public async Task<IActionResult> GetAllByDataSourceId(ConnectorJobTypeEnum jobType)
        {
            Guid companyId = GetCompanyFromHeader();

            var data = await _context.ConnectorJobs.Where(x => x.JobType == jobType && x.CompanyId == companyId).Select(x => new ConnectorJobDto()
            {
                Id = x.Id,
                SerialNumber = x.SerialNumber,
                Code = x.Code,
                Name = x.Name,
                Icon = x.Icon,
                Description = x.Description,
                JobType = x.JobType,
                DataSourceId = x.DataSourceId,
                WooConnectionDataSourceId = x.WooConnectionDataSourceId
            }).ToListAsync();

            return Ok(data);
        }

        [HttpGet("getallbydatasourceid/{id}")]
        public async Task<IActionResult> GetAllByDataSourceId(Guid id)
        {
            Guid companyId = GetCompanyFromHeader();

            var data = await _context.ConnectorJobs.Where(x => x.DataSourceId == id && x.CompanyId==companyId).Select(x => new ConnectorJobDto()
            {
                Id = x.Id,
                SerialNumber = x.SerialNumber,
                Code = x.Code,
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
            Guid companyId = GetCompanyFromHeader();

            var actionUser = await GetActionUser();

            var data = new ConnectorJob();
            data.Name = connectorJobDto.Name;
            data.Icon = connectorJobDto.Icon;
            data.Description = connectorJobDto.Description;
            data.JobType = connectorJobDto.JobType;
            data.DataSourceId = connectorJobDto.DataSourceId;
            data.WooConnectionDataSourceId = connectorJobDto.WooConnectionDataSourceId;
            data.CompanyId = companyId;
            lock (_lockObject)
            {
                var maxNumber = _context.ConnectorJobs.Where(x => x.CompanyId == companyId).Max(x => (x.SerialNumber)) ?? 0;
                data.SerialNumber = maxNumber + 1;
                data.Code = data.SerialNumber.ToString().PadLeft(5, '0');

                try
                {
                    _context.ConnectorJobs.Add(data);
                    _context.SaveChanges();
                    LogService.CreateLog($"Connector job inserted by \"{actionUser.UserName}\"  Connector Job: {JsonConvert.SerializeObject(data)}.", LogTypeEnum.Information, LogOriginEnum.DataNexApp, actionUser.Id, _context);

                }
                catch (Exception ex)
                {
                    LogService.CreateLog($"Connector Job could not be inserted by \"{actionUser.UserName}\"  Connector Job: {JsonConvert.SerializeObject(data)} Error: {ex.Message}.", LogTypeEnum.Error, LogOriginEnum.DataNexApp, actionUser.Id, _context);
                    throw;
                }
            };

            var dto = _mapper.Map<ConnectorJobDto>(data);

            return Ok(dto);
        }

        [HttpPut("updatedto")]
        public async Task<IActionResult> UpdateDto([FromBody] ConnectorJobDto connectorJobDto)
        {
            Guid companyId = GetCompanyFromHeader();

            var actionUser = await GetActionUser();

            var data = await _context.ConnectorJobs.FirstOrDefaultAsync(x => x.Id == connectorJobDto.Id && x.CompanyId == companyId);
            data.Name = connectorJobDto.Name;
            data.Icon = connectorJobDto.Icon;
            data.Description = connectorJobDto.Description;
            data.JobType = connectorJobDto.JobType;
            data.DataSourceId = connectorJobDto.DataSourceId;
            data.WooConnectionDataSourceId = connectorJobDto.WooConnectionDataSourceId;

            data.UserUpdated = actionUser.Id;
            data.DateUpdated = DateTime.Now;
            data.CompanyId = companyId;

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
            Guid companyId = GetCompanyFromHeader();

            var actionUser = await GetActionUser();

            var data = await _context.ConnectorJobs.FirstOrDefaultAsync(x => x.Id == id && x.CompanyId==companyId);

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

        [HttpPost("getconnectorjobresult")]
        public async Task<IActionResult> GetConnectorJobResult([FromBody] ConnectorJobDto connectorJob)
        {
            var apiResponse = new ApiResponseDto();
            var connectorService = new ConnectorService(_context);
       
                if (connectorJob.DataSourceId == AppBase.wordpressDataSource)
                {
                    //Todo add Interface
                    apiResponse = await connectorService.ExecuteWoocommerceJob(connectorJob);

                }
                else if (connectorJob.DataSourceId == AppBase.magentoDataSource)
                {

                }
                return Ok(apiResponse);

           
        }

        
    }
}
