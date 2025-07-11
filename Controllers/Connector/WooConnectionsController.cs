using AutoMapper;
using DataNex.Data;
using DataNex.Model.Dtos;
using DataNex.Model.Dtos.Connector;
using DataNex.Model.Dtos.Woocommerce;
using DataNex.Model.Enums;
using DataNex.Model.Models;
using DataNexApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Text;

namespace DataNexApi.Controllers.Connector
{
    public class WooConnectionsController : BaseController
    {
        private ApplicationDbContext _context;
        private IMapper _mapper;
        private static readonly object _lockObject = new object();

        public WooConnectionsController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet("getall")]
        public async Task<IActionResult> GetAll()
        {
            Guid companyId = GetCompanyFromHeader();

            var data = await _context.WooConnectionsData.Where(x => x.CompanyId == companyId).ToListAsync();

            return Ok(data);
        }

        [HttpPost("insertdto")]
        public async Task<IActionResult> InsertDto([FromBody] WooConnectionsDataDto dto)
        {
            Guid companyId = GetCompanyFromHeader();

            var actionUser = await GetActionUser();

            var data = new WooConnectionsData();
            data.Name = dto.Name;
            data.RequestType = dto.RequestType;
            data.Endpoint = dto.Endpoint;
            data.WooEntity = dto.WooEntity;
            data.CompanyId = companyId;

            lock (_lockObject)
            {
                var maxNumber = _context.WooConnectionsData.Where(x => x.CompanyId == companyId).Max(x => (x.SerialNumber)) ?? 0;
                data.SerialNumber = maxNumber + 1;
                data.Code = data.SerialNumber.ToString().PadLeft(5, '0');

                try
                {
                    _context.WooConnectionsData.Add(data);
                    _context.SaveChanges();
                    LogService.CreateLog($"Woo Connection data inserted by \"{actionUser.UserName}\"  Data: {JsonConvert.SerializeObject(data)}.", LogTypeEnum.Information, LogOriginEnum.DataNexApp, actionUser.Id, _context);

                }
                catch (Exception ex)
                {
                    LogService.CreateLog($"Woo Connection data could not be inserted by \"{actionUser.UserName}\"  Data: {JsonConvert.SerializeObject(data)} Error: {ex.Message}.", LogTypeEnum.Error, LogOriginEnum.DataNexApp, actionUser.Id, _context);

                }
            };


            var dtoToReturn = _mapper.Map<WooConnectionsDataDto>(data);

            return Ok(dtoToReturn);
        }

        [HttpPut("updatedto")]
        public async Task<IActionResult> UpdateDto([FromBody] WooConnectionsDataDto dto)
        {
            Guid companyId = GetCompanyFromHeader();

            var actionUser = await GetActionUser();

            var data = await _context.WooConnectionsData.FirstOrDefaultAsync(x => x.Id == dto.Id && x.CompanyId == companyId);
            data.Name = dto.Name;
            data.RequestType = dto.RequestType;
            data.Endpoint = dto.Endpoint;
            data.WooEntity = dto.WooEntity;

            data.UserUpdated = actionUser.Id;
            data.DateUpdated = DateTime.Now;
            data.CompanyId = companyId;

            try
            {
                await _context.SaveChangesAsync();

                LogService.CreateLog($"Woo Connection data updated by \"{actionUser.UserName}\"  Data: {JsonConvert.SerializeObject(data)}.", LogTypeEnum.Information, LogOriginEnum.DataNexApp, actionUser.Id, _context);

            }
            catch (Exception ex)
            {
                LogService.CreateLog($"Woo Connection data could not be updated by \"{actionUser.UserName}\"  Data: {JsonConvert.SerializeObject(data)} Error: {ex.Message}.", LogTypeEnum.Error, LogOriginEnum.DataNexApp, actionUser.Id, _context);

            }
            var dtoToReturn = _mapper.Map<WooConnectionsDataDto>(data);

            return Ok(dtoToReturn);
        }

        [HttpDelete("deletebyid/{id}")]
        public async Task<IActionResult> DeleteById(Guid id)
        {
            Guid companyId = GetCompanyFromHeader();

            var actionUser = await GetActionUser();

            var data = await _context.WooConnectionsData.FirstOrDefaultAsync(x => x.Id == id && x.CompanyId == companyId);

            try
            {
                _context.WooConnectionsData.Remove(data);
                await _context.SaveChangesAsync();
                LogService.CreateLog($"Woo Connection data deleted by \"{actionUser.UserName}\"  Data: {JsonConvert.SerializeObject(data)}.", LogTypeEnum.Information, LogOriginEnum.DataNexApp, actionUser.Id, _context);

            }
            catch (Exception ex)
            {
                LogService.CreateLog($"Woo Connection data could not be deleted by \"{actionUser.UserName}\"  Data: {JsonConvert.SerializeObject(data)} Error: {ex.Message}.", LogTypeEnum.Error, LogOriginEnum.DataNexApp, actionUser.Id, _context);

            }
            return Ok(data);
        }

        [AllowAnonymous]
        [HttpPost("updateProduct")]
        public async Task<IActionResult> UpdateProduct()
        {
            Request.EnableBuffering(); // Επιτρέπει να ξαναδιαβαστεί το stream

            Request.Body.Position = 0;
            using (var reader = new StreamReader(Request.Body, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, leaveOpen: true))
            {
                var body = await reader.ReadToEndAsync();

                // Log το σώμα του webhook για έλεγχο
                System.IO.File.WriteAllText("C:\\Logs\\webhook_log.txt", body);

                // Αν χρειάζεται deserialize:
                // var data = JsonSerializer.Deserialize<YourModel>(body);

                return Ok();
            }
        }

        //[AllowAnonymous]
        //[HttpPost("insertOrder/{userId}/{companyCode}/{companyId}")]
        //public async Task<IActionResult> InsertOrder(Guid userId, string companyCode, Guid companyId)
        //{
        //    Request.EnableBuffering(); // Επιτρέπει να ξαναδιαβαστεί το stream

        //    Request.Body.Position = 0;
        //    using (var reader = new StreamReader(Request.Body, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, leaveOpen: true))
        //    {
        //        var body = await reader.ReadToEndAsync();
        //        var order = new WooOrderDto();

        //        JsonConvert.PopulateObject(body, order);
        //        var connString = AppBase.ClientConnectionString.Replace("{clientDbName}", companyCode);
        //        using (var context = new ApplicationDbContext(connString))
        //        {
        //            //Check if customerExists using CustomerNumber

        //            var customer = await context.Customers.Where(x => x.UserField5 = product.id).FirstOrDefaultAsync();


        //            if (customer == null)
        //            {
        //                //Insert Customer that doesnt exist
        //            }

                    
        //            foreach(var product in order.line_items)
        //            {
        //                //Check if product exists using wooId
        //                var product = await context.Products.Where(x=>x.UserField5= product.id).FirstOrDefaultAsync();

        //                if(product == null)
        //                {
        //                    //Insert Product that doesnt exist
        //                }
        //            }

        //        };
        //            // Log το σώμα του webhook για έλεγχο
        //            System.IO.File.WriteAllText("C:\\Logs\\webhook_log.txt", body);

        //        // Αν χρειάζεται deserialize:
        //        // var data = JsonSerializer.Deserialize<YourModel>(body);

        //        return Ok();
        //    }
        //}
    }
}