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

        // Constructor que recibe un repositorio genérico para manipular datos
        public ProductsManagementService(IRepository repository)
        {
            _repository = repository;
        }

        // Método para agregar un nuevo producto
        public async Task<ProductModel.Response> AddProduct(ProductModel.Request request)
        {
            // Validar que los datos obligatorios estén presentes y sean válidos
            if (string.IsNullOrWhiteSpace(request.Sku) ||
                string.IsNullOrWhiteSpace(request.Name) ||
                request.CurrentPrice < 0)
            {
                throw new ArgumentException("Valores para el producto no válidos");
            }

            // Verificar si ya existe un producto con el mismo SKU para evitar duplicados
            var exist = await _repository.First<Product>(p => p.Sku == request.Sku);
            if (exist != null) throw new DuplicatedEntityException($"Ya existe un producto con el Sku {request.Sku}");

            // Crear la entidad Producto con los datos recibidos
            var product = new Product(request.Sku, request.InternalCode, request.Name, request.Description, request.CurrentPrice, request.StockQuantity);

            // Agregar el producto al repositorio (persistir en base de datos)
            await _repository.Add(product);

            // Construir y devolver la respuesta con los datos del producto creado
            return new ProductModel.Response(product.Sku, product.InternalCode, product.Name, product.Description, product.CurrentPrice, product.StockQuantity, product.IsActive);
        }

        // Obtener todos los productos existentes
        public async Task<IEnumerable<Product>?> GetProducts()
        {
            return await _repository.GetAll<Product>();
        }

        // Obtener un producto por su Id
        public async Task<Product?> GetProductById(Guid id)
        {
            return await _repository.GetById<Product>(id);
        }

        // Actualizar datos de un producto existente
        public async Task<ProductModel.Response> Update(Guid Id, ProductModel.Request request)
        {
            // Validar los datos de entrada
            if (string.IsNullOrWhiteSpace(request.Sku) ||
                string.IsNullOrWhiteSpace(request.Name) ||
                request.CurrentPrice < 0)
            {
                throw new ArgumentException("Los datos enviados no son válidos.");
            }

            // Buscar el producto a actualizar
            var product = await _repository.GetById<Product>(Id);
            if (product == null)
            {
                return null; // No se encontró el producto
            }

            // Actualizar manualmente cada propiedad con los nuevos datos
            product.Sku = request.Sku;
            product.Name = request.Name;
            product.InternalCode = request.InternalCode;
            product.Description = request.Description;
            product.CurrentPrice = request.CurrentPrice;
            product.StockQuantity = request.StockQuantity;

            // Guardar cambios en base de datos
            await _repository.Update(product);

            // Construir y devolver la respuesta con los datos actualizados
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

        // Método para desactivar un producto (no eliminarlo)
        public async Task<bool> DisableProduct(Guid id)
        {
            // Paso 1: buscar el producto por Id
            var product = await _repository.GetById<Product>(id);
            if (product == null)
                return false; // No existe el producto

            // Paso 2: cambiar la propiedad IsActive a false para "desactivar"
            product.IsActive = false;

            // Paso 3: guardar cambios en el repositorio/base de datos
            await _repository.Update(product);

            return true; // Producto desactivado con éxito
        }
    }
}

