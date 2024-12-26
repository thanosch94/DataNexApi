using AutoMapper;
using DataNex.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DataNexApi.Controllers
{
    [Authorize]
    public class ReportsController:BaseController
    {
        private ApplicationDbContext _context;
        private IMapper _mapper;

        public ReportsController(ApplicationDbContext context, IMapper mapper) : base(context)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet("getOrdersByStatus")]
        public IActionResult GetOrdersByStatus()
        {
            var data =  _context.Documents.Where(x => x.DocumentTypeId == DataSeedIds.SalesOrder).GroupBy(x => x.DocumentStatus).Select(x => new
            {
                value = x.Count(),
                name = x.Key.Name,
            }).ToList();

            return Ok(data);
        }

        [HttpGet("getAverageOrderPerMonth")]
        public IActionResult GetAverageOrderPerMonth()
        {
            var data = _context.Documents
                .Where(x => x.DocumentTypeId == DataSeedIds.SalesOrder && x.DocumentDateTime.Year == DateTime.Now.Year)
                .GroupBy(x => x.DocumentDateTime.Month)
                .Select(x => new
                {
                    month = x.Key,            
                    value = x.Average(y => y.DocumentTotal) 
                })
                .ToList();

            //Create a list of all 12 months and merge with the data (ensure no months are missing)
            var allMonths = Enumerable.Range(1, 12)
                .GroupJoin(data, month => month, dataGroup => dataGroup.month,
                    (month, dataGroup) => new
                    {
                        month = month,
                        value = dataGroup.FirstOrDefault()?.value ?? 0
                    })
                .OrderBy(x => x.month)
                .ToList();

            var max = allMonths.Max(x => x.value);

            var dataToReturn = allMonths
                .Select(x => new
                {
                    value = x.value,
                    itemStyle = x.value == max ? new { color = "darkred" } : new { color = "darkblue" }
                })
                .ToList();

            return Ok(dataToReturn);
        }

        [HttpGet("getOrdersTotalPerMonth")]
        public IActionResult GetOrderTotalPerMonth()
        {
            var data = _context.Documents
                .Where(x => x.DocumentTypeId == DataSeedIds.SalesOrder && x.DocumentDateTime.Year == DateTime.Now.Year)
                .GroupBy(x => x.DocumentDateTime.Month)
                .Select(x => new
                {
                    month = x.Key,
                    value = x.Sum(y => y.DocumentTotal)
                })
                .ToList();

            //Create a list of all 12 months and merge with the data (ensure no months are missing)
            var allMonths = Enumerable.Range(1, 12)
                .GroupJoin(data, month => month, dataGroup => dataGroup.month,
                    (month, dataGroup) => new
                    {
                        month = month,
                        value = dataGroup.FirstOrDefault()?.value ?? 0
                    })
                .OrderBy(x => x.month)
                .ToList();

            var max = allMonths.Max(x => x.value);

            var dataToReturn = allMonths
                .Select(x => new
                {
                    value = x.value,
                    itemStyle = x.value == max ? new { color = "darkred" } : new { color = "darkblue" }
                })
                .ToList();

            return Ok(dataToReturn);
        }
    }
}
