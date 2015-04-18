using Autofac;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Plugin.Misc.OneS.Core;
using Nop.Plugin.Misc.OneS.Core.ExportOrders;
using Nop.Plugin.Misc.OneS.Core.ImportProducts;

namespace Nop.Plugin.Misc.OneS
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public virtual void Register(ContainerBuilder builder, ITypeFinder typeFinder)
        {
            builder.RegisterType<ImportOneS>().As<IImportOneS>().InstancePerLifetimeScope();
            builder.RegisterType<ImportOneSImpl>().As<IImportOneSImpl>().InstancePerLifetimeScope();
            builder.RegisterType<ExportOrder>().As<IExportOrder>().InstancePerLifetimeScope();
            builder.RegisterType<CategoryConfigs>().As<CategoryConfigs>().InstancePerLifetimeScope();
            
        }

        public int Order
        {
            get { return 1; }
        }
    }
}
