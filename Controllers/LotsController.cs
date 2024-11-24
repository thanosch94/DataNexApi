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
    public class LotsController : BaseController
    {
        private ApplicationDbContext _context;
        private IMapper _mapper;
        private static readonly object _lockObject = new object();

        public LotsController(ApplicationDbContext context, IMapper mapper) : base(context)
        {
            _context = context;
            _mapper = mapper;
        }


        [HttpGet("getall")]
        public async Task<IActionResult> GetAll()
        {
            Guid companyId = GetCompanyFromHeader();

            var data = await _context.Lots.Where(x => x.CompanyId == companyId).ToListAsync();

            return Ok(data);
        }       
        
        [HttpGet("getlookup")]
        public async Task<IActionResult> GetLookup()
        {
            Guid companyId = GetCompanyFromHeader();

            var data = await _context.Lots.Where(x => x.CompanyId == companyId).Select(x=> new LotDto()
            {
                Id=x.Id,
                Name = x.Name
            }).ToListAsync();

            return Ok(data);
        }
                
        
        [HttpGet("getlookupbysupplierid/{supplierId}")]
        public async Task<IActionResult> GetLookupBySupplierId(Guid supplierId)
        {
            Guid companyId = GetCompanyFromHeader();

            var data = await _context.Lots.Where(x => x.CompanyId == companyId && x.SupplierId == supplierId).Select(x=> new LotDto()
            {
                Id=x.Id,
                Name = x.Name,
                SupplierId = x.SupplierId
            }).ToListAsync();

            return Ok(data);
        }
                
        
        [HttpGet("getlookupbysupplieridandproductid/{supplierId}/{productId}")]
        public async Task<IActionResult> GetLookupBySupplierIdAndProductId(Guid supplierId, Guid productId)
        {
            Guid companyId = GetCompanyFromHeader();

            var data = await _context.Lots.Where(x => x.CompanyId == companyId && x.SupplierId == supplierId).Select(x=> new LotDto()
            {
                Id=x.Id,
                Name = x.Name,
                SupplierId = x.SupplierId
            }).ToListAsync();

            return Ok(data);
        }

        [HttpGet("getlookupbyproductidwithremainingqty/{productId}")]
        public async Task<IActionResult> GetLookupByProductIdWithRemainingQty(Guid productId)
        {
            Guid companyId = GetCompanyFromHeader();

            var data = await _context.Lots.Where(x => x.CompanyId == companyId && x.ProductId == productId && x.RemainingQty>0).Select(x=> new LotDto()
            {
                Id=x.Id,
                Name = x.Name,
                ProductId = x.ProductId
            }).ToListAsync();

            return Ok(data);
        }
        
        [HttpGet("getlotqtiesonsalesdocbyproductqtyfifo/{productId}/{qty}")]
        public async Task<IActionResult> GetLotQtiesOnSalesDocByProductQtyFIFO(Guid productId, int qty)
        {
            Guid companyId = GetCompanyFromHeader();

            var data = await _context.DocumentProductLotsQuantities.Include(x=>x.Lot)
                .Where(x => x.Lot.ProductId == productId && x.Lot.RemainingQty>0 && x.DocumentProduct.Document.DocumentType.DocumentTypeGroup==DocumentTypeGroupEnum.Purchasing)
                .ToListAsync();

            var settings = await _context.LotsSettings.FirstOrDefaultAsync(x=>x.CompanyId==companyId);

            if(settings.LotStrategyApplyField==LotStrategyApplyFieldEnum.ExpirationDate)
            {
                data = data.OrderBy(x => x.Lot.ExpDate).ToList();
            }else if (settings.LotStrategyApplyField == LotStrategyApplyFieldEnum.ProductionDate)
            {
                data = data.OrderBy(x => x.Lot.ProdDate).ToList();
            }else if (settings.LotStrategyApplyField == LotStrategyApplyFieldEnum.AddedDateTime)
            {
                data = data.OrderBy(x => x.Lot.DateAdded).ToList();
            }

            var documentProductLotsQuantities = new List<DocumentProductLotQuantityDto>();
            foreach (var item in data)
            {
                var dto = new DocumentProductLotQuantityDto();
                if (item.Lot.RemainingQty > qty)
                {
                    dto = new DocumentProductLotQuantityDto()
                    {
                        Quantity = qty,
                        LotId = item.LotId,
                    };
                    qty = qty - qty;

                }
                else
                {
                    dto = new DocumentProductLotQuantityDto()
                    {
                        Quantity = item.Lot.RemainingQty,
                        LotId = item.LotId,
                    };
                    qty=qty- item.Lot.RemainingQty;
                }
                documentProductLotsQuantities.Add(dto);

                if (qty == 0)
                {
                    break;
                }
            
            }
            
            return Ok(documentProductLotsQuantities);
        } 

        [HttpGet("getlotqtiesonsalesdocbyproductqtylifo/{productId}/{qty}")]
        public async Task<IActionResult> GetLotQtiesOnSalesDocByProductQtyLIFO(Guid productId, int qty)
        {
            Guid companyId = GetCompanyFromHeader();

            var data = await _context.DocumentProductLotsQuantities.Include(x => x.Lot)
                .Where(x => x.Lot.ProductId == productId && x.Lot.RemainingQty>0 && x.DocumentProduct.Document.DocumentType.DocumentTypeGroup==DocumentTypeGroupEnum.Purchasing)
                .ToListAsync();


            var settings = await _context.LotsSettings.FirstOrDefaultAsync(x => x.CompanyId == companyId);

            if (settings.LotStrategyApplyField == LotStrategyApplyFieldEnum.ExpirationDate)
            {
                data = data.OrderByDescending(x => x.Lot.ExpDate).ToList();
            }
            else if (settings.LotStrategyApplyField == LotStrategyApplyFieldEnum.ProductionDate)
            {
                data = data.OrderByDescending(x => x.Lot.ProdDate).ToList();
            }
            else if (settings.LotStrategyApplyField == LotStrategyApplyFieldEnum.AddedDateTime)
            {
                data = data.OrderByDescending(x => x.Lot.DateAdded).ToList();
            }


            var documentProductLotsQuantities = new List<DocumentProductLotQuantityDto>();
            foreach (var item in data)
            {
                var dto = new DocumentProductLotQuantityDto();
                if (item.Lot.RemainingQty > qty)
                {
                    dto = new DocumentProductLotQuantityDto()
                    {
                        Quantity = qty,
                        LotId = item.LotId,
                    };
                    qty = qty - qty;

                }
                else
                {
                    dto = new DocumentProductLotQuantityDto()
                    {
                        Quantity = item.Lot.RemainingQty,
                        LotId = item.LotId,
                    };
                    qty=qty- item.Lot.RemainingQty;
                }
               
               
                    documentProductLotsQuantities.Add(dto);
                if (qty == 0)
                {
                    break;
                }

            }
            
            return Ok(documentProductLotsQuantities);
        }

        [HttpPost("insertdto")]
        public async Task<IActionResult> InsertDto([FromBody] LotDto dto)
        {
            Guid companyId = GetCompanyFromHeader();

            var actionUser = await GetActionUser();

            var data = new Lot();

            //TODO check if validation will be with the code and not the name
            var exists = await _context.Statuses.Where(x => x.Name == dto.Name && x.CompanyId == companyId).FirstOrDefaultAsync();
            if (exists == null)
            {
                data.Name = dto.Name;
                data.ProductId = dto.ProductId;
                data.Notes = dto.Notes;
                data.ProdDate = dto.ProdDate;   
                data.ExpDate = dto.ExpDate;
                data.SupplierId = dto.SupplierId;
                data.RemainingQty = 0;
                data.UserAdded = actionUser.Id;
                data.CompanyId = companyId;

                lock (_lockObject)
                {
                    var maxNumber = _context.Lots.Where(x => x.CompanyId == companyId).Max(x => (x.SerialNumber)) ?? 0;
                    data.SerialNumber = maxNumber + 1;
                    data.Code = data.SerialNumber.ToString().PadLeft(5, '0');

                    try
                    {
                        _context.Lots.Add(data);
                        _context.SaveChanges();
                        LogService.CreateLog($"Lot \"{data.Name}\" inserted by \"{actionUser.UserName}\". Lot: {JsonConvert.SerializeObject(data)}", LogTypeEnum.Information, LogOriginEnum.DataNexApp, actionUser.Id, _context);

                    }
                    catch (Exception ex)
                    {
                        LogService.CreateLog($"Lot \"{data.Name}\" could not be inserted by \"{actionUser.UserName}\". Lot: {JsonConvert.SerializeObject(data)} Error: {ex.Message}", LogTypeEnum.Error, LogOriginEnum.DataNexApp, actionUser.Id, _context);
                        throw;
                    }
                };
                return Ok(data);
            }
            else
            {
                return BadRequest("Lot already exists");
            }

        }

        [HttpPut("updatedto")]
        public async Task<IActionResult> UpdateDto([FromBody] LotDto dto)
        {
            Guid companyId = GetCompanyFromHeader();

            var actionUser = await GetActionUser();

            var data = await _context.Lots.Where(x => x.Id == dto.Id && x.CompanyId == companyId).FirstOrDefaultAsync();

            data.Name = dto.Name; 
            data.ProductId = dto.ProductId;
            data.Notes = dto.Notes;
            data.ProdDate = dto.ProdDate;
            data.ExpDate = dto.ExpDate;
            data.SupplierId = dto.SupplierId;
            data.UserAdded = actionUser.Id;
            data.CompanyId = companyId;

            try
            {
                await _context.SaveChangesAsync();
                LogService.CreateLog($"Lot \"{data.Name}\" updated by \"{actionUser.UserName}\". Lot: {JsonConvert.SerializeObject(data)}", LogTypeEnum.Information, LogOriginEnum.DataNexApp, actionUser.Id, _context);

            }
            catch (Exception ex)
            {
                LogService.CreateLog($"Lot \"{data.Name}\" could not be updated by \"{actionUser.UserName}\". Lot: {JsonConvert.SerializeObject(data)} Error: {ex.Message}", LogTypeEnum.Error, LogOriginEnum.DataNexApp, actionUser.Id, _context);

            }

            return Ok(data);

        }

        [HttpDelete("deletebyid/{id}")]
        public async Task<IActionResult> DeleteById(Guid id)
        {
            Guid companyId = GetCompanyFromHeader();

            var actionUser = await GetActionUser();

            var data = await _context.Lots.FirstOrDefaultAsync(x => x.Id == id && x.CompanyId == companyId);

            try
            {
                _context.Lots.Remove(data);

                await _context.SaveChangesAsync();
                LogService.CreateLog($"Lot \"{data.Name}\" deleted by \"{actionUser.UserName}\". Lot: {JsonConvert.SerializeObject(data)}", LogTypeEnum.Information, LogOriginEnum.DataNexApp, actionUser.Id, _context);

            }
            catch (Exception ex)
            {
                LogService.CreateLog($"Lot \"{data.Name}\" could not be deleted by \"{actionUser.UserName}\". Lot: {JsonConvert.SerializeObject(data)} Error: {ex.Message}", LogTypeEnum.Error, LogOriginEnum.DataNexApp, actionUser.Id, _context);
            }
            return Ok(data);
        }
    }
    
}
