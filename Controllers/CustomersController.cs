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
    public class CustomersController : BaseController
    {
        private ApplicationDbContext _context;
        private IMapper _mapper;
        private static readonly object _lockObject = new object();

        public CustomersController(ApplicationDbContext context, IMapper mapper) : base(context)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet("getall")]
        public async Task<IActionResult> GetAll()
        {
            Guid companyId = GetCompanyFromHeader();

            var data = await _context.Customers.Where(x=> x.CompanyId == companyId).OrderBy(x=>x.SerialNumber).ToListAsync();

            return Ok(data);
        }

        [HttpGet("getfromaade/{username}/{password}/{afmCalledFor}/{afmCalledBy}")]
        public async Task<IActionResult> GetFromAade(string username, string password, string afmCalledFor, string? afmCalledBy)
        {
            Guid companyId = GetCompanyFromHeader();

            var data = AadeService.GetDataFromAade(username, password, afmCalledBy, afmCalledFor);

            return Ok(data);
        }

        [HttpGet("getbyid/{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            Guid companyId = GetCompanyFromHeader();

            var data = await _context.Customers.Include(x=>x.CustomerAddresses.OrderByDescending(y=>y.IsDefault)).ThenInclude(x=>x.Address).Where(x => x.Id == id && x.CompanyId == companyId).FirstOrDefaultAsync();

            var dto = _mapper.Map<CustomerDto>(data);

            return Ok(dto);
        }

        [HttpGet("getlookup")]
        public async Task<IActionResult> GetLookup()
        {
            Guid companyId = GetCompanyFromHeader();

            var data = await _context.Customers.Select(x => new CustomerDto()
            {
                Id = x.Id,
                Name = x.Name,
                CompanyId=x.CompanyId
            }).Where(x=>x.CompanyId==companyId).ToListAsync();

            return Ok(data);
        }

        [HttpPost("insertdto")]
        public async Task<IActionResult> InsertDto([FromBody] CustomerDto dto)
        {
            Guid companyId = GetCompanyFromHeader();

            var actionUser = await GetActionUser();

            var data = new Customer();
            data.Name = dto.Name;
            data.Address = dto.Address;
            data.Region = dto.Region;
            data.PostalCode = dto.PostalCode;
            data.City = dto.City;
            data.Country = dto.Country;
            data.Phone1 = dto.Phone1;
            data.Phone2 = dto.Phone2;
            data.Email = dto.Email;
            data.CompanyName = dto.CompanyName;
            data.Occupation = dto.Occupation;
            data.VatNumber = dto.VatNumber;
            data.TaxOffice = dto.TaxOffice;
            data.UserAdded = actionUser.Id;
            data.CompanyId = companyId;
            data.VatClassId = dto.VatClassId;
            data.Notes = dto.Notes;
            data.UserText1 = dto.UserText1;
            data.UserText2 = dto.UserText2;
            data.UserText3 = dto.UserText3;
            data.UserText4 = dto.UserText4;
            data.UserNumber1 = dto.UserNumber1;
            data.UserNumber2 = dto.UserNumber2;
            data.UserNumber3 = dto.UserNumber3;
            data.UserNumber4 = dto.UserNumber4;
            data.UserDate1 = dto.UserDate1;
            data.UserDate2 = dto.UserDate2;
            data.UserDate3 = dto.UserDate3;
            data.UserDate4 = dto.UserDate4;

       
            data.CustomerAddresses = dto.CustomerAddresses.Select(x => new CustomerAddress()
            {
                Id = Guid.NewGuid(),
                AddressType = x.AddressType,
                Address = new Address()
                {
                    Id = Guid.NewGuid(),
                    Street = x.Address.Street,
                    StreetNumber = x.Address.StreetNumber,
                    PostalCode = x.Address.PostalCode,
                    City = x.Address.City,
                    Country = x.Address.Country,
                    CompanyId = companyId
                },
                CustomerId =data.Id,
                IsDefault =x.IsDefault,
                Notes = x.Notes,
                CompanyId = companyId,
            }).ToList();

            lock (_lockObject)
            {
                try
                {
                    var maxNumber = _context.Customers.Where(x=> x.CompanyId == companyId).Max(x => (x.SerialNumber)) ?? 0;
                    data.SerialNumber = maxNumber + 1;
                    data.Code = data.SerialNumber.ToString().PadLeft(5, '0');

                    _context.Customers.Add(data);
                    _context.SaveChanges();
                    LogService.CreateLog($"Customer \"{data.Name}\" inserted by \"{actionUser.UserName}\". Customer: {JsonConvert.SerializeObject(data, new JsonSerializerSettings()
                    {
                        ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
                    })}", LogTypeEnum.Information, LogOriginEnum.DataNexApp, actionUser.Id, _context);

                }
                catch (Exception ex)
                {
                    LogService.CreateLog($"Customer \"{data.Name}\" could not be inserted by \"{actionUser.UserName}\" Customer: {JsonConvert.SerializeObject(data, new JsonSerializerSettings()
                    {
                        ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
                    })} Error:{ex.Message}.", LogTypeEnum.Error, LogOriginEnum.DataNexApp, actionUser.Id, _context);
                    throw;
                }

            };
            

            var dataToReturn = _mapper.Map<CustomerDto>(data);

            return Ok(dataToReturn);
        }


        [HttpPut("updatedto")]
        public async Task<IActionResult> UpdateDto([FromBody] CustomerDto dto)
        {
            Guid companyId = GetCompanyFromHeader();

            var actionUser = await GetActionUser();

            var data = await _context.Customers.FirstOrDefaultAsync(x => x.Id == dto.Id && x.CompanyId == companyId);

            data.Name = dto.Name;
            data.Address = dto.Address;
            data.Region = dto.Region;
            data.PostalCode = dto.PostalCode;
            data.City = dto.City;
            data.Country = dto.Country;
            data.Phone1 = dto.Phone1;
            data.Phone2 = dto.Phone2;
            data.Email = dto.Email;
            data.CompanyName = dto.CompanyName;
            data.Occupation = dto.Occupation;
            data.VatNumber = dto.VatNumber;
            data.TaxOffice = dto.TaxOffice;
            data.CompanyId = companyId;
            data.VatClassId = dto.VatClassId;
            data.Notes = dto.Notes;
            data.UserText1 = dto.UserText1;
            data.UserText2 = dto.UserText2;
            data.UserText3 = dto.UserText3;
            data.UserText4 = dto.UserText4;
            data.UserNumber1 = dto.UserNumber1;
            data.UserNumber2 = dto.UserNumber2;
            data.UserNumber3 = dto.UserNumber3;
            data.UserNumber4 = dto.UserNumber4;
            data.UserDate1 = dto.UserDate1;
            data.UserDate2 = dto.UserDate2;
            data.UserDate3 = dto.UserDate3;
            data.UserDate4 = dto.UserDate4;
            try
            {
                await _context.SaveChangesAsync();
                LogService.CreateLog($"Customer \"{data.Name}\" updated by \"{actionUser.UserName}\". Customer: {JsonConvert.SerializeObject(data)}", LogTypeEnum.Information, LogOriginEnum.DataNexApp, actionUser.Id, _context);

            }
            catch (Exception ex)
            {
                LogService.CreateLog($"Customer \"{data.Name}\" could not be updated by \"{actionUser.UserName}\" Error:{ex.Message}.", LogTypeEnum.Error, LogOriginEnum.DataNexApp, actionUser.Id, _context);

            }

            return Ok(data);
        }

        [HttpDelete("deletebyid/{id}")]
        public async Task<IActionResult> DeleteById(Guid id)
        {
            Guid companyId = GetCompanyFromHeader();

            var actionUser = await GetActionUser();

            var data = await _context.Customers.FirstOrDefaultAsync(x => x.Id == id && x.CompanyId==companyId);

            try
            {
                _context.Customers.Remove(data);
                await _context.SaveChangesAsync();
                LogService.CreateLog($"Customer \"{data.Name}\" deleted by \"{actionUser.UserName}\"  Customer: {JsonConvert.SerializeObject(data)}.", LogTypeEnum.Information, LogOriginEnum.DataNexApp, actionUser.Id, _context);

            }
            catch (Exception ex)
            {
                LogService.CreateLog($"Customer \"{data.Name}\" could not be deleted by \"{actionUser.UserName}\"  Customer: {JsonConvert.SerializeObject(data)} Error:{ex.Message}.", LogTypeEnum.Error, LogOriginEnum.DataNexApp, actionUser.Id, _context);
            }

            return Ok(data);
        }
    }
}
