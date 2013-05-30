purpose:
Use the Windows Registry debugger intercept to pass file names meant for notepad.exe to a text editor of your choice. This effectively replaces notepad without having to try to overwrite the binary -- which is difficult because Windows tracks changes to system files like notepad.exe.

known issues:
May not match correctly file names that differ only by spaces in the file name. This is because hijack.exe recieves an array of strings split on witespace in the name, so spaces are removed. I use regex to attempt to guess the correct file name, but best not to name files that differ only by spaces.

compile:
csc /target:winexe .\hijack.cs

register:
Set-ItemProperty 'HKLM:\software\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\notepad.exe' -name  Debugger -value '"C:\Program Files (x86)\Utilities\hijack.exe" "C:\Program Files\EmEditor\EmEditor.exe"'

see discussion here:
http://brianreiter.org/2010/08/06/use-image-hijacking-to-globally-replace-notepad-exe/