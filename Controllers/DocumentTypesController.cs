using AutoMapper;
using DataNex.Data;
using DataNex.Model.Dtos;
using DataNex.Model.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Xml.Linq;

namespace DataNexApi.Controllers
{
    [Authorize]
    public class DocumentTypesController:BaseController
    {

        private ApplicationDbContext _context;
        private IMapper _mapper;
        public DocumentTypesController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }



        [HttpGet("getall")]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.DocumentTypes.ToListAsync();

            return Ok(data);
        }

        [HttpGet("getbyid/{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var data = await _context.DocumentTypes.Where(x => x.Id == id).FirstOrDefaultAsync();

            var dto = _mapper.Map<DocumentTypeDto>(data);


            return Ok(dto);
        }

        [HttpPost("insertdto")]
        public async Task<IActionResult> InsertDto([FromBody] DocumentTypeDto documentType)

        {
            var data = new DocumentType();

            data.Name = documentType.Name;
            data.Description = documentType.Description;

            _context.DocumentTypes.Add(data);
            await _context.SaveChangesAsync();

            var dto = _mapper.Map<DocumentTypeDto>(data);

            return Ok(dto);
        }


        [HttpPut("updatedto")]
        public async Task<IActionResult> UpdatetDto([FromBody] DocumentTypeDto documentType)

        {
            var data = await _context.DocumentTypes.Where(x => x.Id == documentType.Id).FirstOrDefaultAsync();

            data.Name = documentType.Name;
            data.Description = documentType.Description;
            await _context.SaveChangesAsync();

            var dto = _mapper.Map<DocumentTypeDto>(data);

            return Ok(dto);
        }

        [HttpDelete("deletebyid/{id}")]
        public async Task<IActionResult> DeleteById(Guid id)
        {
            var data = await _context.DocumentTypes.FirstOrDefaultAsync(x => x.Id == id);

            _context.DocumentTypes.Remove(data);

            await _context.SaveChangesAsync();

            var dto = _mapper.Map<DocumentTypeDto>(data);

            return Ok(dto);
        }
    }
}

