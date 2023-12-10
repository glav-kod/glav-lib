create user sys with replication encrypted password '123';
create database glavdb with owner sys;
select pg_create_physical_replication_slot('replication_slot');
