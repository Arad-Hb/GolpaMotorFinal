using GolpaMotorFinal.Models.ViewModels.CRUD;

namespace GolpaMotorFinal.FrameworkUI.Services
{
    public class GridConfigurationFactory : IGridConfigurationFactory
    {
        private readonly IServiceProvider _provider;

        public GridConfigurationFactory(IServiceProvider provider)
        {
            _provider = provider;
        }

        public CrudGridViewModel Build<T>(IEnumerable<T> items)
        {
            var config =
                _provider.GetRequiredService<ICrudGridConfiguration<T>>();

            return config.Configure(items);
        }
    }
}
