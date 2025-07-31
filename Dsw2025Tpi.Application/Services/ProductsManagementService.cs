using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Exceptions;
using Dsw2025Tpi.Domain.Entities;
using Dsw2025Tpi.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Dsw2025Tpi.Application.Services
{
    public class ProductsManagementService
    {
        private readonly IRepository _repository;
        private readonly ILogger<ProductsManagementService> _logger;

        public ProductsManagementService(IRepository repository, ILogger<ProductsManagementService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<ProductModel.Response> AddProduct(ProductModel.Request request)
        {
            _logger.LogInformation("Iniciando creación de producto con SKU: {Sku}", request.Sku);

            if (string.IsNullOrWhiteSpace(request.Sku) ||
                string.IsNullOrWhiteSpace(request.Name) ||
                request.CurrentPrice < 0 || request.StockQuantity < 0)
            {
                _logger.LogWarning("Datos inválidos para producto: {@Request}", request);
                throw new BadRequestException("Valores para el producto no válidos");
            }

            var exist = await _repository.First<Product>(p => p.Sku == request.Sku);
            if (exist != null)
            {
                _logger.LogWarning("Intento de duplicación de producto con SKU existente: {Sku}", request.Sku);
                throw new DuplicatedEntityException($"Ya existe un producto con el Sku {request.Sku}");
            }

            var product = new Product(request.Sku, request.InternalCode, request.Name, request.Description, request.CurrentPrice, request.StockQuantity);
            await _repository.Add(product);

            _logger.LogInformation("Producto creado correctamente: {Sku}", product.Sku);

            return new ProductModel.Response(product.Sku, product.InternalCode, product.Name, product.Description, product.CurrentPrice, product.StockQuantity, product.IsActive);
        }

        public async Task<IEnumerable<Product>?> GetProducts()
        {
            _logger.LogInformation("Obteniendo lista de todos los productos");

            var products = await _repository.GetAll<Product>();

            if (products == null || !products.Any())
            {
                _logger.LogWarning("No hay productos disponibles en la base de datos");
                throw new NoContentException("No hay productos disponibles en la base de datos.");
            }

            _logger.LogInformation("Se encontraron {Count} productos", products.Count());
            return products;
        }

        public async Task<Product?> GetProductById(Guid id)
        {
            _logger.LogInformation("Buscando producto por ID: {Id}", id);

            var product = await _repository.GetById<Product>(id);
            if (product == null)
            {
                _logger.LogWarning("Producto no encontrado con ID: {Id}", id);
                throw new NotFoundException("El producto solicitado no existe.");
            }

            _logger.LogInformation("Producto encontrado: {Sku}", product.Sku);
            return product;
        }

        public async Task<ProductModel.Response> Update(Guid Id, ProductModel.Request request)
        {
            _logger.LogInformation("Actualizando producto con ID: {Id}", Id);

            if (string.IsNullOrWhiteSpace(request.Sku) ||
                string.IsNullOrWhiteSpace(request.Name) ||
                request.CurrentPrice < 0 || request.StockQuantity < 0)
            {
                _logger.LogWarning("Datos inválidos para actualización: {@Request}", request);
                throw new BadRequestException("Los datos enviados no son válidos.");
            }

            var product = await _repository.GetById<Product>(Id);
            if (product == null)
            {
                _logger.LogWarning("Producto no encontrado para actualizar. ID: {Id}", Id);
                throw new NotFoundException($"No se encontro un producto con el ID: {Id}.");
            }

            product.Sku = request.Sku;
            product.Name = request.Name;
            product.InternalCode = request.InternalCode;
            product.Description = request.Description;
            product.CurrentPrice = request.CurrentPrice;
            product.StockQuantity = request.StockQuantity;

            await _repository.Update(product);

            _logger.LogInformation("Producto actualizado correctamente. SKU: {Sku}", product.Sku);

            return new ProductModel.Response(
                product.Sku,
                product.Name,
                product.InternalCode,
                product.Description,
                product.CurrentPrice,
                product.StockQuantity,
                product.IsActive
            );
        }

        public async Task<bool> DisableProduct(Guid Id)
        {
            _logger.LogInformation("Desactivando producto con ID: {Id}", Id);

            var product = await _repository.GetById<Product>(Id);
            if (product == null)
            {
                _logger.LogWarning("Producto no encontrado para desactivar. ID: {Id}", Id);
                throw new NotFoundException($"No se encontro un producto con el Id: {Id}.");
            }

            product.IsActive = false;
            await _repository.Update(product);

            _logger.LogInformation("Producto desactivado correctamente. SKU: {Sku}", product.Sku);
            return true;
        }
    }

}

