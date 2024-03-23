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
            var data = await _context.Documents.OrderByDescending(x=>x.DocumentNumber).ToListAsync();

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
                ShippingEmail = x.ShippingEmail


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
