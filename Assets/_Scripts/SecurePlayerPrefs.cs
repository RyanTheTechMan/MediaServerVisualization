using UnityEngine;
using System.Security.Cryptography;
using System.Text;
using System;

public static class SecurePlayerPrefs {
    private static byte[] key = Encoding.UTF8.GetBytes("your-32-byte-long-security-key-here"); // Must be 256 bits

    public static void SetString(string key, string value) {
        var encrypted = EncryptString(value);
        PlayerPrefs.SetString(key, encrypted);
    }

    public static string GetString(string key) {
        var encrypted = PlayerPrefs.GetString(key);
        return DecryptString(encrypted);
    }

    public static string EncryptString(string plainText) {
        using (var aes = Aes.Create()) {
            aes.Key = key;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            aes.GenerateIV();
            var iv = aes.IV;
            using (var encryptor = aes.CreateEncryptor(aes.Key, iv))
            using (var ms = new System.IO.MemoryStream()) {
                using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                using (var sw = new System.IO.StreamWriter(cs)) {
                    sw.Write(plainText);
                }

                var encrypted = ms.ToArray();
                return Convert.ToBase64String(iv) + Convert.ToBase64String(encrypted);
            }
        }
    }

    public static string DecryptString(string cipherText) {
        var fullCipher = Convert.FromBase64String(cipherText);

        using (var aes = Aes.Create()) {
            aes.Key = key;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            var iv = new byte[16];
            Array.Copy(fullCipher, 0, iv, 0, iv.Length);

            var cipher = new byte[fullCipher.Length - iv.Length];
            Array.Copy(fullCipher, iv.Length, cipher, 0, cipher.Length);

            using (var decryptor = aes.CreateDecryptor(aes.Key, iv))
            using (var ms = new System.IO.MemoryStream(cipher))
            using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
            using (var sr = new System.IO.StreamReader(cs)) {
                return sr.ReadToEnd();
            }
        }
    }
}