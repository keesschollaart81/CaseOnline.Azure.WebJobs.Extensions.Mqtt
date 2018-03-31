using Microsoft.Azure.WebJobs;
using System;

namespace CaseOnline.Azure.WebJobs.Extensions.Mqtt.Tests.Util.Helpers
{
    class MockNameResolver : INameResolver
    {
        public Func<string, string> OnResolve = (name) => name;

        public string Resolve(string name)
        {
            return OnResolve(name);
        }
    }
}
