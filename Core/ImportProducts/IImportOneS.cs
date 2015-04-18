using Nop.Plugin.Misc.OneS.Models;

namespace Nop.Plugin.Misc.OneS.Core.ImportProducts
{
    public interface IImportOneS
    {
        bool ImportProductStorages(string path,ConfigImportCategoryEntity config);

    }
}
