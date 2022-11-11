using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShippingWebsite.Models
{
    public class CargoModel
    {
        public string Code { get; set; }

        public string Name { get; set; }

        public string Quantity { get; set; }

        public string Status { get; set; }

        public string Description { get; set; }
    }
}
