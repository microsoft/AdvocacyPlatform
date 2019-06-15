// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace AdvocacyPlatformInstaller
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Helper utility.
    /// </summary>
    public static class Helpers
    {
        private static ConcurrentDictionary<string, string> _idCache = new ConcurrentDictionary<string, string>();

        /// <summary>
        /// Generate a new identifier by hashing the current datetime string
        ///
        /// NOTE: This is not being used as a cryptographic hash.
        /// </summary>
        /// <param name="sharedIdName">Key for using a shared id.</param>
        /// <param name="idLength">The desired length of the id.</param>
        /// <returns>If sharedIdName is null, a new id, otherwise the previous id for the sharedIdName (if already created) or a new id.</returns>
        public static string NewId(string sharedIdName = null, int idLength = 4)
        {
            if (idLength > 32)
            {
                throw new ArgumentOutOfRangeException("idLength", "idLength can not be greater than 32!");
            }

            if (!string.IsNullOrWhiteSpace(sharedIdName) &&
                _idCache.ContainsKey(sharedIdName))
            {
                return _idCache[sharedIdName];
            }

            string guid = Guid.NewGuid().ToString();

            SHA256 hashAlgorithm = SHA256.Create();

            byte[] hashBytes = hashAlgorithm.ComputeHash(Encoding.ASCII.GetBytes(guid));

            string id = GetHexString(hashBytes).Substring(0, idLength).ToLowerInvariant();

            if (!string.IsNullOrWhiteSpace(sharedIdName))
            {
                _idCache.TryAdd(sharedIdName, id);
            }

            return id;
        }

        /// <summary>
        /// Converts a byte array to a hex string.
        /// </summary>
        /// <param name="bytes">The bytes to convert.</param>
        /// <returns>The hex string.</returns>
        public static string GetHexString(byte[] bytes)
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < bytes.Length; i++)
            {
                sb.Append(bytes[i].ToString("X2"));
            }

            return sb.ToString();
        }
    }
}
