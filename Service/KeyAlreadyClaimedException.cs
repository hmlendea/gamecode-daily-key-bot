using System;

namespace GameCodeDailyKeyBot.Service
{
    /// <summary>
    /// Account suspended exception.
    /// </summary>
    public class KeyAlreadyClaimedException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="KeyAlreadyClaimedException"/> class.
        /// </summary>
        public KeyAlreadyClaimedException()
            : base($"This account already claimed a key today")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyAlreadyClaimedException"/> class.
        /// </summary>
        /// <param name="innerException">Inner exception.</param>
        public KeyAlreadyClaimedException(Exception innerException)
            : base($"This account already claimed a key today", innerException)
        {
        }
    }
}
