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

namespace DataNexApi.Controllers.Connector
{
    [Authorize]
    public class CntorDatasourcesController : BaseController
    {
        private ApplicationDbContext _context;
        private IMapper _mapper;
        private static readonly object _lockObject = new object();

        public CntorDatasourcesController(ApplicationDbContext context, IMapper mapper) : base(context)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet("getall")]
        public async Task<IActionResult> GetAll()
        {
            Guid companyId = GetCompanyFromHeader();

            var data = await _context.CntorDatasources.Where(x => x.CompanyId == companyId).ToListAsync();
            return Ok(data);
        }


        [HttpGet("getbyid/{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            Guid companyId = GetCompanyFromHeader();

            var data = await _context.CntorDatasources.Where(x => x.Id == id && x.CompanyId == companyId).FirstOrDefaultAsync();

            var dto = _mapper.Map<CntorDatasourceDto>(data);

            return Ok(dto);
        }

        [HttpGet("getlookup")]
        public async Task<IActionResult> GetLookup()
        {
            Guid companyId = GetCompanyFromHeader();

            var data = await _context.CntorDatasources.Select(x => new CntorDatasourceDto()
            {
                Id = x.Id,
                Name = x.Name,
                CompanyId = x.CompanyId,
            }).Where(x => x.CompanyId == companyId).ToListAsync();

            return Ok(data);
        }

        [HttpPost("insertdto")]
        public async Task<IActionResult> InsertDto([FromBody] CntorDatasourceDto dto)
        {
            Guid companyId = GetCompanyFromHeader();

            var actionUser = await GetActionUser();
            var data = new CntorDatasource();
            //var source = await _context.CntorDatasources.OrderByDescending(x => x.SerialNumber).FirstOrDefaultAsync();

            data.Name = dto.Name;
            data.Description = dto.Description;
            data.Icon = dto.Icon;
            data.IconColor = dto.IconColor;
            data.HasCustomImage = dto.HasCustomImage;
            data.CustomImagePath = dto.CustomImagePath;
            data.CustomImageWidth = dto.CustomImageWidth;
            data.UserAdded = actionUser.Id;
            data.CompanyId = companyId;
            lock (_lockObject)
            {
                var maxNumber = _context.CntorDatasources.Where(x => x.CompanyId == companyId).Max(x => (x.SerialNumber)) ?? 0;
                data.SerialNumber = maxNumber + 1;
                data.Code = data.SerialNumber.ToString().PadLeft(5, '0');

                try
                {
                    _context.CntorDatasources.Add(data);
                    _context.SaveChanges();
                    LogService.CreateLog($"Connector Datasource \"{data.Name}\" inserted by \"{actionUser.UserName}\"  Data: {JsonConvert.SerializeObject(data)}.", LogTypeEnum.Information, LogOriginEnum.DataNexApp, actionUser.Id, _context);

                }
                catch (Exception ex)
                {
                    LogService.CreateLog($"Connector Datasource \"{data.Name}\" could not be inserted by \"{actionUser.UserName}\"  Data: {JsonConvert.SerializeObject(data)} Error: {ex.Message}.", LogTypeEnum.Error, LogOriginEnum.DataNexApp, actionUser.Id, _context);
                    throw;
                }

            };

            var dataToReturn = _mapper.Map<CntorDatasourceDto>(data);
            return Ok(dataToReturn);
        }

        [HttpPut("updatedto")]
        public async Task<IActionResult> UpdateDto([FromBody] CntorDatasourceDto dto)
        {
            Guid companyId = GetCompanyFromHeader();

            var actionUser = await GetActionUser();

            var data = await _context.CntorDatasources.FirstOrDefaultAsync(x => x.Id == dto.Id && x.CompanyId == companyId);
            data.Name = dto.Name;
            data.Description = dto.Description;
            data.Icon = dto.Icon;
            data.IconColor = dto.IconColor;
            data.HasCustomImage = dto.HasCustomImage;
            data.CustomImagePath = dto.CustomImagePath;
            data.CustomImageWidth = dto.CustomImageWidth;
            data.UserUpdated = actionUser.Id;
            data.DateUpdated = DateTime.Now;
            data.CompanyId = companyId;

            try
            {
                await _context.SaveChangesAsync();

                LogService.CreateLog($"Connector Datasource \"{data.Name}\" updated by \"{actionUser.UserName}\". Data: {JsonConvert.SerializeObject(data)}", LogTypeEnum.Information, LogOriginEnum.DataNexApp, actionUser.Id, _context);

            }
            catch (Exception ex)
            {
                LogService.CreateLog($"Connector Datasource \"{data.Name}\" could not be updated by \"{actionUser.UserName}\"  Data: {JsonConvert.SerializeObject(data)} Error:{ex.Message}.", LogTypeEnum.Error, LogOriginEnum.DataNexApp, actionUser.Id, _context);

            }
            var dataToReturn = _mapper.Map<CntorDatasourceDto>(data);

            return Ok(dataToReturn);
        }

        [HttpDelete("deletebyid/{id}")]
        public async Task<IActionResult> DeleteById(Guid id)
        {
            Guid companyId = GetCompanyFromHeader();

            var actionUser = await GetActionUser();
            var data = await _context.CntorDatasources.FirstOrDefaultAsync(x => x.Id == id && x.CompanyId == companyId);

            try
            {
                _context.CntorDatasources.Remove(data);
                await _context.SaveChangesAsync();
                LogService.CreateLog($"Connector Datasource \"{data.Name}\" deleted by \"{actionUser.UserName}\"  Data: {JsonConvert.SerializeObject(data)}.", LogTypeEnum.Information, LogOriginEnum.DataNexApp, actionUser.Id, _context);

            }
            catch (Exception ex)
            {
                LogService.CreateLog($"Connector Datasource \"{data.Name}\" could not be deleted by \"{actionUser.UserName}\"  Data: {JsonConvert.SerializeObject(data)} Error:{ex.Message}.", LogTypeEnum.Error, LogOriginEnum.DataNexApp, actionUser.Id, _context);

            }
            return Ok(data);
        }
    }
}
