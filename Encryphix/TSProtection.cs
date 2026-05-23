using System;
using System.IO;
using System.Text;
using System.IO.Compression;
using System.Security.Cryptography;

namespace Encryphix{
    internal class TSProtection{
        // Meta Data Sequence: [Salt (16)] [FileType (1)] [ExtLength (4)] [Extension (Variable)] [IV (16)] [CipherData]
        // ---------------------------------------------------------------------------------------------------------
        private const int IterationCount = 210_000;         // Iteration Count
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
        // MODULE USER FRIENDLY MESSAGE SEND
        // ======================================================================================================
        public static Func<string, string> GetErrorMessage = key => {
            if (string.IsNullOrEmpty(key)){
                return "An unknown error occurred";
            }
            if (EncryphixMain.TSProtectionErrorMessages.Messages != null && 
                EncryphixMain.TSProtectionErrorMessages.Messages.TryGetValue(key, out var msg) &&
                !string.IsNullOrEmpty(msg)){
                return msg;
            }
            return "An unknown error occurred";
        };
        // ENCRYPT FOLDER
        // ======================================================================================================
        public static void EncryptFolder(string folderPath, string password, string outputDirectory = null, Action<int> reportProgress = null, bool deleteOriginal = false, CompressionLevel compressionLevel = CompressionLevel.NoCompression){
            if (string.IsNullOrWhiteSpace(folderPath)){
                throw new ArgumentException(GetErrorMessage("InvalidFolderPath"), nameof(folderPath));
            }
            if (string.IsNullOrWhiteSpace(password)){
                throw new ArgumentException(GetErrorMessage("InvalidPassword"), nameof(password));
            }
            if (!Directory.Exists(folderPath)){
                throw new DirectoryNotFoundException(GetErrorMessage("FolderNotFound"));
            }
            string folderName = Path.GetFileName(folderPath.TrimEnd(Path.DirectorySeparatorChar));
            string zipPath = Path.Combine(outputDirectory ?? Path.GetDirectoryName(folderPath), GetUniquePath(folderName + ZipExtension));
            string encryptedPath = Path.Combine(outputDirectory ?? Path.GetDirectoryName(folderPath), GetUniquePath(folderName + EncryptedExtension));
            SafeDeleteFile(encryptedPath);
            try{
                ZipFile.CreateFromDirectory(folderPath, zipPath, compressionLevel, false);
                EncryptFile(zipPath, encryptedPath, password, reportProgress);
                SafeDeleteFile(zipPath);
                if (deleteOriginal && Directory.Exists(folderPath)){
                    SafeDeleteDirectory(folderPath);
                }
            }catch (Exception ex){
                Console.WriteLine(ex.Message);
                throw new IOException(GetErrorMessage("FolderEncryptionError"), ex);
            }
        }
        // ENCRYPT FILE
        // ======================================================================================================
        public static void EncryptFile(string inputFile, string outputFile, string password, Action<int> reportProgress = null, bool deleteOriginal = true){
            if (string.IsNullOrWhiteSpace(inputFile)){
                throw new ArgumentException(GetErrorMessage("InvalidInputFile"), nameof(inputFile));
            }
            if (string.IsNullOrWhiteSpace(outputFile)){
                throw new ArgumentException(GetErrorMessage("InvalidOutputFile"), nameof(outputFile));
            }
            if (string.IsNullOrWhiteSpace(password)){
                throw new ArgumentException(GetErrorMessage("InvalidPassword"), nameof(password));
            }
            if (!File.Exists(inputFile)){
                throw new FileNotFoundException(GetErrorMessage("FileNotFound"), inputFile);
            }
            byte[] salt = new byte[SaltSize];
            using (var rng = RandomNumberGenerator.Create()) rng.GetBytes(salt);
            string originalExtension;
            byte fileType;
            if (Path.GetExtension(inputFile).Equals(ZipExtension, StringComparison.OrdinalIgnoreCase)){
                originalExtension = ZipExtension;
                fileType = FileType_Folder;
            }else{
                originalExtension = Path.GetExtension(inputFile);
                fileType = FileType_Single;
            }
            byte[] extensionBytes = Encoding.UTF8.GetBytes(originalExtension);
            byte[] extensionLengthBytes = BitConverter.GetBytes(extensionBytes.Length);
            try{
                using (FileStream fsOut = new FileStream(GetUniquePath(outputFile), FileMode.Create))
                using (Aes aes = Aes.Create()){
                    fsOut.Write(salt, 0, salt.Length);
                    fsOut.Write(new byte[] { fileType }, 0, FileTypeSize);
                    fsOut.Write(extensionLengthBytes, 0, extensionLengthBytes.Length);
                    fsOut.Write(extensionBytes, 0, extensionBytes.Length);
                    //
                    byte[] keyBytes;
                    using (var key = new Rfc2898DeriveBytes(password, salt, IterationCount, HashAlgorithmName.SHA512)){
                        keyBytes = key.GetBytes(32);
                    }
                    aes.Key = keyBytes;
                    aes.GenerateIV();
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;
                    //
                    fsOut.Write(aes.IV, 0, aes.IV.Length);
                    using (CryptoStream cs = new CryptoStream(fsOut, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    using (FileStream fsIn = new FileStream(inputFile, FileMode.Open)){
                        CopyStreamWithProgress(fsIn, cs, fsIn.Length, reportProgress);
                    }
                    Array.Clear(keyBytes, 0, keyBytes.Length);
                    Array.Clear(aes.IV, 0, aes.IV.Length);
                }
            }
            finally{
                Array.Clear(salt, 0, salt.Length);
                Array.Clear(extensionBytes, 0, extensionBytes.Length);
                Array.Clear(extensionLengthBytes, 0, extensionLengthBytes.Length);
            }
            if (deleteOriginal && File.Exists(inputFile)){
                SafeDeleteFile(inputFile);
            }
        }
        // DECRYPT FILE
        // ======================================================================================================
        public static string DecryptFile(string inputFile, string outputFile, string password, Action<int> reportProgress = null){
            if (string.IsNullOrWhiteSpace(inputFile)){
                throw new ArgumentException(GetErrorMessage("InvalidInputFile"), nameof(inputFile));
            }
            if (string.IsNullOrWhiteSpace(outputFile)){
                throw new ArgumentException(GetErrorMessage("InvalidOutputFile"), nameof(outputFile));
            }
            if (string.IsNullOrWhiteSpace(password)){
                throw new ArgumentException(GetErrorMessage("InvalidPassword"), nameof(password));
            }
            if (!File.Exists(inputFile)){
                throw new FileNotFoundException(GetErrorMessage("FileNotFound"), inputFile);
            }
            string originalExtension = string.Empty;
            byte[] salt = null;
            try{
                using (FileStream fsIn = new FileStream(inputFile, FileMode.Open))
                using (Aes aes = Aes.Create()){
                    salt = new byte[SaltSize];
                    if (fsIn.Read(salt, 0, salt.Length) != salt.Length){
                        throw new CryptographicException(GetErrorMessage("SaltReadError"));
                    }
                    byte[] fileType = new byte[FileTypeSize];
                    if (fsIn.Read(fileType, 0, fileType.Length) != fileType.Length){
                        throw new CryptographicException(GetErrorMessage("FileTypeReadError"));
                    }
                    byte[] extLengthBytes = new byte[ExtensionLengthSize];
                    if (fsIn.Read(extLengthBytes, 0, extLengthBytes.Length) != extLengthBytes.Length){
                        throw new CryptographicException(GetErrorMessage("ExtLengthReadError"));
                    }
                    int extLength = BitConverter.ToInt32(extLengthBytes, 0);
                    long remainingBytes = fsIn.Length - fsIn.Position;
                    if (extLength < 0 || extLength > 255 || (long)extLength > (remainingBytes - (aes.BlockSize / 8))){
                        throw new InvalidDataException(GetErrorMessage("InvalidExtensionLength"));
                    }
                    byte[] extensionBytes = new byte[extLength];
                    if (fsIn.Read(extensionBytes, 0, extensionBytes.Length) != extensionBytes.Length){
                        throw new CryptographicException(GetErrorMessage("ExtensionReadError"));
                    }
                    originalExtension = Encoding.UTF8.GetString(extensionBytes);
                    byte[] keyBytes;
                    using (var key = new Rfc2898DeriveBytes(password, salt, IterationCount, HashAlgorithmName.SHA512)){
                        keyBytes = key.GetBytes(32);
                    }
                    aes.Key = keyBytes;
                    //
                    byte[] iv = new byte[aes.BlockSize / 8];
                    if (fsIn.Read(iv, 0, iv.Length) != iv.Length){
                        throw new CryptographicException(GetErrorMessage("IVReadError"));
                    }
                    aes.IV = iv;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;
                    //
                    long totalMetaDataSize = SaltSize + FileTypeSize + ExtensionLengthSize + extLength + iv.Length;
                    long totalBytes = fsIn.Length - totalMetaDataSize;
                    using (CryptoStream cs = new CryptoStream(fsIn, aes.CreateDecryptor(), CryptoStreamMode.Read))
                    using (FileStream fsOut = new FileStream(outputFile, FileMode.Create)){
                        try{
                            CopyStreamWithProgress(cs, fsOut, totalBytes, reportProgress);
                        }catch (CryptographicException){
                            fsOut.SetLength(0);
                            throw new InvalidDataException(GetErrorMessage("InvalidPasswordOrCorruptFile"));
                        }
                    }
                    Array.Clear(keyBytes, 0, keyBytes.Length);
                    Array.Clear(iv, 0, iv.Length);
                    Array.Clear(extLengthBytes, 0, extLengthBytes.Length);
                    Array.Clear(extensionBytes, 0, extensionBytes.Length);
                }
            }
            finally{
                if (salt != null){
                    Array.Clear(salt, 0, salt.Length);
                }
            }
            return originalExtension;
        }
        // UNIQUE FOLDER & FILE NAME
        // ======================================================================================================
        public static string GetUniquePath(string orj_path){
            if (string.IsNullOrWhiteSpace(orj_path)){
                throw new ArgumentException("Path cannot be null or empty", nameof(orj_path));
            }
            string fullPath = Path.GetFullPath(orj_path);
            string file_dir = Path.GetDirectoryName(fullPath) ?? throw new InvalidOperationException("Invalid directory path");
            string file_name = Path.GetFileNameWithoutExtension(fullPath);
            string file_ext = Path.GetExtension(fullPath);
            string new_file_path = fullPath;
            int unique_count = 1;
            while (File.Exists(new_file_path) || Directory.Exists(new_file_path)){
                new_file_path = Path.Combine(file_dir, $"{file_name}_{unique_count}{file_ext}");
                unique_count++;
            }
            return new_file_path;
        }
        // COPY STREAM WITH PROGRESS
        // ======================================================================================================
        private static void CopyStreamWithProgress(Stream input, Stream output, long length, Action<int> reportProgress){
            byte[] buffer = new byte[BufferSize];
            long totalRead = 0;
            int lastReportedPercent = 0;
            int bytesRead;
            try{
                while ((bytesRead = input.Read(buffer, 0, buffer.Length)) > 0){
                    output.Write(buffer, 0, bytesRead);
                    totalRead += bytesRead;
                    int percent = (int)((totalRead * 100) / length);
                    if (percent > 100) percent = 100;
                    if (percent != lastReportedPercent){
                        lastReportedPercent = percent;
                        reportProgress?.Invoke(percent);
                    }
                }
            }
            finally{
                Array.Clear(buffer, 0, buffer.Length);
            }
        }
        // SAFE DELETE FILE
        // ======================================================================================================
        public static void SafeDeleteFile(string path){
            try{
                if (File.Exists(path)){
                    File.Delete(path);
                }
            }catch (UnauthorizedAccessException){
                throw new UnauthorizedAccessException(GetErrorMessage("AccessError"));
            }catch (Exception ex){
                throw new IOException(GetErrorMessage("UnknownError"), ex);
            }
        }
        // SAFE DELETE DIRECTORY
        // ======================================================================================================
        public static void SafeDeleteDirectory(string path){
            try{
                if (Directory.Exists(path)){
                    Directory.Delete(path, true);
                }
            }catch (UnauthorizedAccessException){
                throw new UnauthorizedAccessException(GetErrorMessage("AccessError"));
            }catch (Exception ex){
                throw new IOException(GetErrorMessage("UnknownError"), ex);
            }
        }
    }
}