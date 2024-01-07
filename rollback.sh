#!/bin/bash

./migrations/liquibase-tool/liquibase.sh rollbackCount $1
