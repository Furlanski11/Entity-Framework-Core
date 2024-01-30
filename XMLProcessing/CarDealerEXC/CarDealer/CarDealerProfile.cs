using AutoMapper;
using CarDealer.DTOs.Export;
using CarDealer.DTOs.Import;
using CarDealer.Models;

namespace CarDealer
{
    public class CarDealerProfile : Profile
    {
        public CarDealerProfile()
        {
            CreateMap<ImportSuppliersDTO, Supplier>();
			CreateMap<Supplier, ExportLocalSuppliers>();

			CreateMap<ImportPartsDTO, Part>();

            CreateMap<ImportCarDTO, Car>();
            CreateMap<Car, ExportCarsWithDistance>();
            CreateMap<Car, ExportBMWcars>();

			CreateMap<ImportCustomerDTO, Customer>();

			CreateMap<ImportSaleDTO, Sale>();

            CreateMap<Part, ExportPartsDto>();
            CreateMap<Car, ExportCarsWithParts>();
		}
    }
}
