using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Exceptions;
using Dsw2025Tpi.Application.Services;
using Dsw2025Tpi.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dsw2025Tpi.Api.Controllers
{

    [ApiController]
    [Authorize]
    [Route("api/products")]
    public class ProductsController : ControllerBase
    {
        private readonly ProductsManagementService _service;

        public ProductsController(ProductsManagementService service)
        {
            _service = service;
        }


        [HttpPost()]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddProduct([FromBody] ProductModel.Request request)
        {
            try
            {
                var product = await _service.AddProduct(request);
                return Created($"/api/products/{product.Sku}", product);
            }
            catch (ArgumentException ae)
            {
                return BadRequest(ae.Message);
            }
            catch (DuplicatedEntityException de)
            {
                return Conflict(de.Message);
            }
            catch (Exception e)
            {
                return StatusCode(500,$"Se produjo un error interno del servidor: {e.Message}");
            }
        }

        [HttpGet()]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetProducts()
        {
            try
            {
                var products = await _service.GetProducts();
                if (products == null || !products.Any()) return NoContent();
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
                return Ok(result);
            }
            catch(Exception e)
            { 
                return StatusCode(500, $"Se produjo un error al obtener los productos{e.Message}");
            }
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetProductBySku(Guid id)
        {
            try
            {
                var product = await _service.GetProductById(id);
                if (product == null) return NotFound();

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

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateProduct(Guid id, [FromBody] ProductModel.Request request)
        {
            try
            {
                var updatedProduct = await _service.Update(id, request);

                if (updatedProduct == null)
                    return NotFound($"No se encontró un producto con el ID {id}"); //404 

                return Ok(updatedProduct); // 200 OK con el objeto actualizado
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message); // 400 Bad Request por datos inválidos
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}"); // 500 Internal Server Error
            }

        }

        [HttpPatch("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DisableProduct(Guid id)
        {
            try
            {
                var success = await _service.DisableProduct(id);

                if (!success)
                    return NotFound($"No se encontró un producto con el ID {id}");

                return NoContent(); // 204 No Content
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

    }
}
