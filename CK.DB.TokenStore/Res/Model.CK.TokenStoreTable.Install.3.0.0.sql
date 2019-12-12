--[beginscript]

create table CK.tTokenStore
(
	TokenId int not null identity(0, 1),
	CreatedById int not null,
    -- Scope: Functional area or token purpose. Scope + TokenKey is unique.
    TokenScope varchar(63) collate Latin1_General_100_CI_AI not null,
    -- Key: Identifies the token in the Scope. This typically contains business data
    --      that describes the token and for which unicity must be enforced (i.e. an email
    --      address, the SHA1 of a document, etc.).
    TokenKey nvarchar(255) collate Latin1_General_100_CI_AI not null,
	ExpirationDateUtc datetime2(2) not null,
    Active bit not null,
	TokenGuid uniqueidentifier not null constraint DF_CK_TokenStore_TokenGuid default( newid() ),
    Token as cast(TokenId as varchar(10)) collate Latin1_General_BIN2 + '.' + cast(TokenGuid as varchar(40)) collate Latin1_General_BIN2,
    -- ExtraData can be null.
    ExtraData varbinary(max) null,
    LastCheckedDate datetime2(2) not null constraint DF_CK_TokenStore_LastCheckedDate default( '0001-01-01' ),
    ValidCheckedCount int not null constraint DF_CK_TokenStore_ValidCheckedCount default( 0 ),

	constraint PK_CK_DF_CK_TokenStore_TokenGuid primary key( TokenId ),
	constraint UK_CK_DF_CK_TokenStore_TokenKey_TokenContext unique( TokenScope, TokenKey ),
	constraint FK_CK_TokenStore_CreatedById foreign key( CreatedById ) references CK.tUser( UserId )
);

insert into CK.tTokenStore( CreatedById, TokenKey, TokenScope, ExpirationDateUtc, TokenGuid, Active, ExtraData )
    values( 0, N'', '', N'0001-01-01', '00000000-0000-0000-0000-000000000000', 0, null );

--[endscript]
