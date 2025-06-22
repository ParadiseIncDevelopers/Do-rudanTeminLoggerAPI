using AutoMapper;
using DoğrudanTeminLoggerAPI.Dto;
using DoğrudanTeminLoggerAPI.Helpers;
using DoğrudanTeminLoggerAPI.Models;
using Newtonsoft.Json;

namespace DoğrudanTeminLoggerAPI.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<LogEntryDto, LogEntry>();
            CreateMap<LogEntry, LogEntryDto>().ForMember(d => d.LogDateTime, o => o.MapFrom(s => s.LogDateTime.ToTurkeyTime()));

            CreateMap<PageEntryDto, PageEntry>();
            CreateMap<PageEntry, PageEntryDto>()
                .ForMember(d => d.PageLogDateTime, o => o.MapFrom(s => s.LogDateTime.ToTurkeyTime()));
        }
    }
}
