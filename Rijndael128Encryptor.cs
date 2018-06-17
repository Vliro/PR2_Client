using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

public class Rijndael128Encryptor
{
    /*
     * Encrypt method
     * Both Keys and IVs need to be 16 characters encoded in base64. 
     */ 
    public String AES_encrypt(String Input, String AES_Key, String AES_IV)
    {
        // Create encryptor
        var aes = new RijndaelManaged();
        aes.KeySize = 128;
        aes.BlockSize = 128;
        aes.Padding = PaddingMode.Zeros;
        aes.Key = Convert.FromBase64String(AES_Key);
        aes.IV = Convert.FromBase64String(AES_IV);
        var encrypt = aes.CreateEncryptor(aes.Key, aes.IV);
 
        // Encrypt Input
        byte[] xBuff = null;
        using (var ms = new MemoryStream())
        {
            // Convert from UTF-8 String to byte array, write to memory stream and encrypt, then convert to byte array
            using (var cs = new CryptoStream(ms, encrypt, CryptoStreamMode.Write))
            {
                byte[] xXml = Encoding.UTF8.GetBytes(Input);
                cs.Write(xXml, 0, xXml.Length);
            }
            xBuff = ms.ToArray();
        }
 
        // Convert from byte array to base64 string then return
        String Output = Convert.ToBase64String(xBuff);
        return Output;
    }
 
    /*
     * Decrypt method
     * Both Keys and IVs need to be 16 characters encoded in base64. 
     */ 
    public String AES_decrypt(String Input, String AES_Key, String AES_IV)
    {
        // Create decryptor
        RijndaelManaged aes = new RijndaelManaged();
        aes.KeySize = 128;
        aes.BlockSize = 128;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        aes.Key = Convert.FromBase64String(AES_Key);
        aes.IV = Convert.FromBase64String(AES_IV);
        var decrypt = aes.CreateDecryptor();
 
        // Decrypt Input
        byte[] xBuff = null;
        using (var ms = new MemoryStream())
        {
            // Convert from base64 string to byte array, write to memory stream and decrypt, then convert to byte array.
            using (var cs = new CryptoStream(ms, decrypt, CryptoStreamMode.Write))
            {
                byte[] xXml = Convert.FromBase64String(Input);
                cs.Write(xXml, 0, xXml.Length);
            }
            xBuff = ms.ToArray();
        }
 
        // Convert from byte array to UTF-8 string then return
        String Output = Encoding.UTF8.GetString(xBuff);
        return Output;
    }
}