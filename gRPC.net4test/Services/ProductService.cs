
using Google.Protobuf.WellKnownTypes;
using gRPC.net4test.Data;
using gRPC.net4test.Models;
using gRPC.net4test.Protos;
using Grpc.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Security.AccessControl;
using System.Threading.Tasks;

namespace gRPC.net4test.Services
{
    public class ProductService : ProductProtoService.ProductProtoServiceBase
    {   
        private readonly IConfiguration _configuration;
        private readonly ILogger<ProductService> _logger;
        private readonly DataManager dataManager = new DataManager();

        public ProductService() {
            _configuration = new ConfigurationBuilder().AddJsonFile("myConfig.json", optional: false, reloadOnChange: false).Build();
        }

        public bool ServiceInvalidTime() {

            var current = DateTime.Now.Hour;
            var start = int.Parse(_configuration["serviceStartTime"]);
            var end = int.Parse(_configuration["serviceEndTime"]);
            /*
            if (start < end)
            {
                return !(start <= current || current > end);
            }
            else {
                return (start <= current || current > end);
            }
            */
            return (start < end) ? !(start <= current || current > end) : (start <= current || current > end);
        }

        public override async Task GetProducts(GetProductsRequest request,
                                                    IServerStreamWriter<ProductModel> responseStream,
                                                    ServerCallContext context){
            if (ServiceInvalidTime())
            {
                throw new Exception("Service Invalid time");
            }
            else
            {
                var data = DateTime.ParseExact(request.Filter.CreatedTime, "dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                var productList = await dataManager.SearchProductsByDayAndSpeed(data.Date, request.Filter.Speed);
                foreach (var item in productList)
                {
                    await responseStream.WriteAsync(item);
                }
            }
        }

        public override async Task<ProductModel> AddProduct(AddProductRequest request, ServerCallContext context){
            var product = new Product();
            product.Speed = request.Product.Speed;
            product.Name = request.Product.Name;
            product.CreatedTime = DateTime.ParseExact(request.Product.CreatedTime, "dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture); //20.12.2019 14:31:25


            product  = await dataManager.AddProduct(product);
            return request.Product;
        }

        public override async Task GetMinMaxProducts(GetProductsRequest request,
                                                    IServerStreamWriter<ProductModel> responseStream,
                                                    ServerCallContext context){
            if (ServiceInvalidTime())
            {
                throw new Exception("Service Invalid time");
            }
            else
            {
                var data = DateTime.ParseExact(request.Filter.CreatedTime, "dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                var productList = await dataManager.SearchMinMaxProductsByDay(data.Date);
                foreach (var item in productList)
                {
                    await responseStream.WriteAsync(item);
                }
            }
        }
    }
}
