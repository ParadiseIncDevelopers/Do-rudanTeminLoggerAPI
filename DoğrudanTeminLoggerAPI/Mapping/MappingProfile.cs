using AutoMapper;
using DoğrudanTeminLoggerAPI.Dto;
using DoğrudanTeminLoggerAPI.Models;
using Newtonsoft.Json;

namespace DoğrudanTeminLoggerAPI.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<LogEntryDto, LogEntry>();
            CreateMap<LogEntry, LogEntryDto>();

            CreateMap<PageEntryDto, PageEntry>();
            CreateMap<PageEntry, PageEntryDto>();
        }
    }
}
