using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Misc.OneS.Models
{
    public class ConfigImportCategoryEntity
    {
        public string Title { get; set; }
        public string FileName { get; set; }
        public string RootName { get; set; }
        public string RootCategoryName { get; set; }
        public string RootCategoryElementsName { get; set; }
        public int CategoryId { get; set; }
        public IList<ImportProductSpecification> ProductSpecifications { get; set; }
       
    }

    public class ImportProductSpecification
    {
        public string XName { get; set; }
        public string RusName { get; set; }
        public bool FilterBy { get; set; }
    }
}
