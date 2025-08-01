using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Exceptions;
using Dsw2025Tpi.Application.Interfaces;
using Dsw2025Tpi.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dsw2025Tpi.Api.Controllers
{
    // Indica que esta clase es un controlador de API
    [ApiController]

    // Requiere autenticación en todos los métodos por defecto
    [Authorize]

    // Ruta base para los endpoints de este controlador
    [Route("api/products")]
    public class ProductsController : ControllerBase
    {
        // Servicio de lógica de productos
        private readonly IProductsManagementService _service;

        // Constructor con inyección de dependencia del servicio
        public ProductsController(IProductsManagementService service)
        {
            _service = service;
        }

        // === POST: api/products ===
        // Agrega un nuevo producto (solo rol Admin)
        [HttpPost()]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddProduct([FromBody] ProductModel.Request request)
        {
            // Agrega el producto usando el servicio
            var product = await _service.AddProduct(request);

            // Retorna 201 Created con la ubicación del nuevo producto
            return Created($"/api/products/{product.Sku}", product);
        }

        // === GET: api/products ===
        // Devuelve todos los productos (solo Admin)
        [HttpGet()]
        [AllowAnonymous]
        public async Task<IActionResult> GetProducts()
        {
            // Obtiene la lista de productos
            var products = await _service.GetProducts();

            // Mapea los campos que se quieren exponer
            var result = products.Select(p => new
            {
                p.Id,
                p.Sku,
                p.InternalCode,
                p.Name,
                p.Description,
                p.CurrentPrice,
                p.StockQuantity,
                p.IsActive
            });

            // Retorna 200 OK con la lista de productos
            return Ok(result);
        }

        // === GET: api/products/{id} ===
        // Devuelve un producto por su ID (solo Admin)
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetProductBySku(Guid id)
        {
            // Busca el producto por su ID
            var product = await _service.GetProductById(id);

            // Retorna los campos del producto
            var result = new
            {
                product.Id,
                product.Sku,
                product.InternalCode,
                product.Name,
                product.Description,
                product.CurrentPrice,
                product.StockQuantity,
                product.IsActive
            };

            return Ok(result);
        }

        // === PUT: api/products/{id} ===
        // Actualiza todos los campos del producto (solo Admin)
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        
        public async Task<IActionResult> UpdateProduct(Guid id, [FromBody] ProductModel.Request request)
        {
            // Llama al servicio para actualizar el producto
            var updatedProduct = await _service.Update(id, request);

            // Retorna 200 OK con el producto actualizado
            return Ok(updatedProduct);
        }

        // === PATCH: api/products/{id} ===
        // Deshabilita (soft delete) un producto (solo Admin)
        [HttpPatch("{id}")]
        [Authorize(Roles = "Admin")]
        
        public async Task<IActionResult> DisableProduct(Guid id)
        {
            // Llama al servicio para deshabilitar el producto
            var success = await _service.DisableProduct(id);

            // Si se deshabilitó correctamente, retorna 204 sin contenido
            return NoContent();
        }
    }
}
