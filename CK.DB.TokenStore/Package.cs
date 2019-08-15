using CK.Core;

namespace CK.DB.TokenStore
{
    /// <summary>
    /// Package that supports a token store.
    /// </summary>
    [SqlPackage( Schema = "CK", ResourcePath = "Res" )]
    [Versions( "1.0.0" )]
    public abstract class Package : SqlPackage
    {
        internal void StObjConstruct( CK.DB.Actor.Package actor ) { }
    }
}
