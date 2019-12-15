using System;
using System.Collections.Generic;
using System.Linq;

namespace ApFramework.Core
{
    public interface IRequestValidationResult
    {
        ICollection<Failure> ValidationFailures { get; }
        bool HasFailures { get; }
        IRequestValidationResult AddValidationFailure(string property, string message);
        IRequestValidationResult AddValidationFailure(Failure failure);
    }

    public class RequestValidationResult : IRequestValidationResult
    {
        public ICollection<Failure> ValidationFailures { get; } = new List<Failure>();
        public bool HasFailures
        {
            get
            {
                return ValidationFailures != null && ValidationFailures.Any();
            }
        }

        public IRequestValidationResult AddValidationFailure(string property, string message)
        {
            ValidationFailures.Add(new Failure()
            {
                Message = message,
                Property = property
            });

            return this;
        }

        public IRequestValidationResult AddValidationFailure(Failure failure)
        {
            ValidationFailures.Add(failure);
            return this;
        }

        public static IRequestValidationResult Instance()
        {
            return new RequestValidationResult();
        }
    }

    public class RequestValidationEception : Exception
    {
        public IRequestValidationResult RequestValidaionResult { get; }

        public RequestValidationEception(IRequestValidationResult validationResult)
        {
            this.RequestValidaionResult = validationResult;
        }

        public RequestValidationEception(string message, IRequestValidationResult validationResult) : base(message)
        {
            this.RequestValidaionResult = validationResult;
        }
    }    
}
