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
    public class SuppliersController:BaseController
    {
        private ApplicationDbContext _context;
        private IMapper _mapper;
        private static readonly object _lockObject = new object();

        public SuppliersController(ApplicationDbContext context, IMapper mapper):base(context)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet("getall")]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.Suppliers.ToListAsync();

            return Ok(data);
        }

        [HttpGet("getbyid/{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var data = await _context.Suppliers.Where(x => x.Id == id).FirstOrDefaultAsync();

            var dto = _mapper.Map<SupplierDto>(data);


            return Ok(dto);
        }

        [HttpGet("getlookup")]
        public async Task<IActionResult> GetLookup()
        {
            var data = await _context.Suppliers.Select(x => new SupplierDto()
            {
                Id = x.Id,
                Name = x.Name
            }).ToListAsync();

            return Ok(data);
        }

        [HttpPost("insertdto")]
        public async Task<IActionResult> InsertDto([FromBody] SupplierDto supplier)

        {
            var actionUser = await GetActionUser();

            var data = new Supplier();
            data.Name = supplier.Name;
            data.Address = supplier.Address;
            data.Region = supplier.Region;
            data.PostalCode = supplier.PostalCode;
            data.City = supplier.City;
            data.Country = supplier.Country;
            data.Phone1 = supplier.Phone1;
            data.Phone2 = supplier.Phone2;
            data.Email = supplier.Email;
            data.VatNumber = supplier.VatNumber;
            data.TaxOffice = supplier.TaxOffice;
            data.UserAdded = actionUser.Id;

            lock (_lockObject)
            {
                var maxNumber = _context.Suppliers.Max(x => (x.SerialNumber)) ?? 0;
                data.SerialNumber = maxNumber + 1;
                data.Code = data.SerialNumber.ToString().PadLeft(5, '0');

                try
                {
                    _context.Suppliers.Add(data);
                    _context.SaveChanges();
                    LogService.CreateLog($"Supplier \"{data.Name}\" inserted by \"{actionUser.UserName}\". Supplier: {JsonConvert.SerializeObject(data)}", LogTypeEnum.Information, LogOriginEnum.DataNexApp, actionUser.Id, _context);

                }
                catch (Exception ex)
                {
                    LogService.CreateLog($"Supplier \"{data.Name}\" could not be inserted by \"{actionUser.UserName}\" Supplier: {JsonConvert.SerializeObject(data)} Error:{ex.Message}.", LogTypeEnum.Error, LogOriginEnum.DataNexApp, actionUser.Id, _context);
                    throw;
                }
            };

            var dto = _mapper.Map<SupplierDto>(data);

            return Ok(dto);
        }


        [HttpPut("updatedto")]
        public async Task<IActionResult> UpdateDto([FromBody] SupplierDto dto)
        {
            var actionUser = await GetActionUser();

            var data = await _context.Suppliers.FirstOrDefaultAsync(x => x.Id == dto.Id);

            data.Name = dto.Name;
            data.Address = dto.Address;
            data.Region = dto.Region;
            data.PostalCode = dto.PostalCode;
            data.City = dto.City;
            data.Country = dto.Country;
            data.Phone1 = dto.Phone1;
            data.Phone2 = dto.Phone2;
            data.Email = dto.Email;
            data.VatNumber = dto.VatNumber;
            data.TaxOffice = dto.TaxOffice;

            try
            {
                await _context.SaveChangesAsync();
                LogService.CreateLog($"Supplier \"{data.Name}\" updated by \"{actionUser.UserName}\". Supplier: {JsonConvert.SerializeObject(data)}", LogTypeEnum.Information, LogOriginEnum.DataNexApp, actionUser.Id, _context);

            }
            catch (Exception ex)
            {
                LogService.CreateLog($"Supplier \"{data.Name}\" could not be updated by \"{actionUser.UserName}\" Error:{ex.Message}.", LogTypeEnum.Error, LogOriginEnum.DataNexApp, actionUser.Id, _context);

            }

            return Ok(data);
        }

        [HttpDelete("deletebyid/{id}")]
        public async Task<IActionResult> DeleteById(Guid id)
        {
            var actionUser = await GetActionUser();

            var data = await _context.Suppliers.FirstOrDefaultAsync(x => x.Id == id);

            _context.Suppliers.Remove(data);

            try
            {
                _context.Suppliers.Remove(data);
                await _context.SaveChangesAsync();
                LogService.CreateLog($"Supplier \"{data.Name}\" deleted by \"{actionUser.UserName}\"  Supplier: {JsonConvert.SerializeObject(data)}.", LogTypeEnum.Information, LogOriginEnum.DataNexApp, actionUser.Id, _context);

            }
            catch (Exception ex)
            {
                LogService.CreateLog($"Supplier \"{data.Name}\" could not be deleted by \"{actionUser.UserName}\"  Supplier: {JsonConvert.SerializeObject(data)} Error:{ex.Message}.", LogTypeEnum.Error, LogOriginEnum.DataNexApp, actionUser.Id, _context);
            }

            return Ok(data);
        }
    }
}
