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
    public class DocumentsController:BaseController
    {
        private ApplicationDbContext _context;
        private IMapper _mapper;
        public DocumentsController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet("getall")]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.Documents.Include(x => x.Customer).Include(x => x.DocumentType).OrderByDescending(x=>x.DocumentNumber).Select(x => new DocumentDto()
            {
                Id = x.Id,
                DocumentDateTime = x.DocumentDateTime,
                DocumentTypeId = x.DocumentTypeId,
                DocumentTypeName = x.DocumentType.Name,
                DocumentNumber = x.DocumentNumber,
                DocumentCode = x.DocumentCode,
                DocumentStatusId = x.DocumentStatusId,
                CustomerId = x.CustomerId,
                CustomerName = x.Customer.Name,
                CustomerPhone1 = x.Customer.Phone1,
                DocumentTotal = x.DocumentTotal,
                ShippingAddress = x.ShippingAddress,
                ShippingRegion = x.ShippingRegion,
                ShippingPostalCode = x.ShippingPostalCode,
                ShippingCity = x.ShippingCity,
                ShippingCountry = x.ShippingCountry,
                ShippingPhone1 = x.ShippingPhone1,
                ShippingPhone2 = x.ShippingPhone2,
                ShippingEmail = x.ShippingEmail,
                UserText1 = x.UserText1,
                UserText2 = x.UserText2,
                UserText3 = x.UserText3,
                UserText4 = x.UserText4,
                UserNumber1 = x.UserNumber1,
                UserNumber2 = x.UserNumber2,
                UserNumber3 = x.UserNumber3,
                UserNumber4 = x.UserNumber4,
                UserDate1 = x.UserDate1,
                UserDate2 = x.UserDate2,
                UserDate3 = x.UserDate3,
                UserDate4 = x.UserDate4
            }).ToListAsync();

            return Ok(data);
        }

        [HttpGet("getbyid/{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var data = await _context.Documents.Include(x => x.DocumentStatus).Include(x=>x.Customer).Include(x=>x.DocumentType).Where(x => x.Id == id).Select(x=>new DocumentDto()
            {
                Id = x.Id,
                DocumentDateTime = x.DocumentDateTime,
                DocumentTypeId = x.DocumentTypeId,
                DocumentCode  = x.DocumentCode,
                DocumentTypeName = x.DocumentType.Abbreviation,
                DocumentNumber = x.DocumentNumber,
                DocumentStatusId = x.DocumentStatusId,
                DocumentStatusName = x.DocumentStatus.Name,
                CustomerId =  x.CustomerId,
                SupplierId = x.SupplierId,
                CustomerName = x.Customer.Name,
                CustomerPhone1 =x.Customer.Phone1,
                DocumentTotal = x.DocumentTotal,
                ShippingAddress = x.ShippingAddress,
                ShippingRegion =x.ShippingRegion,
                ShippingPostalCode =x.ShippingPostalCode,
                ShippingCity =x.ShippingCity,
                ShippingCountry = x.ShippingCountry,
                ShippingPhone1 =x.ShippingPhone1,
                ShippingPhone2 =x.ShippingPhone2,
                ShippingEmail = x.ShippingEmail,
                UserText1 =x.UserText1,
                UserText2 =x.UserText2,
                UserText3 =x.UserText3,
                UserText4 =x.UserText4,
                UserNumber1 =x.UserNumber1,
                UserNumber2 =x.UserNumber2,
                UserNumber3 =x.UserNumber3,
                UserNumber4 =x.UserNumber4,
                UserDate1 =x.UserDate1,
                UserDate2 =x.UserDate2,
                UserDate3 =x.UserDate3,
                UserDate4 =x.UserDate4,

            }).FirstOrDefaultAsync();


            return Ok(data);
        }


        [HttpGet("getbydocumenttypegroup/{documentTypeGroup}")]
        public async Task<IActionResult> GetByDocumnentTypeGroup(DocumentTypeGroupEnum documentTypeGroup)
        {
            var data = await _context.Documents.Where(x => x.DocumentType.DocumentTypeGroup == documentTypeGroup).Select(x => new DocumentDto()
            {
                Id = x.Id,
                DocumentDateTime = x.DocumentDateTime,
                DocumentTypeId = x.DocumentTypeId,
                DocumentTypeName = x.DocumentType.Name,
                DocumentNumber = x.DocumentNumber,
                DocumentCode = x.DocumentCode,
                DocumentStatusId = x.DocumentStatusId,
                CustomerId = x.CustomerId,
                SupplierId = x.SupplierId,
                CustomerName = x.Customer.Name,
                CustomerPhone1 = x.Customer.Phone1,
                DocumentTotal = x.DocumentTotal,
                ShippingAddress = x.ShippingAddress,
                ShippingRegion = x.ShippingRegion,
                ShippingPostalCode = x.ShippingPostalCode,
                ShippingCity = x.ShippingCity,
                ShippingCountry = x.ShippingCountry,
                ShippingPhone1 = x.ShippingPhone1,
                ShippingPhone2 = x.ShippingPhone2,
                ShippingEmail = x.ShippingEmail,
                UserText1 = x.UserText1,
                UserText2 = x.UserText2,
                UserText3 = x.UserText3,
                UserText4 = x.UserText4,
                UserNumber1 = x.UserNumber1,
                UserNumber2 = x.UserNumber2,
                UserNumber3 = x.UserNumber3,
                UserNumber4 = x.UserNumber4,
                UserDate1 = x.UserDate1,
                UserDate2 = x.UserDate2,
                UserDate3 = x.UserDate3,
                UserDate4 = x.UserDate4
            }).ToListAsync();

            return Ok(data);
        }

        [HttpPost("insertdto")]
        public async Task<IActionResult> InsertDto([FromBody] DocumentDto document)

        {
            var actionUser = await GetActionUser();

            var data = new Document();
            data.Id = document.Id;
            data.DocumentTypeId = document.DocumentTypeId;
            data.DocumentDateTime = document.DocumentDateTime;
            var source = await _context.Documents.Where(x=>x.DocumentTypeId== document.DocumentTypeId).OrderByDescending(x=>x.DocumentNumber).FirstOrDefaultAsync();
            if (source!=null)
            {
                data.DocumentNumber = source.DocumentNumber + 1;
            }
            else
            {
                data.DocumentNumber = 1;
            }

            var documentType = await _context.DocumentTypes.FirstOrDefaultAsync(x => x.Id == document.DocumentTypeId);
            if (documentType!=null)
            {
                data.DocumentCode = documentType.Abbreviation +"-"+ data.DocumentNumber.ToString().PadLeft(6,'0');

            }
            data.CustomerId = document.CustomerId;
            data.SupplierId = document.SupplierId;
            data.DocumentStatusId = document.DocumentStatusId;
            data.DocumentTotal = document.DocumentTotal;
            data.ShippingAddress = document.ShippingAddress;
            data.ShippingRegion = document.ShippingRegion;
            data.ShippingPostalCode = document.ShippingPostalCode;
            data.ShippingCity = document.ShippingCity;
            data.ShippingCountry = document.ShippingCountry;
            data.ShippingPhone1 = document.ShippingPhone1;
            data.ShippingPhone2 = document.ShippingPhone2;
            data.ShippingEmail = document.ShippingEmail;
            data.UserText1 = document.UserText1;
            data.UserText2 = document.UserText2;
            data.UserText3 = document.UserText3;
            data.UserText4 = document.UserText4;
            data.UserNumber1 = document.UserNumber1;
            data.UserNumber2 = document.UserNumber2;
            data.UserNumber3 = document.UserNumber3;
            data.UserNumber4 = document.UserNumber4;
            data.UserDate1 = document.UserDate1;
            data.UserDate2 = document.UserDate2;
            data.UserDate3 = document.UserDate3;
            data.UserDate4 = document.UserDate4;
            data.UserAdded = actionUser.Id;

            try
            {
                _context.Documents.Add(data);
                await _context.SaveChangesAsync();
                LogService.CreateLog($"New document inserted by \"{actionUser.UserName}\". Document: {JsonConvert.SerializeObject(data, new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
                })}", LogTypeEnum.Information, LogOriginEnum.DataNexApp, actionUser.Id, _context);
            }
            catch (Exception ex)
            {
                LogService.CreateLog($"Document could not be inserted by \"{actionUser.UserName}\". Document: {JsonConvert.SerializeObject(data, new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
                })} Error: {ex.Message}", LogTypeEnum.Error, LogOriginEnum.DataNexApp, actionUser.Id, _context);

            }


            var dto = _mapper.Map<DocumentDto>(data);
            dto.DocumentTypeName = documentType.Name;

            if (dto.CustomerId != null)
            {
                var customer = await _context.Customers.Where(x => x.Id == dto.CustomerId).FirstOrDefaultAsync();
                dto.CustomerPhone1 = customer.Phone1;

            }else if(dto.SupplierId != null) 
            {
                ////
            }

            return Ok(dto);
        }


        [HttpPut("updatedto")]
        public async Task<IActionResult> UpdatetDto([FromBody] DocumentDto document)

        {
            var actionUser = await GetActionUser();

            var data = await _context.Documents.Where(x => x.Id == document.Id).FirstOrDefaultAsync();

            data.DocumentTypeId = document.DocumentTypeId;
            var source = await _context.Documents.Where(x => x.DocumentTypeId == document.DocumentTypeId).OrderByDescending(x => x.DocumentNumber).FirstOrDefaultAsync();
           
            data.CustomerId = document.CustomerId;
            data.SupplierId = document.SupplierId;
            data.DocumentStatusId = document.DocumentStatusId;
            data.DocumentTotal = document.DocumentTotal;
            data.ShippingAddress = document.ShippingAddress;
            data.ShippingRegion = document.ShippingRegion;
            data.ShippingPostalCode = document.ShippingPostalCode;
            data.ShippingCity = document.ShippingCity;
            data.ShippingCountry = document.ShippingCountry;
            data.ShippingPhone1 = document.ShippingPhone1;
            data.ShippingPhone2 = document.ShippingPhone2;
            data.ShippingEmail = document.ShippingEmail;
            data.UserText1 = document.UserText1;
            data.UserText2 = document.UserText2;
            data.UserText3 = document.UserText3;
            data.UserText4 = document.UserText4;
            data.UserNumber1 = document.UserNumber1;
            data.UserNumber2 = document.UserNumber2;
            data.UserNumber3 = document.UserNumber3;
            data.UserNumber4 = document.UserNumber4;
            data.UserDate1 = document.UserDate1;
            data.UserDate2 = document.UserDate2;
            data.UserDate3 = document.UserDate3;
            data.UserDate4 = document.UserDate4;

            var documentProducts = await _context.DocumentProducts.Include(x=>x.Product).Where(x => x.DocumentId == data.Id).ToListAsync();
            decimal total = 0;
            foreach (var product in documentProducts)
            {
                total += (decimal)product.Product.RetailPrice*product.Quantity;
            }
            data.DocumentTotal = total;
            try
            {
                await _context.SaveChangesAsync();
                LogService.CreateLog($"Document updated by \"{actionUser.UserName}\". Document: {JsonConvert.SerializeObject(data, new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
                })}", LogTypeEnum.Information, LogOriginEnum.DataNexApp, actionUser.Id, _context);
            }
            catch (Exception ex)
            {
                LogService.CreateLog($"Document could not be updated by \"{actionUser.UserName}\". Document: {JsonConvert.SerializeObject(data, new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
                })} Error: {ex.Message}", LogTypeEnum.Error, LogOriginEnum.DataNexApp, actionUser.Id, _context);

            }

            var dto = _mapper.Map<DocumentDto>(data);

            return Ok(dto);
        }

        [HttpDelete("deletebyid/{id}")]
        public async Task<IActionResult> DeleteById(Guid id)
        {
            var actionUser = await GetActionUser();

            var data = await _context.Documents.FirstOrDefaultAsync(x => x.Id == id);

            try
            {
                _context.Documents.Remove(data);
                await _context.SaveChangesAsync();
                LogService.CreateLog($"Document deleted by \"{actionUser.UserName}\"  Document: {JsonConvert.SerializeObject(data, new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
                })}.", LogTypeEnum.Information, LogOriginEnum.DataNexApp, actionUser.Id, _context);

            }
            catch (Exception ex)
            {
                LogService.CreateLog($"Document could not be deleted by \"{actionUser.UserName}\"  Document: {JsonConvert.SerializeObject(data, new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
                })} Error:{ex.Message}.", LogTypeEnum.Error, LogOriginEnum.DataNexApp, actionUser.Id, _context);

            }
            return Ok(data);
        }
    }
}
