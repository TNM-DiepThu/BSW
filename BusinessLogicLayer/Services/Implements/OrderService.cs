using AutoMapper;
using BusinessLogicLayer.Services.Interface;
using BusinessLogicLayer.Viewmodels.Order;
using BusinessLogicLayer.Viewmodels.OrderDetails;
using DataAccessLayer.Application;
using DataAccessLayer.Entity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DataAccessLayer.Entity.Base.EnumBase;
using static DataAccessLayer.Entity.Voucher;

namespace BusinessLogicLayer.Services.Implements
{
    public class OrderService : IOrderService
    {
        private readonly ApplicationDBContext _dbcontext;
        private readonly IMapper _mapper;
        public OrderService(ApplicationDBContext ApplicationDBContext, IMapper mapper)
        {
            _dbcontext = ApplicationDBContext;
            _mapper = mapper;
        }
        public async Task<bool> CreateAsync(OrderCreateVM request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }
            using var transaction = await _dbcontext.Database.BeginTransactionAsync();
            try
            {
                string defaultUserID = "";
                var order = _mapper.Map<Order>(request);
                order.ID = Guid.NewGuid();
                order.CreateBy = request.CreateBy;
                order.CreateDate = DateTime.Now;
                order.IDUser = request.IDUser ?? defaultUserID;
                order.CustomerName = request.CustomerName;
                order.CustomerPhone = request.CustomerPhone;
                order.CustomerEmail = request.CustomerEmail;
                order.OrderStatus = OrderStatus.Pending;
                order.ShipDate = request.ShipDate;
                order.ShippingAddress = request.ShippingAddress;
                order.ShippingAddressLine2 = request.ShippingAddressLine2;
                order.TrackingCheck = false;
                order.VoucherCode = request.VoucherCode;
                order.PaymentMethods = request.PaymentMethods;
                order.PaymentStatus = request.PaymentStatus;
                order.ShippingMethods = request.ShippingMethods;
                order.Status = 1;
                var orderDetailsList = new List<OrderDetails>();
                decimal totalAmount = 0;

                foreach (var directItem in request.OrderDetailsCreateVM)
                {
                    // Tìm IdOption từ IdProduct
                    var option = await _dbcontext.Options.FirstOrDefaultAsync(o => o.IDProductDetails == directItem.IDOptions);

                    if (option != null)
                    {
                        var ordervariant = new OrderDetails()
                        {
                            ID = Guid.NewGuid(),
                            IDOrder = order.ID,
                            IDOptions = option.ID, // Sử dụng IdOption từ bảng Option
                            UnitPrice = option.RetailPrice,
                            Quantity = directItem.Quantity,
                            TotalAmount = (option.RetailPrice * directItem.Quantity) - ((option.Discount ?? 0) * directItem.Quantity),
                            Status = 1,
                            CreateBy = option.CreateBy,
                            CreateDate = DateTime.Now
                        };
                        orderDetailsList.Add(ordervariant);
                        totalAmount += (ordervariant.UnitPrice * ordervariant.Quantity) - ((option.Discount ?? 0) * ordervariant.Quantity);

                        // Giảm số lượng sản phẩm trong kho
                        bool stockUpdated = await CheckAndReduceStock(option.ID, directItem.Quantity);
                        if (!stockUpdated)
                        {
                            await transaction.RollbackAsync();
                            return false;
                        }
                    }
                    else
                    {
                        await transaction.RollbackAsync();
                        return false; // Nếu không tìm thấy Option, rollback và trả về false
                    }
                }

                // Xử lý voucher nếu có
                if (!string.IsNullOrWhiteSpace(request.VoucherCode))
                {
                    var voucher = await _dbcontext.Voucher.FirstOrDefaultAsync(v => v.Code == request.VoucherCode);
                    if (voucher == null || !voucher.IsActive || voucher.Quantity <= 0)
                    {
                        await transaction.RollbackAsync();
                        return false;
                    }

                    var discountAmount = CalculateDiscountAmount(voucher, totalAmount);
                    totalAmount -= Math.Min(discountAmount, totalAmount - 5000);

                    voucher.Quantity -= 1;
                    if (voucher.Quantity <= 0)
                    {
                        voucher.IsActive = false;
                    }
                    _dbcontext.Voucher.Update(voucher);
                }

                decimal minimumTotal = 5000;
                if (totalAmount < minimumTotal)
                {
                    order.TotalAmount = minimumTotal;
                }
                else
                {
                    order.TotalAmount = totalAmount;
                }

                // Lưu đơn hàng và chi tiết đơn hàng vào database
                await _dbcontext.Order.AddAsync(order);
                _dbcontext.OrderDetails.AddRange(orderDetailsList);

                // Lưu lịch sử đơn hàng
                var orderHistory = new OrderHistory
                {
                    ID = Guid.NewGuid(),
                    IDOrder = order.ID,
                    IDUser = request.IDUser ?? defaultUserID,
                    EditingHistory = "Order Create",
                    ChangeDate = DateTime.Now,
                    ChangeType = "",
                    OldStatus = 0,
                    NewStatus = 1,
                    ChangeDetails = "",
                    Status = 1,
                    CreateBy = request.CreateBy,
                    CreateDate = DateTime.Now,
                };
                await _dbcontext.OrderHistory.AddAsync(orderHistory);

                // Lưu thay đổi vào database
                await _dbcontext.SaveChangesAsync();
                await transaction.CommitAsync();

                return true;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return false;
            }
        }
        private decimal CalculateDiscountAmount(Voucher voucher, decimal totalAmount)
        {
            if (voucher.Type == Types.Percent)
            {
                return (voucher.ReducedValue / 100m) * totalAmount;
            }
            if (voucher.Type == Types.Cash)
            {
                return voucher.ReducedValue;
            }

            return 0;
        }
        private async Task<bool> CheckAndReduceStock(Guid IDOptions, int quantity)
        {
            if (IDOptions != Guid.Empty && IDOptions != null)
            {
                var variantItem = await _dbcontext.Options.FindAsync(IDOptions);
                if (variantItem != null)
                {
                    if (variantItem.StockQuantity < quantity)
                    {
                        return false;
                    }
                    variantItem.StockQuantity -= quantity;
                    await _dbcontext.SaveChangesAsync();
                    return true;
                }
            }
            return false;
        }
        public async Task<List<OrderVM>> GetAllActiveAsync()
        {
            var activeOrders = await _dbcontext.Order
                           .Where(o => o.Status == 1)
                           .ToListAsync();

            var activeOrderVMs = _mapper.Map<List<OrderVM>>(activeOrders);

            return activeOrderVMs;
        }
        public async Task<List<OrderVM>> GetAllAsync()
        {
            var activeOrders = await _dbcontext.Order
                                      .ToListAsync();

            var activeOrderVMs = _mapper.Map<List<OrderVM>>(activeOrders);

            return activeOrderVMs;
        }
        public async Task<OrderVM> GetByHexCodeAsync(string HexCode)
        {
            var order = await _dbcontext.Order.FirstOrDefaultAsync(o => o.HexCode.ToString() == HexCode);
            return order != null ? _mapper.Map<OrderVM>(order) : null;
        }
        public async Task<OrderVM> GetByIDAsync(Guid ID)
        {
            var order = await _dbcontext.Order.FirstOrDefaultAsync(o => o.ID == ID);
            return order != null ? _mapper.Map<OrderVM>(order) : null;
        }
        public async Task<List<OrderDetailsVM>> GetOrderDetailsVMByIDAsync(Guid IDOrder)
        {
            var orderDetails = await(
                            from p in _dbcontext.Order
                            join dp in _dbcontext.OrderDetails on p.ID equals dp.IDOrder
                            where dp.IDOrder == IDOrder
                            select new OrderDetailsVM
                            {
                                ID = dp.ID,
                                IDOrder = p.ID,
                                IDOptions = dp.IDOptions,
                                ColorName = dp.Options.Colors.Name,
                                SizeName = dp.Options.Sizes.Name,
                                ProductName = dp.Options.ProductDetails.Products.Name,
                                UnitPrice = dp.UnitPrice,
                                Discount = dp.Discount,
                                Quantity = dp.Quantity,
                                Status = dp.Status,
                                TotalAmount = dp.TotalAmount,
                            }
                        ).ToListAsync();
            if (!orderDetails.Any())
            {
                return new List<OrderDetailsVM>();
            }

            return orderDetails;
        }
        public async Task<bool> RemoveAsync(Guid ID, string IDUserdelete)
        {
            using (var transaction = _dbcontext.Database.BeginTransaction())
            {
                try
                {
                    var obj = await _dbcontext.Order.FirstOrDefaultAsync(c => c.ID == ID);

                    if (obj != null)
                    {
                        obj.Status = 0;
                        obj.DeleteDate = DateTime.Now;
                        obj.DeleteBy = IDUserdelete;

                        _dbcontext.Order.Attach(obj);
                        await _dbcontext.SaveChangesAsync();


                        transaction.Commit();
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    return false;
                }
            }
        }
        public async Task<bool> UpdateAsync(Guid ID, OrderUpdateVM request)
        {
            var order = await _dbcontext.Order.FirstOrDefaultAsync(o => o.ID == ID);

            if (order == null || order.TrackingCheck)
            {
                return false;
            }

            order.ModifiedBy = request.ModifieBy;
            order.ModifiedDate = DateTime.Now;
            order.HexCode = request.HexCode;
            order.VoucherCode = request.VoucherCode;
            order.IDUser = request.IDUser;
            order.CustomerName = request.CustomerName;
            order.CustomerPhone = request.CustomerPhone;
            order.CustomerEmail = request.CustomerEmail;
            order.ShippingAddress = request.ShippingAddress;
            order.ShippingAddressLine2 = request.ShippingAddressLine2;
            order.ShipDate = request.ShipDate;
            order.TotalAmount = request.TotalAmount;
            order.Cotsts = request.Cotsts;
            order.Notes = request.Notes;
            order.TrackingCheck = request.TrackingCheck;
            order.PaymentMethods = request.PaymentMethod;
            order.PaymentStatus = request.PaymentStatus;
            order.ShippingMethods = request.ShippingMethod;
            order.OrderStatus = request.OrderStatus;
            order.Status = request.Status;

            _dbcontext.Order.Update(order);
            await _dbcontext.SaveChangesAsync();
            return true;
        }
        public async Task<List<OrderVM>> GetByCustomerIDAsync(string IDUser)
        {
            var orders = await _dbcontext.Order
                                         .Where(o => o.IDUser == IDUser && o.Status != 0)
                                         .ToListAsync();

            return _mapper.Map<List<OrderVM>>(orders);
        }
    }
}
