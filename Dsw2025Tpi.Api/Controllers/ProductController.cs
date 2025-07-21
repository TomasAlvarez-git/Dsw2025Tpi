using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Exceptions;
using Dsw2025Tpi.Application.Services;
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
        private readonly ProductsManagementService _service;

        // Constructor con inyección de dependencia del servicio
        public ProductsController(ProductsManagementService service)
        {
            _service = service;
        }

        // === POST: api/products ===
        // Agrega un nuevo producto (solo rol Admin)
        [HttpPost()]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddProduct([FromBody] ProductModel.Request request)
        {
            try
            {
                // Agrega el producto usando el servicio
                var product = await _service.AddProduct(request);

                // Retorna 201 Created con la ubicación del nuevo producto
                return Created($"/api/products/{product.Sku}", product);
            }
            catch (ArgumentException ae)
            {
                // Retorna 400 Bad Request si hay errores de validación
                return BadRequest(ae.Message);
            }
            catch (DuplicatedEntityException de)
            {
                // Retorna 409 Conflict si ya existe el producto
                return Conflict(de.Message);
            }
            catch (Exception e)
            {
                // Retorna 500 en caso de error inesperado
                return StatusCode(500, $"Se produjo un error interno del servidor: {e.Message}");
            }
        }

        // === GET: api/products ===
        // Devuelve todos los productos (solo Admin)
        [HttpGet()]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetProducts()
        {
            try
            {
                // Obtiene la lista de productos
                var products = await _service.GetProducts();

                // Si no hay productos, devuelve 204 No Content
                if (products == null || !products.Any()) return NoContent();

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
            catch (Exception e)
            {
                // Retorna 500 si hay un error en el servidor
                return StatusCode(500, $"Se produjo un error al obtener los productos: {e.Message}");
            }
        }

        // === GET: api/products/{id} ===
        // Devuelve un producto por su ID (solo Admin)
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetProductBySku(Guid id)
        {
            try
            {
                // Busca el producto por su ID
                var product = await _service.GetProductById(id);

                // Si no se encuentra, retorna 404 Not Found
                if (product == null) return NotFound();

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
            catch (Exception e)
            {
                return StatusCode(500, $"Se produjo un error al obtener el producto: {e.Message}");
            }
        }

        // === PUT: api/products/{id} ===
        // Actualiza todos los campos del producto (solo Admin)
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateProduct(Guid id, [FromBody] ProductModel.Request request)
        {
            try
            {
                // Llama al servicio para actualizar el producto
                var updatedProduct = await _service.Update(id, request);

                // Si no se encuentra, retorna 404
                if (updatedProduct == null)
                    return NotFound($"No se encontró un producto con el ID {id}");

                // Retorna 200 OK con el producto actualizado
                return Ok(updatedProduct);
            }
            catch (ArgumentException ex)
            {
                // Error de validación del modelo (400 Bad Request)
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                // Error interno del servidor
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        // === PATCH: api/products/{id} ===
        // Deshabilita (soft delete) un producto (solo Admin)
        [HttpPatch("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DisableProduct(Guid id)
        {
            try
            {
                // Llama al servicio para deshabilitar el producto
                var success = await _service.DisableProduct(id);

                // Si no se encuentra el producto, retorna 404
                if (!success)
                    return NotFound($"No se encontró un producto con el ID {id}");

                // Si se deshabilitó correctamente, retorna 204 sin contenido
                return NoContent();
            }
            catch (Exception ex)
            {
                // Error inesperado del servidor
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }
    }
}
