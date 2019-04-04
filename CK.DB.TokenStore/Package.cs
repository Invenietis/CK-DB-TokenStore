using CK.Setup;
using CK.SqlServer.Setup;

namespace CK.DB.TokenStore
{
    [SqlPackage( Schema = "CK", ResourcePath = "Res" )]
    [Versions( "1.0.0" )]
    public abstract class Package : SqlPackage
    {
        internal void StObjConstruct( CK.DB.Actor.Package actor ) { }
    }
}
