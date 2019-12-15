using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ApFramework.Core
{
    public interface IRequestAuthorizationProcessor
    {
        Task AuthorizeRecursiveAsync(IRequest request
            , IDictionary<string, object> contextBag
            , CancellationToken cancellationToken);
    }

    public class RequestAuthorizationProcessor : IRequestAuthorizationProcessor
    {
        public async Task AuthorizeRecursiveAsync(IRequest request
            , IDictionary<string, object> contextBag
            , CancellationToken cancellationToken)
        {

            var collection = new List<IRequestAuthorizationResult>();
            await AuthorizeRecursive(request, contextBag, collection, cancellationToken);

            var result = new RequestAuthorizationResult();
            collection.Where(t => t.HasFailures)
                .SelectMany(t => t.AuthorizationFailures)
                .ToList()
                .ForEach(vf => result.AuthorizationFailures.Add(vf));

            if (result.AuthorizationFailures != null && result.AuthorizationFailures.Any())
                throw new RequestAuthorizationException("Request authorization failed", result);

        }


        private async Task AuthorizeRecursive(IRequest request
            , IDictionary<string, object> contextBag
            , List<IRequestAuthorizationResult> validations
            , CancellationToken cancellationToken
            )
        {
            if (request == null) return;
            if (request.RequestCollection != null && request.RequestCollection.Count > 0)
            {
                foreach (var subReq in request.RequestCollection)
                {
                    await AuthorizeRecursive(subReq, contextBag, validations, cancellationToken);
                }
            }

            if (request.AuthorizeRequest == null) return;

            var result = await request.AuthorizeRequest(request, contextBag, cancellationToken);
            validations.Add(result);
        }
    }
}

