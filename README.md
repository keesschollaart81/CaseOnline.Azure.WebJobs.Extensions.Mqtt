# CaseOnline.Azure.WebJobs.Extensions.Mqtt

This is a work-in-progress of a Mqtt Trigger for Azure Functions.

The code currently works as it is but I have to make it better configurable before I publish it as a NuGet Package. Things that have to be done:
- Move the connection configuration to a connectionstring or config-thing
- Make the connection more resilient, no recovery / state logic has been implemented
- Think of how to make advanced Mqtt things like last-will, tls, QoS, CleanSession, etc. configurable
- Unit Tests & Integration tests
- extensions.json is now manually created, which should not be needed
- Use ILogger instead of TraceWriter (I think) which currently does not output for some reason?
- Figure out if this is stable in the long run, will the connection persist days (currently tested for hours)
