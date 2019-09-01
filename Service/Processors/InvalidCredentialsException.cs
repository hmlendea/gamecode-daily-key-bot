using System;

namespace GameCodeDailyKeyBot.Service.Processors
{
    /// <summary>
    /// Account suspended exception.
    /// </summary>
    public class InvalidCredentialsException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidCredentialsException"/> class.
        /// </summary>
        /// <param name="username">Account username.</param>
        public InvalidCredentialsException(string username)
            : base($"The '{username}' account has invalid credentials")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidCredentialsException"/> class.
        /// </summary>
        /// <param name="username">Account username.</param>
        /// <param name="innerException">Inner exception.</param>
        public InvalidCredentialsException(string username, Exception innerException)
            : base($"The '{username}' account has invalid credentials", innerException)
        {
        }
    }
}
