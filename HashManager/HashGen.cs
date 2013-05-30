//*****************************************************************
// <copyright file="HashGen.cs" company="WolfeReiter">
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
	class HashGen
	{
		public static void Main(string[] args)
		{
			string transform  = "SHA-256";
			ulong  iterations = 65536;
			int    saltbytes  = 32;
			
			if( args.Length==0)
			{
				Console.WriteLine("Usage: hashgen.exe password [algorithm] [iterations] [salt-byte-size]");
				return;
			}
			string pwd = args[0];
			
			if( args.Length>1 )
				transform = args[1].ToUpper();
			if( args.Length>2 )
				iterations = ulong.Parse(args[2]);
			if( args.Length>3 )
				saltbytes = int.Parse(args[3]);
			
			HashManager hashman = new HashManager(transform, iterations);
			byte[] salt = HashManager.GenerateSalt(saltbytes);
			byte[] hash = hashman.Encode( pwd, salt );
			
			Console.WriteLine( "hash: " + HashManager.ConvertToHexString(hash) );
			Console.WriteLine( "salt: " + HashManager.ConvertToHexString(salt) );
		
		}
	}
}