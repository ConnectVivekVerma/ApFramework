using ApFramework.Core;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace ApFramework.Tests
{
    public class AddtionalActionsTests
    {
        private const string _requestCancellationOnMatch = "2";

        [Fact]
        public async Task ActionsShouldExecuteInOrder()
        {
            var contextBag = ApFrameworkHelper.GetEmptyBag();
            contextBag.Add("testObj", new TestArguments());

            var request = new Request<string>(ProcessRequest);
            var processor = new RequestProcessor(null);

            processor.AddAddtionalAction(Last, AddtionalActionSequence.Last);
            processor.AddAddtionalAction(First, AddtionalActionSequence.First); // 0
            processor.AddAddtionalAction(BeforeAuth, AddtionalActionSequence.BeforeAuthorization);
            processor.AddAddtionalAction(AfterAuth, AddtionalActionSequence.AfterAuthorization);
            processor.AddAddtionalAction(BeforeVal, AddtionalActionSequence.BeforeValidation);
            processor.AddAddtionalAction(AfterVal, AddtionalActionSequence.AfterValidation);
            processor.AddAddtionalAction(BeforeProcess, AddtionalActionSequence.BeforeRequestProcessing);
            processor.AddAddtionalAction(AfterProcess, AddtionalActionSequence.AfterRequestProcessing);
            

            await processor.ProcessRequestAsync(request, contextBag);
            var updatedObj = contextBag["testObj"] as TestArguments;

            for (int i = 0; i < 9; i++)
            {
                Assert.True(updatedObj.Logs[i] == i.ToString());
            }
        }

        [Fact]
        public async Task StopExectionIfCancellationIsRequested()
        {
            var contextBag = ApFrameworkHelper.GetEmptyBag();
            
            var testArgs = new TestArguments();
            CancellationTokenSource source = new CancellationTokenSource();
            testArgs.CancellationTokenSource = source;
            testArgs.CancellationMatchStrinig = "2";


            contextBag.Add("testObj", testArgs);
            
            var request = new Request<string>(ProcessRequest);
            var processor = new RequestProcessor(null);

            processor.AddAddtionalAction(Last, AddtionalActionSequence.Last);
            processor.AddAddtionalAction(First, AddtionalActionSequence.First); // 0
            processor.AddAddtionalAction(BeforeAuth, AddtionalActionSequence.BeforeAuthorization);
            processor.AddAddtionalAction(AfterAuth, AddtionalActionSequence.AfterAuthorization);
            processor.AddAddtionalAction(BeforeVal, AddtionalActionSequence.BeforeValidation);
            processor.AddAddtionalAction(AfterVal, AddtionalActionSequence.AfterValidation);
            processor.AddAddtionalAction(BeforeProcess, AddtionalActionSequence.BeforeRequestProcessing);
            processor.AddAddtionalAction(AfterProcess, AddtionalActionSequence.AfterRequestProcessing);


            await processor.ProcessRequestAsync(request, contextBag, source.Token);
            var updatedObj = contextBag["testObj"] as TestArguments;

            for (int i = 0; i < 3; i++)
            {
                Assert.True(updatedObj.Logs[i] == i.ToString());
            }

            for (int i = 3; i < 9; i++)
            {
                Assert.True(string.IsNullOrEmpty(updatedObj.Logs[i]));
            }
        }

        private async Task<string> ProcessRequest(IRequest request
            , IDictionary<string, object> contextBag
             , CancellationToken cancellationToken)
        {
            ActualTask(contextBag, "6");
            return string.Empty;
        }

        public async Task First(IRequest request, IDictionary<string, object> contextBag, CancellationToken cancellationToken)
        {
            ActualTask(contextBag, "0");
        }
        public async Task Last(IRequest request, IDictionary<string, object> contextBag, CancellationToken cancellationToken)
        {
            ActualTask(contextBag, "8");
        }
        public async Task BeforeAuth(IRequest request, IDictionary<string, object> contextBag, CancellationToken cancellationToken)
        {
            ActualTask(contextBag, "1");
        }
        public async Task AfterAuth(IRequest request, IDictionary<string, object> contextBag, CancellationToken cancellationToken)
        {
            ActualTask(contextBag, "2");
        }
        public async Task BeforeVal(IRequest request, IDictionary<string, object> contextBag, CancellationToken cancellationToken)
        {
            ActualTask(contextBag, "3");
        }
        public async Task AfterVal(IRequest request, IDictionary<string, object> contextBag, CancellationToken cancellationToken)
        {
            ActualTask(contextBag, "4");
        }
        public async Task BeforeProcess(IRequest request, IDictionary<string, object> contextBag, CancellationToken cancellationToken)
        {
            ActualTask(contextBag, "5");
        }
        public async Task AfterProcess(IRequest request, IDictionary<string, object> contextBag, CancellationToken cancellationToken)
        {
            ActualTask(contextBag, "7");
        }
        private void ActualTask(IDictionary<string, object> contextBag, string msg)
        {
            var testObj = contextBag["testObj"] as TestArguments;
            int pointer = testObj.Pointer;
            testObj.Logs[pointer] = msg;
            testObj.Pointer++;

            if(msg == testObj.CancellationMatchStrinig)
            {
                testObj.CancellationTokenSource.Cancel();

            }
        }
        private class TestArguments
        {
            public int Pointer { get; set; } = 0;
            public string[] Logs { get; set; } = new string[9];
            public CancellationTokenSource CancellationTokenSource { get; set; }
            public string CancellationMatchStrinig { get; set; } = string.Empty;
        }

    }
}
