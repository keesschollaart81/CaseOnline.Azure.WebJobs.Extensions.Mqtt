using System;
using System.Diagnostics;
using Microsoft.Azure.WebJobs.Host;

namespace CaseOnline.Azure.WebJobs.Extensions.Mqtt.Tests
{
    public class SimpleTraceWriter : TraceWriter
    {
        public SimpleTraceWriter() : base(TraceLevel.Verbose)
        {

        }
        public override void Trace(TraceEvent traceEvent)
        {
            Console.WriteLine(traceEvent.Message);
        }
    }
}
