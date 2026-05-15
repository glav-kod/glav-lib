#!/usr/bin/env pwsh

$rollbackCount = $args[0]

.\tools\rollback.ps1 -path "./migrations" `
                -url "jdbc:postgresql://master_db:5432/glavdb" `
                -username sys `
                -password 123 `
                -rollbackCount $rollbackCount
