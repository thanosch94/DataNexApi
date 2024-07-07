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
    public class VatClassesController:BaseController
    {
        private ApplicationDbContext _context;
        private IMapper _mapper;
        public VatClassesController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet("getall")]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.VatClasses.ToListAsync();

            return Ok(data);
        }

        [HttpPost("insertdto")]
        public async Task<IActionResult> InsertDto([FromBody] VatClassDto dto)
        {
            var actionUser = await GetActionUser();

            var data = new VatClass();

            var exists = await _context.VatClasses.Where(x => x.Name == dto.Name).FirstOrDefaultAsync();
            if (exists == null)
            {
                data.Name = dto.Name;
                data.Description = dto.Description;
                data.Abbreviation = dto.Abbreviation;   
                data.Rate = dto.Rate;
                data.UserAdded = actionUser.Id;

                try
                {
                    _context.VatClasses.Add(data);
                    await _context.SaveChangesAsync();
                    LogService.CreateLog($"Vat Class \"{data.Name}\" inserted by \"{actionUser.UserName}\". Vat Class: {JsonConvert.SerializeObject(data)}", LogTypeEnum.Information, LogOriginEnum.DataNexApp, actionUser.Id, _context);

                }
                catch (Exception ex)
                {
                    LogService.CreateLog($"Vat Class \"{data.Name}\" could not be inserted by \"{actionUser.UserName}\". Vat Class: {JsonConvert.SerializeObject(data)} Error: {ex.Message}", LogTypeEnum.Error, LogOriginEnum.DataNexApp, actionUser.Id, _context);
                }

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
            var actionUser = await GetActionUser();

            var data = await _context.VatClasses.Where(x => x.Id == dto.Id).FirstOrDefaultAsync();

            data.Name = dto.Name;
            data.Description = dto.Description;
            data.Abbreviation = dto.Abbreviation;
            data.Rate = dto.Rate;
            data.IsActive = dto.IsActive;

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
            var actionUser = await GetActionUser();

            var data = await _context.VatClasses.FirstOrDefaultAsync(x => x.Id == id);

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
