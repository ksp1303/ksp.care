namespace ksp.care.Helpers
{
    public static class PasswordHelper
    {
        public static string HashPassword(string plainText)
            => BCrypt.Net.BCrypt.HashPassword(plainText);

        public static bool VerifyPassword(string plainText, string hash)
        {
            try { return BCrypt.Net.BCrypt.Verify(plainText, hash); }
            catch { return false; }    // gracefully handle legacy plain-text passwords
        }

        /// <summary>
        /// Returns true if the stored value looks like a BCrypt hash (starts with $2).
        /// Used to support migration from plain-text passwords.
        /// </summary>
        public static bool IsBCryptHash(string stored)
            => !string.IsNullOrEmpty(stored) && stored.StartsWith("$2");
    }
}
