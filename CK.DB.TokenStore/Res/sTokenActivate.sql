-- SetupConfig: {}
--
-- Updates the Active status and/or the ExpirationDateUtc (to a date that MUST be in the future).
-- Using null for either of the paramaters skips the update.
--
create procedure CK.sTokenActivate
(
	 @ActorId int,
     @TokenId int,
     @Active bit = null,
     @ExpirationDateUtc datetime2(2) = null
)
as
begin
    declare @Now datetime2(2) = sysutcdatetime();
    if @TokenId <= 0 throw 50000, 'Argument.InvalidTokenId', 1;
	if @ExpirationDateUtc is not null and @ExpirationDateUtc <= @Now throw 50000, 'Argument.InvalidExpirationDateUtc', 1;

	--[beginsp]

	update CK.tTokenStore set
            Active = case when @Active is not null then @Active else Active end,
            ExpirationDateUtc = case when @ExpirationDateUtc is not null then @ExpirationDateUtc else ExpirationDateUtc end
        where TokenId = @TokenId;

	--[endsp]
end
