using AutoMapper;

namespace MvcMovie.Models.AutoMapper;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        CreateMap<Movie, MovieViewModel>().ReverseMap();
        CreateMap<MovieRequest, Movie>().ReverseMap();
    }
}