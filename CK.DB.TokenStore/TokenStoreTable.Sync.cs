using System;
using CK.SqlServer;
using CK.Core;

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
        /// Sets any extra data associated to this token. This data typically supports the process associated to this token.
        /// </summary>
        /// <param name="ctx">The call context to use.</param>
        /// <param name="actorId">The current actor identifier.</param>
        /// <param name="tokenId">The token identifier to configure.</param>
        /// <param name="extraData">The date. Can be null.</param>
        [SqlProcedure( "sTokenExtraDataSet" )]
        public abstract void SetExtraData( ISqlCallContext ctx, int actorId, int tokenId, byte[] extraData );

        /// <summary>
        /// Updates the token activity state and/or expiration date.
        /// </summary>
        /// <param name="ctx">The call context to use.</param>
        /// <param name="actorId">The current actor identifier.</param>
        /// <param name="tokenId">The token identifier to activate.</param>
        /// <param name="active">When not null, this is the new activity state. <c>false</c> will deactivate the token.</param>
        /// <param name="expirationDateUtc">When not null, this new expiration date must be in the future otherwise an exception is thrown.</param>
        [SqlProcedure( "sTokenActivate" )]
        public abstract void Activate( ISqlCallContext ctx, int actorId, int tokenId, bool? active = null, DateTime? expirationDateUtc = null );

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
        /// Checks whether a token is valid or not.
        /// If valid, <see cref="ITokenInfo.ValidCheckedCount"/> and <see cref="ITokenInfo.LastCheckedDate"/> will be updated.
        /// If not valid, <see cref="ITokenInfo.TokenId"/> will be zero and
        /// <see cref="ITokenInfo.ExpirationDateUtc"/> will be <see cref="Core.Util.UtcMinValue"/>.
        /// </summary>
        /// <param name="ctx">The call context to use.</param>
        /// <param name="actorId">The current actor identifier.</param>
        /// <param name="token">The token identifier.</param>
        /// <param name="safeTimeSeconds">Time security: applies only if the token is about to expire.</param>
        /// <returns>The <see cref="ITokenInfo"/>.</returns>
        [SqlProcedure( "sTokenCheck" )]
        public abstract ITokenInfo Check( ISqlCallContext ctx, int actorId, string token, int safeTimeSeconds );


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
