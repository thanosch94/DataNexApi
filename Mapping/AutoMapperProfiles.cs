using AutoMapper;
using DataNex.Model.Dtos;
using DataNex.Model.Dtos.Connector;
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
            CreateMap<ConnectorParameters, ConnectorParametersDto>().ReverseMap();
            CreateMap<WooConnectionsData, WooConnectionsDataDto>().ReverseMap();
            CreateMap<AdditionalCharge, AdditionalChargeDto>().ReverseMap();
            CreateMap<DocumentAdditionalCharge, DocumentAdditionalChargeDto>().ReverseMap();
            CreateMap<ConnectorJob, ConnectorJobDto>().ReverseMap();
            CreateMap<WareHouse, WareHouseDto>().ReverseMap();
            CreateMap<Supplier, SupplierDto>().ReverseMap();
            CreateMap<VatClass, VatClassDto>().ReverseMap();
            CreateMap<Company, CompanyDto>().ReverseMap();
            CreateMap<Lot, LotDto>().ReverseMap();
            CreateMap<GeneralOptions, GeneralOptionsDto>().ReverseMap();
            CreateMap<LotSettings, LotSettingsDto>().ReverseMap();
            CreateMap<DocumentProductLotQuantity, DocumentProductLotQuantityDto>().ReverseMap();
            CreateMap<Category, CategoryDto>().ReverseMap();
            CreateMap<Client, ClientDto>().ReverseMap();
            CreateMap<WorkItem, WorkItemDto>().ReverseMap();
            CreateMap<WorkItemType, WorkItemTypeDto>().ReverseMap();
            CreateMap<AppPermission, AppPermissionDto>().ReverseMap();
            CreateMap<UserAppPermission, UserAppPermissionDto>().ReverseMap();
            CreateMap<CntorDatasource, CntorDatasourceDto>().ReverseMap();
            CreateMap<CntorDatasourceEntity, CntorDatasourceEntityDto>().ReverseMap();
            CreateMap<Address, AddressDto>().ReverseMap();
            CreateMap<CustomerAddress, CustomerAddressDto>().ReverseMap();
            CreateMap<DocumentSeries, DocumentSeriesDto>().ReverseMap();

        }
    }
}
