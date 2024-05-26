using AutoMapper;
using DataNex.Model.Dtos;
using DataNex.Model.Models;

namespace DataNexApi.Mapping
{
    public class AutoMapperProfiles:Profile
    {

        public AutoMapperProfiles()
        {
            CreateMap<User,UserDto>().ReverseMap();
            CreateMap<Customer,CustomerDto>().ReverseMap();

            CreateMap<Document, DocumentDto>().ReverseMap();
            CreateMap<DocumentType, DocumentTypeDto>().ReverseMap();
            CreateMap<Product, ProductDto>().ReverseMap();
            CreateMap<Status, StatusDto>().ReverseMap();
            CreateMap<Brand, BrandDto>().ReverseMap();
            CreateMap<ProductSize, ProductSizeDto>().ReverseMap();
            CreateMap<DocumentProduct, DocumentProductDto>().ReverseMap();
            CreateMap<ProductBarcode, ProductBarcodeDto>().ReverseMap();
            CreateMap<User, UserDto>().ReverseMap();
            CreateMap<Log, LogDto>().ReverseMap();

        }
    }
}
