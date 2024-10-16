using AutoMapper;
using DataNex.Data;
using DataNex.Model.Dtos;
using DataNex.Model.Enums;
using DataNex.Model.Models;
using DataNexApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Security.Claims;

namespace DataNexApi.Controllers
{
    [Authorize]
    public class BrandsController : BaseController
    {
        private ApplicationDbContext _context;
        private IMapper _mapper;
        private static readonly object _lockObject = new object();

        public BrandsController(ApplicationDbContext context, IMapper mapper):base(context)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet("getall")]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.Brands.ToListAsync();

            return Ok(data);
        }


        [HttpGet("getbyid/{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var data = await _context.Brands.Where(x => x.Id == id).FirstOrDefaultAsync();

            var dto = _mapper.Map<BrandDto>(data);

            return Ok(dto);
        }

        [HttpGet("getlookup")]
        public async Task<IActionResult> GetLookup()
        {
            var data = await _context.Brands.Select(x => new BrandDto()
            {
                Id = x.Id,
                Name = x.Name
            }).ToListAsync();

            return Ok(data);
        }

        [HttpPost("insertdto")]
        public async Task<IActionResult> InsertDto([FromBody] BrandDto brand)
        {
            var actionUser = await GetActionUser();
            var data = new Brand();
            var source = await _context.Brands.OrderByDescending(x => x.SerialNumber).FirstOrDefaultAsync();

            data.Name = brand.Name;
            data.UserAdded = actionUser.Id;
            lock (_lockObject)
            {
                var maxNumber = _context.Brands.Max(x => (x.SerialNumber)) ?? 0;
                data.SerialNumber = maxNumber+1;
                data.Code = data.SerialNumber.ToString().PadLeft(5, '0');

                try
                {
                    _context.Brands.Add(data);
                    _context.SaveChanges();
                    LogService.CreateLog($"Brand \"{data.Name}\" inserted by \"{actionUser.UserName}\"  Brand: {JsonConvert.SerializeObject(data)}.", LogTypeEnum.Information, LogOriginEnum.DataNexApp, actionUser.Id, _context);

                }
                catch (Exception ex)
                {
                    LogService.CreateLog($"Brand \"{data.Name}\" could not be inserted by \"{actionUser.UserName}\"  Brand: {JsonConvert.SerializeObject(data)} Error: {ex.Message}.", LogTypeEnum.Error, LogOriginEnum.DataNexApp, actionUser.Id, _context);
                    throw;
                }

            };

            var dto = _mapper.Map<BrandDto>(data);

            return Ok(dto);
        }

        [HttpPut("updatedto")]
        public async Task<IActionResult> UpdateDto([FromBody] BrandDto brand)
        {
            var actionUser = await GetActionUser();

            var data = await _context.Brands.FirstOrDefaultAsync(x => x.Id == brand.Id);

            data.Name = brand.Name;
            data.UserUpdated = actionUser.Id;
            data.DateUpdated = DateTime.Now;

            try
            {
                await _context.SaveChangesAsync();

                LogService.CreateLog($"Brand \"{data.Name}\" updated by \"{actionUser.UserName}\". Brand: {JsonConvert.SerializeObject(data)}", LogTypeEnum.Information, LogOriginEnum.DataNexApp, actionUser.Id, _context);

            }
            catch (Exception ex)
            {
                LogService.CreateLog($"Brand \"{data.Name}\" could not be updated by \"{actionUser.UserName}\"  Brand: {JsonConvert.SerializeObject(data)} Error:{ex.Message}.", LogTypeEnum.Error, LogOriginEnum.DataNexApp, actionUser.Id, _context);

            }

            return Ok(data);
        }

        [HttpDelete("deletebyid/{id}")]
        public async Task<IActionResult> DeleteById(Guid id)
        {
            var actionUser = await GetActionUser();

            var data = await _context.Brands.FirstOrDefaultAsync(x => x.Id == id);

            try
            {            
                _context.Brands.Remove(data);
                await _context.SaveChangesAsync();
                LogService.CreateLog($"Brand \"{data.Name}\" deleted by \"{actionUser.UserName}\"  Brand: {JsonConvert.SerializeObject(data)}.", LogTypeEnum.Information, LogOriginEnum.DataNexApp, actionUser.Id, _context);

            }
            catch (Exception ex)
            {
                LogService.CreateLog($"Brand \"{data.Name}\" could not be deleted by \"{actionUser.UserName}\"  Brand: {JsonConvert.SerializeObject(data)} Error:{ex.Message}.", LogTypeEnum.Error, LogOriginEnum.DataNexApp, actionUser.Id, _context);

            }
            return Ok(data);
        }

   

    }
}
