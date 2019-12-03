-- SetupConfig: {}
--
-- Simple setter on ExtraData field.
-- 
create procedure CK.sTokenExtraDataSet
(
	 @ActorId int,
     @TokenId int,
     @ExtraData varbinary(max)
)
as
begin
    if @TokenId <= 0 throw 50000, 'Argument.InvalidTokenId', 1;

	--[beginsp]

	update CK.tTokenStore set ExtraData = @ExtraData where TokenId = @TokenId;

	--[endsp]
end
