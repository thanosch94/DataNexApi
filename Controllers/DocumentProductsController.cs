using AutoMapper;
using DataNex.Data;
using DataNex.Model.Dtos;
using DataNex.Model.Enums;
using DataNex.Model.Models;
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
        private static readonly object _lockObject = new object();

        public DocumentProductsController(ApplicationDbContext context, IMapper mapper) : base(context)
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
                SerialNumber = x.SerialNumber,
                Code = x.Code,
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
                VatClassId = x.Product.VatClassId,
                TotalPrice = x.TotalPrice,
                DocumentProductLotsQuantities = x.DocumentProductLotsQuantities.Select(y => new DocumentProductLotQuantityDto()
                {
                    Id = Guid.NewGuid(),
                    DocumentProductId = y.DocumentProductId,
                    LotId = y.LotId,
                    Quantity = y.Quantity,
                }).ToList()

            }).AsSplitQuery().ToListAsync();

            var dto = _mapper.Map<List<DocumentProductDto>>(data);


            return Ok(dto);
        }




        [HttpPost("insertdto")]
        public async Task<IActionResult> InsertDto([FromBody] DocumentProductDto documentProduct)
        {
            var actionUser = await GetActionUser();
            var success = true;
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
            data.DocumentProductLotsQuantities = documentProduct.DocumentProductLotsQuantities.Select(x => new DocumentProductLotQuantity()
            {
                LotId = x.LotId,
                DocumentProductId = data.Id,
                Quantity = x.Quantity,
            }).ToList();


            lock (_lockObject)
            {
                var maxNumber = _context.DocumentProducts.Max(x => (x.SerialNumber)) ?? 0;
                data.SerialNumber = maxNumber + 1;
                data.Code = data.SerialNumber.ToString().PadLeft(5, '0');


                try
                {
                    _context.DocumentProducts.Add(data);

                    _context.SaveChanges();
                    LogMessage($"Document product inserted by \"{actionUser.UserName}\". Document product: {JsonConvert.SerializeObject(data, new JsonSerializerSettings
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                    })}", LogTypeEnum.Information, LogOriginEnum.DataNexApp, actionUser.Id);
                }
                catch (Exception ex)
                {
                    LogMessage($"Document product could not be inserted by \"{actionUser.UserName}\". Document product: {JsonConvert.SerializeObject(data, new JsonSerializerSettings
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                    })}  Error:{ex.Message}", LogTypeEnum.Error, LogOriginEnum.DataNexApp, actionUser.Id);
                }

            }
            var document = await _context.Documents.Include(x => x.DocumentType).AsSplitQuery().FirstOrDefaultAsync(x => x.Id == data.DocumentId);


            //Recalculate Document Total

            var documentProducts = await _context.DocumentProducts.Where(x => x.DocumentId == documentProduct.DocumentId).ToListAsync();

            document.DocumentTotal = documentProducts.Sum(x => x.TotalPrice);
            _context.SaveChanges();




            if (success)
            {
                foreach (var lotQtyLine in data.DocumentProductLotsQuantities)
                {
                    var lot = await _context.Lots.FirstOrDefaultAsync(x => x.Id == lotQtyLine.LotId);
                    if (document.DocumentType.DocumentTypeGroup == DocumentTypeGroupEnum.Purchasing)
                    {
                        lot.RemainingQty += lotQtyLine.Quantity;
                    }
                    else if (document.DocumentType.DocumentTypeGroup == DocumentTypeGroupEnum.Sales)
                    {
                        lot.RemainingQty -= lotQtyLine.Quantity;

                    }
                    _context.SaveChanges();

                }


            }
            var dto = _mapper.Map<DocumentProductDto>(data);

            return Ok(dto);
        }

        [HttpPut("updatedto")]
        public async Task<IActionResult> UpdateDto([FromBody] DocumentProductDto documentProduct)
        {
            var actionUser = await GetActionUser();

            var data = await _context.DocumentProducts.Include(x => x.DocumentProductLotsQuantities).FirstOrDefaultAsync(x => x.Id == documentProduct.Id);
            var document = await _context.Documents.Include(x => x.DocumentType).AsSplitQuery().FirstOrDefaultAsync(x => x.Id == data.DocumentId);

            if (data.DocumentProductLotsQuantities != null && data.DocumentProductLotsQuantities.Any())
            {
                foreach (var lotQtyLine in data.DocumentProductLotsQuantities)
                {
                    var lot = await _context.Lots.FirstOrDefaultAsync(x => x.Id == lotQtyLine.LotId);
                    if (document.DocumentType.DocumentTypeGroup == DocumentTypeGroupEnum.Purchasing)
                    {
                        lot.RemainingQty -= lotQtyLine.Quantity;
                    }
                    else if (document.DocumentType.DocumentTypeGroup == DocumentTypeGroupEnum.Sales)
                    {
                        lot.RemainingQty += lotQtyLine.Quantity;

                    }
                    _context.DocumentProductLotsQuantities.Remove(lotQtyLine);

                }
                await _context.SaveChangesAsync();

            }


            data.DocumentId = documentProduct.DocumentId;
            data.ProductId = documentProduct.ProductId;
            data.Price = documentProduct.ProductRetailPrice;
            data.VatAmount = documentProduct.VatAmount;
            data.TotalVatAmount = documentProduct.TotalVatAmount;
            data.Quantity = documentProduct.Quantity;
            data.TotalPrice = documentProduct.TotalPrice;
            data.ProductSizeId = documentProduct.ProductSizeId;


            var documentProductLotsQuantities = documentProduct.DocumentProductLotsQuantities.Select(x => new DocumentProductLotQuantity()
            {
                LotId = x.LotId,
                DocumentProductId = data.Id,
                Quantity = x.Quantity,
            }).ToList();

            _context.DocumentProductLotsQuantities.AddRange(documentProductLotsQuantities);

            try
            {
                await _context.SaveChangesAsync();
                LogMessage($"Document product updated by \"{actionUser.UserName}\". Document product: {JsonConvert.SerializeObject(data, new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                })}", LogTypeEnum.Information, LogOriginEnum.DataNexApp, actionUser.Id);

            }
            catch (Exception ex)
            {
                LogMessage($"Document product could not be updated by \"{actionUser.UserName}\". Document product: {JsonConvert.SerializeObject(data, new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                })}  Error:{ex.Message}", LogTypeEnum.Error, LogOriginEnum.DataNexApp, actionUser.Id);
            }

            //Recalculate Document Total

            var documentProducts = await _context.DocumentProducts.Where(x => x.DocumentId == documentProduct.DocumentId).ToListAsync();

            document.DocumentTotal = documentProducts.Sum(x => x.TotalPrice);
            _context.SaveChanges();




            foreach (var lotQtyLine in data.DocumentProductLotsQuantities)
            {
                var lot = await _context.Lots.FirstOrDefaultAsync(x => x.Id == lotQtyLine.LotId);
                if (document.DocumentType.DocumentTypeGroup == DocumentTypeGroupEnum.Purchasing)
                {
                    lot.RemainingQty += lotQtyLine.Quantity;
                }
                else if (document.DocumentType.DocumentTypeGroup == DocumentTypeGroupEnum.Sales)
                {
                    lot.RemainingQty -= lotQtyLine.Quantity;

                }
                _context.SaveChanges();

            }



            var dto = _mapper.Map<DocumentProductDto>(data);

            return Ok(dto);
        }

        [HttpDelete("deletebyid/{id}")]
        public async Task<IActionResult> DeleteById(Guid id)
        {
            var actionUser = await GetActionUser();

            var data = await _context.DocumentProducts.FirstOrDefaultAsync(x => x.Id == id);

            var lotQties = await _context.DocumentProductLotsQuantities.Where(x => x.DocumentProductId == id).ToListAsync();
            try
            {
                _context.RemoveRange(lotQties);
                _context.DocumentProducts.Remove(data);
                await _context.SaveChangesAsync();

                //Recalculate Document Total

                var documentProducts = await _context.DocumentProducts.Where(x => x.DocumentId == data.DocumentId).ToListAsync();
                var document = await _context.Documents.Include(x => x.DocumentType).AsSplitQuery().FirstOrDefaultAsync(x => x.Id == data.DocumentId);

                document.DocumentTotal = documentProducts.Sum(x => x.TotalPrice);
                _context.SaveChanges();


                LogMessage($"Document Product deleted by \"{actionUser.UserName}\"  Document Product: {JsonConvert.SerializeObject(data, new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                })}.", LogTypeEnum.Information, LogOriginEnum.DataNexApp, actionUser.Id);

            }
            catch (Exception ex)
            {
                LogMessage($"Document Product could not be deleted by \"{actionUser.UserName}\"  Document Product: {JsonConvert.SerializeObject(data, new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                })} Error:{ex.Message}.", LogTypeEnum.Error, LogOriginEnum.DataNexApp, actionUser.Id);
            }
            return Ok(data);
        }


        [HttpGet("getpendingordersforproductid/{productId}")]
        public async Task<IActionResult> GetPendingOrdersForProductId(Guid productId)
        {
            var data = await _context.DocumentProducts.Include(x => x.Document).Include(x => x.Product).Where(x => x.ProductId == productId).Select(x => new DocumentProductDto()
            {
                Id = x.Id,
                SerialNumber = x.SerialNumber,
                Code = x.Code,
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
