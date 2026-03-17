using System;
using System.IO;
using System.Text;
using System.Text.Json;

namespace RoguelikeDarling.Core.Storage
{
    public static class BinaryJsonFileStore
    {
        private const string MagicHeader = "RDBJ";
        private static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions
        {
            WriteIndented = false,
        };

        public static void Save<T>(string filePath, T data)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("A target file path is required.", nameof(filePath));
            }

            string directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string json = JsonSerializer.Serialize(data, SerializerOptions);
            byte[] utf8Bytes = Encoding.UTF8.GetBytes(json);

            using var stream = File.Open(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
            using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: false);
            writer.Write(MagicHeader);
            writer.Write(1);
            writer.Write(utf8Bytes.Length);
            writer.Write(utf8Bytes);
        }

        public static bool TryLoad<T>(string filePath, out T data)
        {
            data = default;
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            {
                return false;
            }

            try
            {
                using var stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                using var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: false);

                string header = reader.ReadString();
                int version = reader.ReadInt32();
                int byteCount = reader.ReadInt32();

                if (!string.Equals(header, MagicHeader, StringComparison.Ordinal) || version != 1 || byteCount < 0)
                {
                    return false;
                }

                byte[] jsonBytes = reader.ReadBytes(byteCount);
                if (jsonBytes.Length != byteCount)
                {
                    return false;
                }

                data = JsonSerializer.Deserialize<T>(jsonBytes, SerializerOptions);
                return data != null;
            }
            catch
            {
                return false;
            }
        }
    }
}
