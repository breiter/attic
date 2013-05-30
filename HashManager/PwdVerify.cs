//*****************************************************************
// <copyright file="PwdVerify.cs" company="WolfeReiter">
// Copyright (c) 2005 WolfeReiter, LLC.
// </copyright>
//*****************************************************************

/*
** Copyright (c) 2005 WolfeReiter, LLC
**
** This software is provided 'as-is', without any express or implied warranty. In no 
** event will the authors be held liable for any damages arising from the use of 
** this software.
**
** Permission is granted to anyone to use this software for any purpose, including 
** commercial applications, and to alter it and redistribute it freely, subject to 
** the following restrictions:
**
**    1. The origin of this software must not be misrepresented; you must not claim 
**       that you wrote the original software. If you use this software in a product,
**       an acknowledgment in the product documentation would be appreciated but is 
**       not required.
**
**    2. Altered source versions must be plainly marked as such, and must not be 
**       misrepresented as being the original software.
**
**    3. This notice may not be removed or altered from any source distribution.
**
*/

using System;
using System.IO;
using System.Security;
using System.Security.Cryptography;
using System.Text;

namespace WolfeReiter.Security.Cryptography
{
	class PwdVerify
	{
		public static void Main(string[] args)
		{
			string transform  = "SHA-256";
			ulong  iterations = 65536;
			
			if( args.Length<3)
			{
				Console.WriteLine("Usage: pwdverify.exe password hash salt [algorithm] [iterations]");
				return;
			}
			string pwd  = args[0];
			string hash = args[1];
			string salt = args[2];
			
			if( args.Length>3 )
				transform = args[3].ToUpper();
			if( args.Length>4 )
				iterations = ulong.Parse(args[4]);
			
			HashManager hashman = new HashManager(transform, iterations);
			byte[] hashbuff = HashManager.ConvertFromHexString(hash);
			byte[] saltbuff = HashManager.ConvertFromHexString(salt);
			
			Console.WriteLine( hashman.Verify( pwd, hashbuff, saltbuff ) ? "OK" : "INVALID PASSWORD" );
		}
	}
}