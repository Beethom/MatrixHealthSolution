using System.Collections.Generic;

namespace MatrixHealthSolution.Models.ViewModels
{
    public class HomeViewModel
    {
        public IEnumerable<Service> Services { get; set; } = new List<Service>();
        public IEnumerable<Product> Products { get; set; } = new List<Product>();
    }
}
