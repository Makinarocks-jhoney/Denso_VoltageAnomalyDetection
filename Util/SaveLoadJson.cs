using MVP_Voltage.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVP_Voltage.Util
{
    internal class SaveLoadJson
    {
        public static T LoadData<T>(string Path, T target) where T : class
        {
            var json = System.Text.Json.JsonSerializer.Deserialize<T>(File.ReadAllText(Path));
            JsonSaveLoadModel.MapCommonProperties(json, (T)target);
            return target;
        }
        public static async Task SaveData<T>(string Path, T target) where T : class
        {
            await JsonSaveLoadModel.SaveAsync(Path, (T)target);
        }
    }
}
