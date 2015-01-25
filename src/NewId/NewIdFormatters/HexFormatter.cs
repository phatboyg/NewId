namespace MassTransit.NewIdFormatters
{
    public class HexFormatter :
        INewIdFormatter
    {
        readonly int _alpha;

        public HexFormatter(bool upperCase = false)
        {
            _alpha = upperCase ? 'A' : 'a';
        }

        public string Format(byte[] bytes)
        {
            var result = new char[32];

            int offset = 0;
            for (int i = 0; i < 16; i++)
            {
                byte value = bytes[i];
                result[offset++] = HexToChar(value >> 4, _alpha);
                result[offset++] = HexToChar(value, _alpha);
            }

            return new string(result, 0, 32);
        }

        static char HexToChar(int value, int alpha)
        {
            value = value & 0xf;
            return (char)((value > 9) ? value - 10 + alpha : value + 0x30);
        }
    }
}