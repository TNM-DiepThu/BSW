using BusinessLogicLayer.Viewmodels.ViewKH;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Text;

namespace PresentationLayer.Controllers
{
    public class CartMController : Controller
    {
        private readonly ILogger<CartMController> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public CartMController(ILogger<CartMController> logger, IHttpClientFactory httpClientFactory, HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
        }
        public IActionResult Index()
        {
            return View();
        }
        
        public async Task<IActionResult> AddToCart(Guid productId, int quantity)
        {

            try
            {


                if (string.IsNullOrEmpty(HttpContext.Session.GetString("JWT")))
                {
                    return RedirectToAction("Login", "Home");
                }
                var jwtToken = HttpContext.Session.GetString("JWT");
                if (string.IsNullOrEmpty(jwtToken))
                {
                    return RedirectToAction("Login", "Home");
                }
                var handler = new JwtSecurityTokenHandler();
                var token = handler.ReadJwtToken(jwtToken);
                var userId = token.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

                if (string.IsNullOrEmpty(userId))
                {
                    TempData["Error"] = "Invalid JWT token. Please log in again.";
                    return RedirectToAction("Login", "Home");
                }
                var request = new CartMVM
                {
                    Description = "CreateBy DiepThu",
                    IDUser = userId,
                    CartProductDetails = new List<CartProductDetailsViewModel>
                {
                    new CartProductDetailsViewModel
                    {
                        IDProductDetails = productId,
                        Quantity = quantity
                    }
                }
                };

                var json = JsonConvert.SerializeObject(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("https://localhost:7241/api/CartM/AddToCart", content);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("ListProduct", "Home");
                }
                else
                {
                    TempData["Error"] = "Failed to add item to cart.";
                    return RedirectToAction("Home", "Home");
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error adding item to cart: {ex.Message}";
                return RedirectToAction("Home", "Home");
            }
        }
        public async Task<IActionResult> GetAllCart(Guid id)
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("JWT")))
            {
                return RedirectToAction("Login", "Home");
            }
            var jwtToken = HttpContext.Session.GetString("JWT");
            if (string.IsNullOrEmpty(jwtToken))
            {
                return RedirectToAction("Login", "Home");
            }
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(jwtToken);
            var userId = token.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                TempData["Error"] = "Invalid JWT token. Please log in again.";
                return RedirectToAction("Login", "Home");
            }
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwtToken);

            var response = await client.GetAsync($"https://localhost:7241/api/CartM/GetCart/{userId}");
            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                var cartDetails = JsonConvert.DeserializeObject<List<GetCartDetailVM>>(jsonResponse);
                return View(cartDetails);
            }

            TempData["Error"] = "Unable to retrieve cart details. Please try again later.";
            return View(new List<GetCartDetailVM>());
        }

        [HttpPost]
        public async Task<IActionResult> UpdateCartItem(Guid id, int quantity)
        {
            

            

            return View();
        }

  
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
            var requestUri = $"https://localhost:7241/api/CartM/Delete/{id}";
            var response = await client.DeleteAsync(requestUri);

            if (response.IsSuccessStatusCode)
            {
                ViewBag.Message = "Cart item deleted successfully.";
            }
            else
            {
                ViewBag.Message = "Failed to delete cart item.";
            }
            }
            catch (Exception ex)
            {
                // Log the exception or handle it as needed
                ViewBag.Message = $"An error occurred: {ex.Message}";
            }
            return RedirectToAction("GetAllCart", "CartM");
        }
    }
}
