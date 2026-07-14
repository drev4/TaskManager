using AutoMapper;
using TaskMgr.Api.Application.Mappings;

namespace TaskMgr.Api.Tests.TestSupport;

public static class MapperFactory
{
    public static IMapper Create()
    {
        var configuration = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        return configuration.CreateMapper();
    }
}
