using Blish_HUD;
using Blish_HUD.Modules.Managers;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;
using Kenedia.Modules.BuildsManager.DataModels.Items;
using Kenedia.Modules.Core.Extensions;
using Kenedia.Modules.Core.Models;
using Gw2Sharp.WebApi;
using Kenedia.Modules.Core.Utility;
using System.Threading;
using Gw2Sharp.WebApi.V2.Models;
using File = System.IO.File;
using Kenedia.Modules.BuildsManager.Res;
using System.Collections.Generic;
using Kenedia.Modules.BuildsManager.Models;

namespace Kenedia.Modules.BuildsManager.Services
{
    public class ItemMappedDataEntry<T> : MappedDataEntry<int, T> where T : BaseItem, new()
    {
        private List<int> _pendingIds = [];

        public async Task<List<int>> LoadAndGetPending(string name, ByteIntMap map, string path)
        {
            try
            {
                MappedDataEntry<int, T> loaded = null;

                if (!DataLoaded && File.Exists(path))
                {
                    BuildsManager.Logger.Debug($"Load {name}.json");
                    string json = File.ReadAllText(path);
                    loaded = JsonConvert.DeserializeObject<MappedDataEntry<int, T>>(json, SerializerSettings.Default);
                    DataLoaded = true;
                }

                Map = map;
                Items = loaded?.Items ?? Items;
                Version = loaded?.Version ?? Version;

                BuildsManager.Logger.Debug($"{name} Current Version: {Version} | Required Version: {map.Version}");

                foreach (int id in Map.Ignored.Values)
                {
                    _ = Items.Remove(id);
                }

                var lang = GameService.Overlay.UserLocale.Value is Locale.Korean or Locale.Chinese ? Locale.English : GameService.Overlay.UserLocale.Value;

                _pendingIds = [];

                switch (map.Version > Version)
                {
                    case true:
                        BuildsManager.Logger.Debug($"The current version does not match the map version. Updating all values for {name}.");
                        Version = map.Version;
                        _pendingIds = [.. _pendingIds, .. Map.Values.Except(Items.Keys).Except(Map.Ignored.Values)];
                        break;

                    case false:
                        _pendingIds = [.. Items.Values.Where(item => item.Names[lang] == null)?.Select(e => e.Id)];
                        break;
                }

                if (_pendingIds.Count > 0)
                {
                    BuildsManager.Logger.Debug($"A total of {_pendingIds.Count} {name} need to be fetched.");
                    return _pendingIds;
                }

                return [];
            }
            catch (Exception ex)
            {
                BuildsManager.Logger.Warn(ex, $"Failed to load {name} data.");
                return [];
            }
        }

        public async override Task<bool> LoadAndUpdate(string name, ByteIntMap map, string path, Gw2ApiManager gw2ApiManager, CancellationToken cancellationToken)
        {
            try
            {
                bool saveRequired = _pendingIds.Count > 0;
               
                if (_pendingIds.Count > 0)
                {
                    var idSets = _pendingIds.ChunkBy(200);

                    var legyArmorHelmet = await gw2ApiManager.Gw2ApiClient.V2.Items.GetAsync(80384, cancellationToken);
                    var statChoices = (legyArmorHelmet is ItemArmor armor) ? armor.Details.StatChoices : new List<int>();

                    saveRequired = saveRequired || idSets.Count > 0;
                    BuildsManager.Logger.Debug($"Fetch a total of {_pendingIds.Count} {name} in {idSets.Count} sets.");
                    foreach (var ids in idSets)
                    {
                        var items = await gw2ApiManager.Gw2ApiClient.V2.Items.ManyAsync(ids, cancellationToken);
                        if (cancellationToken.IsCancellationRequested)
                        {
                            return false;
                        }

                        foreach (var item in items)
                        {
                            bool exists = Items.Values.TryFind(e => e.Id == item.Id, out T entryItem);
                            entryItem ??= new()
                            {
                                MappedId = Map?.Items?.FirstOrDefault(e => e.Value == item.Id).Key ?? 0,
                            };

                            entryItem?.Apply(item);

                            if (entryItem is not null && entryItem.Type is Core.DataModels.ItemType.Relic)
                            {
                                entryItem.TemplateSlot = name is nameof(Data.PvpRelics) ? Models.Templates.TemplateSlotType.PvpRelic : Models.Templates.TemplateSlotType.PveRelic;
                            }

                            if (entryItem is not null && Data.SkinDictionary.TryGetValue(item.Id, out int? assetId) && assetId is not null)
                            {
                                entryItem.Rarity = ItemRarity.Ascended;

                                if (entryItem.TemplateSlot is Models.Templates.TemplateSlotType.AquaBreather && entryItem is Armor aquaBreather)
                                {
                                    aquaBreather.StatChoices = statChoices;
                                }

                                if (entryItem.Type is Core.DataModels.ItemType.Trinket)
                                {
                                    entryItem.AssetId = assetId.Value;
                                    entryItem.Name = entryItem.TemplateSlot switch
                                    {
                                        Models.Templates.TemplateSlotType.Amulet => strings.Amulet,
                                        Models.Templates.TemplateSlotType.Ring_1 => strings.Ring,
                                        Models.Templates.TemplateSlotType.Ring_2 => strings.Ring,
                                        Models.Templates.TemplateSlotType.Accessory_1 => strings.Accessory,
                                        Models.Templates.TemplateSlotType.Accessory_2 => strings.Accessory,
                                        _ => entryItem.Name
                                    };
                                }
                                else
                                {
                                    var skin = await gw2ApiManager.Gw2ApiClient.V2.Skins.GetAsync(assetId.Value);
                                    entryItem.AssetId = skin?.Icon.GetAssetIdFromRenderUrl() ?? 0;

                                    entryItem.Name = skin?.Name;
                                }
                            }

                            if (!exists)
                                Items.Add(item.Id, entryItem);
                        }
                    }
                }

                if (saveRequired)
                {
                    BuildsManager.Logger.Debug($"Saving {name}.json");
                    string json = JsonConvert.SerializeObject(this, SerializerSettings.Default);
                    File.WriteAllText(path, json);
                }

                DataLoaded = DataLoaded || Items.Count > 0;
                return true;
            }
            catch (Exception ex)
            {
                BuildsManager.Logger.Warn(ex, $"Failed to load {name} data.");
                return false;
            }
        }
    }
}
