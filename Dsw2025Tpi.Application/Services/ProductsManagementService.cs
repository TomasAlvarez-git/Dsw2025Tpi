using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Exceptions;
using Dsw2025Tpi.Domain.Entities;
using Dsw2025Tpi.Domain.Interfaces;

namespace Dsw2025Tpi.Application.Services
{
    public class ProductsManagementService
    {
        private readonly IRepository _repository;

        public ProductsManagementService(IRepository repository)
        {
            _repository = repository;
        }

        public async Task<ProductModel.Response> AddProduct(ProductModel.Request request)
        {
            if (string.IsNullOrWhiteSpace(request.Sku) ||
                string.IsNullOrWhiteSpace(request.Name) ||
                request.CurrentPrice < 0)
            {
                throw new ArgumentException("Valores para el producto no válidos");
            }

            var exist = await _repository.First<Product>(p => p.Sku == request.Sku);
            if (exist != null) throw new DuplicatedEntityException($"Ya existe un producto con el Sku {request.Sku}");

            var product = new Product(request.Sku, request.Name, request.internalCode, request.Description, request.CurrentPrice, request.StockQuantity);
            await _repository.Add(product);
            return new ProductModel.Response(product.Sku, product.Name, product.InternalCode, product.Description, product.CurrentPrice, product.StockQuantity);
        }

        public async Task<IEnumerable<Product>?> GetProducts()
        {
            return await _repository.GetAll<Product>();
        }

        public async Task<Product?> GetProductById(Guid id)
        {
            return await _repository.GetById<Product>(id);
        }

    }
}
