# sudo
A command line tool for rights elevation.

## About
Invoke this tool by typing `sudo <command> [arg1 [arg2 [arg3 [...]]]]` in a cmd/powershell. A UAC (if enabled) prompt will pop up and the command you've specified will be run with elevated privileges.
The difference to many other similar tools out there is that this program will not open a new console window, so if you type `sudo powershell` in a powershell, you will have an elevated powershell available in the same window.

## Installing
To use it, build the "sudo" project and copy sudo.exe and sudoRun.exe into a directory on your path. That's it!
