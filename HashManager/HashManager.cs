//*****************************************************************
// <copyright file="HashManager.cs" company="WolfeReiter">
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
using System.Globalization;
using System.Text;
using System.Security.Cryptography;

namespace WolfeReiter.Security.Cryptography
{
	/// <summary>
	/// HashManager provides the service of cryptographically hashing and verifying strings
	/// against an existing hash. HashManager uses the SHA-256 hashing transform by default.
	/// </summary>
	/// <remarks>
	/// <para>The MD5 and SHA1 algorithms should be avoided because they are not strong enough
	/// to resist attack with modern processors. Also, it appears that the algorithms for MD5 and SHA1
	/// may have a flaw known as "collision". The short story is that  an attacker can discover the 
	/// plaintext used to generate the hash in much fewer steps than expected.</para>
	/// <para>For example, SHA1 should theoretically require 2<sup>80</sup> steps to brute-force recover 
	/// the original plaintext. Researchers have recently claimed to have a method of recovering SHA1 plaintext
	/// in 2<sup>33</sup> steps.</para></remarks>
	[Serializable]
	public sealed class HashManager 
	{
		private HashAlgorithm _transform;
		private readonly ulong _iterations;

		/// <summary>
		/// CTOR. Creates a new HashManager object that uses the SHA-256 algorithm and stretches the entropy in the hash
		/// with 2<sup>16</sup> iterations.
		/// </summary>
		public HashManager() : this("SHA-256",65536){}
		/// <summary>
		/// CTOR. Creates a new HashManager object that uses the specified well-known hash transform algorithm.
		/// </summary>
		/// <param name="transform">Well-known transform algorithm (eg. MD5, SHA-1, SHA-256, SHA-384, SHA-512, etc.).</param>
		/// <param name="iterations">Number of iterations used to sretch the entropy of the plaintext. 2<sup>16</sup> iterations
		/// is a recommended minimum.</param>
		/// <exception cref="ArgumentOutOfRangeException">Throws if iterations is less than 1.</exception>
		public HashManager(string transform, ulong iterations) : this(HashAlgorithm.Create(transform),iterations){}

		/// <summary>
		/// CTOR. Creates a new HashManager object that uses provided transform hash algorithm.
		/// </summary>
		/// <param name="transform">HashAlgorithm object to use.</param>
		/// <param name="iterations">Number of iterations used to sretch the entropy of the plaintext. 2<sup>16</sup> iterations
		/// is a recommended minimum.</param>
		/// <exception cref="ArgumentOutOfRangeException">Throws if iterations is less than 1.</exception>
		public HashManager(HashAlgorithm transform, ulong iterations)
		{
			if( iterations < 1 )
				throw new ArgumentOutOfRangeException("iterations", iterations, "The number of iterations cannot be less than 1");
			_transform  = transform;
			_iterations = iterations;
		}
		/// <summary>
		/// CTOR. Creates a new HashManager object that uses the specified well-known hash transform algorithm.
		/// </summary>
		/// <param name="transform">Well-known transform algorithm (eg. MD5, SHA-1, SHA-256, SHA-384, SHA-512, etc.).</param>
		/// <param name="iterations">Number of iterations used to sretch the entropy of the plaintext. 2<sup>16</sup> iterations
		/// is a recommended minimum.</param>
		/// <exception cref="ArgumentOutOfRangeException">Throws if iterations is less than 1.</exception>
		public HashManager(string transform, long iterations) : this(HashAlgorithm.Create(transform),iterations){}

		/// <summary>
		/// CTOR. Creates a new HashManager object that uses provided transform hash algorithm.
		/// </summary>
		/// <param name="transform">HashAlgorithm object to use.</param>
		/// <param name="iterations">Number of iterations used to sretch the entropy of the plaintext. 2<sup>16</sup> iterations
		/// is a recommended minimum.</param>
		/// <exception cref="ArgumentOutOfRangeException">Throws if iterations is less than 1.</exception>
		public HashManager(HashAlgorithm transform, long iterations)
		{
			if( iterations < 1 )
				throw new ArgumentOutOfRangeException("iterations", iterations, "The number of iterations cannot be less than 1");
			_transform  = transform;
			_iterations = (ulong)iterations;
		}
		/// <summary>
		/// Hashes a plaintext string.
		/// </summary>
		/// <param name="s">Plaintext to hash. (not nullable)</param>
		/// <param name="salt">Salt entropy to mix with the s. (not nullable)</param>
		/// <returns>HashManager byte array.</returns>
		/// <exception cref="ArgumentNullException">Throws if either the s or salt arguments are null.</exception>
		public byte[] Encode( string s, byte[] salt )
		{
			if( s==null )
				throw new ArgumentNullException("s");
			if( salt==null )
				throw new ArgumentNullException("salt");

            return Encode( ConvertStringToByteArray( s ), salt );
		}

        public byte[] Encode( byte[] plaintext, byte[] salt )
        {
            byte[] sp = salt;
            byte[] hash = plaintext;
            //stretching via multiple iterations injects entropy that makes collision plaintext recovery much
            //more difficult.
            for( ulong i = 0; i < _iterations; i++ )
            {
                sp = Salt( hash, salt );
                hash = _transform.ComputeHash( sp );
            }

            return hash;
        }

