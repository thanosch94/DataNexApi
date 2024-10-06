using DataNex.Data;
using DataNex.Model.Dtos.Connector;
using DataNex.Model.Dtos;
using DataNex.Model.Dtos.Woocommerce;
using DataNex.Model.Enums;
using DataNex.Model.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using WooCommerceService;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace DataNexApi.Services
{
    public class ConnectorService
    {
        private ApplicationDbContext _context;
        public ConnectorService(ApplicationDbContext context)
        {
            _context = context;

        }

        public async Task<ApiResponseDto> ExecuteWoocommerceJob(ConnectorJobDto connectorJob)
        {
            var apiResponse = new ApiResponseDto();
            var wooResponse = new WooResponseDto();
            var parameters = await _context.ConnectorParameters.FirstOrDefaultAsync();

            var woocommerceService = new WoocommerceService(parameters.WooConsumerKey, parameters.WooConsumerSecret);
            var wooConnectionData = await _context.WooConnectionsData.FirstOrDefaultAsync(x => x.Id == connectorJob.WooConnectionDataSourceId);

            if (wooConnectionData != null)
            {
                var wooResponseItems = getResponseType(wooConnectionData.WooEntity);


                if (connectorJob.JobType == ConnectorJobTypeEnum.Receive)
                {
                    if (wooConnectionData.RequestType == RequestTypeEnum.Get)
                    {
                        var url = parameters.WooBaseUrl + wooConnectionData.Endpoint;

                        wooResponse = await woocommerceService.GetAsync(url);
                        if (wooResponse.Success)
                        {
                            try
                            {
                                JsonConvert.PopulateObject(wooResponse.Response, wooResponseItems);
                            }
                            catch (Exception ex)
                            {
                                //Log Exception
                            }

                        }
                        else
                        {
                            wooResponse.Response = DecodeResponse(wooResponse.Response);

                        }

                    }
                }
                else if (connectorJob.JobType == ConnectorJobTypeEnum.Transfer)
                {
                    if (wooConnectionData.RequestType == RequestTypeEnum.Post)
                    {
                       
                    }
                    else if (wooConnectionData.RequestType == RequestTypeEnum.Put)
                    {

                    }
                    else if (wooConnectionData.RequestType == RequestTypeEnum.Delete)
                    {

                    }
                }
            

            if (wooResponse != null)
            {
                apiResponse.Success = wooResponse.Success;
                apiResponse.StatusCode = wooResponse.StatusCode;
                apiResponse.Result = wooResponse.Success?wooResponseItems:wooResponse.Response;
                apiResponse.Message = wooResponse.Message;
                apiResponse.ExceptionMessage = wooResponse.ExceptionMessage;
            }
            }
            return apiResponse;

        }
        //public async Task GetProductsFromWoo(string url)
        //{
        //    var parameters = await _context.ConnectorParameters.FirstOrDefaultAsync();
        //    var woocommerceService = new WoocommerceService(parameters.WooConsumerKey, parameters.WooConsumerSecret);

        //    var result = await woocommerceService.GetAsync(url);

        //    var products = new List<WooProductDto>();

        //    JsonConvert.PopulateObject(result, products);

        //    await PostProductsToDb(products);
        //}

        //public async Task PostProductsToDb(List<WooProductDto> products)
        //{

        //    foreach (var product in products)
        //    {
        //        var dnProduct = new Product();

        //        dnProduct.Name = product.name;
        //        dnProduct.Description = product.description;
        //        dnProduct.ImagePath = product.images[0].src;
        //        dnProduct.Sku = product.sku;
        //        dnProduct.Price = product.price;

        //        _context.Products.Add(dnProduct);
        //    }
        //    _context.SaveChanges();


        //}

        private dynamic getResponseType(WooEntityEnum wooEntity)
        {
            if (wooEntity == WooEntityEnum.Products)
            {
                var list = new List<WooProductDto>();
                return list;

            }else if (wooEntity == WooEntityEnum.Attributes)
            {
                var list = new List<WooAttributeDto>();
                return list;

            }else if (wooEntity == WooEntityEnum.Categories)
            {
                var list = new List<WooCategoryDto>();
                return list;

            }
            else
            {
                return null;

            }
        }

        private string DecodeResponse(string data)
        {
            var splitted = Regex.Split(data, @"\\u([a-fA-F\d]{4})");
            string outString = "";
            foreach (var s in splitted)
            {
                try
                {
                    if (s.Length == 4)
                    {
                        var decoded = ((char)Convert.ToUInt16(s, 16)).ToString();
                        outString += decoded;
                    }
                    else
                    {
                        outString += s;
                    }
                }
                catch (Exception e)
                {
                    outString += s;
                }
            }
            return outString;
        }
    }
}
