#!/usr/bin/env bash

chmod 755 ./migrations/liquibase-tool/liquibase
./migrations/src/liquibase-tool/liquibase migrate 2>&1
