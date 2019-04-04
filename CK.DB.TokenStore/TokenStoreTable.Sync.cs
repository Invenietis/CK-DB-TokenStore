using System;
using CK.SqlServer;
using CK.SqlServer.Setup;

namespace CK.DB.TokenStore
{
    public abstract partial class TokenStoreTable
    {
        [SqlProcedure( "sTokenCreate" )]
        public abstract CreateResult Create( ISqlCallContext ctx, int actorId, [ParameterSource] ITokenInfo info );

        [SqlProcedure( "sTokenRefresh" )]
        public abstract void Refresh( ISqlCallContext ctx, int actorId, int tokenId, DateTime expirationDateUtc );

        [SqlProcedure( "sTokenActivate" )]
        public abstract void Activate( ISqlCallContext ctx, int actorId, int tokenId, bool active );

        [SqlProcedure( "sTokenCheck" )]
        public abstract ITokenInfo Check( ISqlCallContext ctx, int actorId, string token );

        [SqlProcedure( "sTokenDestroy" )]
        public abstract void Destroy( ISqlCallContext ctx, int actorId, int tokenId );
    }
}
