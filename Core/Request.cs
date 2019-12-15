using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ApFramework.Core
{

    public delegate Task<IRequestValidationResult> ValidateRequestDelegate(IRequest request, IDictionary<string, object> contextBag, CancellationToken cancellationToken);
    public delegate Task<IRequestAuthorizationResult> AuthorizeRequestDelegate(IRequest request, IDictionary<string, object> contextBag, CancellationToken cancellationToken);
    public delegate Task<T> ProcessRequestDelegate<T>(IRequest request, IDictionary<string, object> contextBag, CancellationToken cancellationToken);
    

    public interface IRequest
    {
        Task Process(IRequest request, IDictionary<string, object> contextBag, CancellationToken cancellationToken);
        ValidateRequestDelegate ValidateRequest { get; }
        AuthorizeRequestDelegate AuthorizeRequest { get; }
        ICollection<IRequest> RequestCollection { get; }
        IDictionary<string, object> RequestBag { get; }
    }

    public abstract class RequestBase : IRequest
    {        
        public RequestBase(
              ValidateRequestDelegate validateRequest
            , AuthorizeRequestDelegate authorizeRequest
            )
        {
            _validateRequest = validateRequest;
            _authorizeRequest = authorizeRequest;
            _requestBag = new Dictionary<string, object>();
        }
        public virtual async Task Process(IRequest request
            , IDictionary<string, object> contextBag
            , CancellationToken cancellationToken) 
        {
            throw new NotImplementedException(); 
        }
        public ICollection<IRequest> RequestCollection { get; } = new List<IRequest>();



        private readonly ValidateRequestDelegate _validateRequest;
        public ValidateRequestDelegate ValidateRequest
        {
            get
            {
                return _validateRequest;
            }
        }



        private readonly AuthorizeRequestDelegate _authorizeRequest;
        public AuthorizeRequestDelegate AuthorizeRequest
        {
            get
            {
                return _authorizeRequest;
            }
        }

                
        private IDictionary<string, object> _requestBag;
        public IDictionary<string, object> RequestBag { get { return _requestBag; } }
    }

    public class Request<T> : RequestBase
    {
        public T Response { get; private set; }
        private readonly ProcessRequestDelegate<T> _operation;

        public Request(
              ProcessRequestDelegate<T> operation            
            , ValidateRequestDelegate validateRequest = null
            , AuthorizeRequestDelegate authorizeRequest = null
            ) : base(validateRequest, authorizeRequest)
        {
            _operation = operation;            
        }        

        public override async Task Process(IRequest request
            , IDictionary<string, object> contextBag
            , CancellationToken cancellationToken
            )
        {           
            if(_operation != null)
                Response = await _operation(this, contextBag, cancellationToken);
        }
    }    
}
