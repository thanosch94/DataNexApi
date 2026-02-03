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
    public class DocTypeTransformationsController : BaseController
    {
        private ApplicationDbContext _context;
        private IMapper _mapper;
        private static readonly object _lockObject = new object();

        public DocTypeTransformationsController(ApplicationDbContext context, IMapper mapper) : base(context)
        {
            _context = context;
            _mapper = mapper;
        }


        [HttpGet("getall")]
        public async Task<IActionResult> GetAll()
        {
            Guid companyId = GetCompanyFromHeader();

            var data = await _context.DocTypeTransformations.Where(x => x.CompanyId == companyId).ToListAsync();
            var dataToReturn = _mapper.Map<List<DocTypeTransformationDto>>(data);

            return Ok(dataToReturn);
        }

        [HttpGet("getbyid/{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            Guid companyId = GetCompanyFromHeader();

            var data = await _context.DocTypeTransformations.FirstOrDefaultAsync(x => x.Id == id && x.CompanyId == companyId);
            var dto = _mapper.Map<DocTypeTransformationDto>(data);

            return Ok(dto);
        }

        [HttpPost("insertdto")]
        public async Task<IActionResult> InsertDto([FromBody] DocTypeTransformationDto dto)
        {
            Guid companyId = GetCompanyFromHeader();

            var actionUser = await GetActionUser();

            var data = new DocTypeTransformation();

            var exists = await _context.DocTypeTransformations.Where(x => x.From == dto.To && x.CompanyId == companyId).FirstOrDefaultAsync();
            if (exists == null)
            {
                data.From = dto.From;
                data.To = dto.To;
                data.UserAdded = actionUser.Id;
                data.SourceStatusId = dto.SourceStatusId;
                data.TargetStatusId = dto.TargetStatusId;
                data.CompanyId = companyId;

                lock (_lockObject)
                {
                    var maxNumber = _context.DocTypeTransformations.Where(x => x.CompanyId == companyId).Max(x => (x.SerialNumber)) ?? 0;
                    data.SerialNumber = maxNumber + 1;
                    data.Code = data.SerialNumber.ToString().PadLeft(5, '0');

                    try
                    {
                        _context.Add(data);
                        _context.SaveChanges();
                        LogService.CreateLog($"Data \"{data.From}\" to \"{data.To}\" inserted by \"{actionUser.UserName}\". Vat Class: {JsonConvert.SerializeObject(data)}", LogTypeEnum.Information, LogOriginEnum.DataNexApp, actionUser.Id, _context);

                    }
                    catch (Exception ex)
                    {
                        LogService.CreateLog($"Data \"{data.From}\" to \"{data.To}\" could not be inserted by \"{actionUser.UserName}\". Vat Class: {JsonConvert.SerializeObject(data)} Error: {ex.Message}", LogTypeEnum.Error, LogOriginEnum.DataNexApp, actionUser.Id, _context);
                        throw;
                    }
                };

                return Ok(data);
            }
            else
            {
                return BadRequest("Record already exists");
            }

        }

        [HttpPut("updatedto")]
        public async Task<IActionResult> UpdateDto([FromBody] DocTypeTransformationDto dto)
        {
            Guid companyId = GetCompanyFromHeader();

            var actionUser = await GetActionUser();

            var data = await _context.DocTypeTransformations.Where(x => x.Id == dto.Id && x.CompanyId == companyId).FirstOrDefaultAsync();

            data.From = dto.From;
            data.To = dto.To;
            data.SourceStatusId = dto.SourceStatusId;
            data.TargetStatusId = dto.TargetStatusId;
            data.CompanyId = companyId;
            data.UserUpdated = actionUser.Id;

            try
            {
                await _context.SaveChangesAsync();
                LogService.CreateLog($"Data \"{data.From}\" to \"{data.To}\"  updated by \"{actionUser.UserName}\". Vat Class: {JsonConvert.SerializeObject(data)}", LogTypeEnum.Information, LogOriginEnum.DataNexApp, actionUser.Id, _context);

            }
            catch (Exception ex)
            {
                LogService.CreateLog($"Data \"{data.From}\" to \"{data.To}\"  could not be updated by \"{actionUser.UserName}\" Vat Class: {JsonConvert.SerializeObject(data)} Error: {ex.Message}.", LogTypeEnum.Error, LogOriginEnum.DataNexApp, actionUser.Id, _context);

            }

            var dtoData = _mapper.Map<DocTypeTransformationDto>(data);

            return Ok(dtoData);

        }

        [HttpDelete("deletebyid/{id}")]
        public async Task<IActionResult> DeleteById(Guid id)
        {
            Guid companyId = GetCompanyFromHeader();

            var actionUser = await GetActionUser();

            var data = await _context.DocTypeTransformations.FirstOrDefaultAsync(x => x.Id == id && x.CompanyId == companyId);

            if (data != null)
            {
                _context.Remove(data);
                try
                {
                    await _context.SaveChangesAsync();
                    LogService.CreateLog($"Data \"{data.From}\" to \"{data.To}\"  deleted by \"{actionUser.UserName}\". Vat Class: {JsonConvert.SerializeObject(data)}", LogTypeEnum.Information, LogOriginEnum.DataNexApp, actionUser.Id, _context);

                }
                catch (Exception ex)
                {
                    LogService.CreateLog($"Data \"{data.From}\" to \"{data.To}\"  could not be deleted by \"{actionUser.UserName}\"  Vat Class: {JsonConvert.SerializeObject(data)} Error: {ex.Message}.", LogTypeEnum.Error, LogOriginEnum.DataNexApp, actionUser.Id, _context);

                }
                var dtoData = _mapper.Map<DocTypeTransformationDto>(data);

                return Ok(dtoData);
            }
            else
            {
                return BadRequest("Entity not found. It may be already deleted");
            }

        }


    }

}
