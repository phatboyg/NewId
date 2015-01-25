namespace MassTransit.NewIdParsers
{
    using System;
    using System.Diagnostics.Contracts;


    public class Base32Parser :
        INewIdParser
    {
        const string ConvertChars = "abcdefghijklmnopqrstuvwxyz234567ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";

        const string HexChars = "0123456789ABCDEF";

        readonly string _chars;

        public Base32Parser(string chars)
        {
            if (chars.Length % 32 != 0)
                throw new ArgumentException("The characters must be a multiple of 32");

            _chars = chars;
        }

        public Base32Parser()
        {
            _chars = ConvertChars;
        }

        public NewId Parse(string text)
        {
            Contract.Requires(text.Length == 26);

            var buffer = new char[32];

            int bufferOffset = 0;
            int offset = 0;
            long number;
            for (int i = 0; i < 6; ++i)
            {
                number = 0;
                for (int j = 0; j < 4; j++)
                {
                    int index = _chars.IndexOf(text[offset + j]);
                    if (index < 0)
                        throw new ArgumentException("Tracking number contains invalid characters");

                    number = number * 32 + (index % 32);
                }

                ConvertLongToBase16(buffer, bufferOffset, number, 5);

                offset += 4;
                bufferOffset += 5;
            }

            number = 0;
            for (int j = 0; j < 2; j++)
            {
                int index = _chars.IndexOf(text[offset + j]);
                if (index < 0)
                    throw new ArgumentException("Tracking number contains invalid characters");

                number = number * 32 + (index % 32);
            }
            ConvertLongToBase16(buffer, bufferOffset, number, 2);

            return new NewId(new string(buffer, 0, 32));
        }

        static void ConvertLongToBase16(char[] buffer, int offset, long value, int count)
        {
            for (int i = count - 1; i >= 0; i--)
            {
                var index = (int)(value % 16);
                buffer[offset + i] = HexChars[index];
                value /= 16;
            }
        }
    }
}