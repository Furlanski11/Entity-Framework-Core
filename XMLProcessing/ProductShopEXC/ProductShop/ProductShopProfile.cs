using AutoMapper;
using ProductShop.DTOs.Export;
using ProductShop.DTOs.Import;
using ProductShop.Models;

namespace ProductShop
{
    public class ProductShopProfile : Profile
    {
        public ProductShopProfile()
        {
            CreateMap<ImportUserDTO, User>();

            CreateMap<ImportProductDTO, Product>();

            CreateMap<ImportCategoriesDTO, Category>();

            CreateMap<ImportCategoriesProductsDTO, CategoryProduct>();

            CreateMap<Product, ExportProductsInRange>();

            CreateMap<CategoryProduct, ExportProductsInRange>();
        }
    }
}
