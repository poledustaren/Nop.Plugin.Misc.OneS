using System.Collections;
using System.ComponentModel;
using Nop.Core.Configuration;
using Nop.Services.Configuration;

namespace Nop.Plugin.Misc.OneS.Models
{
    public class MiscOneSSettings:ISettings
    {
        [DisplayName("Путь до папка Exchange")]
        public string PathToExchange { get; set; }
         [DisplayName("При импорте публиковать только актуальные товары сосклада бериколес")]

        public bool PublishOnlyBerikolesaStorage { get; set; }
    }

    public class MiscOneSSetting
    {
        private ISettingService _settingService { get; set; }
        public MiscOneSSetting(ISettingService settingService)
        {
            this._settingService = settingService;
        }
        public MiscOneSSettings GetSettings()
        {
            var settings = _settingService.LoadSetting<MiscOneSSettings>();
            return settings;
        }
    }
}
