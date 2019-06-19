--SetupConfig: {}

create procedure CK.sTokenCheck
(
     @ActorId int
    ,@Token varchar(128)
    ,@TokenId int output
    ,@TokenKey nvarchar(255) output
    ,@TokenScope varchar(63) output
    ,@ExpirationDateUtc datetime2(2) output
    ,@Active bit output
    ,@LastCheckedDate datetime2(2) output
    ,@ValidCheckedCount int output
)
as
begin
    set @TokenId = cast( parsename( @Token, 2 ) as int );
    declare @TokenGuid uniqueidentifier = cast( parsename( @Token, 1 ) as uniqueidentifier );
    declare @Now datetime2(2) = sysutcdatetime();

    --[beginsp]

    declare @IsValid bit = 1;

    select
         @ExpirationDateUtc = ExpirationDateUtc
        ,@Active = Active
        ,@TokenKey = TokenKey
        ,@TokenScope = TokenScope
        ,@LastCheckedDate = @Now
        ,@ValidCheckedCount = ValidCheckedCount + 1
    from
        CK.tTokenStore
    where
            TokenId = @TokenId
        and TokenGuid = @TokenGuid;

    if @@rowcount = 0
    begin
        set @IsValid = 0;

        set @TokenId = 0;
        set @ExpirationDateUtc = '0001-01-01';
        set @Active = 0;
        set @LastCheckedDate = '0001-01-01';
        set @ValidCheckedCount = 0;

        --<OnTokenMissing />
    end

    if @IsValid = 1 and @ExpirationDateUtc < @Now
    begin
        set @IsValid = 0;

        --<OnTokenExpired />
    end

    --<AdditionalSecurity />

    if @IsValid = 1
    begin
        --<OnTokenChecked />
        declare @SafeExpires datetime2(2) = dateadd(minute, 10, @Now);

        if @ExpirationDateUtc < @SafeExpires
        begin
            set @ExpirationDateUtc = @SafeExpires;
        end

        update CK.tTokenStore set
              ExpirationDateUtc = @ExpirationDateUtc
             ,LastCheckedDate = @LastCheckedDate
             ,ValidCheckedCount = @ValidCheckedCount
        where
                TokenId = @TokenId;
    end

    --[endsp]

end
