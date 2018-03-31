using System;
using System.Collections.Generic;
using Microsoft.Azure.WebJobs;

namespace CaseOnline.Azure.WebJobs.Extensions.Mqtt.Tests.Helpers
{
    public class TestNameResolver : INameResolver
    {
        private readonly Dictionary<string, string> _values = new Dictionary<string, string>();
        private readonly bool _throwException;

        public TestNameResolver(bool throwNotImplementedException = false)
        {
            // DefaultNameResolver throws so this helps simulate that for testing
            _throwException = throwNotImplementedException;
        }

        public Dictionary<string, string> Values => _values;

        public string Resolve(string name)
        {
            if (_throwException)
            {
                throw new NotImplementedException("INameResolver must be supplied to resolve '%" + name + "%'.");
            }

            string value = null;
            Values.TryGetValue(name, out value);
            return value;
        }
    }
}
