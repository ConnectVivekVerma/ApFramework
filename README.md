# ApFramework

ApFramework abstracts out cross cutting concerns like authorization, validation, logging, performance monitoring etc. hence keeps code clean without polluting other layers of code.

Additionally it gives predefined functionality to add validation and authorization to processing requests.

### To begin lets assume we have a function to Save Empoyee.
 ```csharp
 async Task<bool> EmployeeSaverFunction(EmployeeModel employee) 
 {
	 // Save employee  
 }
 ```

###  Let's wrap this in a Request and pass it to RequestProcessor. 
```csharp
var saveEmployeeRequest = new Request<bool>(SaveEmployee);
saveEmployeeRequest.RequestBag.Add("EmplooyeeModel", employeeModel);
await requestProcessor.ProcessRequestAsync(saveEmployeeRequest);

// Wrapping function:
private async Task<bool> SaveEmployee(
    IRequest request 
   ,IDictionary<string, object> contextBag
   ,CancellationToken cancellationToken)
        {
	        var model = request.RequestBag["EmplooyeeModel"] as EmployeeModel; 
            return await EmployeeSaverFunction(model);             
        }
```

We can add any number of objects to request bag that can be used by wrapping delegate to use as parameters or context sharing.



### How about validating request.
We can add **Validator delegate** to request which will be called by RequestProcessor before processing Request. If validation fails then validation exception is thrown.

Create Validator 


```csharp
async Task<IRequestValidationResult> ValidateSaveEmployeeRequest(
            IRequest request,
            IDictionary<string, object> contextBag,
            CancellationToken cancellationToken
            )
        {
            var model = request.RequestBag["EmplooyeeModel"] as EmployeeModel;
            return await ValidationFactory.ValidateSaveEmployeeRequest(model);
        }
 ```

Add validator to request.
```csharp
try
  {
     var saveEmployeeRequest = new Request<bool>(SaveEmployee, ValidateSaveEmployeeRequest);
     .....
  }
 catch(RequestValidationEception requestValidationException)
  {
      HandleValidationException(requestValidationException);
  }
```
### How about Request Authorization. 
Similar to request valdation we can add function to authorize request, Failed request authorization will throw **RequestAuthorizationException**


### Great, but I want to add Global actions which are called at various stages of request processing.
Alright, lets take an example where I wish to log performance of each request processed. So we will register a function that Request Processor will call at start of processing request. This can be called at starting point of application like startup.cs to register global actions.
```csharp
ApFrameworkHelper.AddApFrameworkGloablAction(StartPerformanceLog, AddtionalActionSequence.First);
```

Then we add a couple of actions at end.
```csharp
ApFrameworkHelper.AddApFrameworkGloablAction(StopPerformanceLog, AddtionalActionSequence.Last);
ApFrameworkHelper.AddApFrameworkGloablAction(SavePerformanceLog, AddtionalActionSequence.Last);
```

We can inject actions at:
```csharp
public enum AddtionalActionSequence
    {
        First = 0,
        Last = 1,
        BeforeAuthorization = 2,
        AfterAuthorization = 3,
        BeforeValidation = 4,
        AfterValidation = 5,
        BeforeRequestProcessing = 6,
        AfterRequestProcessing = 7
    }
```

Sample implementation of Performance logging functions. 
```csharp

async Task StartPerformanceLog(IRequest request, IDictionary<string, object> contextBag, CancellationToken cancellationToken)
  {
      var stopWatch = Stopwatch.StartNew();
      contextBag.Add("GlobalPerfromanceStopWatch", stopWatch);
      await Task.CompletedTask;
  }

async Task StopPerformanceLog(IRequest request, IDictionary<string, object> contextBag, CancellationToken cancellationToken)
  {
       var watch = contextBag["GlobalPerfromanceStopWatch"] as Stopwatch;
       watch.Stop();
       request.RequestBag.Add("ElapsedTime", watch.ElapsedMilliseconds);
       await Task.CompletedTask;
  }

async Task SavePerformanceLog(IRequest request, IDictionary<string, object> contextBag, CancellationToken cancellationToken)
  {
       await SavePerformanceStats(request);
  }
```

