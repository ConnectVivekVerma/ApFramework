using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ApFramework.Core
{
    public enum AddtionalActionSequence
    {
        First,
        Last,
        BeforeAuthorization,
        AfterAuthorization,
        BeforeValidation,
        AfterValidation,
        BeforeRequestProcessing,
        AfterRequestProcessing,
    }

    public delegate Task AdditionalActionDelegate(
        IRequest request
        , IDictionary<string, object> contextBag
        , CancellationToken cancellationToken);

    public interface IRequestProcessor
    {
        Task ProcessRequestAsync(
              IRequest request
            );

        Task ProcessRequestAsync(
              IRequest request
            , IDictionary<string, object> contextBag = null
            );
        Task ProcessRequestAsync(
              IRequest request
            , IDictionary<string, object> contextBag
            , CancellationToken cancellationToken
            );
        void AddAddtionalAction(AdditionalActionDelegate additionalActionDelegate, AddtionalActionSequence addtionalActionSequence);

    }
    public class RequestProcessor : IRequestProcessor
    {
        //ILogger _logger;
        private readonly IRequestAuthorizationProcessor _authorizer;
        private readonly IRequestValidationProcessor _validator;

        private readonly List<AdditionalActionDelegate> _firstSteps;
        private readonly List<AdditionalActionDelegate> _lastSteps;
        private readonly List<AdditionalActionDelegate> _beforeAuthorizartion;
        private readonly List<AdditionalActionDelegate> _afterAutorization;
        private readonly List<AdditionalActionDelegate> _beforeValidation;
        private readonly List<AdditionalActionDelegate> _afterValidation;
        private readonly List<AdditionalActionDelegate> _beforeRequestProcessing;
        private readonly List<AdditionalActionDelegate> _afterRequestProcessing;

        public RequestProcessor()
        {            
            _authorizer = new RequestAuthorizationProcessor();
            _validator = new RequestValidationProcessor();

            _firstSteps = new List<AdditionalActionDelegate>();
            _lastSteps = new List<AdditionalActionDelegate>();
            _beforeAuthorizartion = new List<AdditionalActionDelegate>();
            _afterAutorization = new List<AdditionalActionDelegate>();
            _beforeValidation = new List<AdditionalActionDelegate>();
            _afterValidation = new List<AdditionalActionDelegate>();
            _beforeRequestProcessing = new List<AdditionalActionDelegate>();
            _afterRequestProcessing = new List<AdditionalActionDelegate>();

            ApFrameworkHelper.GlobalActions.ForEach(a => {
                if (a.Value != null)
                    AddAddtionalAction(a.Value, a.Key);
            });
            
        }

        public async Task ProcessRequestAsync(
            IRequest request
           )
        {
            await ProcessRequestAsync(request, null, CancellationToken.None);
        }

        public async Task ProcessRequestAsync(
            IRequest request
            , IDictionary<string, object> contextBag
           )
        {
            await ProcessRequestAsync(request, contextBag, CancellationToken.None);
        }

        public async Task ProcessRequestAsync(IRequest request
            , IDictionary<string, object> contextBag
             , CancellationToken cancellationToken
            )
        {   
            if (!cancellationToken.IsCancellationRequested)
                ProcessActions(_firstSteps, request, contextBag, cancellationToken);
            

            if (!cancellationToken.IsCancellationRequested)
                ProcessActions(_beforeAuthorizartion, request, contextBag, cancellationToken);
            
            if (_authorizer != null && !cancellationToken.IsCancellationRequested)
                await _authorizer.AuthorizeRecursiveAsync(request, contextBag, cancellationToken);
            
            if (!cancellationToken.IsCancellationRequested)
                ProcessActions(_afterAutorization, request, contextBag, cancellationToken);
            
            if (!cancellationToken.IsCancellationRequested)
                ProcessActions(_beforeValidation, request, contextBag, cancellationToken);

            if (_validator != null && !cancellationToken.IsCancellationRequested)
                await _validator.Validate(request, contextBag, cancellationToken);

            if (!cancellationToken.IsCancellationRequested)
                ProcessActions(_afterValidation, request, contextBag, cancellationToken);

            if (!cancellationToken.IsCancellationRequested)
                ProcessActions(_beforeRequestProcessing, request, contextBag, cancellationToken);

            if (!cancellationToken.IsCancellationRequested)
                await ProcessRecursive(request, contextBag, cancellationToken);

            if (!cancellationToken.IsCancellationRequested)
                ProcessActions(_afterRequestProcessing, request, contextBag, cancellationToken);

            if (!cancellationToken.IsCancellationRequested)
                ProcessActions(_lastSteps, request, contextBag, cancellationToken);
        }

        private async Task ProcessRecursive(IRequest request
            , IDictionary<string, object> contextBag
            , CancellationToken cancellationToken
            )
        {
            if (request == null) return;
            if (request.RequestCollection != null && request.RequestCollection.Count > 0)
            {
                foreach (var subReq in request.RequestCollection)
                {
                    await ProcessRecursive(subReq, contextBag, cancellationToken);
                }
            }

            await request.Process(request, contextBag, cancellationToken);
        }

        private void ProcessActions(List<AdditionalActionDelegate> delegtes
            , IRequest request
            , IDictionary<string, object> contextBag
            , CancellationToken cancellationToken)
        {
            if (delegtes != null && delegtes.Any())
                delegtes.ForEach(
                       async a =>
                       {
                           if (a != null)
                               await a(request, contextBag, cancellationToken);
                       }
                );

        }

        public void AddAddtionalAction(AdditionalActionDelegate additionalActionDelegate
            , AddtionalActionSequence addtionalActionSequence)
        {
            switch (addtionalActionSequence)
            {
                case AddtionalActionSequence.First:
                    AddToFirstStep(additionalActionDelegate);
                    break;

                case AddtionalActionSequence.Last:
                    AddToLastStep(additionalActionDelegate);
                    break;

                case AddtionalActionSequence.BeforeAuthorization:
                    AddToBeforeAuthorization(additionalActionDelegate);
                    break;

                case AddtionalActionSequence.AfterAuthorization:
                    AddToAfterAuthorization(additionalActionDelegate);
                    break;

                case AddtionalActionSequence.BeforeValidation:
                    AddToBeforeValidation(additionalActionDelegate);
                    break;

                case AddtionalActionSequence.AfterValidation:
                    AddToAfterValidation(additionalActionDelegate);
                    break;

                case AddtionalActionSequence.BeforeRequestProcessing:
                    AddToBeforeRequestProcessing(additionalActionDelegate);
                    break;

                case AddtionalActionSequence.AfterRequestProcessing:
                    AddToAfterRequestProcessing(additionalActionDelegate);
                    break;
            }
        }

        private void AddToFirstStep(AdditionalActionDelegate additionalActionDelegate)
        {
            _firstSteps.Add(additionalActionDelegate);
        }

        private void AddToLastStep(AdditionalActionDelegate additionalActionDelegate)
        {
            _lastSteps.Add(additionalActionDelegate);
        }

        private void AddToBeforeAuthorization(AdditionalActionDelegate additionalActionDelegate)
        {
            _beforeAuthorizartion.Add(additionalActionDelegate);
        }

        private void AddToAfterAuthorization(AdditionalActionDelegate additionalActionDelegate)
        {
            _afterAutorization.Add(additionalActionDelegate);
        }

        private void AddToBeforeValidation(AdditionalActionDelegate additionalActionDelegate)
        {
            _beforeValidation.Add(additionalActionDelegate);
        }

        private void AddToAfterValidation(AdditionalActionDelegate additionalActionDelegate)
        {
            _afterValidation.Add(additionalActionDelegate);
        }

        private void AddToBeforeRequestProcessing(AdditionalActionDelegate additionalActionDelegate)
        {
            _beforeRequestProcessing.Add(additionalActionDelegate);
        }

        private void AddToAfterRequestProcessing(AdditionalActionDelegate additionalActionDelegate)
        {
            _afterRequestProcessing.Add(additionalActionDelegate);
        }        
    }
}
