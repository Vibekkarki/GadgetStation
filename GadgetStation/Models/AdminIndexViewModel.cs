using System.Collections.Generic;

namespace GadgetStation.Models
{
    public class AdminIndexViewModel
    {
        public List<Product> Products { get; set; }
        public List<User> Users { get; set; }
        public List<Order> Orders { get; set; }
        public PaginationModel ProductPagination { get; set; }
        public PaginationModel UserPagination { get; set; }
        public PaginationModel OrderPagination { get; set; }
    }

}
