#!/usr/bin/env pwsh

.\tools\migrate.ps1 -path "./migrations" `
                    -url "jdbc:postgresql://master_db:5432/glavdb" `
                    -username sys `
                    -password 123
