using ApFramework.Core;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace ApFramework.Tests
{
    public class RequestWithGlobalAction
    {

        [Fact]
        public async Task HookUpPerformanceLoggingGlobally()
        {
            IServiceCollection services = new ServiceCollection();

            
            services.AddTransient<IRequestProcessor, RequestProcessor>();

            // add a property to request to represent if it is a child request. Thogh i dont need it.
            ApFrameworkHelper.AddApFrameworkGloablAction(StartPerformanceMonitoring, AddtionalActionSequence.First);
            ApFrameworkHelper.AddApFrameworkGloablAction(StopPerformanceMonitoring, AddtionalActionSequence.Last);

            var contextBag = ApFrameworkHelper.GetEmptyBag();            
            var serviceProvider = services.BuildServiceProvider();

            var request = new Request<string>(null);
            var reqProcessor = serviceProvider.GetService<IRequestProcessor>();
            await reqProcessor.ProcessRequestAsync(request, contextBag);

            Assert.False(string.IsNullOrWhiteSpace(contextBag["Perf_log"].ToString()));
            
        }

        private async Task StartPerformanceMonitoring(IRequest request
        , IDictionary<string, object> contextBag
        , CancellationToken cancellationToken)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            contextBag.Add("Perf_StopWatch", watch);
            await Task.CompletedTask;
        }

        private async Task StopPerformanceMonitoring(IRequest request
        , IDictionary<string, object> contextBag
        , CancellationToken cancellationToken)
        {
            var watch = contextBag["Perf_StopWatch"] as Stopwatch;
            watch.Stop();            
            contextBag["Perf_log"] = watch.ElapsedMilliseconds.ToString(); 
            await Task.CompletedTask;
        }
    }
}
