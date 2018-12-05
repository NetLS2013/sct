using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace BlockApp.Helpers
{
    public static class Crypto
    {
        /// <summary>
        /// Computes SHA256 hash of a given input.
        /// </summary>
        /// <param name="input">Input data which needs to be hashed.</param>
        /// <returns>Returns hash in a string form.</returns>
        public static string GetSha256Hash(string input)
        {
            using (var hashAlgorithm = new SHA256CryptoServiceProvider())
            {
                var byteValue = Encoding.UTF8.GetBytes(input);
                var byteHash = hashAlgorithm.ComputeHash(byteValue);
                var base64String = Convert.ToBase64String(byteHash);

                var result = base64String
                    .Where(char.IsLetterOrDigit)
                    .ToArray();
                
                return new string(result);
            }
        }
    }
}