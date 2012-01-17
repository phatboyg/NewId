namespace NewId
{
    using System;
    using System.Diagnostics.Contracts;

    /// <summary>
    /// A NewId is a type that fits into the same space as a Guid/Uuid/uniqueidentifier,
    /// but is guaranteed to be both unique and ordered, assuming it is generated using
    /// a single instance of the generator for each network address used.
    /// </summary>
    public struct NewId :
        IEquatable<NewId>,
        IComparable<NewId>,
        IFormattable
    {
        const int ByteLength = 16;
        static NewId _empty;
        static byte[] _emptyBytes = new byte[ByteLength] {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0};
        readonly byte[] _bytes;

        /// <summary>
        /// Creates a NewId using the specified byte array.
        /// </summary>
        /// <param name="bytes"></param>
        public NewId(byte[] bytes)
        {
            Contract.Requires(bytes != null, "bytes cannot be null");
            Contract.Requires(bytes.Length == ByteLength, "Exactly 16 bytes expected");
            Contract.EndContractBlock();

            _bytes = bytes;
        }

        public NewId(string value)
        {
            Contract.Requires(value != null, "value cannot be null");
            Contract.Requires(value.Length > 0, "value cannot be empty");
            Contract.EndContractBlock();

            var guid = new Guid(value);

            _bytes = guid.ToByteArray();
        }

        public static NewId Empty
        {
            get { return _empty; }
        }

        public int CompareTo(NewId other)
        {
            for (int i = 0; i < ByteLength; i++)
            {
                byte myByte = _bytes[i];
                byte otherByte = other._bytes[i];

                if (myByte == otherByte)
                    continue;

                return myByte > otherByte ? 1 : -1;
            }

            return 0;
        }

        public bool Equals(NewId other)
        {
            return Equals(other._bytes, _bytes);
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            if (string.IsNullOrEmpty(format))
                format = "D";

            if (format.Length != 1)
            {
                throw new FormatException("The format string must be exactly one character or null");
            }

            char formatCh = format[0];

            var result = new char[38];
            int offset = 0;
            int length = 38;

            bool dash = true;

            if (formatCh == 'D' || formatCh == 'd')
            {
                length = 36;
            }
            else if (formatCh == 'N' || formatCh == 'n')
            {
                length = 32;
                dash = false;
            }
            else if (formatCh == 'B' || formatCh == 'b')
            {
                result[offset++] = '{';
                result[37] = '}';
            }
            else if (formatCh == 'P' || formatCh == 'p')
            {
                result[offset++] = '(';
                result[37] = ')';
            }
            else
            {
                throw new FormatException("The format string was not valid");
            }

            byte[] bytes = _bytes ?? _emptyBytes;

            offset = HexToChars(result, offset, bytes[0]);
            offset = HexToChars(result, offset, bytes[1]);
            offset = HexToChars(result, offset, bytes[2]);
            offset = HexToChars(result, offset, bytes[3]);
            if (dash)
                result[offset++] = '-';
            offset = HexToChars(result, offset, bytes[4]);
            offset = HexToChars(result, offset, bytes[5]);
            if (dash)
                result[offset++] = '-';
            offset = HexToChars(result, offset, bytes[6]);
            offset = HexToChars(result, offset, bytes[7]);
            if (dash)
                result[offset++] = '-';
            offset = HexToChars(result, offset, bytes[8]);
            offset = HexToChars(result, offset, bytes[9]);
            if (dash)
                result[offset++] = '-';
            offset = HexToChars(result, offset, bytes[10]);
            offset = HexToChars(result, offset, bytes[11]);
            offset = HexToChars(result, offset, bytes[12]);
            offset = HexToChars(result, offset, bytes[13]);
            offset = HexToChars(result, offset, bytes[14]);
            HexToChars(result, offset, bytes[15]);

            return new string(result, 0, length);
        }

        public byte[] ToByteArray()
        {
            var result = new byte[ByteLength];

            Buffer.BlockCopy(_bytes ?? _emptyBytes, 0, result, 0, ByteLength);

            return result;
        }

        public override string ToString()
        {
            return ToString("D", null);
        }

        public string ToString(string format)
        {
            return ToString(format, null);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (obj.GetType() != typeof (NewId)) return false;
            return Equals((NewId) obj);
        }

        public override int GetHashCode()
        {
            return (_bytes != null ? _bytes.GetHashCode() : 0);
        }

        static char HexToChar(int value)
        {
            value = value & 0xf;
            return (char) ((value > 9) ? value - 10 + 0x61 : value + 0x30);
        }

        static int HexToChars(char[] chars, int offset, int value)
        {
            chars[offset++] = HexToChar(value >> 4);
            chars[offset++] = HexToChar(value);

            return offset;
        }
    }
}