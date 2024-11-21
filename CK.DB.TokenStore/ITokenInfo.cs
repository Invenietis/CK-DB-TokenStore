using System;
using CK.Core;

namespace CK.DB.TokenStore;

/// <summary>
/// Simple poco which represents token information.
/// </summary>
public interface ITokenInfo : IPoco
{
    /// <summary>
    /// Gets the token identifier.
    /// This property should only be read from the database.
    /// </summary>
    int TokenId { get; set; }

    // <summary>
    /// Gets the actorId who created the token.
    /// This property should only be read from the database.
    /// </summary>
    int CreatedById { get; set; }

    /// <summary>
    /// Gets the token to use.
    /// This token combines the TokenId and the TokenGUID.
    /// This property should only be read from the database.
    /// </summary>
    string? Token { get; set; }

    /// <summary>
    /// The scope of the token.
    /// Functional area or token purpose. Scope + <see cref="TokenKey"/> is unique.
    /// </summary>
    string? TokenScope { get; set; }

    /// <summary>
    /// The key of the token.
    /// Key is unique in a given <see cref="TokenScope"/>.
    /// </summary>
    string? TokenKey { get; set; }

    /// <summary>
    /// Gets or sets any external data that supports the process associated to this token.
    /// </summary>
    byte[]? ExtraData { get; set; }

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
    /// This property should only be read from the database.
    /// </summary>
    DateTime LastCheckedDate { get; set; }

    /// <summary>
    /// Gets the number of valid check so far.
    /// This property should only be read from the database.
    /// </summary>
    int ValidCheckedCount { get; set; }
}
