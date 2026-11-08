using System;
using System.Linq;

namespace Pk3DSRNGTool
{
    public static class EggSeedCodec
    {
        public static int GetHexLength(bool gen7) => gen7 ? 32 : 16;

        public static bool TryParse(string input, bool gen7, out uint[] words)
        {
            words = null;
            if (input == null) return false;
            string value = new string(input.Where(Uri.IsHexDigit).ToArray()).ToUpperInvariant();
            int expected = GetHexLength(gen7);
            if (value.Length != expected) return false;
            int count = gen7 ? 4 : 2;
            var result = new uint[4];
            for (int i = 0; i < count; i++)
            {
                if (!uint.TryParse(value.Substring(i * 8, 8), System.Globalization.NumberStyles.HexNumber,
                    System.Globalization.CultureInfo.InvariantCulture, out uint word)) return false;
                result[count - 1 - i] = word;
            }
            words = result;
            return true;
        }

        public static string Format(uint[] words, bool gen7)
        {
            if (words == null || words.Length < (gen7 ? 4 : 2)) return string.Empty;
            return string.Concat((gen7 ? new[] { words[3], words[2], words[1], words[0] } :
                new[] { words[1], words[0] }).Select(x => x.ToString("X8")));
        }
    }
}
