@echo off

docker stop glav-master-db
docker stop glav-replica-db

docker rm glav-master-db
docker rm glav-replica-db

docker volume rm glavlib_postgres-master-data
docker volume rm glavlib_postgres-replica-data
