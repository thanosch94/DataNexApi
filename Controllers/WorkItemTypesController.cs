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
    public class WorkItemTypesController : BaseController
    {
        private ApplicationDbContext _context;
        private IMapper _mapper;
        private static readonly object _lockObject = new object();

        public WorkItemTypesController(ApplicationDbContext context, IMapper mapper) : base(context)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet("getall")]
        public async Task<IActionResult> GetAll()
        {
            Guid companyId = GetCompanyFromHeader();

            var data = await _context.WorkItemTypes.Where(x => x.CompanyId == companyId).ToListAsync();

            return Ok(data);
        }

        [HttpGet("getallByWorkItemCategory/{workItemCategory}")]
        public async Task<IActionResult> GetallByWorkItemCategory(WorkItemCategoryEnum workItemCategory)
        {
            Guid companyId = GetCompanyFromHeader();

            var data = await _context.WorkItemTypes.Where(x => x.CompanyId == companyId && x.Category == workItemCategory).ToListAsync();

            return Ok(data);
        }




        [HttpPost("insertdto")]
        public async Task<IActionResult> InsertDto([FromBody] WorkItemTypeDto workItemType)
        {
            Guid companyId = GetCompanyFromHeader();

            var actionUser = await GetActionUser();

            var data = new WorkItemType();

            var exists = await _context.WorkItemTypes.Where(x => x.Name == workItemType.Name && x.Category == workItemType.Category && x.CompanyId == companyId).FirstOrDefaultAsync();
            if (exists == null)
            {
                data.Name = workItemType.Name;
                data.Category = workItemType.Category;
                data.UserAdded = actionUser.Id;
                data.CompanyId = companyId;

                lock (_lockObject)
                {
                    var maxNumber = _context.WorkItemTypes.Where(x => x.CompanyId == companyId && x.Category == workItemType.Category).Max(x => (x.SerialNumber)) ?? 0;
                    data.SerialNumber = maxNumber + 1;
                    data.Code = data.SerialNumber.ToString().PadLeft(5, '0');

                    try
                    {
                        _context.WorkItemTypes.Add(data);
                        _context.SaveChanges();
                        LogService.CreateLog($"Work Item Type \"{data.Name}\" inserted by \"{actionUser.UserName}\". Work Item Type: {JsonConvert.SerializeObject(data)}", LogTypeEnum.Information, LogOriginEnum.DataNexApp, actionUser.Id, _context);

                    }
                    catch (Exception ex)
                    {
                        LogService.CreateLog($"Work Item Type \"{data.Name}\" could not be inserted by \"{actionUser.UserName}\". Work Item Type: {JsonConvert.SerializeObject(data)} Error: {ex.Message}", LogTypeEnum.Error, LogOriginEnum.DataNexApp, actionUser.Id, _context);
                        throw;
                    }
                };
                return Ok(data);
            }
            else
            {
                return BadRequest("Work Item Type already exists");
            }

        }

        [HttpPut("updatedto")]
        public async Task<IActionResult> UpdateDto([FromBody] WorkItemTypeDto workItemType)
        {
            Guid companyId = GetCompanyFromHeader();

            var actionUser = await GetActionUser();

            var data = await _context.WorkItemTypes.Where(x => x.Id == workItemType.Id && x.CompanyId == companyId).FirstOrDefaultAsync();

            data.Name = workItemType.Name;
            data.Category = workItemType.Category;
            data.CompanyId = companyId;

            try
            {
                await _context.SaveChangesAsync();
                LogService.CreateLog($"Work Item Type \"{data.Name}\" updated by \"{actionUser.UserName}\". Work Item Type: {JsonConvert.SerializeObject(data)}", LogTypeEnum.Information, LogOriginEnum.DataNexApp, actionUser.Id, _context);

            }
            catch (Exception ex)
            {
                LogService.CreateLog($"Work Item Type \"{data.Name}\" could not be updated by \"{actionUser.UserName}\". Work Item Type: {JsonConvert.SerializeObject(data)} Error: {ex.Message}", LogTypeEnum.Error, LogOriginEnum.DataNexApp, actionUser.Id, _context);

            }

            return Ok(data);

        }

        [HttpDelete("deletebyid/{id}")]
        public async Task<IActionResult> DeleteById(Guid id)
        {
            Guid companyId = GetCompanyFromHeader();

            var actionUser = await GetActionUser();

            var data = await _context.WorkItemTypes.FirstOrDefaultAsync(x => x.Id == id && x.CompanyId == companyId);

            try
            {
                _context.WorkItemTypes.Remove(data);

                await _context.SaveChangesAsync();
                LogService.CreateLog($"Work Item Type \"{data.Name}\" deleted by \"{actionUser.UserName}\". Work Item Type: {JsonConvert.SerializeObject(data)}", LogTypeEnum.Information, LogOriginEnum.DataNexApp, actionUser.Id, _context);

            }
            catch (Exception ex)
            {
                LogService.CreateLog($"Work Item Type \"{data.Name}\" could not be deleted by \"{actionUser.UserName}\". Work Item Type: {JsonConvert.SerializeObject(data)} Error: {ex.Message}", LogTypeEnum.Error, LogOriginEnum.DataNexApp, actionUser.Id, _context);
            }
            return Ok(data);
        }
    }
}
