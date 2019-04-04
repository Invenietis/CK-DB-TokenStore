using System;
using System.Threading.Tasks;
using CK.Core;
using CK.Setup;
using CK.SqlServer;
using CK.SqlServer.Setup;

namespace CK.DB.TokenStore
{
    [SqlTable( "tUserSimpleInvitation", Schema = "CK", Package = typeof( Package ) ),
     Versions( "1.0.0" )]
    public abstract partial class TokenStoreTable : SqlTable
    {
        private IPocoFactory<ITokenInfo> _tokenFactory;

        internal void StObjConstruct( IPocoFactory<ITokenInfo> tokenFactory )
        {
            _tokenFactory = tokenFactory;
        }

        public T CreateInfo<T>( Action<T> configurator ) where T : ITokenInfo
            => ((IPocoFactory<T>)_tokenFactory).Create( configurator );

        public readonly struct CreateResult
        {
            public readonly int TokenId;
            public readonly string Token;

            public bool Success => TokenId > 0;

            public CreateResult( int tokenIdResult, string tokenResult )
            {
                TokenId = tokenIdResult;
                Token = tokenResult;
            }
        }

        [SqlProcedure( "sTokenCreate" )]
        public abstract Task<CreateResult> CreateAsync( ISqlCallContext ctx, int actorId, [ParameterSource] ITokenInfo info );

        [SqlProcedure( "sTokenRefresh" )]
        public abstract Task RefreshAsync( ISqlCallContext ctx, int actorId, int tokenId, DateTime expirationDateUtc );

        [SqlProcedure( "sTokenActivate" )]
        public abstract Task ActivateAsync( ISqlCallContext ctx, int actorId, int tokenId, bool active );

        [SqlProcedure( "sTokenCheck" )]
        public abstract Task<ITokenInfo> CheckAsync( ISqlCallContext ctx, int actorId, string token );

        [SqlProcedure( "sTokenDestroy" )]
        public abstract Task DestroyAsync( ISqlCallContext ctx, int actorId, int tokenId );
    }
}
