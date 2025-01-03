﻿using AutoMapper;
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
        private static readonly object _lockObject = new object();

        public CustomersController(ApplicationDbContext context, IMapper mapper) : base(context)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet("getall")]
        public async Task<IActionResult> GetAll()
        {
            Guid companyId = GetCompanyFromHeader();

            var data = await _context.Customers.Where(x=> x.CompanyId == companyId).ToListAsync();

            return Ok(data);
        }

        [HttpGet("getfromaade/{username}/{password}/{afmCalledFor}/{afmCalledBy}")]
        public async Task<IActionResult> GetFromAade(string username, string password, string afmCalledFor, string? afmCalledBy)
        {
            Guid companyId = GetCompanyFromHeader();

            var data = AadeService.GetDataFromAade(username, password, afmCalledBy, afmCalledFor);

            return Ok(data);
        }

        [HttpGet("getbyid/{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            Guid companyId = GetCompanyFromHeader();

            var data = await _context.Customers.Where(x => x.Id == id && x.CompanyId == companyId).FirstOrDefaultAsync();

            var dto = _mapper.Map<CustomerDto>(data);


            return Ok(dto);
        }

        [HttpGet("getlookup")]
        public async Task<IActionResult> GetLookup()
        {
            Guid companyId = GetCompanyFromHeader();

            var data = await _context.Customers.Select(x => new CustomerDto()
            {
                Id = x.Id,
                Name = x.Name,
                CompanyId=x.CompanyId
            }).Where(x=>x.CompanyId==companyId).ToListAsync();

            return Ok(data);
        }

        [HttpPost("insertdto")]
        public async Task<IActionResult> InsertDto([FromBody] CustomerDto customer)
        {
            Guid companyId = GetCompanyFromHeader();

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
            data.CompanyId = companyId;
            lock (_lockObject)
            {
                try
                {
                    var maxNumber = _context.Customers.Where(x=> x.CompanyId == companyId).Max(x => (x.SerialNumber)) ?? 0;
                    data.SerialNumber = maxNumber + 1;
                    data.Code = data.SerialNumber.ToString().PadLeft(5, '0');

                    _context.Customers.Add(data);
                    _context.SaveChanges();
                    LogService.CreateLog($"Customer \"{data.Name}\" inserted by \"{actionUser.UserName}\". Customer: {JsonConvert.SerializeObject(data)}", LogTypeEnum.Information, LogOriginEnum.DataNexApp, actionUser.Id, _context);

                }
                catch (Exception ex)
                {
                    LogService.CreateLog($"Customer \"{data.Name}\" could not be inserted by \"{actionUser.UserName}\" Customer: {JsonConvert.SerializeObject(data)} Error:{ex.Message}.", LogTypeEnum.Error, LogOriginEnum.DataNexApp, actionUser.Id, _context);
                    throw;
                }

            };
            

            var dto = _mapper.Map<CustomerDto>(data);

            return Ok(dto);
        }


        [HttpPut("updatedto")]
        public async Task<IActionResult> UpdateDto([FromBody] CustomerDto dto)
        {
            Guid companyId = GetCompanyFromHeader();

            var actionUser = await GetActionUser();

            var data = await _context.Customers.FirstOrDefaultAsync(x => x.Id == dto.Id && x.CompanyId == companyId);

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
            data.CompanyId = companyId;

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
            Guid companyId = GetCompanyFromHeader();

            var actionUser = await GetActionUser();

            var data = await _context.Customers.FirstOrDefaultAsync(x => x.Id == id && x.CompanyId==companyId);

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
