using System;

namespace CaseOnline.Azure.WebJobs.Extensions.Mqtt.Bindings
{
    /// <summary>
    /// Thrown when an invalid custom config creator is provided.
    /// </summary>
    public class InvalidCustomConfigCreatorException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidCustomConfigCreatorException"/> class.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="inner">The inner exception.</param>
        public InvalidCustomConfigCreatorException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