		private byte[] Salt( byte[] p, byte[] salt )
		{
			byte[] buff = new byte[p.Length + salt.Length];
			for( int i=0; i<p.Length; i++ )
				buff[i] = p[i];
			for( int i=0; i<salt.Length; i++ )
				buff[i + p.Length] = salt[i];
			
			return buff;
		}

		/// <summary>
		/// Verifies a plaintext string against an existing hash. This is a case-sensitive operation.
		/// </summary>
		/// <param name="s">Plaintext to verify</param>
		/// <param name="hash">HashManager to compare with the s</param>
		/// <param name="salt">Salt that was used with the original s to generate the hash</param>
		/// <returns>True if the s is the same as the one used to generate the original hash.
		/// Otherwise false.</returns>
		/// <exception cref="ArgumentNullException">Throws if any of the s, hash or salt arguments are null.</exception>
		public bool Verify( string s, byte[] hash, byte[] salt )
		{
			if( s == null )
				throw new ArgumentNullException("s");
			if( salt == null )
				throw new ArgumentNullException("salt");
			if( hash == null )
				throw new ArgumentNullException("hash");

            byte[] testhash = Encode( s, salt );
            return _Verify( testhash, hash );
		}

        public bool Verify( byte[] plaintext, byte[] hash, byte[] salt )
        {
            if( plaintext == null )
                throw new ArgumentNullException( "plaintext" );
            if( salt == null )
                throw new ArgumentNullException( "salt" );
            if( hash == null )
                throw new ArgumentNullException( "hash" );

            byte[] testhash = Encode( plaintext, salt );
            return _Verify( testhash, hash );
        }

        private bool _Verify( byte[] a, byte[] b )
        {
            if( a.Length != b.Length )
                return false;
            for( int i = 0; i < a.Length; i++ )
            {
                if( a[i] != b[i] )
                    return false;
            }
            return true;
        }

		/// <summary>
		/// Generates a 32 byte (256 bit) cryptographically random salt.
		/// </summary>
		/// <returns>256 element Byte array containing cryptographically random bytes.</returns>
		public static byte[] GenerateSalt()
		{
			return GenerateSalt(32);
		}
		/// <summary>
		/// Generates a cryptogrpahically random salt of arbirary size.
		/// </summary>
		/// <param name="size">size of the salt</param>
		/// <returns>Byte array containing cryptographically random bytes.</returns>
		public static byte[] GenerateSalt(int size)
		{
			Byte[] saltBuff = new Byte[size];
			RandomNumberGenerator.Create().GetBytes( saltBuff );
			return saltBuff;
		}

		/// <summary>
		/// Convert a hash (or salt or any other) byte[] to a hexadecimal string.
		/// </summary>
		/// <param name="hash">Byte[] to convert to hex.</param>
		/// <returns></returns>
		public static string ConvertToHexString( byte[] hash )
		{
			StringBuilder sb = new StringBuilder();
			foreach( byte b in hash )
			{
				string bytestr = Convert.ToInt32(b).ToString("X").ToLower();
				if( bytestr.Length==1)
					bytestr = '0' + bytestr;
				sb.Append( bytestr );
			}
			return sb.ToString();
		}

		/// <summary>
		/// Convert a hexadecimal string to byte[].
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>
		public static byte[] ConvertFromHexString( string s )
		{
			string hex = s.ToLower();
			if( hex.StartsWith("0x") )
				hex = hex.Substring(2);

			//ensure an even hex number
			hex = (hex.Length % 2 == 0) ? hex : '0' + hex;
			//2 hex digits per byte
			byte[] b = new byte[hex.Length/2];
			for(int i=0,j=0; i<hex.Length; i+=2,j++)
			{
				b[j] = byte.Parse( hex.Substring(i,2), NumberStyles.HexNumber );
			}
			return b;
		}

		/// <summary>
		/// Convert a hash (or salt or any other) byte[] to a base64 string.
		/// </summary>
		/// <param name="hash">Byte[] to convert to base64</param>
		/// <returns></returns>
		public static string ConvertToBase64String( byte[] hash )
		{
			return Convert.ToBase64String(hash);
		}
		
		/// <summary>
		/// Convert a base64 encoded string into a byte array.
		/// </summary>
		/// <param name="s">Base64 string.</param>
		/// <returns>Byte array.</returns>
		public static byte[] ConvertFromBase64String( string s )
		{
			return Convert.FromBase64String( s );
		}

		/// <summary>
		/// Converts a String to a Byte array.
		/// </summary>
		/// <remarks>Only works on .NET string objects or Unicode encoded strings.</remarks>
		/// <param name="s">String to convert</param>
		/// <returns>Byte array.</returns>
		public static byte[] ConvertStringToByteArray( string s )
		{
			if (s == null)
				return null;
			Char[] chars = s.ToCharArray();
			Encoder encoder = Encoding.Unicode.GetEncoder();
			int bytecount = encoder.GetByteCount( chars, 0, chars.Length, true );
			byte[] outbuff = new byte[bytecount];
			encoder.GetBytes(chars, 0, chars.Length, outbuff, 0, true );
			return outbuff;
		}
	}
}
