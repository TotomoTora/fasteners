using System;
using KIursachTugin.DataAccess;
using KIursachTugin.Models;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KIursachTugin.Models
{
    public class Product
    {
        public int ProductID { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int? CategoriesID { get; set; }
        public int? SuppliersID { get; set; }
        public int Quantity { get; set; }
        public string Description { get; set; }
        public string ImagePath { get; set; }

        public string CategoryName { get; set; } // для отображения
        public string SupplierName { get; set; } // для отображения
    }
}
