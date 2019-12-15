using System.Collections.Generic;

namespace ApFramework.Core
{
    public static class ApFrameworkHelper
    {
        public static IDictionary<string, object> GetEmptyBag()
        {
            return new Dictionary<string, object>();
        }

        public static ProcessRequestDelegate<string> GetEmptyRequestProcess()
        {
            return async (request, contextBag, cancellationToken) => { return string.Empty; };
        }

        public static ValidateRequestDelegate GetEmptyValidator()
        {
            return async (request, contextBag, cancellationToken) => { 
                return RequestValidationResult.Instance(); 
            };
        }

        public static IRequest GetEmptyRequest()
        {
            return new Request<string>(
                    GetEmptyRequestProcess(), GetEmptyValidator()
                );            
        }

        public static void AddApFrameworkGloablAction(
              AdditionalActionDelegate action
            , AddtionalActionSequence sequence)
        {
            GlobalActions.Add(new KeyValuePair<AddtionalActionSequence, AdditionalActionDelegate>(sequence, action));
        }

        internal static List<KeyValuePair<AddtionalActionSequence, AdditionalActionDelegate>>
            GlobalActions = new List<KeyValuePair<AddtionalActionSequence, AdditionalActionDelegate>>();
    }
}
