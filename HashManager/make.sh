#!/bin/sh

echo building hashgen...
mcs -out:hashgen.exe ./HashGen.cs ./HashManager.cs
echo building pwdverify.exe...
mcs -out:pwdverify.exe ./PwdVerify.cs ./HashManager.cs
echo done.