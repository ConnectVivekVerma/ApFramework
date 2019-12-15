using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


namespace ApFramework.Core
{
    public interface IRequestValidationProcessor
    {
        Task Validate(IRequest request, IDictionary<string, object> addtionalObjectBag, CancellationToken cancellationToken);
    }

    public class RequestValidationProcessor : IRequestValidationProcessor
    {
        public async Task Validate(IRequest request, IDictionary<string, object> addtionalObjectBag, CancellationToken cancellationToken)
        {
            
            var validations = new List<IRequestValidationResult>();
            await ValidateRecursive(request, addtionalObjectBag, validations, cancellationToken);

            var validationResult = new RequestValidationResult();
            validations.Where(t => t.HasFailures)
                .SelectMany(t => t.ValidationFailures)
                .ToList()
                .ForEach(vf => validationResult.ValidationFailures.Add(vf));

            if (validationResult.ValidationFailures != null && validationResult.ValidationFailures.Any())
                throw new RequestValidationEception("Request validation failed", validationResult);

        }


        private async Task ValidateRecursive(IRequest request
            , IDictionary<string, object> addtionalObjectBag
            , List<IRequestValidationResult> validations
            , CancellationToken cancellationToken
            )
        {
            if (request == null) return;
            if (request.RequestCollection != null && request.RequestCollection.Count > 0)
            {
                foreach (var subReq in request.RequestCollection)
                {
                    await ValidateRecursive(subReq, addtionalObjectBag, validations, cancellationToken);
                }
            }

            if (request.ValidateRequest == null) return;
            
            var result = await request.ValidateRequest(request, addtionalObjectBag, cancellationToken);
            validations.Add(result);
        }
    }
}
