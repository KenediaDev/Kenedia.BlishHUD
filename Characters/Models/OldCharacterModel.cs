using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Kenedia.Modules.Core.Models;

namespace Kenedia.Modules.Characters.Models
{
    public class OldCharacterModel
    {
        public static SemVer.Version ImportVersion = new("1.0.3");
        public string Name;
        public string Tags;
        public string Icon;
        public int Map;

        public static void Import(string path, ObservableCollection<Character_Model> characters, string imagePath, string accountName, TagList tags)
        {
            if (File.Exists(path))
            {
                try
                {
                    if (!Directory.Exists(imagePath))
                    {
                        _ = Directory.CreateDirectory(imagePath);
                    }

                    string content = File.ReadAllText(path);
                    var old_characters = JsonConvert.DeserializeObject<List<OldCharacterModel>>(content, SerializerSettings.Default);
                    string basePath = path.Replace("\\" + accountName + "\\characters.json", "");
                    imagePath = imagePath.Replace("\\" + accountName + "\\images", "");

                        foreach (var character in characters)
                    {
                        var old = old_characters.Find(e => e.Name == character.Name);

                        old?.Tags.Split('|')?.ToList()?.ForEach(t =>
                        {
                            if (!string.IsNullOrEmpty(t))
                            {
                                character.AddTag(t);
                                if(!tags.Contains(t)) 
                                { 
                                    tags.Add(t); 
                                }
                            }
                        });
                        character.Map = old?.Map is not null ? old.Map : 0;
                        character.IconPath = !string.IsNullOrEmpty(old.Icon) ? old.Icon : string.Empty;

                        if (!string.IsNullOrEmpty(old.Icon))
                        {
                            try
                            {
                                if (File.Exists(basePath + "\\" + old.Icon) && !File.Exists(imagePath + old.Icon))
                                {
                                    Characters.Logger.Info($"Copy Icon for {old.Name} from old path '{basePath + "\\" + old.Icon}' to '{imagePath + old.Icon}'.");
                                    File.Copy(basePath + "\\" + old.Icon, imagePath + old.Icon);
                                }
                            }
                            catch (Exception ex)
                            {

                            }
                        }
                    }
                }
                catch { }
            }
        }
    }
}
