@echo off
REM Batch file to run SusEquip Mail Sender
REM This file should be scheduled to run on the 1st of every month

echo Starting SusEquip Monthly Mail Sender at %date% %time%

REM Change to the directory where your executable is located
cd /d "C:\Path\To\Your\SusEquipMailSender"

REM Run the executable
SusEquipMailSender.exe

REM Log the completion
echo SusEquip Monthly Mail Sender completed at %date% %time%

REM Optional: Log to a file
echo %date% %time% - SusEquip Monthly Mail Sender executed >> "C:\Logs\SusEquipMailSender.log"
