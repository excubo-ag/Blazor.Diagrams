using Excubo.Blazor.LazyStyleSheet;
using Excubo.Blazor.ScriptInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Excubo.Blazor.Diagrams
{
    public static class ServiceExtension
    {
        public static IServiceCollection AddDiagramServices(this IServiceCollection services)
        {
            return services
                .AddStyleSheetLazyLoading()
                .AddScriptInjection();
        }
    }
}
