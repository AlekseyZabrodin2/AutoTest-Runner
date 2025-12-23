using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AutoTestRunnerWinUI.Services.FileSettings
{
    public class FileService : IFileService
    {
        public T Read<T>(string folderPath, string fileName)
        {
            var path = Path.Combine(folderPath, fileName);
            if (File.Exists(path))
            {
                var json = File.ReadAllText(path);
                return JsonSerializer.Deserialize<T>(json);
            }

            return default;
        }

        public void Save<T>(string folderPath, string fileName, T content)
        {
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            var unicodeOptions = new JsonSerializerOptions
            {
                //Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic),
                //Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                //DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,

                WriteIndented = true
            };

            var fileContent = JsonSerializer.Serialize(content, unicodeOptions);
            File.WriteAllText(Path.Combine(folderPath, fileName), fileContent, Encoding.UTF8);
        }

        public void Delete(string folderPath, string fileName)
        {
            if (fileName != null && File.Exists(Path.Combine(folderPath, fileName)))
            {
                File.Delete(Path.Combine(folderPath, fileName));
            }
        }
    }
}
