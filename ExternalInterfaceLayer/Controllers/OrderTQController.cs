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
        public OrderTQController(IOrderTQServiece orderTQ)
        {
            _order = orderTQ;
        }
        [HttpGet]
        [Route("SeachProduct")]
        public async Task<IActionResult> Search(string search)
        {
            var products = await _order.GetProductTQVMs(search);
            return Ok(products);
        }
    }
}
