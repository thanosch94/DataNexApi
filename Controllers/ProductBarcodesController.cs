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
    public class ProductBarcodesController : BaseController
    {
        private ApplicationDbContext _context;
        private IMapper _mapper;
        public ProductBarcodesController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet("getall")]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.ProductBarcodes.ToListAsync();

            return Ok(data);
        }

        [HttpGet("getlookup")]
        public async Task<IActionResult> GetLookup()
        {
            var data = await _context.ProductBarcodes.Select(x => x.Barcode).ToListAsync();

            return Ok(data);
        }


        [HttpGet("getbyid/{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var data = await _context.ProductBarcodes.Where(x => x.Id == id).FirstOrDefaultAsync();

            var dto = _mapper.Map<ProductBarcodeDto>(data);


            return Ok(dto);
        }



        [HttpGet("getbybarcode/{barcode}")]
        public async Task<IActionResult> GetByBarcode(string barcode)
        {
            var product = await _context.ProductBarcodes.Include(x => x.Product).Include(x => x.Size).Where(x => x.Barcode == barcode).Select(x => new ProductBarcodeDto()
            {
                ProductId = x.ProductId,
                ProductName = x.Product.Name,
                Sku = x.Product.Sku,
                SizeId = x.SizeId,
                SizeName = x.Size.Name,
                ProductRetailPrice = (decimal)x.Product.RetailPrice,
                VatClassId = x.Product.VatClassId,
                Barcode =x.Barcode
            }).FirstOrDefaultAsync();

            return Ok(product);
        }

        [HttpGet("getbyproductid/{productid}")]
        public async Task<IActionResult> GetByProductId(Guid productid)
        {
            var data = await _context.ProductBarcodes.Include(x => x.Size).Where(x => x.ProductId == productid).Select(x => new ProductBarcodeDto()
            {
                Id = x.Id,
                ProductId = x.ProductId,
                SizeId = x.SizeId,
                SizeName = x.Size.Name,
                Barcode = x.Barcode,


            }).ToListAsync();

            return Ok(data);
        }



        [HttpPost("insertdto")]
        public async Task<IActionResult> InsertDto([FromBody] ProductBarcodeDto productBarcode)

        {
            var actionUser = await GetActionUser();

            var data = new ProductBarcode();

            data.ProductId = productBarcode.ProductId;
            data.SizeId = productBarcode.SizeId;
            data.Barcode = productBarcode.Barcode;
            data.UserAdded = actionUser.Id;

            if (data.Barcode == null||data.Barcode==string.Empty || data.SizeId == null)
            {
                return BadRequest("Barcode and Size cannot be empty");         
            }

            var sizeExists = await _context.ProductBarcodes.Where(x => x.ProductId == productBarcode.ProductId && x.SizeId == productBarcode.SizeId).FirstOrDefaultAsync();
            var barcodeExists = await _context.ProductBarcodes.Where(x => x.Barcode == productBarcode.Barcode).FirstOrDefaultAsync();

            if (barcodeExists!=null)
            {
                return BadRequest("Barcode exists");
            }
            else if (sizeExists != null)
            {
                return BadRequest("Record for Size already exists");
            }
            else
            {

                try
                {                
                    _context.ProductBarcodes.Add(data);
                    await _context.SaveChangesAsync();
                    LogService.CreateLog($"Product Barcode  \"{data.Barcode}\" inserted by \"{actionUser.UserName}\". Product Barcode: {JsonConvert.SerializeObject(data)}", LogTypeEnum.Information, LogOriginEnum.DataNexApp, actionUser.Id, _context);
                }
                catch (Exception ex)
                {
                    LogService.CreateLog($"Product Barcode could not be inserted by \"{actionUser.UserName}\". Product Barcode: {JsonConvert.SerializeObject(data)} Error: {ex.Message}", LogTypeEnum.Error, LogOriginEnum.DataNexApp, actionUser.Id, _context);
                }

                var dto = _mapper.Map<ProductBarcodeDto>(data);

                return Ok(dto);
            }

        }


        [HttpPut("updatedto")]
        public async Task<IActionResult> UpdateDto([FromBody] ProductBarcodeDto productBarcode)
        {
            var actionUser = await GetActionUser();

            var data = await _context.ProductBarcodes.FirstOrDefaultAsync(x => x.Id == productBarcode.Id);

            data.ProductId = productBarcode.ProductId;
            data.SizeId = productBarcode.SizeId;
            data.Barcode = productBarcode.Barcode;

            if (data.Barcode == null || data.Barcode == string.Empty || data.SizeId == null)
            {
                return BadRequest("Barcode and Size cannot be empty");
            }

            var barcodeExists = await _context.ProductBarcodes.Where(x => x.Barcode == productBarcode.Barcode).FirstOrDefaultAsync();

            if (barcodeExists != null)
            {
                return BadRequest("Barcode exists");
            }
            else
            {
                try
                {
                    await _context.SaveChangesAsync();
                    LogService.CreateLog($"Product Barcode \"{data.Barcode}\" updated by \"{actionUser.UserName}\". Product Barcode: {JsonConvert.SerializeObject(data)}", LogTypeEnum.Information, LogOriginEnum.DataNexApp, actionUser.Id, _context);
                }
                catch (Exception ex)
                {
                    LogService.CreateLog($"Product Barcode could not be updated by \"{actionUser.UserName}\". Product Barcode: {JsonConvert.SerializeObject(data)} Error: {ex.Message}", LogTypeEnum.Error, LogOriginEnum.DataNexApp, actionUser.Id, _context);

                }

                var dto = _mapper.Map<ProductBarcodeDto>(data);

                return Ok(dto);
            }
          
        }

        [HttpDelete("deletebyid/{id}")]
        public async Task<IActionResult> DeleteById(Guid id)
        {
            var actionUser = await GetActionUser();

            var data = await _context.ProductBarcodes.FirstOrDefaultAsync(x => x.Id == id);

            try
            {
                _context.ProductBarcodes.Remove(data);
                await _context.SaveChangesAsync();
                LogService.CreateLog($"Product Barcode \"{data.Barcode}\" deleted by \"{actionUser.UserName}\"  Product Barcode: {JsonConvert.SerializeObject(data)}.", LogTypeEnum.Information, LogOriginEnum.DataNexApp, actionUser.Id, _context);

            }
            catch (Exception ex)
            {
                LogService.CreateLog($"Product Barcode \"{data.Barcode}\" could not be deleted by \"{actionUser.UserName}\"  Product Barcode: {JsonConvert.SerializeObject(data)} Error:{ex.Message}.", LogTypeEnum.Error, LogOriginEnum.DataNexApp, actionUser.Id, _context);

            }
            return Ok(data);
        }
    }
}
