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
     Versions( "1.0.0, 2.0.0, 3.0.0" )]
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
        /// Sets any extra data associated to this token. This data typically supports the process associated to this token.
        /// </summary>
        /// <param name="ctx">The call context to use.</param>
        /// <param name="actorId">The current actor identifier.</param>
        /// <param name="tokenId">The token identifier to configure.</param>
        /// <param name="extraData">The date. Can be null.</param>
        /// <returns>The awaitable.</returns>
        [SqlProcedure( "sTokenExtraDataSet" )]
        public abstract Task SetExtraDataAsync( ISqlCallContext ctx, int actorId, int tokenId, byte[] extraData );

        /// <summary>
        /// Updates the token activity state and/or expiration date.
        /// </summary>
        /// <param name="ctx">The call context to use.</param>
        /// <param name="actorId">The current actor identifier.</param>
        /// <param name="tokenId">The token identifier to activate.</param>
        /// <param name="active">When not null, this is the new activity state. <c>false</c> will deactivate the token.</param>
        /// <param name="expirationDateUtc">When not null, this new expiration date must be in the future otherwise an exception is thrown.</param>
        /// <returns>The awaitable.</returns>
        [SqlProcedure( "sTokenActivate" )]
        public abstract Task ActivateAsync( ISqlCallContext ctx, int actorId, int tokenId, bool? active = null, DateTime? expirationDateUtc = null );

        /// <summary>
        /// Checks whether a token is valid or not.
        /// If valid, <see cref="ITokenInfo.ValidCheckedCount"/> and <see cref="ITokenInfo.LastCheckedDate"/> will be updated.
        /// If not valid, <see cref="ITokenInfo.TokenId"/> will be zero and
        /// <see cref="ITokenInfo.ExpirationDateUtc"/> will be <see cref="Core.Util.UtcMinValue"/>.
        /// By default, this uses a safe period of 600 seconds (10 minutes): whenever this check is successful, the expiration date
        /// is guaranteed to be at least in 10 minutes (it is postponed as required).
        /// </summary>
        /// <param name="ctx">The call context to use.</param>
        /// <param name="actorId">The current actor identifier.</param>
        /// <param name="token">The token identifier.</param>
        /// <returns>The <see cref="ITokenInfo"/>.</returns>
        [SqlProcedure( "sTokenCheck" )]
        public abstract Task<ITokenInfo> CheckAsync( ISqlCallContext ctx, int actorId, string token );

        /// <summary>
        /// Checks whether a token is valid or not.
        /// If valid, <see cref="ITokenInfo.ValidCheckedCount"/> and <see cref="ITokenInfo.LastCheckedDate"/> will be updated.
        /// If not valid, <see cref="ITokenInfo.TokenId"/> will be zero and
        /// <see cref="ITokenInfo.ExpirationDateUtc"/> will be <see cref="Core.Util.UtcMinValue"/>.
        /// </summary>
        /// <param name="ctx">The call context to use.</param>
        /// <param name="actorId">The current actor identifier.</param>
        /// <param name="token">The token identifier.</param>
        /// <param name="safeTimeSeconds">Time security: applies only if the token is about to expire. The expiration date
        /// is guaranteed to be at least in the given number of seconds (it is postponed as required).
        /// </param>
        /// <returns>The <see cref="ITokenInfo"/>.</returns>
        [SqlProcedure( "sTokenCheck" )]
        public abstract Task<ITokenInfo> CheckAsync( ISqlCallContext ctx, int actorId, string token, int safeTimeSeconds );

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
