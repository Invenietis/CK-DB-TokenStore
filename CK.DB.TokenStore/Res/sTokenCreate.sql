--SetupConfig: {}

create procedure CK.sTokenCreate
(
     @ActorId int
    ,@TokenKey nvarchar(255)
    ,@TokenScope varchar(63)
    ,@ExpirationDateUtc datetime2(2)
    ,@Active bit
    ,@TokenIdResult int output
    ,@TokenResult varchar(128) output
)
as
begin
    declare @Now datetime2(2) = sysutcdatetime();
    if @ExpirationDateUtc is null or @ExpirationDateUtc <= @Now throw 50000, 'Argument.InvalidExpirationDateUtc', 1;

    --[beginsp]

    select @TokenIdResult = TokenId from CK.tTokenStore where TokenKey = @TokenKey and TokenScope = @TokenScope;
    if @@RowCount = 0
    begin

        --<PreCreate revert />

        insert into CK.tTokenStore( CreatedById, TokenKey, TokenScope, ExpirationDateUtc, Active )
            values( @ActorId, @TokenKey, @TokenScope, @ExpirationDateUtc, @Active );

        set @TokenIdResult = SCOPE_IDENTITY();
        select @TokenResult = Token from CK.tTokenStore where TokenId = @TokenIdResult;

        --<PostCreate />

    end
    else
    begin

        --<OnTokenDuplicate />
        set @TokenIdResult = 0;
        set @TokenResult = '';

    end

    --[endsp]
end
