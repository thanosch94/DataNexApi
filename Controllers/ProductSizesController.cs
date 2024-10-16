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
    public class ProductSizesController:BaseController
    {
        private ApplicationDbContext _context;
        private IMapper _mapper;
        private static readonly object _lockObject = new object();

        public ProductSizesController(ApplicationDbContext context, IMapper mapper):base(context)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet("getall")]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.ProductSizes.ToListAsync();

            return Ok(data);
        }

        [HttpGet("getbyid/{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var data = await _context.ProductSizes.Where(x => x.Id == id).FirstOrDefaultAsync();

            var dto = _mapper.Map<ProductSizeDto>(data);

            return Ok(dto);
        }

        [HttpGet("getlookup")]
        public async Task<IActionResult> GetLookup()
        {
            var data = await _context.ProductSizes.Select(x => new ProductSizeDto()
            {
                Id = x.Id,
                Name = x.Name
            }).ToListAsync();

            return Ok(data);
        }

        [HttpPost("insertdto")]
        public async Task<IActionResult> InsertDto([FromBody] ProductSizeDto productSize)
        {
            var actionUser = await GetActionUser();

            var data = new ProductSize();
            data.Name = productSize.Name;
            data.Abbreviation = productSize.Abbreviation;
            data.UserAdded = actionUser.Id;

            lock (_lockObject)
            {
                var maxNumber = _context.ProductSizes.Max(x => (x.SerialNumber)) ?? 0;
                data.SerialNumber = maxNumber + 1;
                data.Code = data.SerialNumber.ToString().PadLeft(5, '0');

                try
                {
                    _context.ProductSizes.Add(data);
                    _context.SaveChanges();
                    LogService.CreateLog($"Product Size \"{data.Name}\" inserted by \"{actionUser.UserName}\". Product Size: {JsonConvert.SerializeObject(data)}", LogTypeEnum.Information, LogOriginEnum.DataNexApp, actionUser.Id, _context);
                }
                catch (Exception ex)
                {
                    LogService.CreateLog($"Product Size \"{data.Name}\" could not be inserted by \"{actionUser.UserName}\". Product Size: {JsonConvert.SerializeObject(data)} Error: {ex.Message}", LogTypeEnum.Error, LogOriginEnum.DataNexApp, actionUser.Id, _context);
                    throw;
                }
            };
            var dto = _mapper.Map<ProductSizeDto>(data);

            return Ok(dto);
        }

        [HttpPut("updatedto")]
        public async Task<IActionResult> UpdateDto([FromBody] ProductSizeDto productSize)
        {
            var actionUser = await GetActionUser();

            var data = await _context.ProductSizes.FirstOrDefaultAsync(x => x.Id == productSize.Id);

            data.Name = productSize.Name;
            data.Abbreviation = productSize.Abbreviation;

            try
            {
                await _context.SaveChangesAsync();
                LogService.CreateLog($"Product Size \"{data.Name}\" updated by \"{actionUser.UserName}\". Product Size: {JsonConvert.SerializeObject(data)}", LogTypeEnum.Information, LogOriginEnum.DataNexApp, actionUser.Id, _context);
            }
            catch (Exception ex)
            {
                LogService.CreateLog($"Product Size could not be updated by \"{actionUser.UserName}\". Product Size: {JsonConvert.SerializeObject(data)} Error: {ex.Message}", LogTypeEnum.Error, LogOriginEnum.DataNexApp, actionUser.Id, _context);
            }

            var dto = _mapper.Map<ProductSizeDto>(data);

            return Ok(dto);
        }

        [HttpDelete("deletebyid/{id}")]
        public async Task<IActionResult> DeleteById(Guid id)
        {
            var actionUser = await GetActionUser();

            var data = await _context.ProductSizes.FirstOrDefaultAsync(x => x.Id == id);

            try
            {
                _context.ProductSizes.Remove(data);
                await _context.SaveChangesAsync();
                LogService.CreateLog($"Product Size \"{data.Name}\" deleted by \"{actionUser.UserName}\". Product Size: {JsonConvert.SerializeObject(data)}", LogTypeEnum.Information, LogOriginEnum.DataNexApp, actionUser.Id, _context);
            }
            catch (Exception ex)
            {
                LogService.CreateLog($"Product Size \"{data.Name}\" could not be deleted by \"{actionUser.UserName}\". Product Size: {JsonConvert.SerializeObject(data)} Error: {ex.Message}", LogTypeEnum.Error, LogOriginEnum.DataNexApp, actionUser.Id, _context);
            }
            return Ok(data);
        }
    }
}
