using System;
using System.Threading.Tasks;
using CK.Core;
using CK.DB.TokenStore.Tests.Helpers;
using CK.SqlServer;
using FluentAssertions;
using NUnit.Framework;

using static CK.Testing.DBSetupTestHelper;

namespace CK.DB.TokenStore.Tests
{
    [TestFixture]
    public class TokenTests
    {
        [Test]
        public void create_and_destroy()
        {
            var tokenStoreTable = TestHelper.StObjMap.StObjs.Obtain<TokenStoreTable>();

            using( var ctx = new SqlStandardCallContext( TestHelper.Monitor ) )
            {
                var info = tokenStoreTable.GenerateInvitationInfo();
                var createResult = tokenStoreTable.Create( ctx, 1, info );
                createResult.Success.Should().BeTrue();
                createResult.TokenId.Should().BeGreaterThan( 0 );
                createResult.Token.Should().NotBeEmpty();
                var checkResult = tokenStoreTable.Check( ctx, 1, createResult.Token );
                checkResult.IsValid().Should().BeTrue();
                checkResult.TokenId.Should().Be( createResult.TokenId );
                checkResult.LastCheckedDate.Should().BeCloseTo( DateTime.UtcNow, TimeSpan.FromMilliseconds( 500 ) );
                checkResult.ValidCheckedCount.Should().Be( 1 );
                tokenStoreTable.Destroy( ctx, 1, createResult.TokenId );
            }
        }

        [Test]
        public void scoped_key_uniqueness()
        {
            var tokenStoreTable = TestHelper.StObjMap.StObjs.Obtain<TokenStoreTable>();
            using( var ctx = new SqlStandardCallContext() )
            {
                var info = tokenStoreTable.GenerateInvitationInfo();
                var result1 = tokenStoreTable.Create( ctx, 1, info );
                result1.Success.Should().BeTrue();
                var result2 = tokenStoreTable.Create( ctx, 1, info );
                result2.Success.Should().BeFalse();
                info.TokenScope = "SomeOtherScope";
                var result3 = tokenStoreTable.Create( ctx, 1, info );
                result3.Success.Should().BeTrue();
            }
        }

        [Test]
        public void unexisting_token_does_not_check()
        {
            var tokenStoreTable = TestHelper.StObjMap.StObjs.Obtain<TokenStoreTable>();
            using( var ctx = new SqlStandardCallContext() )
            {
                var token = $"3712.{Guid.NewGuid().ToString()}";
                var result = tokenStoreTable.Check( ctx, 1, token );
                result.TokenId.Should().Be( 0 );
                result.TokenScope.Should().BeNull();
                result.TokenKey.Should().BeNull();
                result.Token.Should().Be( token );
                result.ExpirationDateUtc.Should().Be( Util.UtcMinValue );
                result.LastCheckedDate.Should().Be( Util.UtcMinValue );
                result.ValidCheckedCount.Should().Be( 0 );
            }
        }

        [Test]
        public void invalid_token_raises_an_exception()
        {
            var tokenStoreTable = TestHelper.StObjMap.StObjs.Obtain<TokenStoreTable>();
            using( var ctx = new SqlStandardCallContext() )
            {
                const string token = "This is not.A valid token";
                tokenStoreTable
                    .Invoking( sut => sut.Check( ctx, 1, token ) )
                    .Should().Throw<SqlDetailedException>();
            }
        }

        [Test]
        public void token_expiration_is_boosted_by_at_least_5_minutes_when_token_is_checked()
        {
            var tokenStoreTable = TestHelper.StObjMap.StObjs.Obtain<TokenStoreTable>();
            using( var ctx = new SqlStandardCallContext() )
            {
                var info = tokenStoreTable.GenerateInvitationInfo( DateTime.UtcNow.AddMinutes( 2 ) );
                var result = tokenStoreTable.Create( ctx, 1, info );
                result.Success.Should().BeTrue();

                var boosted = info.ExpirationDateUtc.AddMinutes( 5 );
                var checkResult = tokenStoreTable.Check( ctx, 1, result.Token );
                checkResult.TokenId.Should().BeGreaterThan( 0 );
                boosted.Should().BeBefore( checkResult.ExpirationDateUtc );
            }
        }

        [Test]
        public async Task token_expiration()
        {
            var tokenStoreTable = TestHelper.StObjMap.StObjs.Obtain<TokenStoreTable>();
            using( var ctx = new SqlStandardCallContext() )
            {
                var info = tokenStoreTable.GenerateInvitationInfo( DateTime.UtcNow.AddMilliseconds( 500 ) );
                var result = await tokenStoreTable.CreateAsync( ctx, 1, info );
                result.Success.Should().BeTrue();

                await Task.Delay( 550 );
                var startInfo = await tokenStoreTable.CheckAsync( ctx, 1, result.Token );
                startInfo.IsValid().Should().BeFalse();
            }
        }

        [Test]
        public async Task token_expiration_refresh()
        {
            var tokenStoreTable = TestHelper.StObjMap.StObjs.Obtain<TokenStoreTable>();
            using( var ctx = new SqlStandardCallContext() )
            {
                var info = tokenStoreTable.GenerateInvitationInfo();
                var result = await tokenStoreTable.CreateAsync( ctx, 1, info );
                var expiration = DateTime.UtcNow + TimeSpan.FromMinutes( 15 );
                result.Success.Should().BeTrue();
                tokenStoreTable.Refresh( ctx, 1, result.TokenId, expiration);
                var startInfo = await tokenStoreTable.CheckAsync( ctx, 1, result.Token );
                startInfo.ExpirationDateUtc.Should().BeCloseTo( expiration, TimeSpan.FromMilliseconds( 500 ) );
            }
        }

        [Test]
        public async Task invalid_refresh_raises_an_exception()
        {
            var tokenStoreTable = TestHelper.StObjMap.StObjs.Obtain<TokenStoreTable>();
            using( var ctx = new SqlStandardCallContext() )
            {
                var info = tokenStoreTable.GenerateInvitationInfo();
                var result = await tokenStoreTable.CreateAsync( ctx, 1, info );
                var expiration = DateTime.UtcNow - TimeSpan.FromMinutes( 1 );
                result.Success.Should().BeTrue();
                tokenStoreTable
                    .Invoking( sut => sut.Refresh( ctx, 1, result.TokenId, expiration ) )
                    .Should().Throw<SqlDetailedException>();
            }
        }

        [Test]
        public async Task token_inactive()
        {
            var tokenStoreTable = TestHelper.StObjMap.StObjs.Obtain<TokenStoreTable>();
            using( var ctx = new SqlStandardCallContext() )
            {
                var info = tokenStoreTable.GenerateInvitationInfo();
                var result = await tokenStoreTable.CreateAsync( ctx, 1, info );
                result.Success.Should().BeTrue();
                tokenStoreTable.Activate( ctx, 1, result.TokenId, false );
                var startInfo = await tokenStoreTable.CheckAsync( ctx, 1, result.Token );
                startInfo.IsValid().Should().BeFalse();
            }
        }
    }
}
