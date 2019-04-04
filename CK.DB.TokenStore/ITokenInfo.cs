using System;
using CK.Core;

namespace CK.DB.TokenStore
{
    public interface ITokenInfo : IPoco
    {
        int TokenId { get; }
        string Token { get; }
        string TokenKey { get; set; }
        string TokenScope { get; set; }
        DateTime ExpirationDateUtc { get; set; }
        bool Active { get; set; }
        DateTime LastCheckedDate { get; }
        int ValidCheckedCount { get; }
    }
}
