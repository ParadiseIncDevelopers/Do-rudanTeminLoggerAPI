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
            CreateMap<LogEntryDto, LogEntry>()
                .ForMember(d => d.LogObjectJson, o => o.MapFrom(s => JsonConvert.SerializeObject(s.LogObject)))
                .ForMember(d => d.UserId, o => o.MapFrom(s => s.UserId))
                .ForMember(d => d.Token, o => o.MapFrom(s => s.Token));

            CreateMap<LogEntry, LogEntryDto>()
                .ForMember(d => d.LogObject, o => o.MapFrom(s => JsonConvert.DeserializeObject<object>(s.LogObjectJson)))
                .ForMember(d => d.UserId, o => o.MapFrom(s => s.UserId))
                .ForMember(d => d.Token, o => o.MapFrom(s => s.Token));
        }
    }
}
