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
using System.Reflection.Metadata;

namespace DataNexApi.Controllers
{
    [Authorize]
    public class AdditionalChargesController : BaseController
    {

        private ApplicationDbContext _context;
        private IMapper _mapper;
        private static readonly object _lockObject = new object();

        public AdditionalChargesController(ApplicationDbContext context, IMapper mapper) : base(context)
        {
            _context = context;
            _mapper = mapper;
        }


        [HttpGet("getall")]
        public async Task<IActionResult> GetAll()
        {
            Guid companyId = GetCompanyFromHeader();

            var data = await _context.AdditionalCharges.Where(x=> x.CompanyId==companyId).ToListAsync();

            return Ok(data);
        }

        [HttpPost("insertdto")]
        public async Task<IActionResult> InsertDto([FromBody] AdditionalChargeDto additionalCharge)
        {
            Guid companyId = GetCompanyFromHeader();

            var actionUser = await GetActionUser();

            var data = new AdditionalCharge();
            data.Name = additionalCharge.Name;
            data.CompanyId = companyId;
            //var source = await _context.AdditionalCharges.OrderByDescending(x => x.SerialNumber).FirstOrDefaultAsync();


            lock (_lockObject)
            {
                var maxNumber = _context.AdditionalCharges.Where(x=> x.CompanyId == companyId).Max(x => (x.SerialNumber)) ?? 0;
                data.SerialNumber = maxNumber + 1;
                data.Code = data.SerialNumber.ToString().PadLeft(5, '0');
                try
                {
                    _context.AdditionalCharges.Add(data);
                    _context.SaveChanges();
                    LogService.CreateLog($"Additional Charge \"{data.Name}\" inserted by \"{actionUser.UserName}\"  Additional Charge: {JsonConvert.SerializeObject(data)}.", LogTypeEnum.Information, LogOriginEnum.DataNexApp, actionUser.Id, _context);

                }
                catch (Exception ex)
                {
                    LogService.CreateLog($"Additional Charge \"{data.Name}\" could not be inserted by \"{actionUser.UserName}\"  Additional Charge: {JsonConvert.SerializeObject(data)} Error: {ex.Message}.", LogTypeEnum.Error, LogOriginEnum.DataNexApp, actionUser.Id, _context);
                    throw;
                }

            }

            var dto = _mapper.Map<AdditionalChargeDto>(data);

            return Ok(dto);
        }

        [HttpPut("updatedto")]
        public async Task<IActionResult> UpdateDto([FromBody] AdditionalChargeDto dto)
        {
            Guid companyId = GetCompanyFromHeader();

            var actionUser = await GetActionUser();

            var data = await _context.AdditionalCharges.FirstOrDefaultAsync(x => x.Id == dto.Id && x.CompanyId == companyId);

            data.Name = dto.Name;
            data.UserUpdated = actionUser.Id;
            data.DateUpdated = DateTime.Now;
            data.CompanyId = companyId;

            try
            {
                await _context.SaveChangesAsync();

                LogService.CreateLog($"Additional Charge \"{data.Name}\" updated by \"{actionUser.UserName}\"  Additional Charge: {JsonConvert.SerializeObject(data)}.", LogTypeEnum.Information, LogOriginEnum.DataNexApp, actionUser.Id, _context);

            }
            catch (Exception ex)
            {
                LogService.CreateLog($"Additional Charge \"{data.Name}\" could not be updated by \"{actionUser.UserName}\"  Additional Charge: {JsonConvert.SerializeObject(data)} Error: {ex.Message}.", LogTypeEnum.Error, LogOriginEnum.DataNexApp, actionUser.Id, _context);

            }

            return Ok(data);
        }

        [HttpDelete("deletebyid/{id}")]
        public async Task<IActionResult> DeleteById(Guid id)
        {
            Guid companyId = GetCompanyFromHeader();

            var actionUser = await GetActionUser();

            var data = await _context.AdditionalCharges.FirstOrDefaultAsync(x => x.Id == id && x.CompanyId==companyId);

            try
            {
                _context.AdditionalCharges.Remove(data);
                await _context.SaveChangesAsync();
                LogService.CreateLog($"Additional Charge \"{data.Name}\" deleted by \"{actionUser.UserName}\"  Additional Charge: {JsonConvert.SerializeObject(data)}.", LogTypeEnum.Information, LogOriginEnum.DataNexApp, actionUser.Id, _context);

            }
            catch (Exception ex)
            {
                LogService.CreateLog($"Additional Charge \"{data.Name}\" could not be deleted by \"{actionUser.UserName}\"  Additional Charge: {JsonConvert.SerializeObject(data)} Error: {ex.Message}.", LogTypeEnum.Error, LogOriginEnum.DataNexApp, actionUser.Id, _context);

            }
            return Ok(data);
        }

        //private int GetNextAdditionalChargeSerial()
        //{

        //    var parameter = new Microsoft.Data.SqlClient.SqlParameter("@result", 8);

        //    parameter.Direction = System.Data.ParameterDirection.Output;

        //    _context.Database.ExecuteSqlRaw("set @result = NEXT VALUE FOR AdditionalChargeSerialNumbers", parameter);

        //    var nextVal = (int)parameter.Value;

        //    return nextVal;
        //}
    }
}
