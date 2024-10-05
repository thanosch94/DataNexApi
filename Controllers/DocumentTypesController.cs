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
using System.Xml.Linq;

namespace DataNexApi.Controllers
{
    [Authorize]
    public class DocumentTypesController : BaseController
    {

        private ApplicationDbContext _context;
        private IMapper _mapper;
        public DocumentTypesController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet("getall")]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.DocumentTypes.ToListAsync();

            return Ok(data);
        }

        [HttpGet("getactivedocumenttypeslookupbydocumententity/{documentTypeGroup}")]
        public async Task<IActionResult> GetActiveDocumentTypesLookupByDocumentTypeGroup(DocumentTypeGroupEnum documentTypeGroup)
        {
            var data = await _context.DocumentTypes.Where(x=>x.DocumentTypeGroup == documentTypeGroup && x.IsActive==true).Select(x=> new DocumentTypeDto()
            {
                Id = x.Id,
                Abbreviation = x.Abbreviation,
            }).ToListAsync();

            return Ok(data);
        }

        [HttpGet("getbyid/{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var data = await _context.DocumentTypes.Where(x => x.Id == id).FirstOrDefaultAsync();

            var dto = _mapper.Map<DocumentTypeDto>(data);

            return Ok(dto);
        }


        [HttpPost("insertdto")]
        public async Task<IActionResult> InsertDto([FromBody] DocumentTypeDto documentType)

        {
            var actionUser = await GetActionUser();

            var data = new DocumentType();

            data.Name = documentType.Name;
            data.Description = documentType.Description;
            data.Abbreviation = documentType.Abbreviation;
            data.DocumentTypeGroup = documentType.DocumentTypeGroup;
            data.IsActive = documentType.IsActive;
            data.IsPersonBalanceAffected =documentType.IsPersonBalanceAffected;
            data.IsWareHouseAffected =documentType.IsWareHouseAffected;
            data.UserAdded = actionUser.Id;

            try
            {
                _context.DocumentTypes.Add(data);
                await _context.SaveChangesAsync();
                LogService.CreateLog($"Document Type \"{data.Name}\" inserted by \"{actionUser.UserName}\". Document Type: {JsonConvert.SerializeObject(data)}", LogTypeEnum.Information, LogOriginEnum.DataNexApp, actionUser.Id, _context);
            }
            catch (Exception ex)
            {
                LogService.CreateLog($"Document Type \"{data.Name}\" could not be inserted by \"{actionUser.UserName}\"  Document Type: {JsonConvert.SerializeObject(data)} Error: {ex.Message}.", LogTypeEnum.Error, LogOriginEnum.DataNexApp, actionUser.Id, _context);

            }

            var dto = _mapper.Map<DocumentTypeDto>(data);

            return Ok(dto);
        }


        [HttpPut("updatedto")]
        public async Task<IActionResult> UpdatetDto([FromBody] DocumentTypeDto documentType)
        {
            var actionUser = await GetActionUser();

            var data = await _context.DocumentTypes.Where(x => x.Id == documentType.Id).FirstOrDefaultAsync();


            if (!data.IsSeeded)
            {
                data.Name = documentType.Name;
                data.Description = documentType.Description;
                data.Abbreviation = documentType.Abbreviation;
                data.DocumentTypeGroup = documentType.DocumentTypeGroup;
                data.IsActive = documentType.IsActive;
                data.IsPersonBalanceAffected = documentType.IsPersonBalanceAffected;
                data.IsWareHouseAffected = documentType.IsWareHouseAffected;
                try
                {
                    await _context.SaveChangesAsync();
                    LogService.CreateLog($"Document Type \"{data.Name}\" updated by \"{actionUser.UserName}\". Document Type: {JsonConvert.SerializeObject(data)}", LogTypeEnum.Information, LogOriginEnum.DataNexApp, actionUser.Id, _context);

                }
                catch (Exception ex)
                {
                    LogService.CreateLog($"Document Type \"{data.Name}\" could not be updated by \"{actionUser.UserName}\"  Document Type: {JsonConvert.SerializeObject(data)} Error: {ex.Message}.", LogTypeEnum.Error, LogOriginEnum.DataNexApp, actionUser.Id, _context);

                }

                var dto = _mapper.Map<DocumentTypeDto>(data);

                return Ok(dto);
            }
            else
            {
                //If any change made to a seeded entity except IsActive
                if (data.Name != documentType.Name || data.Description != documentType.Description || data.Abbreviation != documentType.Abbreviation || data.DocumentTypeGroup != documentType.DocumentTypeGroup ||data.IsWareHouseAffected!=documentType.IsWareHouseAffected||data.IsPersonBalanceAffected!=documentType.IsPersonBalanceAffected)
                {
                    return BadRequest("Record cannot be updated. If necessary deactivate it and create a new one.");
                }
                else
                {
                    data.IsActive = documentType.IsActive;
                    try
                    {
                        await _context.SaveChangesAsync();
                        LogService.CreateLog($"Document Type \"{data.Name}\" updated by \"{actionUser.UserName}\". Document Type: {JsonConvert.SerializeObject(data)}", LogTypeEnum.Information, LogOriginEnum.DataNexApp, actionUser.Id, _context);

                    }
                    catch (Exception ex)
                    {
                        LogService.CreateLog($"Document Type \"{data.Name}\" could not be updated by \"{actionUser.UserName}\"  Document Type: {JsonConvert.SerializeObject(data)} Error: {ex.Message}.", LogTypeEnum.Error, LogOriginEnum.DataNexApp, actionUser.Id, _context);

                    }

                    var dto = _mapper.Map<DocumentTypeDto>(data);
                    return Ok(dto);
                }

            }

        }

        [HttpDelete("deletebyid/{id}")]
        public async Task<IActionResult> DeleteById(Guid id)
        {
            var actionUser = await GetActionUser();

            var data = await _context.DocumentTypes.FirstOrDefaultAsync(x => x.Id == id);

            if (!data.IsSeeded)
            {
                try
                {
                    _context.DocumentTypes.Remove(data);
                    await _context.SaveChangesAsync();
                    LogService.CreateLog($"Document Type \"{data.Name}\" deleted by \"{actionUser.UserName}\"  Document Type: {JsonConvert.SerializeObject(data)}.", LogTypeEnum.Information, LogOriginEnum.DataNexApp, actionUser.Id, _context);

                }
                catch (Exception ex)
                {
                    LogService.CreateLog($"Document Type \"{data.Name}\" could not be deleted by \"{actionUser.UserName}\"  Document Type: {JsonConvert.SerializeObject(data)} Error:{ex.Message}.", LogTypeEnum.Error, LogOriginEnum.DataNexApp, actionUser.Id, _context);

                }
                var dto = _mapper.Map<DocumentTypeDto>(data);

                return Ok(dto);
            }
            else
            {
                LogService.CreateLog($"Document Type \"{data.Name}\" could not be deleted by \"{actionUser.UserName}\"  Document Type: {JsonConvert.SerializeObject(data)} Error:Record is seeded.", LogTypeEnum.Error, LogOriginEnum.DataNexApp, actionUser.Id, _context);

                return BadRequest("Record cannot be deleted. It can only be deactivated.");
            }

        }
    }
}

