using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Security.Cryptography;
using System.Reflection;
using System.Collections.ObjectModel;

namespace NewPassword
{
    class Program
    {
        static void Main( string[] args )
        {
            int length = -1;
            int count  = -1;
            if( args.Length > 0 )
            { int.TryParse( args[0], out length ); }
            else
            { length = 8; }
            if( args.Length > 1 )
            { int.TryParse( args[1], out count ); }
            else
            { count = 5; }
            
            var pwdseq = new RandomCharSequence();
            for( int i = 0; i < count; i++ )
            {
                Console.WriteLine( pwdseq.Take( length ).ToArray() );
            }
        }
    }

    [Flags]
    public enum CharTypes
    {
        Lower   = 0x01,
        Upper   = 0x02,
        Digit   = 0x04,
        Special = 0x08,
        All     = Upper | Lower | Digit | Special
    }
    public class RandomCharSequence : IEnumerable<char>
    {
        static readonly IEnumerable<char> s_lower   = new[] { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'q', 'y', 'z' };
        static readonly IEnumerable<char> s_upper   = new[] { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'Q', 'Y', 'Z' };
        static readonly IEnumerable<char> s_digit   = new[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
        static readonly IEnumerable<char> s_special = new[] { '!', '"', '#', '$', '%', '&', '\'', '(', ')', '*', '+', ',', '-', '.', '/', ':', ';', '<', '=', '>', '?', '@', '[', '\\', ']', '^', '_', '`', '{', '|', '}', '~' };

        //these static properties must exactly match the flags defined in the CharTypes enum
        private static IEnumerable<Char> Lower   { get{ return s_lower; } }
        private static IEnumerable<Char> Upper   { get{ return s_upper; } }
        private static IEnumerable<Char> Digit   { get{ return s_digit; } }
        private static IEnumerable<Char> Special { get{ return s_special; } }

        public RandomCharSequence() : this( CharTypes.All ){}
        public RandomCharSequence( CharTypes charTypes )
        {
            CharTypes = charTypes;
        }

        public CharTypes CharTypes {get; set;}

        public IEnumerator<char> GetEnumerator()
        {
            List<char> pool = new List<char>();
            foreach( var type in (CharTypes[])Enum.GetValues(typeof(CharTypes) ) )
            {
                //CharTypes.All is not a single bit flag. We don't want that one.
                if( type != CharTypes.All && (CharTypes & type) == type )
                {
                    //use reflection to add the static char list properties which
                    //match the flag bits.
                    pool.AddRange( 
                        (IEnumerable<char>)typeof(RandomCharSequence).GetProperty( 
                            type.ToString(), 
                            BindingFlags.Static | BindingFlags.NonPublic 
                            ).GetValue( this, null )
                        );
                }
            }
            return new RandomCharEnumerator( pool );
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class RandomCharEnumerator : IEnumerator<char>
    {
        public RandomCharEnumerator( IList<char> pool )
        {
            Disposing = false;
            Pool      = new ReadOnlyCollection<char>( pool );
            Random    = RandomNumberGenerator.Create();
        }

        RandomNumberGenerator Random { get; set; } 
        public IList<char> Pool { get; private set; }
        int Index { get; set; }

        public char  Current
        {
            get { return Pool[Index]; }
        }
        object IEnumerator.Current { get { return Current; } }


        private bool Disposing{ get; set; }
        public void  Dispose()
        {
            //RandomNubmerGenerator only implements IDisposable as of .NET Framework 4.0
            IDisposable randDisposable = Random as IDisposable;
            if( randDisposable != null && !Disposing )
            {
                randDisposable.Dispose();
                Disposing = true;
            }
        }

        public bool  MoveNext()
        {
            Index = Random.GetInt( 0, Pool.Count );
            //infinite sequence of random chars. There's always a next one.
            return true;
        }

        public void  Reset(){ /* nothing */ }
    }

    public static class RandomNumberGeneratorExtension
    {
        public static int GetInt( this RandomNumberGenerator random, int min, int max )
        {
            if( min >= max )
                throw new InvalidOperationException( "The min value must be less than the max value." );
            byte[] buff = new byte[8];
            random.GetBytes(buff);
            long r = Math.Abs( BitConverter.ToInt64( buff, 0 ) );
            return (int)((long)min + (r % ((long)max - (long)min)));
        }
    }
}
