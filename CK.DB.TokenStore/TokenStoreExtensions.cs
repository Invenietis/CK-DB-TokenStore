using System;

namespace CK.DB.TokenStore
{
    public static class TokenStoreExtensions
    {
        public static bool IsValid( this ITokenInfo @this, TimeSpan? allowedDelta = null )
        {
            return @this != null
                   && @this.TokenId > 0
                   && @this.ExpirationDateUtc > DateTime.UtcNow.Add( allowedDelta ?? TimeSpan.Zero )
                   && @this.Active;
        }
    }
}
