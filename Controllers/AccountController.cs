using AutoMapper;
using DataNex.Data;
using DataNex.Model.Dtos;
using DataNex.Model.Enums;
using DataNex.Model.Models;
using DataNexApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


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
                    //Update Client Database if any migration
                    var dbHelper = new DataBaseHelper();
                    await dbHelper.UpdateDatabase(context);
                    //Check if the user exists searching by the username
                    var user = await context.Users.Where(x => x.UserName == dto.UserName).FirstOrDefaultAsync();
                    if (user != null && dto.Password != null)
                    {
                        if(user.PasswordHash != null) 
                        {
                            //If password matches then the login is successful
                            success = BCrypt.Net.BCrypt.EnhancedVerify(dto.Password, user.PasswordHash);

                        }
                        else
                        {
                            return BadRequest("User password has not been defined. Please contact your administrator.");
                        }

                        if (!user.IsActive)
                        {
                            return BadRequest("User is not active. Please contact your administrator.");

                        }
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
                                Company = companyDto,
                                UserRoleId = context.UserRoles.FirstOrDefault(x => x.UserId == user.Id)?.RoleId,
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
                    return BadRequest("Cannot connect to database. Please contact your adminstrator");
                }

            }

        }


        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            var apiResponse = new ApiResponseDto();

            using (var transaction = _coreContext.Database.BeginTransaction())
            {
                try
                {
                    //Add new client in core database
                    var client = AddNewClient(dto);

                    //Create the new database and seed initial data
                    await CreateClientDataBase(dto.CompanyLoginCode);

                    //Create the new company in the client's database
                    AddNewCompany(dto, client);

                    //When all actions complete successfully we commit the transaction 
                    transaction.Commit();
                    apiResponse.Success = true;
                    return Ok(apiResponse);

                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    apiResponse.Success = false;
                    apiResponse.Message = ex.Message;
                    return BadRequest(apiResponse);


                }

            }



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
