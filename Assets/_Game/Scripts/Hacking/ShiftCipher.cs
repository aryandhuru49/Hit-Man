using System.Text;

namespace ExitClause.Hacking
{
    /// <summary>The agency's "encryption": a Caesar shift over letters and digits. Trivially breakable on purpose.</summary>
    public static class ShiftCipher
    {
        public static string Encrypt(string text, int shift) => Apply(text, shift);
        public static string Decrypt(string text, int shift) => Apply(text, -shift);

        static string Apply(string text, int shift)
        {
            var sb = new StringBuilder(text.Length);
            foreach (char c in text)
            {
                if (c >= 'A' && c <= 'Z') sb.Append((char)('A' + Mod(c - 'A' + shift, 26)));
                else if (c >= 'a' && c <= 'z') sb.Append((char)('a' + Mod(c - 'a' + shift, 26)));
                else if (c >= '0' && c <= '9') sb.Append((char)('0' + Mod(c - '0' + shift, 10)));
                else sb.Append(c);
            }
            return sb.ToString();
        }

        static int Mod(int a, int m) => ((a % m) + m) % m;
    }
}
