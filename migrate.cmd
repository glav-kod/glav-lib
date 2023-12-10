@echo off
chcp 1251 > nul
cls

.\migrations\liquibase-tool\liquibase.bat migrate 2>&1