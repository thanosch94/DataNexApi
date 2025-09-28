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
using System;

namespace DataNexApi.Controllers
{
    [Authorize]
    public class DocumentSeriesController:BaseController
    {
        private ApplicationDbContext _context;
        private IMapper _mapper;
        private static readonly object _lockObject = new object();

        public DocumentSeriesController(ApplicationDbContext context, IMapper mapper) : base(context)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet("getall")]
        public async Task<IActionResult> GetAll()
        {
            Guid companyId = GetCompanyFromHeader();

            var data = await _context.DocumentSeries.Where(x => x.CompanyId == companyId).ToListAsync();

            return Ok(data);
        }

        [HttpGet("getbyid/{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            Guid companyId = GetCompanyFromHeader();

            var data = await _context.DocumentSeries.Where(x => x.Id == id && x.CompanyId == companyId).FirstOrDefaultAsync();

            var dto = _mapper.Map<DocumentSeriesDto>(data);

            return Ok(dto);
        }
                
        [HttpGet("getdocumentseriesbyid/{id}")]
        public async Task<IActionResult> GetByDocumentTypeId(Guid id)
        {
            Guid companyId = GetCompanyFromHeader();

            var data = await _context.DocumentSeries.Where(x => x.DocumentTypeId == id && x.CompanyId == companyId).ToListAsync();

            return Ok(data);
        }


        [HttpPost("insertdto")]
        public async Task<IActionResult> InsertDto([FromBody] DocumentSeriesDto dto)
        {
            Guid companyId = GetCompanyFromHeader();

            var actionUser = await GetActionUser();
            var data = new DocumentSeries();

            data.Name = dto.Name;
            data.Abbreviation = dto.Abbreviation;
            data.AllowManualNumbering = dto.AllowManualNumbering;
            data.Prefix = dto.Prefix;
            data.Suffix = dto.Suffix;
            data.Notes = dto.Notes;
            data.DocumentTypeId = dto.DocumentTypeId;
            data.IsActive = dto.IsActive;
            data.UserAdded = actionUser.Id;
            data.CompanyId = companyId;
            lock (_lockObject)
            {
                var maxNumber = _context.DocumentSeries.Where(x => x.CompanyId == companyId).Max(x => (x.SerialNumber)) ?? 0;
                data.SerialNumber = maxNumber + 1;
                data.Code = data.SerialNumber.ToString().PadLeft(5, '0');

                try
                {
                    _context.Add(data);
                    _context.SaveChanges();
                    LogService.CreateLog($"Data \"{data.Name}\" inserted by \"{actionUser.UserName}\"  Data: {JsonConvert.SerializeObject(data)}.", LogTypeEnum.Information, LogOriginEnum.DataNexApp, actionUser.Id, _context);

                }
                catch (Exception ex)
                {
                    LogService.CreateLog($"Data \"{data.Name}\" could not be inserted by \"{actionUser.UserName}\"  Data: {JsonConvert.SerializeObject(data)} Error: {ex.Message}.", LogTypeEnum.Error, LogOriginEnum.DataNexApp, actionUser.Id, _context);
                    throw;
                }
            };

            var dataToReturn = _mapper.Map<DocumentSeriesDto>(data);

            return Ok(dataToReturn);
        }

        [HttpPut("updatedto")]
        public async Task<IActionResult> UpdateDto([FromBody] DocumentSeriesDto dto)
        {
            Guid companyId = GetCompanyFromHeader();

            var actionUser = await GetActionUser();

            var data = await _context.DocumentSeries.FirstOrDefaultAsync(x => x.Id == dto.Id && x.CompanyId == companyId);

            data.Name = dto.Name;
            data.Abbreviation = dto.Abbreviation;
            data.AllowManualNumbering = dto.AllowManualNumbering;
            data.Prefix = dto.Prefix;
            data.Suffix = dto.Suffix;
            data.Notes = dto.Notes;
            data.DocumentTypeId = dto.DocumentTypeId;
            data.IsActive = dto.IsActive; 
            data.UserUpdated = actionUser.Id;
            data.DateUpdated = DateTime.Now;
            data.CompanyId = companyId;

            try
            {
                await _context.SaveChangesAsync();

                LogService.CreateLog($"Data \"{data.Name}\" updated by \"{actionUser.UserName}\". Data: {JsonConvert.SerializeObject(data)}", LogTypeEnum.Information, LogOriginEnum.DataNexApp, actionUser.Id, _context);

            }
            catch (Exception ex)
            {
                LogService.CreateLog($"Data \"{data.Name}\" could not be updated by \"{actionUser.UserName}\"  Data: {JsonConvert.SerializeObject(data)} Error:{ex.Message}.", LogTypeEnum.Error, LogOriginEnum.DataNexApp, actionUser.Id, _context);

            }

            return Ok(data);
        }

        [HttpDelete("deletebyid/{id}")]
        public async Task<IActionResult> DeleteById(Guid id)
        {
            Guid companyId = GetCompanyFromHeader();

            var actionUser = await GetActionUser();

            var data = await _context.DocumentSeries.FirstOrDefaultAsync(x => x.Id == id && x.CompanyId == companyId);

            try
            {
                _context.DocumentSeries.Remove(data);
                await _context.SaveChangesAsync();
                LogService.CreateLog($"Data \"{data.Name}\" deleted by \"{actionUser.UserName}\"  Data: {JsonConvert.SerializeObject(data)}.", LogTypeEnum.Information, LogOriginEnum.DataNexApp, actionUser.Id, _context);

            }
            catch (Exception ex)
            {
                LogService.CreateLog($"Data \"{data.Name}\" could not be deleted by \"{actionUser.UserName}\"  Data: {JsonConvert.SerializeObject(data)} Error:{ex.Message}.", LogTypeEnum.Error, LogOriginEnum.DataNexApp, actionUser.Id, _context);

            }
            return Ok(data);
        }
    }
}
 