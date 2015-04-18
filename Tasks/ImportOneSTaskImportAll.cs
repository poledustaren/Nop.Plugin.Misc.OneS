using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Nop.Plugin.Misc.OneS.Core;
using Nop.Plugin.Misc.OneS.Core.ImportProducts;

using Nop.Plugin.Misc.OneS.Models;
using Nop.Services.Logging;
using Nop.Services.Tasks;

namespace Nop.Plugin.Misc.OneS.Tasks
{
    public partial class ImportOneSTaskImportAll : ITask
    {
        private readonly IImportOneS _importOneS;
        private readonly ILogger _logger;
        private readonly string _pathToExchange;
        private readonly CategoryConfigs _categoryConfigs;
        //сделать опцию логгирования  лишь в случае ошибки
        public ImportOneSTaskImportAll(IImportOneS importOneS,MiscOneSSettings miscOneSSettings, ILogger _logger,CategoryConfigs categoryConfigs)
        {
            _pathToExchange = miscOneSSettings.PathToExchange;
            _importOneS = importOneS;
            this._logger = _logger;
            _categoryConfigs = categoryConfigs;
        }
        public void Execute()
        {

            var pathToImport = _pathToExchange + @"\Exchange\Import";

            foreach (var configImportCategoryEntity in _categoryConfigs.GetConfigList())
            {
                var files = Directory.GetFiles(pathToImport, configImportCategoryEntity.FileName);

                foreach (var file in files)
                {
                    try
                    {
                        _logger.Information("Start import " + file);
                        if (_importOneS.ImportProductStorages(file,configImportCategoryEntity))
                        {
                            _logger.Information("Finish import " + file);
                            FileHelper.MoveToProcessed(file, _pathToExchange);
                        }
                        else
                        {
                            _logger.Information("Skip import " + file);
                        }
                    }
                    catch (Exception e)
                    {
                        FileHelper.MoveToError(file, _pathToExchange);
                        _logger.Error(e.Message, e);
                    }

                }
            }
          
        }
    }

    

}
