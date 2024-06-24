using DataNex.Data;
using DataNex.Model.Dtos.Woocommerce;
using DataNex.Model.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using WoocommerceHandler;

namespace DataNexApi.Services
{
    public class ConnectorService
    {
        private ApplicationDbContext _context;
        public ConnectorService(ApplicationDbContext context) 
        { 
            _context = context;
        }

        public async Task GetProductsFromWoo(string url)
        {
            var parameters = await _context.ConnectorParameters.FirstOrDefaultAsync();
            var woocommerceService = new WoocommerceService(parameters.WooConsumerKey, parameters.WooConsumerSecret);

            var result = await woocommerceService.GetAsync(url);

            var products = new List<WooProductDto>();

            JsonConvert.PopulateObject(result, products);

            await PostProductsToDb(products);
        }

        public async Task PostProductsToDb(List<WooProductDto> products)
        {
            
                foreach (var product in products)
                {
                    var dnProduct = new Product();

                    dnProduct.Name = product.name;
                    dnProduct.Description = product.description;
                    dnProduct.ImagePath = product.images[0].src;
                    dnProduct.Sku = product.sku;
                    dnProduct.Price = product.price;

                    _context.Products.Add(dnProduct);
                }
                _context.SaveChanges();

            
        }
    }
}
