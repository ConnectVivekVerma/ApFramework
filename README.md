# ApFramework
Application framework for managing cross cutting concerns
ApFramework abstracts out cross cutting concerns allowing develper to focus on allocated task like Saving an entity.

Usually in applciaition call hierarchy is:
Action Method -> Business Layer -> Data Layer.

Using ApFramework a request is created that wraps call to Business Layer
Action Method -> RequestProcessor.Process(request)
i.e. Create a request and give it to RequestProcessor to process request. 
Request Processor is set up to call various functions for cross cutting concerns like authorization, validation, logging, performance monitoring etc. Hence these concerns do not spread across various layers polluting code.
