using AutoMapper;
using DataNex.Data;
using Microsoft.AspNetCore.Authorization;

namespace DataNexApi.Controllers
{
    [Authorize]
    public class DocTypeTransformationsController : BaseController
    {
        private ApplicationDbContext _context;
        private IMapper _mapper;
        private static readonly object _lockObject = new object();

        public DocTypeTransformationsController(ApplicationDbContext context, IMapper mapper) : base(context)
        {
            _context = context;
            _mapper = mapper;
        }

    }
}
