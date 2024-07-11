using System;
using System.Threading;
using System.Threading.Tasks;
using CK.Core;
using CK.DB.TokenStore.Tests.Helpers;
using CK.SqlServer;
using FluentAssertions;
using NUnit.Framework;
using static CK.Testing.MonitorTestHelper;

namespace CK.DB.TokenStore.Tests
{
    [TestFixture]
    public class TokenTests
    {
        [Test]
        public void create_and_destroy()
        {
            var tokenStoreTable = SharedEngine.Map.StObjs.Obtain<TokenStoreTable>();
            using( var ctx = new SqlStandardCallContext( TestHelper.Monitor ) )
            {
                var info = tokenStoreTable.GenerateTestInvitationInfo();
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
        public async Task create_and_destroy_Async()
        {
            var tokenStoreTable = SharedEngine.Map.StObjs.Obtain<TokenStoreTable>();
            using( var ctx = new SqlStandardCallContext( TestHelper.Monitor ) )
            {
                var info = tokenStoreTable.GenerateTestInvitationInfo();
                var createResult = await tokenStoreTable.CreateAsync( ctx, 1, info );
                createResult.Success.Should().BeTrue();
                createResult.TokenId.Should().BeGreaterThan( 0 );
                createResult.Token.Should().NotBeEmpty();

                var checkResult = await tokenStoreTable.CheckAsync( ctx, 1, createResult.Token );
                checkResult.IsValid().Should().BeTrue();
                checkResult.TokenId.Should().Be( createResult.TokenId );
                checkResult.LastCheckedDate.Should().BeCloseTo( DateTime.UtcNow, TimeSpan.FromMilliseconds( 500 ) );
                checkResult.ValidCheckedCount.Should().Be( 1 );

                await tokenStoreTable.DestroyAsync( ctx, 1, createResult.TokenId );
            }
        }

        [Test]
        public void scoped_key_uniqueness()
        {
            var tokenStoreTable = SharedEngine.Map.StObjs.Obtain<TokenStoreTable>();
            using( var ctx = new SqlStandardCallContext() )
            {
                var info = tokenStoreTable.GenerateTestInvitationInfo();
                var result1 = tokenStoreTable.Create( ctx, 1, info );
                result1.Success.Should().BeTrue();
                var result2 = tokenStoreTable.Create( ctx, 1, info );
                result2.Success.Should().BeFalse();
                info.TokenScope = "SomeOtherScope";
                var result3 = tokenStoreTable.Create( ctx, 1, info );
                result3.Success.Should().BeTrue();
            }
        }

        [TestCase( "3712.ff92d601-b556-4f33-80b5-08d5a2915bb0" )]
        [TestCase( "wxcvbn.213e6220-3816-492c-830f-abeed5dd8c88" )]
        [TestCase( "3712.azertyuiop" )]
        [TestCase( "This is not.A valid token" )]
        [TestCase( "qsdfghjklm" )]
        [TestCase( null )]
        public void invalid_or_missing_token_does_not_check( string token )
        {
            var tokenStoreTable = SharedEngine.Map.StObjs.Obtain<TokenStoreTable>();
            using( var ctx = new SqlStandardCallContext() )
            {
                var result = tokenStoreTable.Check( ctx, 1, token );
                result.TokenId.Should().Be( 0 );
                result.CreatedById.Should().Be( 0 );
                result.TokenScope.Should().BeNull();
                result.TokenKey.Should().BeNull();
                result.Token.Should().Be( token );
                result.ExpirationDateUtc.Should().Be( Util.UtcMinValue );
                result.LastCheckedDate.Should().Be( Util.UtcMinValue );
                result.ValidCheckedCount.Should().Be( 0 );
            }
        }

        [Test]
        public void token_expiration_is_boosted_by_at_least_5_minutes_when_token_is_checked()
        {
            var tokenStoreTable = SharedEngine.Map.StObjs.Obtain<TokenStoreTable>();
            using( var ctx = new SqlStandardCallContext() )
            {
                var info = tokenStoreTable.GenerateTestInvitationInfo( DateTime.UtcNow.AddMinutes( 2 ) );
                var result = tokenStoreTable.Create( ctx, 1, info );
                result.Success.Should().BeTrue();

                var boosted = info.ExpirationDateUtc.AddMinutes( 5 );
                var checkResult = tokenStoreTable.Check( ctx, 1, result.Token );
                checkResult.TokenId.Should().BeGreaterThan( 0 );
                boosted.Should().BeBefore( checkResult.ExpirationDateUtc );
            }
        }

        [Test]
        public void token_expiration()
        {
            var tokenStoreTable = SharedEngine.Map.StObjs.Obtain<TokenStoreTable>();
            using( var ctx = new SqlStandardCallContext() )
            {
                var info = tokenStoreTable.GenerateTestInvitationInfo( DateTime.UtcNow.AddMilliseconds( 500 ) );
                var result = tokenStoreTable.Create( ctx, 1, info );
                result.Success.Should().BeTrue();

                Thread.Sleep( 550 );
                var startInfo = tokenStoreTable.Check( ctx, 1, result.Token );
                startInfo.IsValid().Should().BeFalse();
            }
        }

        [Test]
        public async Task token_expiration_Async()
        {
            var tokenStoreTable = SharedEngine.Map.StObjs.Obtain<TokenStoreTable>();
            using( var ctx = new SqlStandardCallContext() )
            {
                var info = tokenStoreTable.GenerateTestInvitationInfo( DateTime.UtcNow.AddMilliseconds( 500 ) );
                var result = await tokenStoreTable.CreateAsync( ctx, 1, info );
                result.Success.Should().BeTrue();

                await Task.Delay( 550 );
                var startInfo = await tokenStoreTable.CheckAsync( ctx, 1, result.Token );
                startInfo.IsValid().Should().BeFalse();
            }
        }

        [Test]
        public async Task token_expiration_and_ExtraData_set_Async()
        {
            var tokenStoreTable = SharedEngine.Map.StObjs.Obtain<TokenStoreTable>();
            using( var ctx = new SqlStandardCallContext() )
            {
                var info = tokenStoreTable.GenerateTestInvitationInfo();
                var result = await tokenStoreTable.CreateAsync( ctx, 1, info );
                var expiration = DateTime.UtcNow + TimeSpan.FromMinutes( 15 );
                result.Success.Should().BeTrue();
                await tokenStoreTable.ActivateAsync( ctx, 1, result.TokenId, null, expiration );
                var startInfo = await tokenStoreTable.CheckAsync( ctx, 1, result.Token );
                startInfo.ExpirationDateUtc.Should().BeCloseTo( expiration, TimeSpan.FromMilliseconds( 500 ) );

                await tokenStoreTable.SetExtraDataAsync( ctx, 1, result.TokenId, new byte[] { 0, 1, 2 } );
                info = await tokenStoreTable.CheckAsync( ctx, 1, result.Token );

                info.Should().BeEquivalentTo( startInfo, o => o.Excluding( i => i.ExtraData )
                                                               .Excluding( i => i.LastCheckedDate )
                                                               .Excluding( i => i.ValidCheckedCount ) );
                info.ExtraData.Should().BeEquivalentTo( new byte[] { 0, 1, 2 }, o => o.WithStrictOrdering() );
                info.ValidCheckedCount.Should().Be( startInfo.ValidCheckedCount + 1 );
                info.LastCheckedDate.Should().BeOnOrAfter( startInfo.LastCheckedDate );

                await tokenStoreTable.SetExtraDataAsync( ctx, 1, result.TokenId, null );
                info = await tokenStoreTable.CheckAsync( ctx, 1, result.Token );

                info.Should().BeEquivalentTo( startInfo, o => o.Excluding( i => i.LastCheckedDate )
                                                               .Excluding( i => i.ValidCheckedCount ) );
                info.ExtraData.Should().BeNull();
                info.ValidCheckedCount.Should().Be( startInfo.ValidCheckedCount + 2 );
                info.LastCheckedDate.Should().BeOnOrAfter( startInfo.LastCheckedDate );
            }
        }

        [Test]
        public async Task invalid_set_expiration_date_raises_an_exception_Async()
        {
            var tokenStoreTable = SharedEngine.Map.StObjs.Obtain<TokenStoreTable>();
            using( var ctx = new SqlStandardCallContext() )
            {
                var info = tokenStoreTable.GenerateTestInvitationInfo();
                var result = await tokenStoreTable.CreateAsync( ctx, 1, info );
                var expiration = DateTime.UtcNow - TimeSpan.FromMinutes( 1 );
                result.Success.Should().BeTrue();
                await tokenStoreTable
                           .Awaiting( sut => sut.ActivateAsync( ctx, 1, result.TokenId, null, expiration ) )
                           .Should().ThrowAsync<SqlDetailedException>();
            }
        }

        [Test]
        public async Task token_inactive_Async()
        {
            var tokenStoreTable = SharedEngine.Map.StObjs.Obtain<TokenStoreTable>();
            using( var ctx = new SqlStandardCallContext() )
            {
                var info = tokenStoreTable.GenerateTestInvitationInfo();
                var result = await tokenStoreTable.CreateAsync( ctx, 1, info );
                result.Success.Should().BeTrue();
                await tokenStoreTable.ActivateAsync( ctx, 1, result.TokenId, false );
                var startInfo = await tokenStoreTable.CheckAsync( ctx, 1, result.Token );
                startInfo.IsValid().Should().BeFalse();
            }
        }

        [Test]
        public async Task add_safe_time_superior_to_expiration_should_change()
        {
            var tokenStoreTable = SharedEngine.Map.StObjs.Obtain<TokenStoreTable>();
            using( var ctx = new SqlStandardCallContext() )
            {
                var dateOriginExpirationToken = DateTime.UtcNow.AddSeconds( 10 );
                var info = tokenStoreTable.GenerateTestInvitationInfo( dateOriginExpirationToken );
                var result = await tokenStoreTable.CreateAsync( ctx, 1, info );
                result.Success.Should().BeTrue();

                var startInfo = await tokenStoreTable.CheckAsync( ctx, 1, result.Token,60);
                startInfo.IsValid().Should().BeTrue();

                startInfo.ExpirationDateUtc.Should().NotBe( dateOriginExpirationToken );
            }
        }

        [Test]
        public async Task add_safe_time_inferior_to_expiration_should_not_change_Async()
        {
            var tokenStoreTable = SharedEngine.Map.StObjs.Obtain<TokenStoreTable>();
            using( var ctx = new SqlStandardCallContext() )
            {
                var dateOriginExpirationToken = DateTime.UtcNow.AddSeconds( 60 );
                var info = tokenStoreTable.GenerateTestInvitationInfo( dateOriginExpirationToken );
                var result = await tokenStoreTable.CreateAsync( ctx, 1, info );
                result.Success.Should().BeTrue();

                var startInfo = await tokenStoreTable.CheckAsync( ctx, 1, result.Token, 10 );
                startInfo.IsValid().Should().BeTrue();

                startInfo.ExpirationDateUtc.Should().BeCloseTo( dateOriginExpirationToken, TimeSpan.FromMilliseconds( 100 ) );
            }
        }
    }
}
