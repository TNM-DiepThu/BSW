﻿using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogicLayer.Viewmodels.Options
{
    public class OptionsVM
    {
        public Guid ID { get; set; }
        public string CreateBy { get; set; }
        public DateTime CreateDate { get; set; }
        public Guid IDProductDetails { get; set; }
        public Guid? IDColor { get; set; }
        public string ColorName { get; set; }
        public Guid? IDSize { get; set; }
        public string SizesName { get; set; }
        public int StockQuantity { get; set; }
        public string ImageURL { get; set; }

        public int Status { get; set; }

        public decimal CostPrice { get; set; }
        public decimal RetailPrice { get; set; }
        public decimal? Discount { get; set; }
    }
}
