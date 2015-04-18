using System.Web.Mvc;
using Nop.Core;

using Nop.Plugin.Misc.OneS.Core.ImportProducts;
using Nop.Plugin.Misc.OneS.Core.ImportProducts;
using Nop.Plugin.Misc.OneS.Models;
using Nop.Services.Configuration;
using Nop.Services.Stores;
using Nop.Web.Framework.Controllers;

namespace Nop.Plugin.Misc.OneS.Controllers
{
    public class MiscOneSController : BasePluginController
    {
        private readonly IImportOneS _importOneS;
        private readonly ISettingService _settingService;
        private readonly MiscOneSSettings _miscOneSSettings;
        private readonly IWorkContext _workContext;
        private readonly IStoreService _storeService;


        public MiscOneSController(IImportOneS importOneS,ISettingService settingService,MiscOneSSettings miscOneSSettings,IWorkContext workContext,
            IStoreService storeService)
        {
            this._miscOneSSettings = miscOneSSettings;
            this._settingService = settingService;
            this._importOneS = importOneS;
            this._workContext = workContext;
            this._storeService = storeService;
        }
        
        [ChildActionOnly]
        [AdminAuthorize]
        public ActionResult Configure()
        {
            
            var model = new MiscOneSSettings();
            model.PathToExchange = _miscOneSSettings.PathToExchange;
            model.PublishOnlyBerikolesaStorage = _miscOneSSettings.PublishOnlyBerikolesaStorage;
            return View("~/Plugins/Misc.OneS/Views/MiscOneS/Configure.cshtml", model);
            
        }
        [HttpPost, ActionName("Configure")]
        [FormValueRequired("save")]
        public ActionResult Configure(MiscOneSSettings model)
        {
            if (!ModelState.IsValid)
            {
                return Configure();
            }

            //save settings
            _miscOneSSettings.PathToExchange = model.PathToExchange;
            _miscOneSSettings.PublishOnlyBerikolesaStorage = model.PublishOnlyBerikolesaStorage;
            _settingService.SaveSetting(_miscOneSSettings);

            return View("~/Plugins/Misc.OneS/Views/MiscOneS/Configure.cshtml", model);
        }

    }
}