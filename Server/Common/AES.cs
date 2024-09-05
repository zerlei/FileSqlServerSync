using System.Security.Cryptography;

namespace Common;


/// <summary>
/// 与目标服务器通信将会加密
/// </summary>
public class AESHelper
{
    static readonly byte[] Key =
    [
        0x11,
        0xF2,
        0xAF,
        0xCF,
        0xFF,
        0x8B,
        0x4C,
        0x7D,
        0x23,
        0x96,
        0x1B,
        0x32,
        0x43,
        0xA4,
        0x55,
        0xF6,
        0x29,
        0x1C,
        0x1B,
        0x92,
        0x23,
        0x44,
        0xB5,
        0xF6,
    ];
    static readonly byte[] IV =
    [
        0xD1,
        0xF7,
        0xAB,
        0xCA,
        0xBC,
        0x7B,
        0x2C,
        0x3D,
        0xFA,
        0xAA,
        0xFC,
        0xA8,
        0x28,
        0x19,
        0x9C,
        0xB6,
    ];

    public static byte[] EncryptStringToBytes_Aes(string plainText)
    {
        // Check arguments.
        if (plainText == null || plainText.Length <= 0)
            throw new ArgumentNullException(nameof(plainText), "can't be null");
        byte[] encrypted;

        // Create an Aes object
        // with the specified key and IV.
        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = Key;
            aesAlg.IV = IV;

            // Create an encryptor to perform the stream transform.
            ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

            // Create the streams used for encryption.
            using MemoryStream msEncrypt = new();
            using CryptoStream csEncrypt = new(
                    msEncrypt,
                    encryptor,
                    CryptoStreamMode.Write
                );
            using (StreamWriter swEncrypt = new(csEncrypt))
            {
                //Write all data to the stream.
                swEncrypt.Write(plainText);
            }
            encrypted = msEncrypt.ToArray();
        }

        // Return the encrypted bytes from the memory stream.
        return encrypted;
    }

    public static string DecryptStringFromBytes_Aes(byte[] cipherText)
    {
        // Check arguments.
        if (cipherText == null || cipherText.Length <= 0)
            throw new ArgumentNullException(nameof(cipherText), "can't be null");

        // Declare the string used to hold
        // the decrypted text.
        string plaintext = string.Empty;

        // Create an Aes object
        // with the specified key and IV.
        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = Key;
            aesAlg.IV = IV;

            // Create a decryptor to perform the stream transform.
            ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

            // Create the streams used for decryption.
            using MemoryStream msDecrypt = new(cipherText);
            using CryptoStream csDecrypt = new(msDecrypt, decryptor, CryptoStreamMode.Read);
            using StreamReader srDecrypt = new(csDecrypt);
            // Read the decrypted bytes from the decrypting stream
            // and place them in a string.
            plaintext = srDecrypt.ReadToEnd();
        }

        return plaintext;
    }
}
