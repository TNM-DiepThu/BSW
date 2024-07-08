using BusinessLogicLayer.Services.Implements;
using BusinessLogicLayer.Services.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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
    }
}
