using System;
using System.Text;
using System.Security.Cryptography;
using System.Diagnostics;
using System.Net;

namespace TotpGenerator
{
    /// <summary>
    /// A class for generate and validate totp code based on time
    /// </summary>
    public class TotpService
    {
        private static readonly Encoding _encoding = new UTF8Encoding(false, true);

        private static readonly TimeSpan _timeStep = TimeSpan.FromMinutes(1);
#if NETSTANDARD2_0
        private static readonly DateTime _unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private static readonly RandomNumberGenerator _rng = RandomNumberGenerator.Create();
#endif

        /// <summary>
        /// Genarating totp code based on securityStampToken, modifier and time. 
        /// </summary>
        /// <param name="securityStampToken">securityStampToken of user</param>
        /// <param name="modifier">modifier should include user identifier and totp porpuse. for example: "1:TwoFactorAuthentication" </param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static int GenerateCode(string securityStampToken, string modifier)
        {
            if (securityStampToken == null)
            {
                throw new ArgumentNullException(nameof(securityStampToken));
            }

            byte[] securityTokenBytes = GetBytes(securityStampToken);

            var currentTimeStep = GetCurrentTimeStepNumber();

            using (var hashAlgorithm = new HMACSHA1(securityTokenBytes))
            {
                return ComputeTotp(hashAlgorithm, currentTimeStep, modifier);
            }
        }

        /// <summary>
        /// Validating totp code based on securityStampToken, code, modifier, expirationInMinutes
        /// </summary>
        /// <param name="securityStampToken"></param>
        /// <param name="code"></param>
        /// <param name="modifier"></param>
        /// <param name="expirationInMinutes"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static bool ValidateCode(
            string securityStampToken,
            int code,
            string modifier,
            int expirationInMinutes)
        {
            if (securityStampToken == null)
            {
                throw new ArgumentNullException(nameof(securityStampToken));
            }

            byte[] securityTokenBytes = GetBytes(securityStampToken);

            using (var hashAlgorithm = new HMACSHA1(securityTokenBytes))
            {
                for (var i = -Math.Abs(expirationInMinutes); i <= 1; i++)
                {
                    var currentTimeStep = GetNextTimeStepNumber(i);

                    var computedTotp = ComputeTotp(hashAlgorithm, (ulong)((long)currentTimeStep), modifier);
                    if (computedTotp == code)
                    {
                        return true;
                    }
                }
            }

            // No match
            return false;
        }

        private static int ComputeTotp(HashAlgorithm hashAlgorithm, ulong timestepNumber, string modifier)
        {
            // # of 0's = length of pin
            const int Mod = 1000000;

            // See https://tools.ietf.org/html/rfc4226
            // We can add an optional modifier
            var timestepAsBytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder((long)timestepNumber));
            var hash = hashAlgorithm.ComputeHash(ApplyModifier(timestepAsBytes, modifier));

            // Generate DT string
            var offset = hash[hash.Length - 1] & 0xf;
            Debug.Assert(offset + 4 < hash.Length);
            var binaryCode = (hash[offset] & 0x7f) << 24
                             | (hash[offset + 1] & 0xff) << 16
                             | (hash[offset + 2] & 0xff) << 8
                             | (hash[offset + 3] & 0xff);

            return binaryCode % Mod;
        }

        private static byte[] ApplyModifier(byte[] input, string modifier)
        {
            if (String.IsNullOrEmpty(modifier))
            {
                return input;
            }

            var modifierBytes = GetBytes(modifier);
            var combined = new byte[checked(input.Length + modifierBytes.Length)];
            Buffer.BlockCopy(input, 0, combined, 0, input.Length);
            Buffer.BlockCopy(modifierBytes, 0, combined, input.Length, modifierBytes.Length);
            return combined;
        }

        // More info: https://tools.ietf.org/html/rfc6238#section-4
        private static ulong GetCurrentTimeStepNumber()
        {
#if NETSTANDARD2_0
            var delta = DateTime.UtcNow - _unixEpoch;
#else
            var delta = DateTimeOffset.UtcNow - DateTimeOffset.UnixEpoch;
#endif
            return (ulong)(delta.Ticks / _timeStep.Ticks);
        }

        private static ulong GetNextTimeStepNumber(int minutes)
        {
#if NETSTANDARD2_0
            var delta = DateTime.UtcNow.AddMinutes(minutes) - _unixEpoch;
#else
            var delta = DateTimeOffset.UtcNow.AddMinutes(minutes) - DateTimeOffset.UnixEpoch;
#endif
            return (ulong)(delta.Ticks / _timeStep.Ticks);
        }

        private static byte[] GetBytes(string securityToken)
        {
            return _encoding.GetBytes(securityToken);
        }
    }
}
