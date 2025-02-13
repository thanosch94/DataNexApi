﻿using AutoMapper;
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
    public class VatClassesController:BaseController
    {
        private ApplicationDbContext _context;
        private IMapper _mapper;
        private static readonly object _lockObject = new object();

        public VatClassesController(ApplicationDbContext context, IMapper mapper):base(context)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet("getall")]
        public async Task<IActionResult> GetAll()
        {
            Guid companyId = GetCompanyFromHeader();

            var data = await _context.VatClasses.Where(x=>x.CompanyId==companyId).ToListAsync();

            return Ok(data);
        }               
        
        [HttpGet("getlookup")]
        public async Task<IActionResult> GetLookup()
        {
            Guid companyId = GetCompanyFromHeader();

            var data = await _context.VatClasses.Where(x => x.CompanyId == companyId).Select(x=>new VatClassDto()
            {
                Id=x.Id,
                Name=x.Name,
            }).ToListAsync();

            return Ok(data);
        }        
        
        [HttpGet("getbyid/{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            Guid companyId = GetCompanyFromHeader();

            var data = await _context.VatClasses.FirstOrDefaultAsync(x=>x.Id ==id && x.CompanyId==companyId);
            var dto = _mapper.Map<VatClassDto>(data);

            return Ok(dto);
        }

        [HttpPost("insertdto")]
        public async Task<IActionResult> InsertDto([FromBody] VatClassDto dto)
        {
            Guid companyId = GetCompanyFromHeader();

            var actionUser = await GetActionUser();

            var data = new VatClass();

            var exists = await _context.VatClasses.Where(x => x.Name == dto.Name && x.CompanyId==companyId).FirstOrDefaultAsync();
            if (exists == null)
            {
                data.Name = dto.Name;
                data.Description = dto.Description;
                data.Abbreviation = dto.Abbreviation;   
                data.Rate = dto.Rate;
                data.UserAdded = actionUser.Id;
                data.CompanyId = companyId;

                lock (_lockObject)
                {
                    var maxNumber = _context.VatClasses.Where(x => x.CompanyId == companyId).Max(x => (x.SerialNumber)) ?? 0;
                    data.SerialNumber = maxNumber + 1;
                    data.Code = data.SerialNumber.ToString().PadLeft(5, '0');

                    try
                    {
                        _context.VatClasses.Add(data);
                        _context.SaveChanges();
                        LogService.CreateLog($"Vat Class \"{data.Name}\" inserted by \"{actionUser.UserName}\". Vat Class: {JsonConvert.SerializeObject(data)}", LogTypeEnum.Information, LogOriginEnum.DataNexApp, actionUser.Id, _context);

                    }
                    catch (Exception ex)
                    {
                        LogService.CreateLog($"Vat Class \"{data.Name}\" could not be inserted by \"{actionUser.UserName}\". Vat Class: {JsonConvert.SerializeObject(data)} Error: {ex.Message}", LogTypeEnum.Error, LogOriginEnum.DataNexApp, actionUser.Id, _context);
                        throw;
                    }
                };

                return Ok(data);
            }
            else
            {
                return BadRequest("Vat Class already exists");
            }

        }

        [HttpPut("updatedto")]
        public async Task<IActionResult> UpdateDto([FromBody] VatClassDto dto)
        {
            Guid companyId = GetCompanyFromHeader();

            var actionUser = await GetActionUser();

            var data = await _context.VatClasses.Where(x => x.Id == dto.Id &&x.CompanyId==companyId).FirstOrDefaultAsync();

            data.Name = dto.Name;
            data.Description = dto.Description;
            data.Abbreviation = dto.Abbreviation;
            data.Rate = dto.Rate;
            data.IsActive = dto.IsActive;
            data.CompanyId = companyId;

            try
            {
                await _context.SaveChangesAsync();
                LogService.CreateLog($"Vat Class \"{data.Name}\" updated by \"{actionUser.UserName}\". Vat Class: {JsonConvert.SerializeObject(data)}", LogTypeEnum.Information, LogOriginEnum.DataNexApp, actionUser.Id, _context);

            }
            catch (Exception ex)
            {
                LogService.CreateLog($"Vat Class \"{data.Name}\" could not be updated by \"{actionUser.UserName}\" Vat Class: {JsonConvert.SerializeObject(data)} Error: {ex.Message}.", LogTypeEnum.Error, LogOriginEnum.DataNexApp, actionUser.Id, _context);

            }

            var dtoData = _mapper.Map<VatClassDto>(data);

            return Ok(dtoData);

        }

        [HttpDelete("deletebyid/{id}")]
        public async Task<IActionResult> DeleteById(Guid id)
        {
            Guid companyId = GetCompanyFromHeader();

            var actionUser = await GetActionUser();

            var data = await _context.VatClasses.FirstOrDefaultAsync(x => x.Id == id && x.CompanyId==companyId);

            _context.VatClasses.Remove(data);

            try
            {
                await _context.SaveChangesAsync();
                LogService.CreateLog($"Vat Class \"{data.Name}\" deleted by \"{actionUser.UserName}\". Vat Class: {JsonConvert.SerializeObject(data)}", LogTypeEnum.Information, LogOriginEnum.DataNexApp, actionUser.Id, _context);

            }
            catch (Exception ex)
            {
                LogService.CreateLog($"Vat Class \"{data.Name}\" could not be deleted by \"{actionUser.UserName}\"  Vat Class: {JsonConvert.SerializeObject(data)} Error: {ex.Message}.", LogTypeEnum.Error, LogOriginEnum.DataNexApp, actionUser.Id, _context);

            }
            return Ok(data);
        }


    }
}
