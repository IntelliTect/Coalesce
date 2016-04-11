using Intellitect.ComponentModel.DataAnnotations;

namespace Coalesce.Domain
{
    [CreateController(false, false)]
    public class CaseProduct
    {
        public int CaseProductId { get; set; }
        public int CaseId { get; set; }
        public Case Case { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; }
    }
}
