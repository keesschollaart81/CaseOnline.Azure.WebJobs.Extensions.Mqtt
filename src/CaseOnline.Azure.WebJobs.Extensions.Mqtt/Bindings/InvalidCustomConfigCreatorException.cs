using System;

namespace CaseOnline.Azure.WebJobs.Extensions.Mqtt.Bindings
{

    public class InvalidCustomConfigCreatorException : Exception
    {
        public InvalidCustomConfigCreatorException(string message, Exception inner) : base(message, inner)
        {

        }
    }
}
