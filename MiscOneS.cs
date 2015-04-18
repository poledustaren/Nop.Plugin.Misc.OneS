using System.Web.Routing;
using Nop.Core.Plugins;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Tasks;

namespace Nop.Plugin.Misc.OneS
{
    
    public class MiscOneS : BasePlugin, IMiscPlugin
    {
        private readonly ISettingService _settingService;
        private readonly IScheduleTaskService _scheduleTaskService;


        public MiscOneS(ISettingService settingService, IScheduleTaskService scheduleTaskService)
        {
            _settingService = settingService;
            _scheduleTaskService = scheduleTaskService;
        }

        public override void Install()
        {
            _scheduleTaskService.InsertTask(new Nop.Core.Domain.Tasks.ScheduleTask()
            {
                Enabled = false,
                Name = "Импорт 1С все продукты",
                Seconds = 5*60,
                StopOnError = false,
                Type = "Nop.Plugin.Misc.OneS.Tasks.ImportOneSTaskImportAll, Nop.Plugin.Misc.OneS",
            });
            
            _scheduleTaskService.InsertTask(new Nop.Core.Domain.Tasks.ScheduleTask()
            {
                Enabled = false,
                Name = "Импорт 1С, обновление остатков",
                Seconds = 60,
                StopOnError = false,
                Type = "Nop.Plugin.Misc.OneS.Tasks.ImportOneSTaskImportStorages, Nop.Plugin.Misc.OneS",
            });
            base.Install();
        }

        public override void Uninstall()
        {
            //settings

            Nop.Core.Domain.Tasks.ScheduleTask taskImportAll = _scheduleTaskService.GetTaskByType("Nop.Plugin.Misc.OneS.Core.ImportOneSTaskImportAll, Nop.Plugin.Misc.OneS");
            if (taskImportAll != null)
            {
                _scheduleTaskService.DeleteTask(taskImportAll);
            }
            Nop.Core.Domain.Tasks.ScheduleTask taskImportStorages = _scheduleTaskService.GetTaskByType("Nop.Plugin.Misc.OneS.Core.ImportOneSTaskImportStorages, Nop.Plugin.Misc.OneS");
            if (taskImportStorages != null)
            {

                _scheduleTaskService.DeleteTask(taskImportStorages);
            }
            base.Uninstall();
        }
       
        public void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "Configure";
            controllerName = "MiscOneS";
            routeValues = new RouteValueDictionary { { "Namespaces", "Nop.Plugin.Misc.OneS.Controllers" }, { "area", null } };
        }
    }
}
