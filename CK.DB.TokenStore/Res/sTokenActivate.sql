--SetupConfig: {}
create procedure CK.sTokenActivate
(
	 @ActorId int
	,@TokenId int
	,@Active bit
)
as
begin
    if @TokenId <= 0 throw 50000, 'Argument.InvalidTokenId', 1;

	--[beginsp]

	update CK.tTokenStore set
		Active = @Active
		where
				TokenId = @TokenId;

	--[endsp]
end
