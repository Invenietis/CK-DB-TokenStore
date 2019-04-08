using System;
using CK.SqlServer;
using CK.SqlServer.Setup;

namespace CK.DB.TokenStore
{
    public abstract partial class TokenStoreTable
    {
        /// <summary>
        /// Creates a new token.
        /// </summary>
        /// <param name="ctx">The call context to use.</param>
        /// <param name="actorId">The current actor identifier (becomes the token creator).</param>
        /// <param name="info">Token information.</param>
        /// <returns>The <see cref="CreateResult"/> with the token identifier and token to use.</returns>
        [SqlProcedure( "sTokenCreate" )]
        public abstract CreateResult Create( ISqlCallContext ctx, int actorId, [ParameterSource] ITokenInfo info );

        /// <summary>
        /// Refreshes the token expiration date.
        /// </summary>
        /// <param name="ctx">The call context to use.</param>
        /// <param name="actorId">The current actor identifier.</param>
        /// <param name="tokenId">The token identifier to refresh.</param>
        /// <param name="expirationDateUtc">The new expiration date. Must be in the future.</param>
        [SqlProcedure( "sTokenRefresh" )]
        public abstract void Refresh( ISqlCallContext ctx, int actorId, int tokenId, DateTime expirationDateUtc );

        /// <summary>
        /// Updates the token activity.
        /// </summary>
        /// <param name="ctx">The call context to use.</param>
        /// <param name="actorId">The current actor identifier.</param>
        /// <param name="tokenId">The token identifier to activate.</param>
        /// <param name="active">The new activity state. <c>false</c> will deactivate the token.</param>
        /// <returns>The awaitable.</returns>
        [SqlProcedure( "sTokenActivate" )]
        public abstract void Activate( ISqlCallContext ctx, int actorId, int tokenId, bool active );

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
        public abstract ITokenInfo Check( ISqlCallContext ctx, int actorId, string token );

        /// <summary>
        /// Destroys an existing token.
        /// </summary>
        /// <param name="ctx">The call context to use.</param>
        /// <param name="actorId">The current actor identifier.</param>
        /// <param name="tokenId">The token identifier to destroy.</param>
        /// <returns>The awaitable.</returns>
        [SqlProcedure( "sTokenDestroy" )]
        public abstract void Destroy( ISqlCallContext ctx, int actorId, int tokenId );
    }
}
