using AutoMapper;
using DataNex.Data;
using DataNex.Model.Enums;
using DataNexApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace DataNexApi.Controllers
{
    public class CustomerAddressesController:BaseController
    {
        private ApplicationDbContext _context;
        private IMapper _mapper;
        private static readonly object _lockObject = new object();

        public CustomerAddressesController(ApplicationDbContext context, IMapper mapper) : base(context)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet("getall")]
        public async Task<IActionResult> GetAll()
        {
            Guid companyId = GetCompanyFromHeader();

            var data = await _context.CustomerAddresses.Where(x => x.CompanyId == companyId).ToListAsync();

            return Ok(data);
        }

        [HttpDelete("deletebyid/{id}")]
        public async Task<IActionResult> DeleteById(Guid id)
        {
            Guid companyId = GetCompanyFromHeader();

            var actionUser = await GetActionUser();

            var data = await _context.CustomerAddresses.FirstOrDefaultAsync(x => x.Id == id && x.CompanyId == companyId);


            try
            {
                _context.Remove(data);
                await _context.SaveChangesAsync();
                LogService.CreateLog($"Customer Address \"{data.Id}\" deleted by \"{actionUser.UserName}\"  Customer Address: {JsonConvert.SerializeObject(data)}.", LogTypeEnum.Information, LogOriginEnum.DataNexApp, actionUser.Id, _context);

            }
            catch (Exception ex)
            {
                LogService.CreateLog($"Customer Address \"{data.Id}\" could not be deleted by \"{actionUser.UserName}\"  Customer Address: {JsonConvert.SerializeObject(data)} Error:{ex.Message}.", LogTypeEnum.Error, LogOriginEnum.DataNexApp, actionUser.Id, _context);
            }

            return Ok(data);
        }
    }
}
