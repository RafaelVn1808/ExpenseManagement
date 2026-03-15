@echo off
:: Clique duplo para adicionar exclusao no Windows Defender (pede permissao de Admin)
cd /d "%~dp0"
powershell -Command "Start-Process powershell -ArgumentList '-ExecutionPolicy Bypass -NoProfile -File \"%~dp0Add-WindowsDefenderExclusion.ps1\"' -Verb RunAs"
