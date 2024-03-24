using AutoMapper;
using DataNex.Data;
using DataNex.Model.Dtos;
using DataNex.Model.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DataNexApi.Controllers
{
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
            var data = await _context.Documents.Include(x=>x.Customer).Include(x=>x.DocumentType).Where(x => x.Id == id).Select(x=>new DocumentDto()
            {
                Id = x.Id,
                DocumentDateTime = x.DocumentDateTime,
                DocumentTypeId = x.DocumentTypeId,
                DocumentTypeName = x.DocumentType.Name,
                DocumentNumber = x.DocumentNumber,
                DocumentStatusId = x.DocumentStatusId,
                CustomerId =  x.CustomerId,
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

        [HttpPost("insertdto")]
        public async Task<IActionResult> InsertDto([FromBody] DocumentDto document)

        {
            var data = new Document();

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
            data.CustomerId = document.CustomerId;
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
            _context.Documents.Add(data);
            await _context.SaveChangesAsync();

            var dto = _mapper.Map<DocumentDto>(data);
            var documentType = await _context.DocumentTypes.Where(x => x.Id == dto.DocumentTypeId).FirstOrDefaultAsync();
            dto.DocumentTypeName = documentType.Name;

            var customer = await _context.Customers.Where(x=>x.Id==dto.CustomerId).FirstOrDefaultAsync();
            dto.CustomerPhone1 = customer.Phone1;

            return Ok(dto);
        }


        [HttpPut("updatedto")]
        public async Task<IActionResult> UpdatetDto([FromBody] DocumentDto document)

        {
            var data = await _context.Documents.Where(x => x.Id == document.Id).FirstOrDefaultAsync();

            data.DocumentTypeId = document.DocumentTypeId;
            var source = await _context.Documents.Where(x => x.DocumentTypeId == document.DocumentTypeId).OrderByDescending(x => x.DocumentNumber).FirstOrDefaultAsync();
            if (source != null)
            {
                data.DocumentNumber = source.DocumentNumber + 1;

            }
            else
            {
                data.DocumentNumber = 1;

            }
            data.CustomerId = document.CustomerId;
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
            await _context.SaveChangesAsync();

            var dto = _mapper.Map<DocumentDto>(data);

            return Ok(dto);
        }

        [HttpDelete("deletebyid/{id}")]
        public async Task<IActionResult> DeleteById(Guid id)
        {
            var data = await _context.Documents.FirstOrDefaultAsync(x => x.Id == id);

            _context.Documents.Remove(data);

            await _context.SaveChangesAsync();
            return Ok(data);
        }
    }
}
