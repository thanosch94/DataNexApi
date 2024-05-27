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
using System.Data.Common;

namespace DataNexApi.Controllers
{
    [Authorize]
    public class CustomersController : BaseController
    {
        private ApplicationDbContext _context;
        private IMapper _mapper;
        public CustomersController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet("getall")]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.Customers.ToListAsync();

            return Ok(data);
        }

        [HttpGet("getfromaade/{username}/{password}/{afmCalledFor}/{afmCalledBy}")]
        public async Task<IActionResult> GetFromAade(string username, string password, string afmCalledFor, string? afmCalledBy)
        {
            var data = AadeService.GetDataFromAade(username, password, afmCalledBy, afmCalledFor);

            return Ok(data);
        }

        [HttpGet("getbyid/{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var data = await _context.Customers.Where(x => x.Id == id).FirstOrDefaultAsync();

            var dto = _mapper.Map<CustomerDto>(data);


            return Ok(dto);
        }

        [HttpGet("getlookup")]
        public async Task<IActionResult> GetLookup()
        {
            var data = await _context.Customers.Select(x => new CustomerDto()
            {
                Id = x.Id,
                Name = x.Name
            }).ToListAsync();

            return Ok(data);
        }

        [HttpPost("insertdto")]
        public async Task<IActionResult> InsertDto([FromBody] CustomerDto customer)

        {
            var actionUser = await GetActionUser();

            var data = new Customer();
            data.Name = customer.Name;
            data.Address = customer.Address;
            data.Region = customer.Region;
            data.PostalCode = customer.PostalCode;
            data.City = customer.City;
            data.Country = customer.Country;
            data.Phone1 = customer.Phone1;
            data.Phone2 = customer.Phone2;
            data.Email = customer.Email;
            data.VatNumber = customer.VatNumber;
            data.TaxOffice = customer.TaxOffice;
            data.UserAdded = actionUser.Id;

            try
            {
                _context.Customers.Add(data);
                await _context.SaveChangesAsync();
                LogService.CreateLog($"Customer \"{data.Name}\" inserted by \"{actionUser.UserName}\". Customer: {JsonConvert.SerializeObject(data)}", LogTypeEnum.Information, LogOriginEnum.DataNexApp, actionUser.Id, _context);

            }
            catch (Exception ex)
            {
                LogService.CreateLog($"Customer \"{data.Name}\" could not be inserted by \"{actionUser.UserName}\" Customer: {JsonConvert.SerializeObject(data)} Error:{ex.Message}.", LogTypeEnum.Error, LogOriginEnum.DataNexApp, actionUser.Id, _context);
            }


            var dto = _mapper.Map<CustomerDto>(data);

            return Ok(dto);
        }


        [HttpPut("updatedto")]
        public async Task<IActionResult> UpdateDto([FromBody] CustomerDto dto)
        {
            var actionUser = await GetActionUser();

            var data = await _context.Customers.FirstOrDefaultAsync(x => x.Id == dto.Id);

            data.Name = dto.Name;
            data.Address = dto.Address;
            data.Region = dto.Region;
            data.PostalCode = dto.PostalCode;
            data.City = dto.City;
            data.Country = dto.Country;
            data.Phone1 = dto.Phone1;
            data.Phone2 = dto.Phone2;
            data.Email = dto.Email;
            data.VatNumber = dto.VatNumber;
            data.TaxOffice = dto.TaxOffice;

            try
            {
                await _context.SaveChangesAsync();
                LogService.CreateLog($"Customer \"{data.Name}\" updated by \"{actionUser.UserName}\". Customer: {JsonConvert.SerializeObject(data)}", LogTypeEnum.Information, LogOriginEnum.DataNexApp, actionUser.Id, _context);

            }
            catch (Exception ex)
            {
                LogService.CreateLog($"Customer \"{data.Name}\" could not be updated by \"{actionUser.UserName}\" Error:{ex.Message}.", LogTypeEnum.Error, LogOriginEnum.DataNexApp, actionUser.Id, _context);

            }

            return Ok(data);
        }

        [HttpDelete("deletebyid/{id}")]
        public async Task<IActionResult> DeleteById(Guid id)
        {
            var actionUser = await GetActionUser();

            var data = await _context.Customers.FirstOrDefaultAsync(x => x.Id == id);

            _context.Customers.Remove(data);

            try
            {
                _context.Customers.Remove(data);
                await _context.SaveChangesAsync();
                LogService.CreateLog($"Customer \"{data.Name}\" deleted by \"{actionUser.UserName}\"  Customer: {JsonConvert.SerializeObject(data)}.", LogTypeEnum.Information, LogOriginEnum.DataNexApp, actionUser.Id, _context);

            }
            catch (Exception ex)
            {
                LogService.CreateLog($"Customer \"{data.Name}\" could not be deleted by \"{actionUser.UserName}\"  Customer: {JsonConvert.SerializeObject(data)} Error:{ex.Message}.", LogTypeEnum.Error, LogOriginEnum.DataNexApp, actionUser.Id, _context);
            }

            return Ok(data);
        }
    }
}
