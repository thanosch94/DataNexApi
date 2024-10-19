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
            Guid companyId = GetCompanyFromHeader();

            var data = await _context.ProductSizes.Where(x => x.CompanyId == companyId).ToListAsync();

            return Ok(data);
        }

        [HttpGet("getbyid/{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            Guid companyId = GetCompanyFromHeader();

            var data = await _context.ProductSizes.Where(x => x.Id == id && x.CompanyId==companyId).FirstOrDefaultAsync();

            var dto = _mapper.Map<ProductSizeDto>(data);

            return Ok(dto);
        }

        [HttpGet("getlookup")]
        public async Task<IActionResult> GetLookup()
        {
            Guid companyId = GetCompanyFromHeader();

            var data = await _context.ProductSizes.Where(x=>x.CompanyId==companyId).Select(x => new ProductSizeDto()
            {
                Id = x.Id,
                Name = x.Name
            }).ToListAsync();

            return Ok(data);
        }

        [HttpPost("insertdto")]
        public async Task<IActionResult> InsertDto([FromBody] ProductSizeDto productSize)
        {
            Guid companyId = GetCompanyFromHeader();

            var actionUser = await GetActionUser();

            var data = new ProductSize();
            data.Name = productSize.Name;
            data.Abbreviation = productSize.Abbreviation;
            data.UserAdded = actionUser.Id;
            data.CompanyId=companyId;

            lock (_lockObject)
            {
                var maxNumber = _context.ProductSizes.Where(x=>x.CompanyId==companyId).Max(x => (x.SerialNumber)) ?? 0;
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
            Guid companyId = GetCompanyFromHeader();

            var actionUser = await GetActionUser();

            var data = await _context.ProductSizes.FirstOrDefaultAsync(x => x.Id == productSize.Id && x.CompanyId==companyId);

            data.Name = productSize.Name;
            data.Abbreviation = productSize.Abbreviation;
            data.CompanyId = companyId;

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
            Guid companyId = GetCompanyFromHeader();

            var actionUser = await GetActionUser();

            var data = await _context.ProductSizes.FirstOrDefaultAsync(x => x.Id == id && x.CompanyId==companyId);

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
