﻿using AutoMapper;
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
    public class UsersController : BaseController
    {
        private ApplicationDbContext _context;
        private IMapper _mapper;
        public UsersController(ApplicationDbContext context, IMapper mapper):base(context)  
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet("getall")]
        public async Task<IActionResult> GetAll()
        {
            //DnAdmin User must not be visible in the users list
            var data = await _context.Users.Where(x=>x.Id != AppBase.DnAdmin).ToListAsync();

            return Ok(data);
        }

        [HttpGet("getbyid/{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var data = await _context.Users.Where(x=>x.Id ==id).FirstOrDefaultAsync();

            return Ok(data);
        }

        [HttpPost("insertdto")]
        public async Task<IActionResult> InsertDto([FromBody] UserDto dto)
        {
            var actionUser = await GetActionUser();

            var data = new User();

            var exists = await _context.Users.Where(x => x.UserName == dto.UserName).FirstOrDefaultAsync();
            if (exists == null)
            {
                data.Name = dto.Name;
                data.Email = dto.Email;
                data.UserName = dto.UserName;
                data.UserRole = dto.UserRole;
                data.PasswordHash = BCrypt.Net.BCrypt.EnhancedHashPassword(dto.Password);
                data.UserAdded = actionUser.Id;

                await ExecuteTransaction(async () =>
                {
                    var maxNumber = _context.Users.Max(x => (x.SerialNumber)) ?? 0;
                    data.SerialNumber = maxNumber + 1;
                    data.Code = data.SerialNumber.ToString().PadLeft(5, '0');

                    try
                    {
                        _context.Users.Add(data);
                        await _context.SaveChangesAsync();
                        LogService.CreateLog($"User \"{data.Name}\" inserted by \"{actionUser.UserName}\". User: {data.Id}, {data.Name}", LogTypeEnum.Information, LogOriginEnum.DataNexApp, actionUser.Id, _context);

                    }
                    catch (Exception ex)
                    {
                        LogService.CreateLog($"User \"{data.Name}\" could not be inserted by \"{actionUser.UserName}\". User: {data.Id}, {data.Name} Error: {ex.Message}", LogTypeEnum.Error, LogOriginEnum.DataNexApp, actionUser.Id, _context);
                        throw;
                    }
                });

                var dtoData = _mapper.Map<UserDto>(data);

                return Ok(dtoData);
            }
            else
            {
                return BadRequest("Username already exists");
            }

        }

        [HttpPut("updatedto")]
        public async Task<IActionResult> UpdateDto([FromBody] UserDto dto)
        {
            var actionUser = await GetActionUser();

            var data = await _context.Users.Where(x => x.Id == dto.Id).FirstOrDefaultAsync();

            data.Name = dto.Name;
            data.Email = dto.Email;
            data.UserName = dto.UserName;
            data.UserRole = dto.UserRole;

            if (dto.Password!=null)
            {
                data.PasswordHash = BCrypt.Net.BCrypt.EnhancedHashPassword(dto.Password);
            }

            try
            {
                await _context.SaveChangesAsync();
                LogService.CreateLog($"User \"{data.Name}\" updated by \"{actionUser.UserName}\". User: {data.Id}, {data.Name}", LogTypeEnum.Information, LogOriginEnum.DataNexApp, actionUser.Id, _context);

            }
            catch (Exception ex)
            {
                LogService.CreateLog($"User \"{data.Name}\" could not be updated by \"{actionUser.UserName}\"  User: {data.Id}, {data.Name} Error: {ex.Message}.", LogTypeEnum.Error, LogOriginEnum.DataNexApp, actionUser.Id, _context);

            }

            var dtoData = _mapper.Map<UserDto>(data);

            return Ok(dtoData);

        }

        [HttpDelete("deletebyid/{id}")]
        public async Task<IActionResult> DeleteById(Guid id)
        {
            var actionUser = await GetActionUser();

            var data = await _context.Users.FirstOrDefaultAsync(x => x.Id == id);

            _context.Users.Remove(data);

            try
            {
                await _context.SaveChangesAsync();
                LogService.CreateLog($"User \"{data.Name}\" deleted by \"{actionUser.UserName}\". User: {data.Id}, {data.Name}", LogTypeEnum.Information, LogOriginEnum.DataNexApp, actionUser.Id, _context);

            }
            catch (Exception ex)
            {
                LogService.CreateLog($"User \"{data.Name}\" could not be deleted by \"{actionUser.UserName}\"  User: {data.Id}, {data.Name} Error: {ex.Message}.", LogTypeEnum.Error, LogOriginEnum.DataNexApp, actionUser.Id, _context);

            }
            return Ok(data);
        }
    }
}

