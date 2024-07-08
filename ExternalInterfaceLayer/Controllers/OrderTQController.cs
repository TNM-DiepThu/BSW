using BusinessLogicLayer.Services.Implements;
using BusinessLogicLayer.Services.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static DataAccessLayer.Entity.Base.EnumBase;

namespace ExternalInterfaceLayer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderTQController : ControllerBase
    {
        private readonly IOrderTQServiece _order;
        private readonly IOrderService _orderService;
        public OrderTQController(IOrderTQServiece orderTQ,IOrderService orderService)
        {
            _order = orderTQ;
            _orderService = orderService;
        }
        [HttpGet]
        [Route("SeachProduct")]
        public async Task<IActionResult> Search(string search)
        {
            var products = await _order.GetProductTQVMs(search);
            return Ok(products);
        }
        [HttpGet]
        [Route("GetAllOrder")]
        public async Task<IActionResult> GetAll()
        {
            var order = await _orderService.GetAllAsync();
            return Ok(order);   
        }
        [HttpGet]
        [Route("GetById/{id}")]
        public async Task<IActionResult> GetOrderById(Guid id)
        {
            var order = await _orderService.GetByIDAsync(id);
            if (order == null)
            {
                return NotFound();
            }
            return Ok(order);
        }
        [HttpPost]
        [Route("UpdateOrderStatus")]
        public async Task<IActionResult> UpdateOrderStatus(Guid id, OrderStatus status)
        {
            var success = await _order.UpdateOrderStatusAsync(id, status);

            if (!success)
            {
                return BadRequest(new { success = false, message = "Cập nhật trạng thái đơn hàng không thành công" });
            }

            return Ok(new { success = true });
        }
    }
}
