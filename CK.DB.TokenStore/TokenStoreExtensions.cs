using System;

namespace CK.DB.TokenStore
{
    /// <summary>
    /// Extends <see cref="ITokenInfo"/>.
    /// </summary>
    public static class TokenStoreExtensions
    {
        /// <summary>
        /// Checks whether this <see cref="ITokenInfo"/> is not null,
        /// its <see cref="ITokenInfo.TokenId"/> is not zero,
        /// its <see cref="ITokenInfo.ExpirationDateUtc"/> is greater than now and
        /// its <see cref="ITokenInfo.Active"/> is true.
        /// </summary>
        /// <param name="this">This <see cref="ITokenInfo"/>.</param>
        /// <param name="allowedDelta">Optional that applies to <see cref="DateTime.UtcNow"/>.</param>
        /// <returns>True if this token info is valid.</returns>
        public static bool IsValid( this ITokenInfo @this, TimeSpan? allowedDelta = null )
        {
            return @this != null
                   && @this.TokenId > 0
                   && @this.ExpirationDateUtc > DateTime.UtcNow.Add( allowedDelta ?? TimeSpan.Zero )
                   && @this.Active;
        }
    }
}
