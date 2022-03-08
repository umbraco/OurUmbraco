using System;

namespace Jering.Markdig.Extensions.FlexiBlocks
{
    /// <summary>
    /// Represents an unrecoverable situation caused by invalid options.
    /// </summary>
    [Serializable]
    public class OptionsException : Exception
    {
        /// <summary>
        /// Creates an <see cref="OptionsException"/>.
        /// </summary>
        public OptionsException()
        {
        }

        /// <summary>
        /// Creates an <see cref="OptionsException"/>.
        /// </summary>
        /// <param name="message">The exception's message.</param>
        public OptionsException(string message) : base(message)
        {
        }

        /// <summary>
        /// Creates an <see cref="OptionsException"/>.
        /// </summary>
        /// <param name="message">The exception's message.</param>
        /// <param name="innerException">The exception's inner exception.</param>
        public OptionsException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <summary>
        /// Creates an <see cref="OptionsException"/>.
        /// </summary>
        /// <param name="optionName">The name of the invalid option.</param>
        /// <param name="reason">The reason why the value is invalid..</param>
        /// <param name="innerException">The exception's inner exception.</param>
        public OptionsException(string optionName, string reason, Exception innerException = null) :
            base(string.Format(Strings.OptionsException_OptionsException_InvalidOption,
                    optionName,
                    reason), innerException)
        {
        }
    }
}
