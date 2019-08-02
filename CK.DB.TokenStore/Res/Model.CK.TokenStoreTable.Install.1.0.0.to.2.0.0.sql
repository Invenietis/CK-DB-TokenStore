--[beginscript]

alter table CK.tTokenStore drop constraint UK_CK_DF_CK_TokenStore_TokenKey_TokenContext;
alter table CK.tTokenStore alter column TokenKey nvarchar(255) collate Latin1_General_100_CI_AI not null;
alter table CK.tTokenStore alter column TokenScope varchar(63) collate Latin1_General_100_CI_AI not null;
alter table CK.tTokenStore drop column Token;
alter table CK.tTokenStore add Token as cast(TokenId as varchar(10)) collate Latin1_General_BIN2
                                        + '.'
                                        + cast(TokenGuid as varchar(40)) collate Latin1_General_BIN2;
alter table CK.tTokenStore add constraint UK_CK_DF_CK_TokenStore_TokenKey_TokenContext unique( TokenScope, TokenKey );

--[endscript]
