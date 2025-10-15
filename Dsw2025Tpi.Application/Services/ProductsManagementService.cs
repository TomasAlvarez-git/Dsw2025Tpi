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
using Dsw2025Tpi.Application.Helpers;
using Dsw2025Tpi.Application.Interfaces;

namespace Dsw2025Tpi.Application.Services
{
    public class ProductsManagementService : IProductsManagementService
    {
        private readonly IRepository _repository;
        private readonly ILogger<ProductsManagementService> _logger;
        private readonly ProductsManagmentServiceExtensions _extensions;

        // Constructor con inyección de dependencias
        public ProductsManagementService(IRepository repository, ILogger<ProductsManagementService> logger, ProductsManagmentServiceExtensions extensions)
        {
            _repository = repository;
            _logger = logger;
            _extensions = extensions;
        }

        // Crea un nuevo producto a partir de la solicitud recibida
        public async Task<ProductModel.Response> AddProduct(ProductModel.Request request)
        {
            _logger.LogInformation("Iniciando creación de producto con SKU: {Sku}", request.Sku);

            // Validación del request
             _extensions.ValidateProductRequest(request);

            // Verifica duplicados
            await _extensions.ValidateDuplicatedProductAsync(request);

            // Crea la entidad y la guarda
            var product = new Product(request.Sku, request.InternalCode, request.Name, request.Description, request.CurrentPrice, request.StockQuantity);
            await _repository.Add(product);

            _logger.LogInformation("Producto creado correctamente: {Sku}", product.Sku);

            // Devuelve la respuesta con los datos del nuevo producto
            return new ProductModel.Response(product.Sku, product.InternalCode, product.Name, product.Description, product.CurrentPrice, product.StockQuantity, product.IsActive);
        }

        // Obtiene la lista de todos los productos existentes
        public async Task<IEnumerable<Product>?> GetProducts()
        {
            _logger.LogInformation("Obteniendo lista de todos los productos");

            var products = await _repository.GetAll<Product>();

            // Verifica si hay productos cargados
             _extensions.ValidateProductsNull(products);

            _logger.LogInformation("Se encontraron {Count} productos", products.Count());

            return products;
        }

        // Obtiene un producto por su ID
        public async Task<Product?> GetProductById(Guid id)
        {
            _logger.LogInformation("Buscando producto por ID: {Id}", id);

            // Lanza excepción si no se encuentra
            var product = await _extensions.GetProductOrThrow(id);

            _logger.LogInformation("Producto encontrado: {Sku}", product.Sku);

            return product;
        }

        // Actualiza los datos de un producto
        public async Task<ProductModel.Response> Update(Guid Id, ProductModel.Request request)
        {
            _logger.LogInformation("Actualizando producto con ID: {Id}", Id);

             _extensions.ValidateProductRequest(request);

            var product = await _repository.GetById<Product>(Id);
            _extensions.ValidateProductNull(product);

            await _extensions.ValidateDuplicatedProductUpdateAsync(request, Id);

            // Aplica los cambios
            product.Update(request.Sku, request.InternalCode, request.Name, request.Description, request.CurrentPrice, request.StockQuantity);
            await _repository.Update(product);

            _logger.LogInformation("Producto actualizado correctamente. SKU: {Sku}", product.Sku);

            return _extensions.ToResponse(product);
        }

        // Desactiva (soft delete) un producto existente
        public async Task<bool> DisableProduct(Guid Id)
        {
            _logger.LogInformation("Desactivando producto con ID: {Id}", Id);

            var product = await _repository.GetById<Product>(Id);
            _extensions.ValidateProductNull(product);

            // Cambia el estado a inactivo
            product.IsActive = false;
            await _repository.Update(product);

            _logger.LogInformation("Producto desactivado correctamente. SKU: {Sku}", product.Sku);
            return true;
        }
    }
}

