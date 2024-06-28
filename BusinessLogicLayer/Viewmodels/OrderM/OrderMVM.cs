using BusinessLogicLayer.Viewmodels.Order;
using BusinessLogicLayer.Viewmodels.OrderDetails;
using BusinessLogicLayer.Viewmodels.ViewKH;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogicLayer.Viewmodels.OrderM
{
    public class OrderMVM
    {
        public List<ProductTQVM> lstproduct { get; set; }
        public List<ProductTQVM> SelectedProducts { get; set; }
        public ProductTQVM? product { get; set; }
        public OrderCreateVM? CreateVM { get; set; }
        public OrderDetailsCreateVM? CreateVMDetails { get; set; }
        public IEnumerable<ProDetailKH>? ProDetails { get; set; }
    }
    public class ProductTQVM
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string MaSp { get; set; }
        public decimal CostPrie { get; set; }
        public string urlImg { get; set; }

        public string? Corlor { get; set; }
        public string? Size { get; set; }

    }
}
