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
    public class DocumentAdditionalChargesController:BaseController
    {
        private ApplicationDbContext _context;
        private IMapper _mapper;
        public DocumentAdditionalChargesController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }


        [HttpGet("getall")]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.AdditionalCharges.ToListAsync();

            return Ok(data);
        }

        [HttpGet("getbydocumentid/{id}")]
        public async Task<IActionResult> GetByDocumentId(Guid id)
        {
            var data = await _context.DocumentAdditionalCharges.Where(x=>x.DocumentId == id).ToListAsync();

            return Ok(data);
        }

        [HttpPost("insertdto")]
        public async Task<IActionResult> InsertDto([FromBody] DocumentAdditionalChargeDto documentAdditionalCharge)
        {
            var actionUser = await GetActionUser();

            var data = new DocumentAdditionalCharge();
            data.DocumentId = documentAdditionalCharge.DocumentId;
            data.AdditionalChargeId = documentAdditionalCharge.AdditionalChargeId;
            data.AdditionalChargeAmount = documentAdditionalCharge.AdditionalChargeAmount;

            //Update Orders Total

            var document = await _context.Documents.FirstOrDefaultAsync(x => x.Id == data.DocumentId);

            //Document may have not been inserted yet
            if(document != null)
            {
                document.DocumentTotal += documentAdditionalCharge.AdditionalChargeAmount;
            }

            try
            {
                _context.DocumentAdditionalCharges.Add(data);
                await _context.SaveChangesAsync();
                LogService.CreateLog($"Document Additional Charge inserted by \"{actionUser.UserName}\"  Document Additional Charge: {JsonConvert.SerializeObject(data)}.", LogTypeEnum.Information, LogOriginEnum.DataNexApp, actionUser.Id, _context);

            }
            catch (Exception ex)
            {
                LogService.CreateLog($"Document Additional Charge could not be inserted by \"{actionUser.UserName}\" Document Additional Charge: {JsonConvert.SerializeObject(data)} Error: {ex.Message}.", LogTypeEnum.Error, LogOriginEnum.DataNexApp, actionUser.Id, _context);

            }

            var dto = _mapper.Map<DocumentAdditionalChargeDto>(data);

            return Ok(dto);
        }

        [HttpPut("updatedto")]
        public async Task<IActionResult> UpdateDto([FromBody] DocumentAdditionalChargeDto documentAdditionalCharge)
        {
            var actionUser = await GetActionUser();

            var data = await _context.DocumentAdditionalCharges.FirstOrDefaultAsync(x => x.Id == documentAdditionalCharge.Id);
            var document = await _context.Documents.FirstOrDefaultAsync(x => x.Id == data.DocumentId);

            //Remove previous value
            document.DocumentTotal -= data.AdditionalChargeAmount;

            data.DocumentId = documentAdditionalCharge.DocumentId;
            data.AdditionalChargeId = documentAdditionalCharge.AdditionalChargeId;
            data.AdditionalChargeAmount = documentAdditionalCharge.AdditionalChargeAmount;
            data.UserUpdated = actionUser.Id;
            data.DateUpdated = DateTime.Now;

            //Add new value
            document.DocumentTotal += documentAdditionalCharge.AdditionalChargeAmount;
          
            try
            {
                await _context.SaveChangesAsync();

                LogService.CreateLog($"Document Additional Charge updated by \"{actionUser.UserName}\"  Document Additional Charge: {JsonConvert.SerializeObject(data)}.", LogTypeEnum.Information, LogOriginEnum.DataNexApp, actionUser.Id, _context);

            }
            catch (Exception ex)
            {
                LogService.CreateLog($"Document Additional Charge could not be updated by \"{actionUser.UserName}\" Document Additional Charge: {JsonConvert.SerializeObject(data)} Error: {ex.Message}.", LogTypeEnum.Error, LogOriginEnum.DataNexApp, actionUser.Id, _context);
            }

            return Ok(data);
        }

        [HttpDelete("deletebyid/{id}")]
        public async Task<IActionResult> DeleteById(Guid id)
        {
            var actionUser = await GetActionUser();

            var data = await _context.DocumentAdditionalCharges.FirstOrDefaultAsync(x => x.Id == id);

            var document = await _context.Documents.FirstOrDefaultAsync(x => x.Id == data.DocumentId);

            document.DocumentTotal -= data.AdditionalChargeAmount;

            try
            {
                _context.DocumentAdditionalCharges.Remove(data);
                await _context.SaveChangesAsync();
                LogService.CreateLog($"Document Additional Charge deleted by \"{actionUser.UserName}\"  Document Additional Charge: {JsonConvert.SerializeObject(data)}.", LogTypeEnum.Information, LogOriginEnum.DataNexApp, actionUser.Id, _context);

            }
            catch (Exception ex)
            {
                LogService.CreateLog($"Document Additional Charge could not be deleted by \"{actionUser.UserName}\" Document Additional Charge: {JsonConvert.SerializeObject(data)} Error: {ex.Message}.", LogTypeEnum.Error, LogOriginEnum.DataNexApp, actionUser.Id, _context);
            }
            return Ok(data);
        }
    }
}
