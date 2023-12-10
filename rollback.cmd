@echo off
chcp 1251 > nul

liquibase.bat rollbackCount %1 2>&1