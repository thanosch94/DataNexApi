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
    public class DocumentProductsController : BaseController
    {

        private ApplicationDbContext _context;
        private IMapper _mapper;
        public DocumentProductsController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet("getall")]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.DocumentProducts.ToListAsync();

            return Ok(data);
        }


        [HttpGet("getbyid/{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var data = await _context.DocumentProducts.Where(x => x.Id == id).FirstOrDefaultAsync();

            var dto = _mapper.Map<DocumentProductDto>(data);


            return Ok(dto);
        }


        ///TODO move to the correct controller


        [HttpGet("getbydocumentid/{id}")]
        public async Task<IActionResult> GetByDocumentId(Guid id)
        {
            var data = await _context.DocumentProducts.Include(x => x.Product).ThenInclude(x => x.ProductBarcodes).Include(x => x.ProductSize).Where(x => x.DocumentId == id).Select(x => new DocumentProductDto()
            {
                Id = x.Id,
                DocumentId = x.DocumentId,
                ProductId = x.ProductId,
                Quantity = x.Quantity,
                ProductSizeId = x.ProductSizeId,
                SizeName = x.ProductSize.Name,
                ProductName = x.Product.Name,
                Sku = x.Product.Sku,
                Barcode = x.Product.ProductBarcodes.Where(y => y.SizeId == x.ProductSizeId && y.ProductId == x.ProductId).FirstOrDefault().Barcode,
                ProductRetailPrice = x.Price,
                VatAmount = x.VatAmount,
                TotalVatAmount = x.TotalVatAmount,
                TotalPrice = x.TotalPrice

            }).ToListAsync();

            var dto = _mapper.Map<List<DocumentProductDto>>(data);


            return Ok(dto);
        }




        [HttpPost("insertdto")]
        public async Task<IActionResult> InsertDto([FromBody] DocumentProductDto documentProduct)
        {
            var actionUser = await GetActionUser();

            var data = new DocumentProduct();
            data.DocumentId = documentProduct.DocumentId;
            data.ProductId = documentProduct.ProductId;
            data.Price = documentProduct.ProductRetailPrice;
            data.Quantity = documentProduct.Quantity;
            data.VatAmount = documentProduct.VatAmount;
            data.TotalVatAmount = documentProduct.TotalVatAmount;
            data.TotalPrice = documentProduct.TotalPrice;
            data.ProductSizeId = documentProduct.ProductSizeId;
            data.UserAdded = actionUser.Id;

            _context.DocumentProducts.Add(data);

            try
            {
                await _context.SaveChangesAsync();
                LogService.CreateLog($"Document product inserted by \"{actionUser.UserName}\". Document product: {JsonConvert.SerializeObject(data)}", LogTypeEnum.Information, LogOriginEnum.DataNexApp, actionUser.Id, _context);

            }
            catch (Exception ex) 
            {
                LogService.CreateLog($"Document product could not be inserted by \"{actionUser.UserName}\". Document product: {JsonConvert.SerializeObject(data)}  Error:{ex.Message}", LogTypeEnum.Error, LogOriginEnum.DataNexApp, actionUser.Id, _context);

            }
            var dto = _mapper.Map<DocumentProductDto>(data);

            return Ok(dto);
        }

        [HttpPut("updatedto")]
        public async Task<IActionResult> UpdateDto([FromBody] DocumentProductDto documentProduct)
        {
            var actionUser = await GetActionUser();

            var data = await _context.DocumentProducts.FirstOrDefaultAsync(x => x.Id == documentProduct.Id);

            data.DocumentId = documentProduct.DocumentId;
            data.ProductId = documentProduct.ProductId;
            data.Price = documentProduct.ProductRetailPrice;
            data.VatAmount = documentProduct.VatAmount;
            data.TotalVatAmount = documentProduct.TotalVatAmount;
            data.Quantity = documentProduct.Quantity;
            data.TotalPrice = documentProduct.TotalPrice;
            data.ProductSizeId = documentProduct.ProductSizeId;

            try
            {
                await _context.SaveChangesAsync();
                LogService.CreateLog($"Document product updated by \"{actionUser.UserName}\". Document product: {JsonConvert.SerializeObject(data)}", LogTypeEnum.Information, LogOriginEnum.DataNexApp, actionUser.Id, _context);

            }
            catch (Exception ex)
            {
                LogService.CreateLog($"Document product could not be updated by \"{actionUser.UserName}\". Document product: {JsonConvert.SerializeObject(data)}  Error:{ex.Message}", LogTypeEnum.Error, LogOriginEnum.DataNexApp, actionUser.Id, _context);

            }

            var dto = _mapper.Map<DocumentProductDto>(data);

            return Ok(dto);
        }

        [HttpDelete("deletebyid/{id}")]
        public async Task<IActionResult> DeleteById(Guid id)
        {
            var actionUser = await GetActionUser();

            var data = await _context.DocumentProducts.FirstOrDefaultAsync(x => x.Id == id);
            try
            {
                _context.DocumentProducts.Remove(data);
                await _context.SaveChangesAsync();
                LogService.CreateLog($"Document Product deleted by \"{actionUser.UserName}\"  Document Product: {JsonConvert.SerializeObject(data)}.", LogTypeEnum.Information, LogOriginEnum.DataNexApp, actionUser.Id, _context);

            }
            catch (Exception ex)
            {
                LogService.CreateLog($"Document Product could not be deleted by \"{actionUser.UserName}\"  Document Product: {JsonConvert.SerializeObject(data)} Error:{ex.Message}.", LogTypeEnum.Error, LogOriginEnum.DataNexApp, actionUser.Id, _context);
            }
            return Ok(data);
        }


        [HttpGet("getpendingordersforproductid/{productId}")]
        public async Task<IActionResult> GetPendingOrdersForProductId(Guid productId)
        {
            var data = await _context.DocumentProducts.Include(x => x.Document).Include(x => x.Product).Where(x => x.ProductId == productId).Select(x => new DocumentProductDto()
            {
                Id = x.Id,
                DocumentDateString = x.Document.DocumentDateTime.DateTime.ToString("dd-MM-yyyy"),
                DocumentDate = x.Document.DocumentDateTime,
                Sku = x.Product.Sku,
                DocumentCode = x.Document.DocumentType.Name + "-" + (x.Document.DocumentNumber).ToString().PadLeft(6, '0'), //TODO save this to db as it is
                CustomerName = x.Document.Customer.Name,
                ProductId = x.ProductId,
                ProductRetailPrice = x.Price,
                VatAmount = x.VatAmount,
                TotalVatAmount = x.TotalVatAmount, 
                Quantity = x.Quantity,
                TotalPrice = x.TotalPrice,
                ProductSizeId = x.ProductSizeId
            }).OrderBy(x => x.DocumentDate).ToListAsync();

            return Ok(data);
        }

    }
}
