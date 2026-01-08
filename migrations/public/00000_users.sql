--liquibase formatted sql

--changeset omeshechkov:10
create table public.users
(
    id              bigserial    not null,
    name            varchar(255) not null,

    constraint pk_users primary key (id)
);
--rollback drop table public.users;

--changeset uamangeldiev:20
alter table public.users
    add column birth_date date;
--rollback ;
