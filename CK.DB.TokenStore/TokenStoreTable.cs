using System;
using System.Threading.Tasks;
using CK.Core;
using CK.SqlServer;

namespace CK.DB.TokenStore
{
    /// <summary>
    /// The tTokenStore table contains tokens.
    /// </summary>
    [SqlTable( "tTokenStore", Package = typeof( Package ) ),
     Versions( "1.0.0, 2.0.0, 2.0.1" )]
    public abstract partial class TokenStoreTable : SqlTable
    {
        private IPocoFactory<ITokenInfo> _tokenFactory;

        internal void StObjConstruct( IPocoFactory<ITokenInfo> tokenFactory )
        {
            _tokenFactory = tokenFactory;
        }

        /// <summary>
        /// Creates a new <see cref="ITokenInfo"/> poco.
        /// </summary>
        /// <returns>A poco instance.</returns>
        public ITokenInfo CreateInfo() => _tokenFactory.Create();

        /// <summary>
        /// Creates and configures a new <see cref="ITokenInfo"/> poco.
        /// </summary>
        /// <typeparam name="T">The actual poco type to create.</typeparam>
        /// <param name="configurator">Configuration function.</param>
        /// <returns>A new configured poco instance.</returns>
        public T CreateInfo<T>( Action<T> configurator ) where T : ITokenInfo
            => ((IPocoFactory<T>)_tokenFactory).Create( configurator );

        /// <summary>
        /// Captures the result of <see cref="Create"/> or <see cref="CreateAsync"/> calls.
        /// </summary>
        public readonly struct CreateResult
        {
            /// <summary>
            /// The token identifier.
            /// </summary>
            public readonly int TokenId;

            /// <summary>
            /// The token to use.
            /// </summary>
            public readonly string Token;

            /// <summary>
            /// Gets whether the token has been successfully created.
            /// </summary>
            public bool Success => TokenId > 0;

            /// <summary>
            /// Initializes a new <see cref="CreateResult"/>.
            /// </summary>
            /// <param name="tokenIdResult">The token identifier.</param>
            /// <param name="tokenResult">The token.</param>
            public CreateResult( int tokenIdResult, string tokenResult )
            {
                TokenId = tokenIdResult;
                Token = tokenResult;
            }
        }

        /// <summary>
        /// Creates a new token.
        /// </summary>
        /// <param name="ctx">The call context to use.</param>
        /// <param name="actorId">The current actor identifier (becomes the token creator).</param>
        /// <param name="info">Token information.</param>
        /// <returns>The <see cref="CreateResult"/> with the token identifier and token to use.</returns>
        [SqlProcedure( "sTokenCreate" )]
        public abstract Task<CreateResult> CreateAsync( ISqlCallContext ctx, int actorId, [ParameterSource] ITokenInfo info );

        /// <summary>
        /// Refreshes the token expiration date.
        /// </summary>
        /// <param name="ctx">The call context to use.</param>
        /// <param name="actorId">The current actor identifier.</param>
        /// <param name="tokenId">The token identifier to refresh.</param>
        /// <param name="expirationDateUtc">The new expiration date. Must be in the future.</param>
        /// <returns>The awaitable.</returns>
        [SqlProcedure( "sTokenRefresh" )]
        public abstract Task RefreshAsync( ISqlCallContext ctx, int actorId, int tokenId, DateTime expirationDateUtc );

        /// <summary>
        /// Updates the token activity.
        /// </summary>
        /// <param name="ctx">The call context to use.</param>
        /// <param name="actorId">The current actor identifier.</param>
        /// <param name="tokenId">The token identifier to activate.</param>
        /// <param name="active">The new activity state. <c>false</c> will deactivate the token.</param>
        /// <returns>The awaitable.</returns>
        [SqlProcedure( "sTokenActivate" )]
        public abstract Task ActivateAsync( ISqlCallContext ctx, int actorId, int tokenId, bool active );

        /// <summary>
        /// Checks whether a token is valid or not.
        /// If valid, <see cref="ITokenInfo.ValidCheckedCount"/> and <see cref="ITokenInfo.LastCheckedDate"/> will be updated.
        /// If not valid, <see cref="ITokenInfo.TokenId"/> will be zero and
        /// <see cref="ITokenInfo.ExpirationDateUtc"/> will be <see cref="Core.Util.UtcMinValue"/>.
        /// </summary>
        /// <param name="ctx">The call context to use.</param>
        /// <param name="actorId">The current actor identifier.</param>
        /// <param name="token">The token identifier.</param>
        /// <returns>The <see cref="ITokenInfo"/>.</returns>
        [SqlProcedure( "sTokenCheck" )]
        public abstract Task<ITokenInfo> CheckAsync( ISqlCallContext ctx, int actorId, string token );

        /// <summary>
        /// Destroys an existing token.
        /// </summary>
        /// <param name="ctx">The call context to use.</param>
        /// <param name="actorId">The current actor identifier.</param>
        /// <param name="tokenId">The token identifier to destroy.</param>
        /// <returns>The awaitable.</returns>
        [SqlProcedure( "sTokenDestroy" )]
        public abstract Task DestroyAsync( ISqlCallContext ctx, int actorId, int tokenId );
    }
}
