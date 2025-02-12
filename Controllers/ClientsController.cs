using AutoMapper;
using DataNex.Data;
using DataNex.Model.Dtos;
using DataNex.Model.Enums;
using DataNex.Model.Models;
using DataNexApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace DataNexApi.Controllers
{
    [Authorize]
    public class ClientsController:BaseController
    {
        private CoreDbContext _context;
        private ApplicationDbContext _appContext;
        private IMapper _mapper;
        private static readonly object _lockObject = new object();

        public ClientsController(CoreDbContext context, ApplicationDbContext appContext, IMapper mapper) : base(appContext)
        {
            _context = context;
            _appContext = appContext;
            _mapper = mapper;
        }


        [HttpGet("getall")]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.Clients.ToListAsync();

            return Ok(data);
        }

        [HttpGet("getbyid/{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var data = await _context.Clients.Where(x => x.Id == id).FirstOrDefaultAsync();

            var dto = _mapper.Map<ClientDto>(data);

            return Ok(dto);
        }

        [HttpGet("getlookup")]
        public async Task<IActionResult> GetLookup()
        {
            var data = await _context.Clients.Select(x => new ClientDto()
            {
                Id = x.Id,
                Name = x.Name
            }).ToListAsync();

            return Ok(data);
        }

        [HttpPost("insertdto")]
        public async Task<IActionResult> InsertDto([FromBody] ClientDto client)
        {
            var actionUser = await GetActionUser();

            var data = new Client();
            data.Name = client.Name;
            data.Address = client.Address;
            data.Region = client.Region;
            data.PostalCode = client.PostalCode;
            data.City = client.City;
            data.Country = client.Country;
            data.Phone1 = client.Phone1;
            data.Phone2 = client.Phone2;
            data.Email = client.Email;
            data.UserAdded = actionUser.Id;

            lock (_lockObject)
            {
                var maxNumber = _context.Clients.Max(x => (x.SerialNumber)) ?? 0;
                data.SerialNumber = maxNumber + 1;
                data.Code = data.SerialNumber.ToString().PadLeft(5, '0');

                try
                {
                    _context.Clients.Add(data);
                    _context.SaveChanges();
                    LogService.CreateLog($"Client \"{data.Name}\" inserted by \"{actionUser.UserName}\". Client: {JsonConvert.SerializeObject(data)}", LogTypeEnum.Information, LogOriginEnum.DataNexApp, actionUser.Id, _appContext);

                }
                catch (Exception ex)
                {
                    LogService.CreateLog($"Client \"{data.Name}\" could not be inserted by \"{actionUser.UserName}\" Client: {JsonConvert.SerializeObject(data)} Error:{ex.Message}.", LogTypeEnum.Error, LogOriginEnum.DataNexApp, actionUser.Id, _appContext);
                    throw;
                }
            };

            var dto = _mapper.Map<ClientDto>(data);

            return Ok(dto);
        }

        [HttpPut("updatedto")]
        public async Task<IActionResult> UpdateDto([FromBody] ClientDto dto)
        {
            var actionUser = await GetActionUser();

            var data = await _context.Clients.FirstOrDefaultAsync(x => x.Id == dto.Id);

            data.Name = dto.Name;
            data.Address = dto.Address;
            data.Region = dto.Region;
            data.PostalCode = dto.PostalCode;
            data.City = dto.City;
            data.Country = dto.Country;
            data.Phone1 = dto.Phone1;
            data.Phone2 = dto.Phone2;
            data.Email = dto.Email;
            try
            {
                await _context.SaveChangesAsync();
                LogService.CreateLog($"Client \"{data.Name}\" updated by \"{actionUser.UserName}\". Client: {JsonConvert.SerializeObject(data)}", LogTypeEnum.Information, LogOriginEnum.DataNexApp, actionUser.Id, _appContext);

            }
            catch (Exception ex)
            {
                LogService.CreateLog($"Client \"{data.Name}\" could not be updated by \"{actionUser.UserName}\" Error:{ex.Message}.", LogTypeEnum.Error, LogOriginEnum.DataNexApp, actionUser.Id, _appContext);

            }

            return Ok(data);
        }


        [HttpDelete("deletebyid/{id}")]
        public async Task<IActionResult> DeleteById(Guid id)
        {
            var actionUser = await GetActionUser();

            var data = await _context.Clients.FirstOrDefaultAsync(x => x.Id == id);

            _context.Clients.Remove(data);

            try
            {
                _context.Clients.Remove(data);
                await _context.SaveChangesAsync();
                LogService.CreateLog($"Client \"{data.Name}\" deleted by \"{actionUser.UserName}\"  Client: {JsonConvert.SerializeObject(data)}.", LogTypeEnum.Information, LogOriginEnum.DataNexApp, actionUser.Id, _appContext);

            }
            catch (Exception ex)
            {
                LogService.CreateLog($"Client \"{data.Name}\" could not be deleted by \"{actionUser.UserName}\"  Client: {JsonConvert.SerializeObject(data)} Error:{ex.Message}.", LogTypeEnum.Error, LogOriginEnum.DataNexApp, actionUser.Id, _appContext);
            }

            return Ok(data);
        }
    }
}
