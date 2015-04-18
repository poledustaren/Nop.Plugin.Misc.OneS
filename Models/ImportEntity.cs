namespace Nop.Plugin.Misc.OneS.Models
{
    public class ImportEntity
    {
        public int CategoryId { get; set; }
        public string ProductSku { get; set; }
        public string ModelSku { get; set; }
        public string BrandSku { get; set; }
        public string ProductName { get; set; }
        public string ModelName { get; set; }
        public string BrandName { get; set; }
        public bool IsDeleted { get; set; }
        public ProductStorage[] Storages { get; set; }
        public ProductSpecification[] ProductSpecifications { get; set; }
    }
}