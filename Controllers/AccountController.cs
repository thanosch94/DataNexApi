using AutoMapper;
using DataNex.Data;
using DataNex.Model.Dtos;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using DataNexApi.Services;
using DataNex.Model.Enums;
using DataNex.Model.Models;
using Microsoft.AspNetCore.Http;
using DataNex.Model.Dtos.TimeZone;
using Newtonsoft.Json;
using System;
using System.Xml.Linq;

namespace DataNexApi.Controllers
{
    [AllowAnonymous]
    public class AccountController : BaseController
    {
        private CoreDbContext _coreContext;
        private IMapper _mapper;
        public AccountController(CoreDbContext context, IMapper mapper)
        {
            _coreContext = context;
            _mapper = mapper;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            Guid companyId = GetCompanyFromHeader();

            var apiResponse = new ApiResponseDto();

            var success = false;

            //Replace {clientDbName} with the company code provided by the user during login 
            var connString = AppBase.ClientConnectionString.Replace("{clientDbName}", dto.CompanyCode);

            using (var context = new ApplicationDbContext(connString))
            {
                //Check if a connection can be established using the above connection string
                if (context.Database.CanConnect())
                {
                    //Check if the user exists searching by the username
                    var user = await context.Users.Where(x => x.UserName == dto.UserName).FirstOrDefaultAsync();
                    if (user != null && dto.Password != null)
                    {
                        //If password matches then the login is successful
                        success = BCrypt.Net.BCrypt.EnhancedVerify(dto.Password, user.PasswordHash);
                    }

                    if (success)
                    {

                        AppBase.ConnectionString = connString;
                        var company = context.Companies.Where(x => x.CompanyLoginCode == dto.CompanyCode).FirstOrDefault();
                        if (company != null)
                        {
                            var companyDto = _mapper.Map<CompanyDto>(company);
                            LogService.CreateLog($"User \"{user.UserName}\" logged in successfully.", LogTypeEnum.Information, LogOriginEnum.DataNexApp, user.Id, context);
                            var userToReturn = new UserDto()
                            {
                                Id = user.Id,
                                Name = user.Name,
                                Email = user.Email,
                                UserName = user.UserName,
                                UserRole = user.UserRole,
                                Company = companyDto,
                                Token = TokenService.GenerateToken(user)
                            };
                            apiResponse.Success = success;
                            apiResponse.Result = userToReturn;
                            SetUserTimeZone(user);

                        }
                        return Ok(apiResponse);

                    }
                    else
                    {
                        LogService.CreateLog($"Failed login attempt \"{dto.UserName}\".", LogTypeEnum.Warning, LogOriginEnum.DataNexApp, null, context); ;

                        return BadRequest("Username or Password is incorrect");
                    }
                }
                else
                {
                    return BadRequest("Cannot connect to database. Contact your adminstrator");
                }

            }



        }


        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            using (var transaction = _coreContext.Database.BeginTransaction())
            {
                try
                {
                    var client = AddNewClient(dto);

                    await CreateClientDataBase(dto.CompanyLoginCode);

                    AddNewCompany(dto, client);

                    //When client database and new company inserted successfully we commit the transaction 
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                }

            }


            var apiResponse = new ApiResponseDto();

            return Ok(apiResponse);
        }


        private void SetUserTimeZone(User user)
        {
            var offsetMinutes = GetUserTimeZone();

            TimeZoneSettings.UserOffsetHours = -(offsetMinutes / 60);

        }

        private ClientDto AddNewClient(RegisterDto dto)
        {

            var client = new Client();
            client.Name = dto.ClientName;
            client.Phone1 = dto.ClientPhone1;
            client.Phone2 = dto.ClientPhone2;
            client.Address = dto.ClientAddress;
            client.City = dto.ClientCity;
            client.Region = dto.ClientRegion;
            client.PostalCode = dto.ClientPostalCode;
            client.Country = dto.ClientCountry;
            client.Email = dto.ClientEmail;


            _coreContext.Add(client);
            _coreContext.SaveChanges();

            var clientDto = _mapper.Map<ClientDto>(client);
            return clientDto;
        }
        private async Task CreateClientDataBase(string companyCode)
        {
            try
            {
   
                var dbHelper = new DataBaseHelper();
                await dbHelper.CreateDatabase(companyCode);


                var connString = AppBase.ClientConnectionString.Replace("{clientDbName}", companyCode);
                var context = new ApplicationDbContext(connString);
                await dbHelper.SeedDataToNewDb(context);
                //Seed data on Startup
                var applicationDataSeeder = new ApplicationDataSeeder();
                await applicationDataSeeder.SeedData(context);
            }
            catch (Exception ex)
            {
                //If something goes wrong we throw so to rollback the previous transaction in AddNewClient
                throw;
            }

        }

        private void AddNewCompany(RegisterDto dto, ClientDto clientDto)
        {
            var connString = AppBase.ClientConnectionString.Replace("{clientDbName}", dto.CompanyLoginCode);

            using (var context = new ApplicationDbContext(connString))
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        var company = new Company();
                        company.Name = dto.CompanyName;
                        company.IsDefault = true;
                        company.ClientId = clientDto.Id;
                        company.Phone1 = dto.ClientPhone1;
                        company.Phone2 = dto.ClientPhone2;
                        company.Address = dto.ClientAddress;
                        company.City = dto.ClientCity;
                        company.Region = dto.ClientRegion;
                        company.PostalCode = dto.ClientPostalCode;
                        company.Country = dto.ClientCountry;
                        company.Email = dto.ClientEmail;
                        company.VatNumber = dto.CompanyVatNumber;
                        company.TaxOffice = dto.CompanyTaxOffice;
                        company.CompanyLoginCode = dto.CompanyLoginCode;

                        context.Add(company);
                        context.SaveChanges();
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }



            }


        }

    }
}
