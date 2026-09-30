@echo off
cd /d "%~dp0"
echo AC8 MouseYaw Bridge
echo.
echo 1. Close the game and all old MouseYaw processes.
echo 2. Unplug / switch off the keyboard.
echo 3. Press any key here to start MouseYawBridge.
echo 4. After it starts, check that the virtual pad log says slot 0.
echo 5. Reconnect / switch on the keyboard.
echo 6. Launch the game.
echo.
echo Commands: list, set x_dpi 3000, set y_dpi 7500, set x_deadzone 0, set y_deadzone 0, set x_boost 0, set y_boost 0, set hold 16, exit
echo.
pause
"%~dp0MouseYawBridge.exe"
