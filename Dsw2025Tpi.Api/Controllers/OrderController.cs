using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Exceptions;
using Dsw2025Tpi.Application.Services;
using Dsw2025Tpi.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Dsw2025Tpi.Application.Dtos.OrderModel;

namespace Dsw2025Tpi.Api.Controllers
{
    // Define que esta clase es un controlador API
    [ApiController]

    // Requiere autorización por defecto para todos los métodos
    [Authorize]

    // Ruta base para todos los endpoints del controlador
    [Route("api/order")]
    public class OrderController : ControllerBase
    {
        // Servicio que contiene la lógica de negocio para la gestión de órdenes
        private readonly OrdersManagementService _service;

        // Constructor con inyección del servicio de órdenes
        public OrderController(OrdersManagementService service)
        {
            _service = service;
        }

        // Endpoint para que un cliente cree una nueva orden
        [HttpPost()]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> AddOrder([FromBody] OrderModel.Request request)
        {
            try
            {
                // Crea una orden y la guarda en base de datos
                var order = await _service.AddOrder(request);

                // Devuelve 201 Created con la ruta del nuevo recurso
                return Created($"/api/order/{order.Id}", order);
            }
            catch (ArgumentException ae)
            {
                // Si hay datos inválidos en el cuerpo de la solicitud
                return BadRequest(ae.Message);
            }
            catch (DuplicatedEntityException de)
            {
                // Si ya existe una orden duplicada (ejemplo hipotético)
                return Conflict(de.Message);
            }
            catch (Exception e)
            {
                // Cualquier error inesperado del servidor
                return StatusCode(500, $"Se produjo un error en el servidor {e.Message}");
            }
        }

        // Endpoint para obtener una lista paginada de órdenes (admin y cliente)
        [HttpGet]
        [Authorize(Roles = "Admin, Customer")]
        public async Task<IActionResult> GetOrders(
           [FromQuery] OrderStatus? status,         // Filtrar por estado opcional
           [FromQuery] Guid? customerId,            // Filtrar por cliente opcional
           [FromQuery] int pageNumber = 1,          // Paginación: número de página
           [FromQuery] int pageSize = 10)           // Paginación: cantidad por página
        {
            try
            {
                // Obtiene las órdenes desde el servicio
                var orders = await _service.GetOrders(status, customerId, pageNumber, pageSize);

                // Si no hay resultados, retorna 204 No Content
                if (orders == null || !orders.Any())
                    return NoContent();

                // Devuelve 200 OK con la lista de órdenes
                return Ok(orders);
            }
            catch (Exception e)
            {
                // Manejo de errores generales
                return StatusCode(500, $"Se produjo un error en el servidor {e.Message}");
            }
        }

        // Endpoint para obtener los detalles de una orden por ID
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin, Customer")]
        public async Task<IActionResult> GetOrderById(Guid id)
        {
            try
            {
                // Busca la orden por su identificador
                var order = await _service.GetOrderById(id);
                if (order == null) return NotFound(); // 404 si no existe

                // Devuelve los datos de la orden, incluyendo los ítems y el total
                var result = new
                {
                    order.Id,
                    order.CustomerId,
                    order.ShippingAddress,
                    order.BillingAddress,
                    Date = order.Date.ToString("dd/MM/yyyyTHH:mm:ss"),
                    order.Status,
                    OrderItems = order.Items.Select(oi => new
                    {
                        oi.ProductId,
                        oi.Quantity,
                        oi.UnitPrice,
                        oi.Subtotal
                    }),
                    Total = order.TotalAmount
                };

                return Ok(result);
            }
            catch (Exception e)
            {
                return StatusCode(500, $"Se produjo un error en el servidor {e.Message}");
            }
        }

        // Endpoint para actualizar el estado de una orden (solo admin)
        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateOrderStatus(Guid id, [FromBody] UpdateOrderStatusRequest request)
        {
            try
            {
                // Cambia el estado de la orden utilizando el servicio
                var updatedOrder = await _service.UpdateOrderStatus(id, request.newStatus);

                // Si no se encontró la orden, retorna 404
                if (updatedOrder == null) return NotFound();

                // Devuelve los datos actualizados de la orden
                return Ok(updatedOrder);
            }
            catch (ArgumentException ae)
            {
                // Retorna 400 si el nuevo estado es inválido
                return BadRequest(ae.Message);
            }
            catch (Exception e)
            {
                return StatusCode(500, $"Se produjo un error en el servidor {e.Message}");
            }
        }
    }
}
