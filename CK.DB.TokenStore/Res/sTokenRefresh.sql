--SetupConfig: {}
create procedure CK.sTokenRefresh
(
	 @ActorId int
	,@TokenId int
	,@ExpirationDateUtc datetime2(2)
)
as
begin
    declare @Now datetime2(2) = sysutcdatetime();
    if @TokenId <= 0 throw 50000, 'Argument.InvalidTokenId', 1;
	if @ExpirationDateUtc <= @Now throw 50000, 'Argument.InvalidExpirationDateUtc', 1;

	--[beginsp]

	update CK.tTokenStore set
		ExpirationDateUtc = @ExpirationDateUtc
		where
				TokenId = @TokenId;

	--[endsp]
end
