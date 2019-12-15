using System;
using System.Collections.Generic;
using System.Linq;

namespace ApFramework.Core
{
    public interface IRequestAuthorizationResult
    {
        ICollection<Failure> AuthorizationFailures { get; set; }
        bool HasFailures { get; }
        IRequestAuthorizationResult AddAuthorizationFailure(string property, string message);
        IRequestAuthorizationResult AddAuthorizationFailure(Failure failure);
    }

    public class RequestAuthorizationResult : IRequestAuthorizationResult
    {
        public ICollection<Failure> AuthorizationFailures { get; set; } = new List<Failure>();
        public bool HasFailures
        {
            get
            {
                return AuthorizationFailures != null && AuthorizationFailures.Any();
            }
        }
        public IRequestAuthorizationResult AddAuthorizationFailure(string property, string message)
        {
            AuthorizationFailures.Add(new Failure()
            {
                Message = message,
                Property = property
            });

            return this;
        }

        public IRequestAuthorizationResult AddAuthorizationFailure(Failure failure)
        {
            AuthorizationFailures.Add(failure);
            return this;
        }

        public static IRequestAuthorizationResult Instance()
        {
            return new RequestAuthorizationResult();
        }
    }

    public class RequestAuthorizationException : Exception
    {        
        public IRequestAuthorizationResult RequestAuthorizationResult;
        public RequestAuthorizationException(IRequestAuthorizationResult requestAuthorizationResult)
        {
            RequestAuthorizationResult = requestAuthorizationResult;
        }
        public RequestAuthorizationException(string message, IRequestAuthorizationResult requestAuthorizationResult)
            : base(message)
        {
            RequestAuthorizationResult = requestAuthorizationResult;
        }        
    }

   
}
