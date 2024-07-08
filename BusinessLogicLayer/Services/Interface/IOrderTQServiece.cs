using BusinessLogicLayer.Viewmodels.Order;
using BusinessLogicLayer.Viewmodels.OrderM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DataAccessLayer.Entity.Base.EnumBase;

namespace BusinessLogicLayer.Services.Interface
{
    public interface IOrderTQServiece
    {
        public Task<List<ProductTQVM>> GetProductTQVMs(string seach);
        public Task<bool> UpdateOrderStatusAsync(Guid ID, OrderStatus orderStatus);
    }
}
