using AutoMapper;
using DataNex.Data;
using DataNex.Model.Dtos;
using DataNex.Model.Enums;
using DataNex.Model.Models;
using DataNexApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace DataNexApi.Controllers
{
    public class AddressesController : BaseController
    {
        private ApplicationDbContext _context;
        private IMapper _mapper;
        private static readonly object _lockObject = new object();

        public AddressesController(ApplicationDbContext context, IMapper mapper) : base(context)
        {
            _context = context;
            _mapper = mapper;
        }


        [HttpGet("getall")]
        public async Task<IActionResult> GetAll()
        {
            Guid companyId = GetCompanyFromHeader();

            var data = await _context.Addresses.Where(x => x.CompanyId == companyId).ToListAsync();

            return Ok(data);
        }

        [HttpPost("insertdto")]
        public async Task<IActionResult> InsertDto([FromBody] AddressDto dto)
        {
            Guid companyId = GetCompanyFromHeader();

            var actionUser = await GetActionUser();

            var data = new Address();
            data.Street = dto.Street;
            data.StreetNumber = dto.StreetNumber;
            data.City = dto.City;
            data.Country = dto.Country;

            data.CompanyId = companyId;
            //var source = await _context.Addresses.OrderByDescending(x => x.SerialNumber).FirstOrDefaultAsync();


            lock (_lockObject)
            {
                var maxNumber = _context.Addresses.Where(x => x.CompanyId == companyId).Max(x => (x.SerialNumber)) ?? 0;
                data.SerialNumber = maxNumber + 1;
                data.Code = data.SerialNumber.ToString().PadLeft(5, '0');
                try
                {
                    _context.Addresses.Add(data);
                    _context.SaveChanges();
                    LogService.CreateLog($"Address \"{data.Street} {data.StreetNumber}, {data.PostalCode}, {data.City} \" inserted by \"{actionUser.UserName}\"  Address: {JsonConvert.SerializeObject(data)}.", LogTypeEnum.Information, LogOriginEnum.DataNexApp, actionUser.Id, _context);

                }
                catch (Exception ex)
                {
                    LogService.CreateLog($"Address \"{data.Street} {data.StreetNumber}, {data.PostalCode}, {data.City}\" could not be inserted by \"{actionUser.UserName}\"  Address: {JsonConvert.SerializeObject(data)} Error: {ex.Message}.", LogTypeEnum.Error, LogOriginEnum.DataNexApp, actionUser.Id, _context);
                    throw;
                }

            }

            var dataToReturn = _mapper.Map<AddressDto>(data);
            return Ok(dataToReturn);
        }

        [HttpPut("updatedto")]
        public async Task<IActionResult> UpdateDto([FromBody] AddressDto dto)
        {
            Guid companyId = GetCompanyFromHeader();

            var actionUser = await GetActionUser();

            var data = await _context.Addresses.FirstOrDefaultAsync(x => x.Id == dto.Id && x.CompanyId == companyId);

            data.Street = dto.Street;
            data.StreetNumber = dto.StreetNumber;
            data.City = dto.City;
            data.Country = dto.Country;

            data.UserUpdated = actionUser.Id;
            data.DateUpdated = DateTime.Now;
            data.CompanyId = companyId;
            try
            {
                await _context.SaveChangesAsync();

                LogService.CreateLog($"Address \"{data.Street} {data.StreetNumber}, {data.PostalCode}, {data.City}\" updated by \"{actionUser.UserName}\"  Address: {JsonConvert.SerializeObject(data)}.", LogTypeEnum.Information, LogOriginEnum.DataNexApp, actionUser.Id, _context);

            }
            catch (Exception ex)
            {
                LogService.CreateLog($"Address \"{data.Street} {data.StreetNumber}, {data.PostalCode}, {data.City}\" could not be updated by \"{actionUser.UserName}\"  Address: {JsonConvert.SerializeObject(data)} Error: {ex.Message}.", LogTypeEnum.Error, LogOriginEnum.DataNexApp, actionUser.Id, _context);

            }
            return Ok(data);
        }

        [HttpDelete("deletebyid/{id}")]
        public async Task<IActionResult> DeleteById(Guid id)
        {
            Guid companyId = GetCompanyFromHeader();

            var actionUser = await GetActionUser();
            var data = await _context.Addresses.FirstOrDefaultAsync(x => x.Id == id && x.CompanyId == companyId);

            try
            {
                _context.Addresses.Remove(data);
                await _context.SaveChangesAsync();
                LogService.CreateLog($"Address \"{data.Street} {data.StreetNumber}, {data.PostalCode}, {data.City}\" deleted by \"{actionUser.UserName}\"  Address: {JsonConvert.SerializeObject(data)}.", LogTypeEnum.Information, LogOriginEnum.DataNexApp, actionUser.Id, _context);

            }
            catch (Exception ex)
            {
                LogService.CreateLog($"Address \"{data.Street} {data.StreetNumber}, {data.PostalCode}, {data.City}\" could not be deleted by \"{actionUser.UserName}\"  Address: {JsonConvert.SerializeObject(data)} Error: {ex.Message}.", LogTypeEnum.Error, LogOriginEnum.DataNexApp, actionUser.Id, _context);

            }
            return Ok(data);
        }
    }
}
