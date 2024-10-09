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
    public class ConnectorParametersController : BaseController
    {
        private ApplicationDbContext _context;
        private IMapper _mapper;

        public ConnectorParametersController(ApplicationDbContext context, IMapper mapper):base(context) 
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet("getall")]
        public async Task<IActionResult> GetAll()
        {
            //TODO change to toListAsync when company functionality is added
            var data = await _context.ConnectorParameters.FirstOrDefaultAsync();

            return Ok(data);
        }

        [HttpPost("insertdto")]
        public async Task<IActionResult> InsertDto([FromBody] ConnectorParametersDto connectorParameters)
        {
            var actionUser = await GetActionUser();

            var data = new ConnectorParameters();
            data.WooBaseUrl = connectorParameters.WooBaseUrl;
            data.WooConsumerKey = connectorParameters.WooConsumerKey;
            data.WooConsumerSecret = connectorParameters.WooConsumerSecret;

            await ExecuteTransaction(async () =>
            {
                var maxNumber = _context.ConnectorParameters.Max(x => (x.SerialNumber)) ?? 0;
                data.SerialNumber = maxNumber + 1;
                data.Code = data.SerialNumber.ToString().PadLeft(5, '0');

                try
                {
                    _context.ConnectorParameters.Add(data);
                    await _context.SaveChangesAsync();
                    LogService.CreateLog($"Connector parameters inserted by \"{actionUser.UserName}\"  Connector Parameters: {JsonConvert.SerializeObject(data)}.", LogTypeEnum.Information, LogOriginEnum.DataNexApp, actionUser.Id, _context);

                }
                catch (Exception ex)
                {
                    LogService.CreateLog($"Connector parameters could not be inserted by \"{actionUser.UserName}\"  Connector Parameters: {JsonConvert.SerializeObject(data)} Error: {ex.Message}.", LogTypeEnum.Error, LogOriginEnum.DataNexApp, actionUser.Id, _context);
                    throw;
                }
            });


            var dto = _mapper.Map<ConnectorParametersDto>(data);

            return Ok(dto);
        }

        [HttpPut("updatedto")]
        public async Task<IActionResult> UpdateDto([FromBody] ConnectorParametersDto connectorParameters)
        {
            var actionUser = await GetActionUser();

            var data = await _context.ConnectorParameters.FirstOrDefaultAsync(x => x.Id == connectorParameters.Id);

            data.WooBaseUrl = connectorParameters.WooBaseUrl;
            data.WooConsumerKey = connectorParameters.WooConsumerKey;
            data.WooConsumerSecret = connectorParameters.WooConsumerSecret;
            data.UserUpdated = actionUser.Id;
            data.DateUpdated = DateTime.Now;

            try
            {
                await _context.SaveChangesAsync();

                LogService.CreateLog($"Connector parameters updated by \"{actionUser.UserName}\"  Connector Parameters: {JsonConvert.SerializeObject(data)}.", LogTypeEnum.Information, LogOriginEnum.DataNexApp, actionUser.Id, _context);

            }
            catch (Exception ex)
            {
                LogService.CreateLog($"Connector parameters could not be updated by \"{actionUser.UserName}\"  Connector Parameters: {JsonConvert.SerializeObject(data)} Error: {ex.Message}.", LogTypeEnum.Error, LogOriginEnum.DataNexApp, actionUser.Id, _context);

            }
            var dto = _mapper.Map<ConnectorParametersDto>(data);

            return Ok(dto);
        }

        [HttpDelete("deletebyid/{id}")]
        public async Task<IActionResult> DeleteById(Guid id)
        {
            var actionUser = await GetActionUser();

            var data = await _context.ConnectorParameters.FirstOrDefaultAsync(x => x.Id == id);

            try
            {
                _context.ConnectorParameters.Remove(data);
                await _context.SaveChangesAsync();
                LogService.CreateLog($"Connector parameters deleted by \"{actionUser.UserName}\"  Connector Parameters: {JsonConvert.SerializeObject(data)}.", LogTypeEnum.Information, LogOriginEnum.DataNexApp, actionUser.Id, _context);

            }
            catch (Exception ex)
            {
                LogService.CreateLog($"Connector parameters could not be deleted by \"{actionUser.UserName}\"  Connector Parameters: {JsonConvert.SerializeObject(data)} Error: {ex.Message}.", LogTypeEnum.Error, LogOriginEnum.DataNexApp, actionUser.Id, _context);

            }
            return Ok(data);
        }
    }
}
