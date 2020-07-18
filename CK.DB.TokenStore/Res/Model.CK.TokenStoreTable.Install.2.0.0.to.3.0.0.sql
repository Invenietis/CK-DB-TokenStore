--[beginscript]

alter table CK.tTokenStore add ExtraData varbinary(max) null;

-- Removes the previous sTokenRefresh procedure: sTokenActivate now does its job.
drop procedure CK.sTokenRefresh;

--[endscript]
