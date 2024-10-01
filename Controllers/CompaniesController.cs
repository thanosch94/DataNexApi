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
    public class CompaniesController:BaseController
    {
        private ApplicationDbContext _context;
        private IMapper _mapper;
        public CompaniesController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet("getall")]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.Companies.ToListAsync();

            return Ok(data);
        }

        [AllowAnonymous]
        [HttpGet("getlookup")]
        public async Task<IActionResult> GetLookup()
        {
            var data = await _context.Companies.Select(x=> new CompanyDto()
            {
                Id = x.Id,
                Name = x.Name,
            }).ToListAsync();

            return Ok(data);
        }

        [AllowAnonymous]

        [HttpGet("getbyid/{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var data = await _context.Companies.Where(x=>x.Id==id).FirstOrDefaultAsync();

            return Ok(data);
        }


        [HttpPost("insertdto")]
        public async Task<IActionResult> InsertDto([FromBody] CompanyDto dto)
        {
            var actionUser = await GetActionUser();

            var data = new Company();
            data.Name = dto.Name;
            data.UserAdded = actionUser.Id;

            var hasDefault = await _context.Companies.AnyAsync(x => x.IsDefault == true);

            if(hasDefault && dto.IsDefault)
            {
                return BadRequest("A default company already exists");
            }
            else
            {
                data.IsDefault = dto.IsDefault;

                try
                {
                    _context.Companies.Add(data);
                    await _context.SaveChangesAsync();
                    LogService.CreateLog($"Company \"{data.Name}\" inserted by \"{actionUser.UserName}\"  Brand: {JsonConvert.SerializeObject(data)}.", LogTypeEnum.Information, LogOriginEnum.DataNexApp, actionUser.Id, _context);

                }
                catch (Exception ex)
                {
                    LogService.CreateLog($"Company \"{data.Name}\" could not be inserted by \"{actionUser.UserName}\"  Brand: {JsonConvert.SerializeObject(data)} Error: {ex.Message}.", LogTypeEnum.Error, LogOriginEnum.DataNexApp, actionUser.Id, _context);

                }

            }


            var companyDto = _mapper.Map<CompanyDto>(data);

            return Ok(companyDto);
        }

        [HttpPut("updatedto")]
        public async Task<IActionResult> UpdateDto([FromBody] CompanyDto dto)
        {
            var actionUser = await GetActionUser();

            var data = await _context.Companies.FirstOrDefaultAsync(x => x.Id == dto.Id);

            data.Name = dto.Name;
            data.IsDefault = dto.IsDefault;
            data.UserUpdated = actionUser.Id;
            data.DateUpdated = DateTime.Now;

            var defaultCompany = await _context.Companies.FirstOrDefaultAsync(x => x.IsDefault == true);


            if (defaultCompany != null)
            {
                if (dto.IsDefault == true)
                {
                    defaultCompany.IsDefault = false;
                }
            }
            try
            {
                await _context.SaveChangesAsync();

                LogService.CreateLog($"Company \"{data.Name}\" updated by \"{actionUser.UserName}\". Brand: {JsonConvert.SerializeObject(data)}", LogTypeEnum.Information, LogOriginEnum.DataNexApp, actionUser.Id, _context);

            }
            catch (Exception ex)
            {
                LogService.CreateLog($"Company \"{data.Name}\" could not be updated by \"{actionUser.UserName}\"  Brand: {JsonConvert.SerializeObject(data)} Error:{ex.Message}.", LogTypeEnum.Error, LogOriginEnum.DataNexApp, actionUser.Id, _context);

            }

            return Ok(data);
        }

        [HttpDelete("deletebyid/{id}")]
        public async Task<IActionResult> DeleteById(Guid id)
        {
            var actionUser = await GetActionUser();

            var data = await _context.Companies.FirstOrDefaultAsync(x => x.Id == id);
            if (data.IsDefault)
            {
                return BadRequest("Default Company cannot be deleted");
            }
            else
            {
                try
                {
                    _context.Companies.Remove(data);
                    await _context.SaveChangesAsync();
                    LogService.CreateLog($"Brand \"{data.Name}\" deleted by \"{actionUser.UserName}\"  Brand: {JsonConvert.SerializeObject(data)}.", LogTypeEnum.Information, LogOriginEnum.DataNexApp, actionUser.Id, _context);

                }
                catch (Exception ex)
                {
                    LogService.CreateLog($"Brand \"{data.Name}\" could not be deleted by \"{actionUser.UserName}\"  Brand: {JsonConvert.SerializeObject(data)} Error:{ex.Message}.", LogTypeEnum.Error, LogOriginEnum.DataNexApp, actionUser.Id, _context);

                }
            }
          
            return Ok(data);
        }
    }
}
