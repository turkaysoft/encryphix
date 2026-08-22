using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Encryphix
{
    internal class TSProtection
    {

        // Meta Data Sequence: [Salt (16)] [FileType (1)] [ExtLength (4)] [Extension (Variable)] [HMAC (32)] [IV (16)] [CipherData]
        // ---------------------------------------------------------------------------------------------------------
        private const int IterationCount = 210_000;         // Modern PBKDF2 iteration count
        private const int SaltSize = 16;                    // Salt Size Count
        private const int BufferSize = 4 * 1024 * 1024;     // Buffer Size - 4 MB
        public const string ZipExtension = ".zip";          // ZIP Extension
        public const string EncryptedExtension = ".aes";    // Encryption Extension
        // ---------------------------------------------------------------------------------------------------------
        private const byte FileType_Single = 0x01;          // Single File Hex Code
        private const byte FileType_Folder = 0x02;          // Folder (ZIP) Hex Code
        private const int FileTypeSize = 1;                 // 1 Byte For File Type
        private const int ExtensionLengthSize = 4;          // To store the extension length, 4 bytes (Int32)
        // ---------------------------------------------------------------------------------------------------------
        private const int HMACSize = 32;                    // SHA-256 HMAC for integrity verification
        private const int AesKeySize = 32;                  // AES-256 key size
        private const int AesIvSize = 16;                   // AES IV size
        // ---------------------------------------------------------------------------------------------------------

        // ======================================================================================================
        // DECRYPT SESSION CACHE
        // ======================================================================================================

        public static class DecryptSessionCache
        {
            private static readonly Dictionary<string, (byte[] EncryptionKey, byte[] HMACKey, DateTime Expiry)> _cache = new Dictionary<string, (byte[], byte[], DateTime)>();
            private static readonly TimeSpan CacheLifetime = TimeSpan.FromMinutes(5);
            private static readonly object _lock = new object();

            public static (byte[] encKey, byte[] hmacKey) GetOrCreateKeys(string password, byte[] salt)
            {
                string cacheKey = Convert.ToBase64String(salt);

                lock (_lock)
                {
                    CleanExpiredEntries();

                    if (_cache.TryGetValue(cacheKey, out var cached) && cached.Expiry > DateTime.Now)
                    {
                        byte[] encKeyCopy = new byte[cached.EncryptionKey.Length];
                        byte[] hmacKeyCopy = new byte[cached.HMACKey.Length];
                        Buffer.BlockCopy(cached.EncryptionKey, 0, encKeyCopy, 0, encKeyCopy.Length);
                        Buffer.BlockCopy(cached.HMACKey, 0, hmacKeyCopy, 0, hmacKeyCopy.Length);
                        return (encKeyCopy, hmacKeyCopy);
                    }

                    using (var keyDerivation = new Rfc2898DeriveBytes(password, salt, IterationCount, HashAlgorithmName.SHA512))
                    {
                        byte[] derivedKeyMaterial = keyDerivation.GetBytes(AesKeySize + HMACSize);
                        byte[] encKey = new byte[AesKeySize];
                        byte[] hmacKey = new byte[HMACSize];
                        Buffer.BlockCopy(derivedKeyMaterial, 0, encKey, 0, AesKeySize);
                        Buffer.BlockCopy(derivedKeyMaterial, AesKeySize, hmacKey, 0, HMACSize);
                        Array.Clear(derivedKeyMaterial, 0, derivedKeyMaterial.Length);

                        byte[] encKeyCache = new byte[AesKeySize];
                        byte[] hmacKeyCache = new byte[HMACSize];
                        Buffer.BlockCopy(encKey, 0, encKeyCache, 0, AesKeySize);
                        Buffer.BlockCopy(hmacKey, 0, hmacKeyCache, 0, HMACSize);
                        _cache[cacheKey] = (encKeyCache, hmacKeyCache, DateTime.Now + CacheLifetime);

                        return (encKey, hmacKey);
                    }
                }
            }

            private static void CleanExpiredEntries()
            {
                var expiredKeys = _cache.Where(kv => kv.Value.Expiry <= DateTime.Now).Select(kv => kv.Key).ToList();
                foreach (var key in expiredKeys)
                {
                    if (_cache.TryGetValue(key, out var entry))
                    {
                        Array.Clear(entry.EncryptionKey, 0, entry.EncryptionKey.Length);
                        Array.Clear(entry.HMACKey, 0, entry.HMACKey.Length);
                        _cache.Remove(key);
                    }
                }
            }

            public static void ClearCache()
            {
                lock (_lock)
                {
                    foreach (var (EncryptionKey, HMACKey, Expiry) in _cache.Values)
                    {
                        Array.Clear(EncryptionKey, 0, EncryptionKey.Length);
                        Array.Clear(HMACKey, 0, HMACKey.Length);
                    }
                    _cache.Clear();
                }
            }
        }

        // ======================================================================================================
        // CRYPTO SESSION CLASS
        // ======================================================================================================

        public sealed class CryptoSession : IDisposable
        {
            public byte[] Salt { get; private set; }
            public byte[] EncryptionKey { get; private set; }
            public byte[] HMACKey { get; private set; }
            private bool _disposed = false;

            private CryptoSession(byte[] salt, byte[] encKey, byte[] hmacKey)
            {
                Salt = salt;
                EncryptionKey = encKey;
                HMACKey = hmacKey;
            }

            public static CryptoSession CreateFromPassword(string password)
            {
                if (string.IsNullOrWhiteSpace(password))
                    throw new ArgumentException(GetErrorMessage("InvalidPassword"), nameof(password));

                byte[] salt = new byte[SaltSize];
                using (var rng = RandomNumberGenerator.Create())
                    rng.GetBytes(salt);

                using (var keyDerivation = new Rfc2898DeriveBytes(password, salt, IterationCount, HashAlgorithmName.SHA512))
                {
                    byte[] derivedKeyMaterial = keyDerivation.GetBytes(AesKeySize + HMACSize);
                    byte[] encKey = new byte[AesKeySize];
                    byte[] hmacKey = new byte[HMACSize];
                    Buffer.BlockCopy(derivedKeyMaterial, 0, encKey, 0, AesKeySize);
                    Buffer.BlockCopy(derivedKeyMaterial, AesKeySize, hmacKey, 0, HMACSize);
                    Array.Clear(derivedKeyMaterial, 0, derivedKeyMaterial.Length);
                    return new CryptoSession(salt, encKey, hmacKey);
                }
            }

            public void Dispose()
            {
                if (!_disposed)
                {
                    if (EncryptionKey != null) Array.Clear(EncryptionKey, 0, EncryptionKey.Length);
                    if (HMACKey != null) Array.Clear(HMACKey, 0, HMACKey.Length);
                    if (Salt != null) Array.Clear(Salt, 0, Salt.Length);
                    _disposed = true;
                }
            }
        }

        // ======================================================================================================
        // MODULE USER FRIENDLY MESSAGE SEND
        // ======================================================================================================

        public static Func<string, string> GetErrorMessage = key => {
            if (string.IsNullOrEmpty(key))
            {
                return "An unknown error occurred";
            }
            if (EncryphixMain.TSProtectionErrorMessages.Messages != null && EncryphixMain.TSProtectionErrorMessages.Messages.TryGetValue(key, out var msg) && !string.IsNullOrEmpty(msg))
            {
                return msg;
            }
            return "An unknown error occurred";
        };

        // ======================================================================================================
        // ENCRYPT FOLDER
        // ======================================================================================================

        public static void EncryptFolderWithSession(string folderPath, string outputDirectory, CryptoSession session, bool deleteOriginal = false, CompressionLevel compressionLevel = CompressionLevel.NoCompression)
        {
            if (string.IsNullOrWhiteSpace(folderPath))
            {
                throw new ArgumentException(GetErrorMessage("InvalidFolderPath"), nameof(folderPath));
            }
            if (session == null)
            {
                throw new ArgumentException(GetErrorMessage("InvalidSession"), nameof(session));
            }
            if (!Directory.Exists(folderPath))
            {
                throw new DirectoryNotFoundException(GetErrorMessage("FolderNotFound"));
            }

            string resolvedOutputDir = string.IsNullOrWhiteSpace(outputDirectory) ? Path.GetDirectoryName(folderPath) : outputDirectory;
            if (!Directory.Exists(resolvedOutputDir))
            {
                Directory.CreateDirectory(resolvedOutputDir);
            }

            string folderName = Path.GetFileName(folderPath.TrimEnd(Path.DirectorySeparatorChar));
            folderName = SanitizeFileName(folderName);
            string zipPath = GetUniquePath(Path.Combine(resolvedOutputDir, folderName + ZipExtension));
            string encryptedPath = GetUniquePath(Path.Combine(resolvedOutputDir, folderName + EncryptedExtension));

            SafeDeleteFile(encryptedPath);

            try
            {
                ZipFile.CreateFromDirectory(folderPath, zipPath, compressionLevel, false);
                EncryptFileWithSession(zipPath, encryptedPath, session, null, true);
                if (deleteOriginal && Directory.Exists(folderPath))
                {
                    SecureDeleteDirectory(folderPath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw new IOException(GetErrorMessage("FolderEncryptionError"), ex);
            }
            finally
            {
                SecureDeleteFile(zipPath);
            }
        }

        // ======================================================================================================
        // ENCRYPT FILE WITH SESSION
        // ======================================================================================================

        public static void EncryptFileWithSession(string inputFile, string outputFile, CryptoSession session, Action<int> reportProgress = null, bool deleteOriginal = true)
        {
            if (string.IsNullOrWhiteSpace(inputFile))
            {
                throw new ArgumentException(GetErrorMessage("InvalidInputFile"), nameof(inputFile));
            }
            if (string.IsNullOrWhiteSpace(outputFile))
            {
                throw new ArgumentException(GetErrorMessage("InvalidOutputFile"), nameof(outputFile));
            }
            if (session == null)
            {
                throw new ArgumentException(GetErrorMessage("InvalidSession"), nameof(session));
            }
            if (!File.Exists(inputFile))
            {
                throw new FileNotFoundException(GetErrorMessage("FileNotFound"), inputFile);
            }

            string originalExtension;
            byte fileType;
            if (Path.GetExtension(inputFile).Equals(ZipExtension, StringComparison.OrdinalIgnoreCase))
            {
                originalExtension = ZipExtension;
                fileType = FileType_Folder;
            }
            else
            {
                originalExtension = Path.GetExtension(inputFile);
                fileType = FileType_Single;
            }

            byte[] extensionBytes = Encoding.UTF8.GetBytes(originalExtension);
            byte[] extensionLengthBytes = BitConverter.GetBytes(extensionBytes.Length);

            try
            {
                string resolvedOutputFile = Path.GetFullPath(outputFile);
                string outputDir = Path.GetDirectoryName(resolvedOutputFile);
                if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))
                {
                    Directory.CreateDirectory(outputDir);
                }

                using (FileStream fsOut = new FileStream(resolvedOutputFile, FileMode.Create))
                using (Aes aes = Aes.Create())
                {
                    aes.Key = session.EncryptionKey;
                    aes.GenerateIV();
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;

                    fsOut.Write(session.Salt, 0, session.Salt.Length);

                    long metadataStart = fsOut.Position;
                    fsOut.Write(new byte[] { fileType }, 0, FileTypeSize);
                    fsOut.Write(extensionLengthBytes, 0, extensionLengthBytes.Length);
                    fsOut.Write(extensionBytes, 0, extensionBytes.Length);

                    byte[] iv = aes.IV;
                    fsOut.Write(iv, 0, iv.Length);

                    long metadataEnd = fsOut.Position;
                    long hmacPos = fsOut.Position;
                    byte[] hmacPlaceholder = new byte[HMACSize];
                    fsOut.Write(hmacPlaceholder, 0, HMACSize);

                    long dataStartPos = fsOut.Position;

                    using (CryptoStream cs = new CryptoStream(fsOut, aes.CreateEncryptor(), CryptoStreamMode.Write, leaveOpen: true))
                    using (FileStream fsIn = new FileStream(inputFile, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        CopyStreamWithProgress(fsIn, cs, fsIn.Length, reportProgress);
                    }

                    fsOut.Flush();
                    long cipherDataEnd = fsOut.Position;

                    using (var hmac = new HMACSHA256(session.HMACKey))
                    {
                        byte[] metadataBuffer = new byte[metadataEnd - metadataStart];
                        fsOut.Seek(metadataStart, SeekOrigin.Begin);
                        fsOut.Read(metadataBuffer, 0, metadataBuffer.Length);
                        hmac.TransformBlock(metadataBuffer, 0, metadataBuffer.Length, metadataBuffer, 0);
                        Array.Clear(metadataBuffer, 0, metadataBuffer.Length);

                        long cipherLen = cipherDataEnd - dataStartPos;
                        if (cipherLen > 0)
                        {
                            byte[] hmacBuffer = new byte[BufferSize];
                            fsOut.Seek(dataStartPos, SeekOrigin.Begin);
                            long hmacTotalRead = 0;
                            int hmacBytesRead;
                            while ((hmacBytesRead = fsOut.Read(hmacBuffer, 0, (int)Math.Min(hmacBuffer.Length, cipherLen - hmacTotalRead))) > 0)
                            {
                                hmac.TransformBlock(hmacBuffer, 0, hmacBytesRead, hmacBuffer, 0);
                                hmacTotalRead += hmacBytesRead;
                            }
                            Array.Clear(hmacBuffer, 0, hmacBuffer.Length);
                        }
                        hmac.TransformFinalBlock(new byte[0], 0, 0);
                        byte[] computedHmac = hmac.Hash;
                        fsOut.Seek(hmacPos, SeekOrigin.Begin);
                        fsOut.Write(computedHmac, 0, HMACSize);
                    }

                    Array.Clear(iv, 0, iv.Length);
                }
            }
            finally
            {
                Array.Clear(extensionBytes, 0, extensionBytes.Length);
                Array.Clear(extensionLengthBytes, 0, extensionLengthBytes.Length);
            }

            if (deleteOriginal && File.Exists(inputFile))
            {
                SecureDeleteFile(inputFile);
            }
        }

        // ======================================================================================================
        // DECRYPT FILE
        // ======================================================================================================

        public static string DecryptFile(string inputFile, string outputFile, string password, Action<int> reportProgress = null)
        {
            if (string.IsNullOrWhiteSpace(inputFile))
            {
                throw new ArgumentException(GetErrorMessage("InvalidInputFile"), nameof(inputFile));
            }
            if (string.IsNullOrWhiteSpace(outputFile))
            {
                throw new ArgumentException(GetErrorMessage("InvalidOutputFile"), nameof(outputFile));
            }
            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException(GetErrorMessage("InvalidPassword"), nameof(password));
            }
            if (!File.Exists(inputFile))
            {
                throw new FileNotFoundException(GetErrorMessage("FileNotFound"), inputFile);
            }
            string originalExtension = string.Empty;
            byte[] salt = null;
            byte[] encKey = null;
            byte[] hmacKey = null;
            byte[] iv = null;
            try
            {
                using (FileStream fsIn = new FileStream(inputFile, FileMode.Open))
                using (Aes aes = Aes.Create())
                {
                    salt = new byte[SaltSize];
                    if (fsIn.Read(salt, 0, salt.Length) != salt.Length)
                    {
                        throw new CryptographicException(GetErrorMessage("SaltReadError"));
                    }
                    byte[] fileType = new byte[FileTypeSize];
                    if (fsIn.Read(fileType, 0, fileType.Length) != fileType.Length)
                    {
                        throw new CryptographicException(GetErrorMessage("FileTypeReadError"));
                    }
                    byte[] extLengthBytes = new byte[ExtensionLengthSize];
                    if (fsIn.Read(extLengthBytes, 0, extLengthBytes.Length) != extLengthBytes.Length)
                    {
                        throw new CryptographicException(GetErrorMessage("ExtLengthReadError"));
                    }
                    int extLength = BitConverter.ToInt32(extLengthBytes, 0);
                    long remainingAfterExtLength = fsIn.Length - fsIn.Position;
                    long minRequired = AesIvSize + HMACSize + 1;
                    if (extLength < 0 || extLength > 255 || (long)extLength > (remainingAfterExtLength - minRequired))
                    {
                        throw new InvalidDataException(GetErrorMessage("InvalidExtensionLength"));
                    }
                    byte[] extensionBytes = new byte[extLength];
                    if (fsIn.Read(extensionBytes, 0, extensionBytes.Length) != extensionBytes.Length)
                    {
                        throw new CryptographicException(GetErrorMessage("ExtensionReadError"));
                    }
                    originalExtension = Encoding.UTF8.GetString(extensionBytes);

                    (encKey, hmacKey) = DecryptSessionCache.GetOrCreateKeys(password, salt);

                    aes.Key = encKey;
                    iv = new byte[aes.BlockSize / 8];
                    if (fsIn.Read(iv, 0, iv.Length) != iv.Length)
                    {
                        throw new CryptographicException(GetErrorMessage("IVReadError"));
                    }
                    aes.IV = iv;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;

                    byte[] storedHmac = new byte[HMACSize];
                    if (fsIn.Read(storedHmac, 0, HMACSize) != HMACSize)
                    {
                        throw new CryptographicException(GetErrorMessage("HMACReadError"));
                    }

                    long metadataStart = SaltSize;
                    long metadataEnd = metadataStart + FileTypeSize + ExtensionLengthSize + extLength + AesIvSize;
                    long hmacStoredPos = metadataEnd;
                    long dataStart = hmacStoredPos + HMACSize;
                    long cipherDataLength = fsIn.Length - dataStart;

                    using (var hmac = new HMACSHA256(hmacKey))
                    {
                        byte[] hmdBuffer = new byte[metadataEnd - metadataStart];
                        fsIn.Seek(metadataStart, SeekOrigin.Begin);
                        fsIn.Read(hmdBuffer, 0, hmdBuffer.Length);
                        hmac.TransformBlock(hmdBuffer, 0, hmdBuffer.Length, hmdBuffer, 0);
                        Array.Clear(hmdBuffer, 0, hmdBuffer.Length);
                        if (cipherDataLength > 0)
                        {
                            long hmTotalRead = 0;
                            byte[] hmBuffer = new byte[BufferSize];
                            fsIn.Seek(dataStart, SeekOrigin.Begin);
                            int hmBytesRead;
                            while ((hmBytesRead = fsIn.Read(hmBuffer, 0, (int)Math.Min(hmBuffer.Length, cipherDataLength - hmTotalRead))) > 0)
                            {
                                hmac.TransformBlock(hmBuffer, 0, hmBytesRead, hmBuffer, 0);
                                hmTotalRead += hmBytesRead;
                            }
                            Array.Clear(hmBuffer, 0, hmBuffer.Length);
                        }
                        hmac.TransformFinalBlock(new byte[0], 0, 0);
                        byte[] computedHmac = hmac.Hash;
                        bool hmacValid = computedHmac != null && storedHmac != null && computedHmac.Length == storedHmac.Length;
                        if (hmacValid)
                        {
                            int diff = 0;
                            for (int i = 0; i < computedHmac.Length; i++)
                            {
                                diff |= computedHmac[i] ^ storedHmac[i];
                            }
                            hmacValid = diff == 0;
                        }
                        if (!hmacValid)
                        {
                            Array.Clear(computedHmac, 0, computedHmac.Length);
                            Array.Clear(storedHmac, 0, storedHmac.Length);
                            throw new InvalidDataException(GetErrorMessage("CorruptedFileOrTampered"));
                        }
                        Array.Clear(computedHmac, 0, computedHmac.Length);
                        Array.Clear(storedHmac, 0, storedHmac.Length);
                    }

                    fsIn.Seek(dataStart, SeekOrigin.Begin);
                    long totalBytes = cipherDataLength;
                    string resolvedOutputFile = Path.GetFullPath(outputFile);
                    string outputDir = Path.GetDirectoryName(resolvedOutputFile);
                    if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))
                    {
                        Directory.CreateDirectory(outputDir);
                    }
                    using (CryptoStream cs = new CryptoStream(fsIn, aes.CreateDecryptor(), CryptoStreamMode.Read, leaveOpen: true))
                    using (FileStream fsOut = new FileStream(resolvedOutputFile, FileMode.Create, FileAccess.Write, FileShare.Read))
                    {
                        try
                        {
                            CopyStreamWithProgress(cs, fsOut, totalBytes, reportProgress);
                        }
                        catch (CryptographicException)
                        {
                            fsOut.SetLength(0);
                            throw new InvalidDataException(GetErrorMessage("InvalidPasswordOrCorruptFile"));
                        }
                    }

                    Array.Clear(encKey, 0, encKey.Length);
                    Array.Clear(hmacKey, 0, hmacKey.Length);
                    Array.Clear(iv, 0, iv.Length);
                    Array.Clear(extLengthBytes, 0, extLengthBytes.Length);
                    Array.Clear(extensionBytes, 0, extensionBytes.Length);
                }
            }
            finally
            {
                if (salt != null) Array.Clear(salt, 0, salt.Length);
            }
            return originalExtension;
        }

        // ======================================================================================================
        // YARDIMCI METODLAR
        // ======================================================================================================

        private static string SanitizeFileName(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return "unnamed";
            char[] invalidChars = Path.GetInvalidFileNameChars();
            foreach (char c in invalidChars)
            {
                fileName = fileName.Replace(c.ToString(), "_");
            }
            fileName = fileName.Replace("\\", "_").Replace("/", "_");
            if (string.IsNullOrWhiteSpace(fileName)) return "unnamed";
            return fileName;
        }

        public static void SecureDeleteFile(string path)
        {
            try
            {
                if (!File.Exists(path)) return;
                using (var rng = RandomNumberGenerator.Create())
                {
                    long fileLength = new FileInfo(path).Length;
                    using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Write))
                    {
                        byte[] randomBuffer = new byte[Math.Min(BufferSize, (int)Math.Min(fileLength, int.MaxValue))];
                        long totalBytesWritten = 0;
                        while (totalBytesWritten < fileLength)
                        {
                            rng.GetBytes(randomBuffer);
                            int bytesToWrite = (int)Math.Min(randomBuffer.Length, fileLength - totalBytesWritten);
                            fs.Write(randomBuffer, 0, bytesToWrite);
                            totalBytesWritten += bytesToWrite;
                        }
                        fs.Flush(true);
                    }
                }
                File.Delete(path);
            }
            catch (UnauthorizedAccessException)
            {
                throw new UnauthorizedAccessException(GetErrorMessage("AccessError"));
            }
            catch (Exception ex)
            {
                throw new IOException(GetErrorMessage("UnknownError"), ex);
            }
        }

        public static string GetUniquePath(string orj_path)
        {
            if (string.IsNullOrWhiteSpace(orj_path))
            {
                throw new ArgumentException(GetErrorMessage("InvalidPath"), nameof(orj_path));
            }
            string fullPath = Path.GetFullPath(orj_path);
            string file_dir = Path.GetDirectoryName(fullPath) ?? throw new InvalidOperationException(GetErrorMessage("InvalidDirectory"));
            string file_name = Path.GetFileNameWithoutExtension(fullPath);
            string file_ext = Path.GetExtension(fullPath);
            string new_file_path = fullPath;
            int unique_count = 1;
            while (File.Exists(new_file_path) || Directory.Exists(new_file_path))
            {
                new_file_path = Path.Combine(file_dir, $"{file_name}_{unique_count}{file_ext}");
                unique_count++;
            }
            return new_file_path;
        }

        private static void CopyStreamWithProgress(Stream input, Stream output, long length, Action<int> reportProgress)
        {
            byte[] buffer = new byte[BufferSize];
            long totalRead = 0;
            int lastReportedPercent = 0;
            int bytesRead;
            try
            {
                while ((bytesRead = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    output.Write(buffer, 0, bytesRead);
                    totalRead += bytesRead;
                    int percent = (int)((totalRead * 100) / length);
                    if (percent > 100) percent = 100;
                    if (percent != lastReportedPercent)
                    {
                        lastReportedPercent = percent;
                        reportProgress?.Invoke(percent);
                    }
                }
            }
            finally
            {
                Array.Clear(buffer, 0, buffer.Length);
            }
        }

        public static void SafeDeleteFile(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
            catch (UnauthorizedAccessException)
            {
                throw new UnauthorizedAccessException(GetErrorMessage("AccessError"));
            }
            catch (Exception ex)
            {
                throw new IOException(GetErrorMessage("UnknownError"), ex);
            }
        }

        public static void SecureDeleteDirectory(string path)
        {
            if (!Directory.Exists(path)) return;
            string[] files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
            foreach (string file in files)
            {
                SecureDeleteFile(file);
            }
            SafeDeleteDirectory(path);
        }

        public static void SafeDeleteDirectory(string path)
        {
            try
            {
                if (Directory.Exists(path))
                {
                    Directory.Delete(path, true);
                }
            }
            catch (UnauthorizedAccessException)
            {
                throw new UnauthorizedAccessException(GetErrorMessage("AccessError"));
            }
            catch (Exception ex)
            {
                throw new IOException(GetErrorMessage("UnknownError"), ex);
            }
        }

        // ============================================================
        // CLIPBOARD SECURITY: remember copied text, clear on shutdown
        // ============================================================

        public static class TSClipboardSecurity
        {
            internal static string _lastCopiedClipboard = null;

            public static void TrackCopiedText(string copiedText)
            {
                _lastCopiedClipboard = copiedText;
                ScheduleClipboardClear(copiedText);
            }

            public static void ClearOwnClipboardIfPresent()
            {
                try
                {
                    if (_lastCopiedClipboard != null && Clipboard.GetText() == _lastCopiedClipboard)
                    {
                        Clipboard.Clear();
                    }
                }
                catch { }
                finally
                {
                    _lastCopiedClipboard = null;
                }
            }

            private static void ScheduleClipboardClear(string copiedText)
            {
                string captured = copiedText;
                TaskScheduler scheduler;
                try
                {
                    scheduler = TaskScheduler.FromCurrentSynchronizationContext();
                }
                catch (InvalidOperationException)
                {
                    scheduler = TaskScheduler.Default;
                }
                Task.Delay(30000).ContinueWith(_ =>
                {
                    try
                    {
                        if (Clipboard.GetText() == captured)
                        {
                            Clipboard.Clear();
                        }
                    }
                    catch { }
                }, scheduler);
            }
        }
    }
}