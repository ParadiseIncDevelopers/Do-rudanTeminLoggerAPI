using AutoMapper;
using DoğrudanTeminLoggerAPI.Dto;
using DoğrudanTeminLoggerAPI.Helpers;
using DoğrudanTeminLoggerAPI.Services.Abstract;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DoğrudanTeminLoggerAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PageController : ControllerBase
    {
        private readonly IPageService _pageService;
        private readonly IMapper _mapper;

        public PageController(IPageService pageService, IMapper mapper)
        {
            _pageService = pageService;
            _mapper = mapper;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PageEntryDto dto)
        {
            var request = new PageEntryLogRequest
            {
                PageLogDateTime = dto.PageLogDateTime,
                UserId = dto.UserId,
                PageUrl = dto.PageUrl
            };
            await _pageService.PageAsync(request);
            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] PageQueryParameters q)
        {
            var entries = await _pageService.QueryAsync(q);
            var dtos = _mapper.Map<IEnumerable<PageEntryDto>>(entries);
            return Ok(dtos);
        }
    }
}
