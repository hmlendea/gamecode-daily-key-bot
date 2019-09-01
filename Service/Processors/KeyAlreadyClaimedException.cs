using System;

namespace GameCodeDailyKeyBot.Service.Processors
{
    /// <summary>
    /// Account suspended exception.
    /// </summary>
    public class KeyAlreadyClaimedException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="KeyAlreadyClaimedException"/> class.
        /// </summary>
        /// <param name="username">Account username.</param>
        public KeyAlreadyClaimedException(string username)
            : base($"The '{username}' account already claimed a key today")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyAlreadyClaimedException"/> class.
        /// </summary>
        /// <param name="username">Account username.</param>
        /// <param name="innerException">Inner exception.</param>
        public KeyAlreadyClaimedException(string username, Exception innerException)
            : base($"The '{username}' account already claimed a key today", innerException)
        {
        }
    }
}
