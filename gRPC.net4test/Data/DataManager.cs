using AutoMapper.Execution;
using Google.Protobuf.WellKnownTypes;
using gRPC.net4test.Models;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Runtime.ExceptionServices;
using gRPC.net4test.Protos;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace gRPC.net4test.Data
{
    public class DataManager
    {

        public DataManager() { 
            //configuration = new ConfigurationBuilder().AddJsonFile("myConfig.json", optional: false, reloadOnChange: false).Build(); 
        }


        //private readonly IConfiguration configuration;
        private String path = "D:\\data.txt";

        public async Task<List<ProductModel>> SearchProductsByDayAndSpeed(DateTime day, float speed)
        {
            var result = new List<ProductModel>();
            var products = SortListBySpeed((await SearchAllProducts())[day.Date]);
            var start = FirsIndexSearch(products, speed);
            if (start < products.Count -1 || products.Last().Speed > speed)
            {
                products.GetRange(start, products.Count -1);
                result = ProductsToProductsList(products);
            }
            return result;
        }

        public async Task<List<ProductModel>> SearchMinMaxProductsByDay(DateTime day){
            
            var result = new List<Product>();
            var products = SortListBySpeed(await SearchProductsByDay(day.Date));
            result.Add(products.FirstOrDefault());
            result.Add(products.LastOrDefault());
            return ProductsToProductsList(result);  
        }

        public async Task<List<Product>> SearchProductsByDay(DateTime day){
            return (await SearchAllProducts())[day.Date];
        }


        public async Task<Dictionary<DateTime, List<Product>>> SearchAllProducts(){ 
            var result = new Dictionary<DateTime, List<Product>>();
            var products = new List<Product>(); 

            
            var lines = (await File.ReadAllLinesAsync(path)).ToList();
           

            var product = new Product();
            foreach (string productLine in lines)
            {
                product = ProductFromSaveString(productLine);
                if (result!=null && result.ContainsKey(product.CreatedTime.Date)){
                    products = result[product.CreatedTime.Date];
                    result.Remove(product.CreatedTime.Date);
                }
                else{
                    products = new List<Product>();
                }
                products.Add(product);
                result.Add(product.CreatedTime.Date, products);
                
            }
            
            return result;
        }

        public async Task<Product> AddProduct(Product product) {

            var file = File.Exists(path) ? File.AppendText(path) : File.CreateText(path);
            await file.WriteLineAsync(ProductToSaveString(product));
            await file.FlushAsync();
            file.Close();
 
            return product;
        }




        private String ProductToSaveString(Product product){
            return product.CreatedTime.ToString("dd.MM.yyyy.HH.mm.ss") + " " + product.Speed.ToString() + " " + product.Name;  
        }

        private Product ProductFromSaveString(string text) {
            
            var product = new Product();

            var data = text.Split(" ", 3).ToList();

            product.Speed = float.Parse(data[1]); //CultureInfo.InvariantCulture.NumberFormat);
            product.Name = data[2];
            product.CreatedTime = DateTime.ParseExact(data[0], "dd.MM.yyyy.HH.mm.ss",CultureInfo.InvariantCulture);
            
            return product;
        }

        private List<Product> SortListBySpeed(List<Product> products) {
            return products.OrderBy(p => p.Speed).ToList();
        }

        private int FirsIndexSearch(List<Product> products, float speed){ 
            var i = 0;
            for(i =0; i < products.Count; i++){
                if (products[i].Speed > speed) {
                    break;
                }
            }
            return i;
        }

        private List<ProductModel> ProductsToProductsList(List<Product> products)
        {
            var result = new List<ProductModel>();
            if (products.Count > 0)
            {
                foreach (var product in products)
                {
                    var productmodel = new ProductModel();
                    productmodel.Name = product.Name;
                    productmodel.Speed = product.Speed;
                    productmodel.CreatedTime = product.CreatedTime.ToString("dd.MM.yyyy HH:mm:ss");
                    result.Add(productmodel);
                }
            }
            return result;
        }
    }
}
