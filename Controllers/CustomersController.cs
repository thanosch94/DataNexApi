using AutoMapper;
using DataNex.Data;
using DataNex.Model.Dtos;
using DataNex.Model.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DataNexApi.Controllers
{
    [Route("api/[controller]")]

    public class CustomersController : Controller
    {
        private ApplicationDbContext _context;
        private IMapper _mapper;
        public CustomersController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet("getall")]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.Customers.ToListAsync();

            return Ok(data);
        }

        [HttpGet("getlookup")]
        public async Task<IActionResult> GetLookup()
        {
            var data = await _context.Customers.Select(x => new CustomerDto()
            {
                Id = x.Id,
                Name = x.Name
            }).ToListAsync();

            return Ok(data);
        }

        [HttpPost("insertdto")]
        public async Task<IActionResult> InsertDto(CustomerDto dto)
        {
            var data = new Customer();

            data.Name = dto.Name;
            data.BAddress = dto.BAddress;
            data.BRegion = dto.BRegion;
            data.BPostalCode = dto.BPostalCode;
            data.BCity = dto.BCity;
            data.BCountry = dto.BCountry;
            data.BPhone1 = dto.BPhone1;
            data.BPhone2 = dto.BPhone2;
            data.BEmail = dto.BEmail;
            data.SAddress = dto.SAddress;
            data.SRegion = dto.SRegion;
            data.SPostalCode = dto.SPostalCode;
            data.SCity = dto.SCity;
            data.SCountry = dto.SCountry;
            data.SPhone1 = dto.SPhone1;
            data.SPhone2 = dto.SPhone2;
            data.SEmail = dto.SEmail;

            _context.Customers.Add(data);
            await _context.SaveChangesAsync();

            return Ok(data);
        }


        [HttpPut("updatedto")]
        public async Task<IActionResult> UpdateDto(CustomerDto dto)
        {
            var data = await _context.Customers.FirstOrDefaultAsync(x => x.Id == dto.Id);

            data.Name = dto.Name;
            data.BAddress = dto.BAddress;
            data.BRegion = dto.BRegion;
            data.BPostalCode = dto.BPostalCode;
            data.BCity = dto.BCity;
            data.BCountry = dto.BCountry;
            data.BPhone1 = dto.BPhone1;
            data.BPhone2 = dto.BPhone2;
            data.BEmail = dto.BEmail;
            data.SAddress = dto.SAddress;
            data.SRegion = dto.SRegion;
            data.SPostalCode = dto.SPostalCode;
            data.SCity = dto.SCity;
            data.SCountry = dto.SCountry;
            data.SPhone1 = dto.SPhone1;
            data.SPhone2 = dto.SPhone2;
            data.SEmail = dto.SEmail;

            await _context.SaveChangesAsync();

            return Ok(data);
        }

        [HttpDelete("deletebyid/{id}")]
        public async Task<IActionResult> DeleteById(Guid id)
        {
            var data = await _context.Customers.FirstOrDefaultAsync(x => x.Id == id);

            _context.Customers.Remove(data);

            return Ok(data);
        }
    }
}
