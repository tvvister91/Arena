using MvvmCross.IoC;
using MvvmCross.ViewModels;

using MvvmCross;
using Arena.Core.Services;

namespace Arena.Core
{
    public class CoreApp : MvxApplication
    {
        public override void Initialize()
        {
            //  automatically strap-up all services in this app
            CreatableTypes()
                .EndingWith("Service")
                .AsInterfaces()
                .RegisterAsLazySingleton();

            CreatableTypes()
                .EndingWith("Repository")
                .AsInterfaces()
                .RegisterAsLazySingleton();

            //CreatableTypes()
            //    .EndingWith("Synchronizer")
            //    .AsInterfaces()
            //    .RegisterAsLazySingleton();

            Mvx.IoCProvider.LazyConstructAndRegisterSingleton<IDatabaseInitializer, DatabaseInitializer>();
            //  custom startup
            RegisterCustomAppStart<AppStart>();
        }
    }
}
