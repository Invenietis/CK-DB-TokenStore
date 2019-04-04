--[beginscript]

create table CK.tTokenStore
(
	TokenId int not null identity(0, 1),
	CreatedById int not null,
    TokenKey nvarchar(255) not null,
    TokenScope varchar(63) not null,
	ExpirationDateUtc datetime2(2) not null,
    Active bit not null,
	TokenGuid uniqueidentifier not null constraint DF_CK_TokenStore_TokenGuid default( newid() ),
    Token as cast(TokenId as varchar(30)) + '.' + cast(TokenGuid as varchar(40)),
    LastCheckedDate datetime2(2) not null constraint DF_CK_TokenStore_LastCheckedDate default( '0001-01-01' ),
    ValidCheckedCount int not null constraint DF_CK_TokenStore_ValidCheckedCount default( 0 ),

	constraint PK_CK_DF_CK_TokenStore_TokenGuid primary key( TokenId ),
	constraint UK_CK_DF_CK_TokenStore_TokenKey_TokenContext unique( TokenKey, TokenScope ),
	constraint FK_CK_TokenStore_CreatedById foreign key( CreatedById ) references CK.tUser( UserId )
);

insert into CK.tTokenStore(CreatedById, TokenKey, TokenScope, ExpirationDateUtc, TokenGuid, Active )
    values( 0, N'', '', N'0001-01-01', '00000000-0000-0000-0000-000000000000', 0 );

--[endscript]
