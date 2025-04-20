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
    public class WorkItemsController : BaseController
    {
        private ApplicationDbContext _context;
        private IMapper _mapper;
        private static readonly object _lockObject = new object();

        public WorkItemsController(ApplicationDbContext context, IMapper mapper) : base(context)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet("getall")]
        public async Task<IActionResult> GetAll()
        {
            Guid companyId = GetCompanyFromHeader();

            var data = await _context.WorkItems.Where(x => x.CompanyId == companyId).ToListAsync();

            var dto = _mapper.Map<WorkItemDto[]>(data);
            return Ok(dto);
        }


        [HttpGet("getallByWorkItemCategory/{workItemCategoryId}")]
        public async Task<IActionResult> GetallByWorkItemCategory(WorkItemCategoryEnum workItemCategoryId)
        {
            Guid companyId = GetCompanyFromHeader();

            var data = await _context.WorkItems.Where(x => x.CompanyId == companyId && x.WorkItemCategory == workItemCategoryId).ToListAsync();
            var dto = _mapper.Map<WorkItemDto[]>(data);

            return Ok(dto);
        }

        
        [HttpGet("getallByUserId/{userId}")]
        public async Task<IActionResult> GetallByUserId(Guid userId)
        {
            Guid companyId = GetCompanyFromHeader();

            var data = await _context.WorkItems.Where(x => x.CompanyId == companyId && x.AssigneeId == userId).ToListAsync();
            var dto = _mapper.Map<WorkItemDto[]>(data);

            return Ok(dto);
        }


        
        [HttpGet("getById/{id}")]
        public async Task<IActionResult> GetAllByStatusType(Guid id)
        {
            Guid companyId = GetCompanyFromHeader();

            var data = await _context.WorkItems.Where(x => x.CompanyId == companyId && x.Id == id).FirstOrDefaultAsync();
            var dto = _mapper.Map<WorkItemDto>(data);

            return Ok(dto);
        }

        [HttpPost("insertdto")]
        public async Task<IActionResult> InsertDto([FromBody] WorkItemDto dto)
        {
            Guid companyId = GetCompanyFromHeader();

            var actionUser = await GetActionUser();

            var data = new WorkItem();

            var exists = await _context.WorkItems.Where(x => x.Name == dto.Name && x.CompanyId == companyId).FirstOrDefaultAsync();
            if (exists == null)
            {
                data.Name = dto.Name;
                data.Description = dto.Description;
                data.MasterTaskId = dto.MasterTaskId;
                data.StatusId = dto.StatusId;
                data.AssigneeId = dto.AssigneeId;
                data.WorkItemTypeId = dto.WorkItemTypeId;
                data.WorkItemCategory = dto.WorkItemCategory;
                data.SprintId = dto.SprintId;
                data.DueDate = dto.DueDate;
                data.WorkItemPriority = dto.WorkItemPriority;
                data.UserAdded = actionUser.Id;
                data.CompanyId = companyId;

                lock (_lockObject)
                {
                    var maxNumber = _context.WorkItems.Where(x => x.CompanyId == companyId).Max(x => (x.SerialNumber)) ?? 0;
                    data.SerialNumber = maxNumber + 1;
                    data.Code = data.SerialNumber.ToString().PadLeft(5, '0');

                    try
                    {
                        _context.WorkItems.Add(data);
                        _context.SaveChanges();
                        LogService.CreateLog($"Work Item \"{data.Name}\" inserted by \"{actionUser.UserName}\".Work Item: {JsonConvert.SerializeObject(data, new JsonSerializerSettings()
                        {
                            ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
                        })}", LogTypeEnum.Information, LogOriginEnum.DataNexApp, actionUser.Id, _context);

                    }
                    catch (Exception ex)
                    {
                        LogService.CreateLog($"Work Item \"{data.Name}\" could not be inserted by \"{actionUser.UserName}\". Work Item: {JsonConvert.SerializeObject(data, new JsonSerializerSettings()
                        {
                            ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
                        })} Error: {ex.Message}", LogTypeEnum.Error, LogOriginEnum.DataNexApp, actionUser.Id, _context);
                        throw;
                    }
                };
                var dataToReturn = _mapper.Map<WorkItemDto>(data);

                return Ok(dataToReturn);
            }
            else
            {
                return BadRequest("Work Item already exists");
            }

        }



        [HttpPut("updatedto")]
        public async Task<IActionResult> UpdateDto([FromBody] WorkItemDto dto)
        {
            Guid companyId = GetCompanyFromHeader();

            var actionUser = await GetActionUser();

            var data = await _context.WorkItems.Where(x => x.Id == dto.Id && x.CompanyId == companyId).FirstOrDefaultAsync();

            data.Name = dto.Name;
            data.Description = dto.Description;
            data.MasterTaskId = dto.MasterTaskId;
            data.StatusId = dto.StatusId;
            data.AssigneeId = dto.AssigneeId;
            data.WorkItemTypeId = dto.WorkItemTypeId;
            data.WorkItemCategory = dto.WorkItemCategory;
            data.SprintId = dto.SprintId;
            data.DueDate = dto.DueDate;
            data.WorkItemPriority = dto.WorkItemPriority;
            data.CompanyId = companyId;

            try
            {
                await _context.SaveChangesAsync();
                LogService.CreateLog($"Work Item \"{data.Name}\" updated by \"{actionUser.UserName}\". Work Item: {JsonConvert.SerializeObject(data, new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
                })}", LogTypeEnum.Information, LogOriginEnum.DataNexApp, actionUser.Id, _context);

            }
            catch (Exception ex)
            {
                LogService.CreateLog($"Work Item \"{data.Name}\" could not be updated by \"{actionUser.UserName}\". Work Item: {JsonConvert.SerializeObject(data, new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
                })} Error: {ex.Message}", LogTypeEnum.Error, LogOriginEnum.DataNexApp, actionUser.Id, _context);

            }
            var dataToReturn = _mapper.Map<WorkItemDto>(data);

            return Ok(dataToReturn);

        }



        [HttpDelete("deletebyid/{id}")]
        public async Task<IActionResult> DeleteById(Guid id)
        {
            Guid companyId = GetCompanyFromHeader();

            var actionUser = await GetActionUser();

            var data = await _context.WorkItems.FirstOrDefaultAsync(x => x.Id == id && x.CompanyId == companyId);

            try
            {
                _context.WorkItems.Remove(data);

                await _context.SaveChangesAsync();
                LogService.CreateLog($"Work Item \"{data.Name}\" deleted by \"{actionUser.UserName}\". Work Item: {JsonConvert.SerializeObject(data)}", LogTypeEnum.Information, LogOriginEnum.DataNexApp, actionUser.Id, _context);

            }
            catch (Exception ex)
            {
                LogService.CreateLog($"Work Item \"{data.Name}\" could not be deleted by \"{actionUser.UserName}\". Work Item: {JsonConvert.SerializeObject(data)} Error: {ex.Message}", LogTypeEnum.Error, LogOriginEnum.DataNexApp, actionUser.Id, _context);
            }
            var dataToReturn = _mapper.Map<WorkItemDto>(data);

            return Ok(dataToReturn);
        }
    }
}
