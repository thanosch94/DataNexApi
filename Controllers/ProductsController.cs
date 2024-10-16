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
    public class ProductsController : BaseController
    {


        private ApplicationDbContext _context;
        private IMapper _mapper;
        private static readonly object _lockObject = new object();

        public ProductsController(ApplicationDbContext context, IMapper mapper) : base(context)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet("getall")]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.Products.Include(x => x.Brand).Select(x => new ProductDto()
            {
                Id = x.Id,
                SerialNumber = x.SerialNumber,
                Code = x.Code,
                Name = x.Name,
                Sku = x.Sku,
                Description = x.Description,
                ImagePath = x.ImagePath,
                RetailPrice = x.RetailPrice,
                WholesalePrice = x.WholesalePrice,
                VatClassId = x.VatClassId,
                BrandId = x.BrandId,
                BrandName = x.Brand.Name
            }).ToListAsync();

            return Ok(data);
        }


        [HttpGet("getbyid/{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var data = await _context.Products.Include(x => x.Brand).Where(x => x.Id == id).Select(x => new ProductDto()
            {
                Id = x.Id,
                SerialNumber = x.SerialNumber,
                Code = x.Code,
                Name = x.Name,
                Sku = x.Sku,
                Description = x.Description,
                ImagePath = x.ImagePath,
                RetailPrice = x.RetailPrice,
                WholesalePrice = x.WholesalePrice,
                VatClassId = x.VatClassId,
                BrandId = x.BrandId,
                BrandName = x.Brand.Name
            }).FirstOrDefaultAsync();

            var dto = _mapper.Map<ProductDto>(data);


            return Ok(dto);
        }


        [HttpGet("getbysku/{sku}")]
        public async Task<IActionResult> GetBySku(string sku)
        {
            var data = await _context.Products.Where(x => x.Sku == sku).FirstOrDefaultAsync();

            var dto = _mapper.Map<ProductDto>(data);

            return Ok(dto);
        }

        [HttpGet("getlookup")]
        public async Task<IActionResult> GetLookup()
        {
            var data = await _context.Products.Select(x => new ProductDto()
            {
                Id = x.Id,
                Name = x.Name
            }).ToListAsync();

            return Ok(data);
        }


        [HttpPost("insertdto")]
        public async Task<IActionResult> InsertDto([FromBody] ProductDto product)
        {
            var actionUser = await GetActionUser();

            var data = new Product();
            data.Name = product.Name;
            data.Sku = product.Sku;
            data.Description = product.Description;
            data.ImagePath = product.ImagePath;
            data.RetailPrice = product.RetailPrice;
            data.WholesalePrice = product.WholesalePrice;
            data.BrandId = product.BrandId;
            data.VatClassId = product.VatClassId;
            data.UserAdded = actionUser.Id;


            lock (_lockObject)
            {
                var maxNumber = _context.Products.Max(x => (x.SerialNumber)) ?? 0;
                data.SerialNumber = maxNumber + 1;
                data.Code = data.SerialNumber.ToString().PadLeft(5, '0');

                try
                {
                    _context.Products.Add(data);
                    _context.SaveChanges();
                    LogService.CreateLog($"Product \"{data.Name}\" inserted by \"{actionUser.UserName}\". Product: {JsonConvert.SerializeObject(data)}", LogTypeEnum.Information, LogOriginEnum.DataNexApp, actionUser.Id, _context);
                }
                catch (Exception ex)
                {
                    LogService.CreateLog($"Product \"{data.Name}\" could not be inserted by \"{actionUser.UserName}\". Product: {JsonConvert.SerializeObject(data)} Error: {ex.Message}", LogTypeEnum.Error, LogOriginEnum.DataNexApp, actionUser.Id, _context);
                    throw;
                }
            };
            var dto = _mapper.Map<ProductDto>(data);

            return Ok(dto);
        }


        [HttpPut("updatedto")]
        public async Task<IActionResult> UpdateDto([FromBody] ProductDto product)
        {
            var actionUser = await GetActionUser();

            var data = await _context.Products.FirstOrDefaultAsync(x => x.Id == product.Id);

            data.Name = product.Name;
            data.Sku = product.Sku;
            data.Description = product.Description;
            data.ImagePath = product.ImagePath;
            data.RetailPrice = product.RetailPrice;
            data.WholesalePrice = product.WholesalePrice;
            data.VatClassId = product.VatClassId;
            data.BrandId = product.BrandId;

            try
            {
                await _context.SaveChangesAsync();
                LogService.CreateLog($"Product \"{data.Name}\" updated by \"{actionUser.UserName}\". Product: {JsonConvert.SerializeObject(data)}", LogTypeEnum.Information, LogOriginEnum.DataNexApp, actionUser.Id, _context);

            }
            catch (Exception ex)
            {
                LogService.CreateLog($"Product could not be updated by \"{actionUser.UserName}\". Product: {JsonConvert.SerializeObject(data)} Error: {ex.Message}", LogTypeEnum.Error, LogOriginEnum.DataNexApp, actionUser.Id, _context);

            }

            var dto = _mapper.Map<ProductDto>(data);

            return Ok(dto);
        }

        [HttpDelete("deletebyid/{id}")]
        public async Task<IActionResult> DeleteById(Guid id)
        {
            var actionUser = await GetActionUser();

            var data = await _context.Products.FirstOrDefaultAsync(x => x.Id == id);

            try
            {
                _context.Products.Remove(data);
                await _context.SaveChangesAsync();
                LogService.CreateLog($"Product \"{data.Name}\" deleted by \"{actionUser.UserName}\"  Product: {JsonConvert.SerializeObject(data)}.", LogTypeEnum.Information, LogOriginEnum.DataNexApp, actionUser.Id, _context);

            }
            catch (Exception ex)
            {
                LogService.CreateLog($"Product \"{data.Name}\" could not be deleted by \"{actionUser.UserName}\"  Product: {JsonConvert.SerializeObject(data)} Error:{ex.Message}.", LogTypeEnum.Error, LogOriginEnum.DataNexApp, actionUser.Id, _context);

            }
            return Ok(data);
        }
    }
}

