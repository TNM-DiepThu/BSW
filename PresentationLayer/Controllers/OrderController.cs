﻿using BusinessLogicLayer.Viewmodels.Order;
using BusinessLogicLayer.Viewmodels.OrderDetails;
using BusinessLogicLayer.Viewmodels.OrderM;
using BusinessLogicLayer.Viewmodels.ViewKH;
using DataAccessLayer.Entity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Extensions;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace PresentationLayer.Controllers
{
    public class OrderController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public OrderController(IHttpClientFactory clientFactory, HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _httpClientFactory = clientFactory;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<IActionResult> Index()
        {
            var httpClient = _httpClientFactory.CreateClient();
            var apiUrl = $"https://localhost:7241/api/IViewKH/GetAllTQ";
            var response = await httpClient.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {

                var products = await response.Content.ReadFromJsonAsync<List<ProductTQVM>>();
                var selectedProductsJson = HttpContext.Session.GetString("SelectedProducts");
                var selectedProducts = string.IsNullOrEmpty(selectedProductsJson)
                    ? new List<ProductTQVM>()
                    : JsonConvert.DeserializeObject<List<ProductTQVM>>(selectedProductsJson);

                var orderMVM = new OrderMVM
                {
                    lstproduct = products,
                    SelectedProducts = selectedProducts
                };

                // Lưu danh sách sản phẩm vào TempData để sử dụng trong AddToSelectedProducts
                TempData["Products"] = JsonConvert.SerializeObject(products);
                return View(orderMVM);

            }
            else
            {
                return StatusCode((int)response.StatusCode);
            }
        }

        public IActionResult AddToSelectedProducts(Guid id)
        {
            var selectedProductsJson = HttpContext.Session.GetString("SelectedProducts");
            var selectedProducts = string.IsNullOrEmpty(selectedProductsJson)
                ? new List<ProductTQVM>()
                : JsonConvert.DeserializeObject<List<ProductTQVM>>(selectedProductsJson);

            var productsJson = TempData["Products"] as string;
            if (string.IsNullOrEmpty(productsJson))
            {
                // Xử lý lỗi nếu không tìm thấy danh sách sản phẩm
                return BadRequest("Danh sách sản phẩm không tìm thấy.");
            }

            var products = JsonConvert.DeserializeObject<List<ProductTQVM>>(productsJson);
            var productToAdd = products.FirstOrDefault(p => p.Id == id);

            if (productToAdd != null && !selectedProducts.Any(p => p.Id == id))
            {
                selectedProducts.Add(productToAdd);
            }

            HttpContext.Session.SetString("SelectedProducts", JsonConvert.SerializeObject(selectedProducts));
            return RedirectToAction("Index");
        }
        public async Task<IActionResult> CreateOrder(OrderMVM order)
        {
            try
            {
                var jwtToken = HttpContext.Session.GetString("JWT");
                if (string.IsNullOrEmpty(jwtToken))
                {
                    return RedirectToAction("Login", "Home");
                }
                var handler = new JwtSecurityTokenHandler();
                var token = handler.ReadJwtToken(jwtToken);
                var userId = token.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;
                var request = new OrderCreateVM
                {
                    CreateBy = userId.ToString(),
                    CustomerName = order.CreateVM.CustomerName,
                    CustomerPhone = order.CreateVM.CustomerPhone,
                    CustomerEmail = order.CreateVM.CustomerEmail,
                    ShippingAddress = order.CreateVM.ShippingAddress,
                    TotalAmount = order.CreateVM.TotalAmount,
                    Cotsts = order.CreateVM.Cotsts,
                    VoucherCode = order.CreateVM.VoucherCode,
                    Notes = order.CreateVM.Notes,
                    PaymentMethods = order.CreateVM.PaymentMethods,
                    PaymentStatus = order.CreateVM.PaymentStatus,
                    ShippingMethods = order.CreateVM.ShippingMethods,
                    OrderStatus = order.CreateVM.OrderStatus,
                    Status = order.CreateVM.Status,
                    OrderDetailsCreateVM = new List<OrderDetailsCreateVM> {
                        new OrderDetailsCreateVM
                        {
                            CreateBy = userId.ToString(),
                            IDOptions = order.CreateVMDetails.IDOptions,
                            Quantity = order.CreateVMDetails.Quantity,
                            UnitPrice = order.CreateVMDetails.UnitPrice,
                            Discount = order.CreateVMDetails.Discount,
                        }

                    }
                };
                var json = JsonConvert.SerializeObject(order);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("https://localhost:7241/api/Order/create", content);
                if (response.IsSuccessStatusCode)
                {
                    return View();
                }
                else
                {
                    return View();
                }

            }
            catch (Exception ex)
            {
                return View("OrderError");
            }
        }
    }
}
