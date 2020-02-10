using System;
using System.IO;
using System.Security;
using System.Security.Cryptography;

namespace PE.Framework.Cryptography
{
    public static class CryptoHelper
    {
        private const int BUFFER_SIZE = 1024;
        private const string WRONG_ARGUMENTS_MSG = "Cannot encrypt/decrypt data. Wrong arguments";

        public static byte[] EncryptData(byte[] data, byte[] key, byte[] iv)
        {
            CheckArguments(data, key, iv);
            try
            {
                var memoryStream = new MemoryStream();
                var aes = Aes.Create();
                aes.Key = key;
                aes.IV = iv;
                var cryptoStream = new CryptoStream(memoryStream, aes.CreateEncryptor(), CryptoStreamMode.Write);

                cryptoStream.Write(data, 0, data.Length);
                cryptoStream.FlushFinalBlock();
                var cipherBytes = memoryStream.ToArray();

                memoryStream.Close();
                cryptoStream.Close();

                return cipherBytes;
            }
            catch (SecurityException ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                throw;
            }
        }

        public static byte[] DecryptData(byte[] encryptedData, byte[] key, byte[] iv)
        {
            CheckArguments(encryptedData, key, iv);
            try
            {
                var memoryStream = new MemoryStream(encryptedData);
                var aes = Aes.Create();
                aes.Key = key;
                aes.IV = iv;
                var cryptoStream = new CryptoStream(memoryStream, aes.CreateDecryptor(), CryptoStreamMode.Read);

                var decryptedData = new byte[encryptedData.Length];
                var count = cryptoStream.Read(decryptedData, 0, decryptedData.Length);

                memoryStream.Close();
                cryptoStream.Close();

                return decryptedData;
            }
            catch (SecurityException ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                throw;
            }
        }

        public static void EncryptFile(string inputFileName, string outputFileName, byte[] key, byte[] iv)
        {
            CheckArguments(inputFileName, outputFileName, key, iv);
            try
            {
                var inputFile = new FileStream(inputFileName, FileMode.Open, FileAccess.Read);
                var outputFile = new FileStream(outputFileName, FileMode.OpenOrCreate, FileAccess.Write);
                outputFile.SetLength(0);

                var aes = Aes.Create();
                aes.Key = key;
                aes.IV = iv;
                var cryptoStream = new CryptoStream(outputFile, aes.CreateEncryptor(), CryptoStreamMode.Write);

                var buffer = new byte[BUFFER_SIZE];
                long readLength = 0;
                while (readLength < inputFile.Length)
                {
                    var length = inputFile.Read(buffer, 0, BUFFER_SIZE);
                    cryptoStream.Write(buffer, 0, length);
                    readLength = readLength + length;
                }

                cryptoStream.Close();
                inputFile.Close();
                outputFile.Close();
            }
            catch (SecurityException ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                throw;
            }
            catch (IOException ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                throw;
            }
        }

        public static void DecryptFile(string inputFileName, string outputFileName, byte[] key, byte[] iv)
        {
            CheckArguments(inputFileName, outputFileName, key, iv);
            try
            {
                var inputFile = new FileStream(inputFileName, FileMode.Open, FileAccess.Read);
                var outputFile = new FileStream(outputFileName, FileMode.OpenOrCreate, FileAccess.Write);
                outputFile.SetLength(0);

                var aes = Aes.Create();
                aes.Key = key;
                aes.IV = iv;
                var cryptoStream = new CryptoStream(inputFile, aes.CreateDecryptor(), CryptoStreamMode.Read);

                var buffer = new byte[BUFFER_SIZE];
                var length = cryptoStream.Read(buffer, 0, buffer.Length);
                while (length > 0)
                {
                    outputFile.Write(buffer, 0, length);
                    length = cryptoStream.Read(buffer, 0, buffer.Length);
                }

                cryptoStream.Close();
                inputFile.Close();
                outputFile.Close();
            }
            catch (SecurityException ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                throw;
            }
            catch (IOException ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                throw;
            }
        }

        private static void CheckArguments(byte[] encryptedData, byte[] key, byte[] iv)
        {
            if (encryptedData == null || encryptedData.Length <= 0)
            {
                System.Diagnostics.Debug.WriteLine(WRONG_ARGUMENTS_MSG);
                throw new ArgumentNullException($"{nameof(encryptedData)}");
            }
            if (key == null || key.Length <= 0)
            {
                System.Diagnostics.Debug.WriteLine(WRONG_ARGUMENTS_MSG);
                throw new ArgumentNullException($"{nameof(key)}");
            }
            if (iv == null || iv.Length <= 0)
            {
                System.Diagnostics.Debug.WriteLine(WRONG_ARGUMENTS_MSG);
                throw new ArgumentNullException($"{nameof(iv)}");
            }
        }

        private static void CheckArguments(string inputFileName, string outputFileName, byte[] key, byte[] iv)
        {
            if (string.IsNullOrEmpty(inputFileName))
            {
                System.Diagnostics.Debug.WriteLine(WRONG_ARGUMENTS_MSG);
                throw new ArgumentNullException($"{nameof(inputFileName)}");
            }
            if (string.IsNullOrEmpty(outputFileName))
            {
                System.Diagnostics.Debug.WriteLine(WRONG_ARGUMENTS_MSG);
                throw new ArgumentNullException($"{nameof(outputFileName)}");
            }
            if (key == null || key.Length <= 0)
            {
                System.Diagnostics.Debug.WriteLine(WRONG_ARGUMENTS_MSG);
                throw new ArgumentNullException($"{nameof(key)}");
            }
            if (iv == null || iv.Length <= 0)
            {
                System.Diagnostics.Debug.WriteLine(WRONG_ARGUMENTS_MSG);
                throw new ArgumentNullException($"{nameof(iv)}");
            }
        }
    }
}
