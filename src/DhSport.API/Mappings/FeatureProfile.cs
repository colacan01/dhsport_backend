using AutoMapper;
using DhSport.API.DTOs;
using DhSport.Domain.Entities.Features;

namespace DhSport.API.Mappings;

public class FeatureProfile : Profile
{
    public FeatureProfile()
    {
        // FeatureType mappings
        CreateMap<FeatureType, FeatureTypeDto>();
        CreateMap<CreateFeatureTypeDto, FeatureType>();
        CreateMap<UpdateFeatureTypeDto, FeatureType>();

        // AddFeature mappings
        CreateMap<AddFeature, AddFeatureDto>();
        CreateMap<CreateAddFeatureDto, AddFeature>();
        CreateMap<UpdateAddFeatureDto, AddFeature>();
    }
}
