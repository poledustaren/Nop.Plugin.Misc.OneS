using Nop.Plugin.Misc.OneS.Models;

namespace Nop.Plugin.Misc.OneS.Core.ImportProducts
{
    public interface IImportOneSImpl
    {
        void SetConfig(ConfigImportCategoryEntity configImportCategoryEntity);
        void ImportAllEntities(ImportEntity[] importEntities);
        void ImportStoragesEntities(ImportEntity[] importEntities);


    }
}