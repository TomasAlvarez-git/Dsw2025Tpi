using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Exceptions;
using Dsw2025Tpi.Application.Services;
using Dsw2025Tpi.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Dsw2025Tpi.Application.Dtos.OrderModel;

namespace Dsw2025Tpi.Api.Controllers
{

    [ApiController]
    [Authorize]
    [Route("api/order")]

    public class OrderController : ControllerBase
    {
        private readonly OrdersManagementService _service;

        public OrderController(OrdersManagementService service)
        {
            _service = service;
        }

        [HttpPost()]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> AddOrder([FromBody] OrderModel.Request request)
        {
            try
            {
                var order = await _service.AddOrder(request);
                return Created($"/api/order/{order.Id}", order);
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
                return StatusCode(500, $"Se produjo un error en el servidor {e.Message}");
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> GetOrders(
           [FromQuery] OrderStatus? status,
           [FromQuery] Guid? customerId,
           [FromQuery] int pageNumber = 1,
           [FromQuery] int pageSize = 10)
        {
            try
            {
                var orders = await _service.GetOrders(status, customerId, pageNumber, pageSize);
                if (orders == null || !orders.Any())
                    return NoContent();

                return Ok(orders);
            }
            catch (Exception e)
            {
                return StatusCode(500, $"Se produjo un error en el servidor {e.Message}");
            }
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> GetOrderById(Guid id)
        {
            try
            {
                var order = await _service.GetOrderById(id);
                if (order == null) return NotFound();

                var result = new
                {
                    order.Id,
                    order.CustomerId,
                    order.ShippingAddress,
                    order.BillingAddress,
                    order.Date,
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

        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin")]

        public async Task<IActionResult> UpdateOrderStatus(Guid id, [FromBody] UpdateOrderStatusRequest request)
        {
            try
            {
                var updatedOrder = await _service.UpdateOrderStatus(id, request.newStatus);
                if (updatedOrder == null) return NotFound();
                return Ok(updatedOrder);
            }
            catch (ArgumentException ae)
            {
                return BadRequest(ae.Message);
            }
            catch (Exception e)
            {
                return StatusCode(500, $"Se produjo un error en el servidor {e.Message}");
            }
        }


    }
}
