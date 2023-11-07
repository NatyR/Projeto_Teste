using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System;
using Portal.API.Common;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;

namespace Portal.API.Integrations.Bullla.Utils
{
    public class BulllaEncryptor
    {
        private readonly EnvironmentsBase environmentsBase;
        public BulllaEncryptor(IConfiguration _configuration)
        {
            environmentsBase = new EnvironmentsBase(_configuration);
        }
        public T Decrypt<T>(string content) where T : class
        {
            using var aesAlg = Aes.Create();
            var keyB = new Rfc2898DeriveBytes(environmentsBase.BulllaPassphrase, StringToByteArray(environmentsBase.BulllaSaltToken), 100);
            aesAlg.Key = keyB.GetBytes(32);
            aesAlg.IV = StringToByteArray(environmentsBase.BulllaIvToken);
            var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
            using var msDecrypt = new MemoryStream(Convert.FromBase64String(content));
            using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
            using var srDecrypt = new StreamReader(csDecrypt);
            var plaintext = srDecrypt.ReadToEnd();
            return JsonConvert.DeserializeObject<T>(plaintext);
        }

        public string Encrypt(object content)
        {
            using var aesAlg = Aes.Create();
            var keyB = new Rfc2898DeriveBytes(environmentsBase.BulllaPassphrase, StringToByteArray(environmentsBase.BulllaSaltToken), 100);
            aesAlg.Key = keyB.GetBytes(32);
            aesAlg.IV = StringToByteArray(environmentsBase.BulllaIvToken);
            var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
            using var msEncrypt = new MemoryStream();
            using var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
            using (var swEncrypt = new StreamWriter(csEncrypt))
            {
                var text = JsonConvert.SerializeObject(content);
                swEncrypt.Write(text);
            }
            var encrypted = msEncrypt.ToArray();
            return Convert.ToBase64String(encrypted);
        }

        private static byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                .Where(x => x % 2 == 0)
                .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                .ToArray();
        }
    }
}
