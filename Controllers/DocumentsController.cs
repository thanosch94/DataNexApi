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
            var data = await _context.Documents.ToListAsync();

            return Ok(data);
        }

        [HttpGet("getbyid/{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var data = await _context.Documents.Where(x => x.Id == id).FirstOrDefaultAsync();

            var dto = _mapper.Map<DocumentDto>(data);


            return Ok(dto);
        }

        [HttpPost("insertdto")]
        public async Task<IActionResult> InsertDto([FromBody] DocumentDto document)

        {
            var data = new Document();

            data.DocumentTypeId = document.DocumentTypeId;
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

            _context.Documents.Add(data);
            await _context.SaveChangesAsync();

            var dto = _mapper.Map<DocumentDto>(data);

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
