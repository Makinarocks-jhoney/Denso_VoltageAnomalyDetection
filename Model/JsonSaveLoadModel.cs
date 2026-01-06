using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MVP_Voltage.Model
{
    internal static class JsonSaveLoadModel
    {
        private static readonly JsonSerializerOptions _options = new()
        {
            WriteIndented = true // 보기 좋게 들여쓰기
        };

        public static async Task SaveAsync<T>(string path, T data)
        {
            string json = JsonSerializer.Serialize(data, _options);
            await File.WriteAllTextAsync(path, json);
        }
        public static async Task<T?> LoadAsync<T>(string path)
        {
            if (!File.Exists(path))
                return default;

            string json = await File.ReadAllTextAsync(path);
            return JsonSerializer.Deserialize<T>(json);
        }

        public static void MapCommonProperties<TSrc, TDest>(TSrc src, TDest dest)
        {
            var sProps = typeof(TSrc).GetProperties().Where(p => p.CanRead);
            var dMap = typeof(TDest).GetProperties().Where(p => p.CanWrite)
                         .ToDictionary(p => (p.Name, p.PropertyType));

            foreach (var sp in sProps)
            {
                if (dMap.TryGetValue((sp.Name, sp.PropertyType), out var dp))
                    dp.SetValue(dest, sp.GetValue(src)); // setter 호출 → OnPropertyChanged → 후처리 호출
            }
        }

    }
}
