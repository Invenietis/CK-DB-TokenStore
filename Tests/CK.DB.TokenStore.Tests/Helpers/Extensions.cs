using System;

namespace CK.DB.TokenStore.Tests.Helpers
{
    public static class Extensions
    {
        public static ITokenInfo GenerateTestInvitationInfo( this TokenStoreTable @this, DateTime? expires = null )
            => @this.CreateInfo<ITokenInfo>( info =>
            {
                info.TokenKey = Guid.NewGuid().ToString();
                info.TokenScope = "CK.DB.TokenStore.Tests";
                info.ExpirationDateUtc = expires ?? DateTime.UtcNow + TimeSpan.FromMinutes( 1 );
                info.Active = true;
            } );

    }
}
