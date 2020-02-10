using System.IO;
using System.Security.Cryptography;
using NUnit.Framework;
using PE.Shared.Cryptography;

namespace Lightning.IntegrationTests
{
    public class CryptoHelperTests
    {
        private const string FILE_PATH = "./../../../";
        
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void FileEncryptDecryptTest()
        {
            var startFileContent = File.ReadAllText(FILE_PATH + "test.png");

            var aes = Aes.Create();
            aes.Key = CreateKey("Password");
            
            CryptoHelper.EncryptFile(FILE_PATH + "test.png", FILE_PATH + "test_encrypted.png", aes.Key, aes.IV);
            
            CryptoHelper.DecryptFile(FILE_PATH + "test_encrypted.png", FILE_PATH + "test_decrypted.png", aes.Key, aes.IV);
            
            var finalFileContent = File.ReadAllText(FILE_PATH + "test_decrypted.png");
            Assert.AreEqual(startFileContent, finalFileContent);
        }
        
        private static byte[] CreateKey(string password, int keyBytes = 32)
        {
            const int iterations = 300;
            var salt = new byte[] { 80, 70, 60, 50, 40, 30, 20, 10 };
            var keyGenerator = new Rfc2898DeriveBytes(password, salt, iterations);
            return keyGenerator.GetBytes(keyBytes);
        }
    }
}