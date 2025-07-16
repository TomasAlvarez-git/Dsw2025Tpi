using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Exceptions;
using Dsw2025Tpi.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
            catch (Exception)
            {
                return Problem("Se produjo un error al guardar la orden");
            }
        }

    }
}
