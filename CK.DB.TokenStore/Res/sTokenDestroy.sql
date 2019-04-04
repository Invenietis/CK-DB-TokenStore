-- SetupConfig: {}

create procedure CK.sTokenDestroy
(
     @ActorId int
    ,@TokenId int
)
as
begin

    if @TokenId <= 0 throw 50000, 'Argument.InvalidTokenId', 1;

    --[beginsp]

    --<PreDestroy />

    delete from CK.tTokenStore where TokenId = @TokenId;

    --<PostDestroy revert />

    --[endsp]

end
