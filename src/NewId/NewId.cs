namespace NewId
{
    using System;
    using System.Diagnostics.Contracts;
    using Providers;

    /// <summary>
    /// A NewId is a type that fits into the same space as a Guid/Uuid/uniqueidentifier,
    /// but is guaranteed to be both unique and ordered, assuming it is generated using
    /// a single instance of the generator for each network address used.
    /// </summary>
    public struct NewId :
        IEquatable<NewId>,
        IComparable<NewId>,
        IComparable,
        IFormattable
    {
        static NewId _empty;
        static NewIdGenerator _generator;
        static WorkerIdProvider _workerIdProvider;
        static TickProvider _tickProvider;

        readonly Int32 _a;
        readonly Int32 _b;
        readonly Int32 _c;
        readonly Int32 _d;

        /// <summary>
        /// Creates a NewId using the specified byte array.
        /// </summary>
        /// <param name="bytes"></param>
        public NewId(byte[] bytes)
        {
            Contract.Requires(bytes != null, "bytes cannot be null");
            Contract.Requires(bytes.Length == 16, "Exactly 16 bytes expected");
            Contract.EndContractBlock();

            ConvertBytes(bytes, out _a, out _b, out _c, out _d);
        }

        public NewId(string value)
        {
            Contract.Requires(value != null, "value cannot be null");
            Contract.Requires(value.Length > 0, "value cannot be empty");
            Contract.EndContractBlock();

            var guid = new Guid(value);

            byte[] bytes = guid.ToByteArray();

            ConvertBytes(bytes, out _a, out _b, out _c, out _d);
        }

        public NewId(int a, int b, int c, int d)
        {
            _a = a;
            _b = b;
            _c = c;
            _d = d;
        }

        public static NewId Empty
        {
            get { return _empty; }
        }

        static NewIdGenerator Generator
        {
            get { return _generator ?? (_generator = new NewIdGenerator(TickProvider, WorkerIdProvider)); }
        }

        static WorkerIdProvider WorkerIdProvider
        {
            get { return _workerIdProvider ?? (_workerIdProvider = new NetworkAddressWorkerIdProvider()); }
        }

        static TickProvider TickProvider
        {
            get { return _tickProvider ?? (_tickProvider = new DateTimeTickProvider()); }
        }

        public int CompareTo(object obj)
        {
            if (obj == null)
                return 1;
            if (!(obj is NewId))
                throw new ArgumentException("Argument must be a NewId");

            return CompareTo((NewId)obj);
        }

        public int CompareTo(NewId other)
        {
            if (_a != other._a)
                return ((uint) _a < (uint) other._a) ? -1 : 1;
            if (_b != other._b)
                return ((uint) _b < (uint) other._b) ? -1 : 1;
            if (_c != other._c)
                return ((uint) _c < (uint) other._c) ? -1 : 1;
            if (_d != other._d)
                return ((uint) _d < (uint) other._d) ? -1 : 1;

            return 0;
        }

        public bool Equals(NewId other)
        {
            return other._a == _a && other._b == _b && other._c == _c && other._d == _d;
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
            const int alpha = 0x61;

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

            offset = TwoBytesToChars(result, offset, _a >> 24, _a >> 16, alpha);
            offset = TwoBytesToChars(result, offset, _a >> 8, _a, alpha);
            if (dash)
                result[offset++] = '-';
            offset = TwoBytesToChars(result, offset, _b >> 24, _b >> 16, alpha);
            if (dash)
                result[offset++] = '-';
            offset = TwoBytesToChars(result, offset, _b >> 8, _b, alpha);
            if (dash)
                result[offset++] = '-';
            offset = TwoBytesToChars(result, offset, _c >> 24, _c >> 16, alpha);
            if (dash)
                result[offset++] = '-';
            offset = TwoBytesToChars(result, offset, _c >> 8, _c, alpha);
            offset = TwoBytesToChars(result, offset, _d >> 24, _d >> 16, alpha);
            TwoBytesToChars(result, offset, _d >> 8, _d, alpha);

            return new string(result, 0, length);
        }

        public byte[] ToByteArray()
        {
            byte[] bytes = new byte[16];

            bytes[0] = (byte)(_a);
            bytes[1] = (byte)(_a >> 8);
            bytes[2] = (byte)(_a >> 16);
            bytes[3] = (byte)(_a >> 24);
            bytes[4] = (byte)(_b >> 16);
            bytes[5] = (byte)(_b >> 24);
            bytes[6] = (byte)(_b);
            bytes[7] = (byte)(_b >> 8);
            bytes[8] = (byte)(_c >> 24);
            bytes[9] = (byte)(_c >> 16);
            bytes[10] = (byte)(_c >> 8);
            bytes[11] = (byte)(_c);
            bytes[12] = (byte)(_d >> 24);
            bytes[13] = (byte)(_d >> 16);
            bytes[14] = (byte)(_d >> 8);
            bytes[15] = (byte)(_d);

            return bytes;
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
            unchecked
            {
                int result = _a;
                result = (result*397) ^ _b;
                result = (result*397) ^ _c;
                result = (result*397) ^ _d;
                return result;
            }
        }

        static void ConvertBytes(byte[] bytes, out Int32 a, out Int32 b, out Int32 c, out Int32 d)
        {
            a = bytes[3] << 24 | bytes[2] << 16 | bytes[1] << 8 | bytes[0];
            b = bytes[5] << 24 | bytes[4] << 16 | bytes[7] << 8 | bytes[6];
            c = bytes[8] << 24 | bytes[9] << 16 | bytes[10] << 8 | bytes[11];
            d = bytes[12] << 24 | bytes[13] << 16 | bytes[14] << 8 | bytes[15];
        }

        static char HexToChar(int value, int alpha)
        {
            value = value & 0xf;
            return (char) ((value > 9) ? value - 10 + alpha : value + 0x30);
        }

        static int TwoBytesToChars(char[] chars, int offset, int first, int second, int alpha)
        {
            offset = ByteToChars(chars, offset, first, alpha);
            return ByteToChars(chars, offset, second, alpha);
        }

        static int ByteToChars(char[] chars, int offset, int value, int alpha)
        {
            chars[offset++] = HexToChar(value >> 4, alpha);
            chars[offset++] = HexToChar(value, alpha);

            return offset;
        }

        public static void SetGenerator(NewIdGenerator generator)
        {
            _generator = generator;
        }

        public static void SetWorkerIdProvider(WorkerIdProvider provider)
        {
            _workerIdProvider = provider;
        }

        public static void SetTickProvider(TickProvider provider)
        {
            _tickProvider = provider;
        }

        public static NewId Next()
        {
            return Generator.Next();
        }
    }
}