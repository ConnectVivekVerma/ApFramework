using ApFramework.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace ApFramework.Tests
{
    public class InitializationTests
    {
        [Fact]
        public async Task RequestProcessorShouldIntializeWithRegisteredAuthorizerAndValidator()
        {
            var contextBag = ApFrameworkHelper.GetEmptyBag();

            //var request = new QueryRequest<string>(ProcessRequest);
            var processor = new RequestProcessor(null);

        }

        [Fact]
        public async Task VerifyComponentsHookUpCorrectly()
        {
            IServiceCollection services = new ServiceCollection();

            ILogger<RequestProcessor> logger = new Mock<ILogger<RequestProcessor>>().Object;            
            services.AddSingleton<ILogger<RequestProcessor>>(logger);
            services.AddTransient<IRequestProcessor, RequestProcessor>();
            
            var serviceProvider = services.BuildServiceProvider();
           
            string logKey = "TestLog";
            // Request processing
            ProcessRequestDelegate<string> process = 
                async (request, contextBag, cancellationToken) => 
                {
                    var log = contextBag[logKey] as StringBuilder;
                    log.Append("Processed");
                    return string.Empty;
                };

            ValidateRequestDelegate validator = async (request, contextBag, cancellationToken) =>
            {
                var log = contextBag[logKey] as StringBuilder;
                log.Append("Validated>>");
                return RequestValidationResult.Instance();
            };

            AuthorizeRequestDelegate authorizer = async (request, contextBag, cancellationToken) =>
            {
                var log = contextBag[logKey] as StringBuilder;
                log.Append("Authorized>>");
                return new RequestAuthorizationResult();
            };

            var contextBag = ApFrameworkHelper.GetEmptyBag();
            contextBag[logKey] = new StringBuilder();

            var expected = "Authorized>>Validated>>Processed";

            var request = new Request<string>(process, validator, authorizer);
            var processor = serviceProvider.GetService<IRequestProcessor>();
            
            await processor.ProcessRequestAsync(request, contextBag);

            var log = contextBag[logKey] as StringBuilder;
            Assert.True(log.ToString() == expected);

        }
       
    }
}
