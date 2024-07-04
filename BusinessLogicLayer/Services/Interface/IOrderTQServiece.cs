using BusinessLogicLayer.Viewmodels.OrderM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogicLayer.Services.Interface
{
    public interface IOrderTQServiece
    {
        public Task<List<ProductTQVM>> GetProductTQVMs(string seach);
    }
}
