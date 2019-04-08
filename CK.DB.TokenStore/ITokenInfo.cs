using System;
using CK.Core;

namespace CK.DB.TokenStore
{
    /// <summary>
    /// Simple POCO which represents token informations.
    /// </summary>
    public interface ITokenInfo : IPoco
    {
        /// <summary>
        /// Gets the token identifier.
        /// This property can only be read from the database.
        /// </summary>
        int TokenId { get; }

        /// <summary>
        /// Gets the token to use.
        /// This token combines the TokenId and the TokenGUID.
        /// This property can only be read from the database.
        /// </summary>
        string Token { get; }

        /// <summary>
        /// The key of the token.
        /// Only one token can exist by Key in a given <see cref="TokenScope"/>.
        /// </summary>
        string TokenKey { get; set; }

        /// <summary>
        /// The scope of the token.
        /// Only one token can exist by <see cref="TokenKey"/> in a given scope.
        /// </summary>
        string TokenScope { get; set; }

        /// <summary>
        /// The expiration date. Must always be in the future.
        /// </summary>
        DateTime ExpirationDateUtc { get; set; }

        /// <summary>
        /// Gets or set whether this token is active.
        /// An inactive token acts as if it was expired.
        /// See <see cref="TokenStoreTable.Activate"/>.
        /// </summary>
        bool Active { get; set; }

        /// <summary>
        /// Gets the last time (Utc) this token has been successfully checked.
        /// Defaults to <see cref="Util.UtcMinValue"/>.
        /// This property can only be read from the database.
        /// </summary>
        DateTime LastCheckedDate { get; }

        /// <summary>
        /// Gets the number of valid check so far.
        /// This property can only be read from the database.
        /// </summary>
        int ValidCheckedCount { get; }
    }
}
