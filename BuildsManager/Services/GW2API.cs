﻿using Blish_HUD;
using Blish_HUD.Modules.Managers;
using Gw2Sharp.WebApi;
using Gw2Sharp.WebApi.V2.Models;
using Kenedia.Modules.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using Kenedia.Modules.Core.Extensions;
using Kenedia.Modules.BuildsManager.Models;
using Kenedia.Modules.Core.Controls;
using Kenedia.Modules.BuildsManager.Extensions;

namespace Kenedia.Modules.BuildsManager.Services
{
    public class GW2API
    {
        private readonly Logger _logger = Logger.GetLogger(typeof(GW2API));

        private CancellationTokenSource _cancellationTokenSource;
        private Exception _lastException = null;

        private readonly List<int> _infusions =
        [
            39336, //Mighty Infusion
            39335, //Precise Infusion
            39337, //Malign Infusion
            39339, //Resilient Infusion
            39338, //Vital Infusion
            39340, //Healing Infusion

            39616, //Healing +5 Agony Infusion
            39617, //Resilient +5 Agony Infusion
            39618, //Vital +5 Agony Infusion
            39619, //Malign +5 Agony Infusion
            39620, //Mighty +5 Agony Infusion
            39621, //Precise +5 Agony Infusion
            85971, //Spiteful +5 Agony Infusion
            86338, //Mystical +5 Agony Infusion

            37133, //Resilient +7 Agony Infusion
            37134, //Vital +7 Agony Infusion
            37123, //Healing +7 Agony Infusion
            37127, //Mighty +7 Agony Infusion
            37128, //Precise +7 Agony Infusion
            37129, //Malign +7 Agony Infusion
            85881, //Mystical +7 Agony Infusion
            86150, //Spiteful +7 Agony Infusion

            37130, //Malign +9 Agony Infusion
            37131, //Mighty +9 Agony Infusion
            37132, //Precise +9 Agony Infusion
            37135, //Resilient +9 Agony Infusion
            37136, //Vital +9 Agony Infusion            
            37125, //Healing +9 Agony Infusion
            86180, //Mystical +9 Agony Infusion
            86113, //Spiteful +9 Agony Infusion

            43250, //Healing WvW Infusion
            43251, //Resilient WvW Infusion
            43252, //Vital WvW Infusion
            43253, //Malign WvW Infusion
            43254, //Mighty WvW Infusion
            43255, //Precise WvW Infusion
            86986, //Concentration WvW Infusion
            87218, //Expertise WvW Infusion

            49424, //+1 Agony Infusion
            49425, //+2 Agony Infusion
            49426, //+3 Agony Infusion
            49427, //+4 Agony Infusion
            49428, //+5 Agony Infusion
            49429, //+6 Agony Infusion
            49430, //+7 Agony Infusion
            49431, //+8 Agony Infusion
            49432, //+9 Agony Infusion
            49433, //+10 Agony Infusion
            49434, //+11 Agony Infusion
            49435, //+12 Agony Infusion
            49436, //+13 Agony Infusion
            49437, //+14 Agony Infusion
            49438, //+15 Agony Infusion
            49439, //+16 Agony Infusion
            49440, //+17 Agony Infusion
            49441, //+18 Agony Infusion
            49442, //+19 Agony Infusion
            49443, //+20 Agony Infusion
            49444, //+21 Agony Infusion
            49445, //+22 Agony Infusion
            49446, //+23 Agony Infusion
            49447, //+24 Agony Infusion
            
            87528, //Swim-Speed Infusion +10
            87518, //Swim-Speed Infusion +11
            87493, //Swim-Speed Infusion +12
            87503, //Swim-Speed Infusion +13
            87526, //Swim-Speed Infusion +14
            87496, //Swim-Speed Infusion +15
            87497, //Swim-Speed Infusion +16
            87508, //Swim-Speed Infusion +17
            87516, //Swim-Speed Infusion +18
            87532, //Swim-Speed Infusion +19
            87495, //Swim-Speed Infusion +20
            87525, //Swim-Speed Infusion +21
            87511, //Swim-Speed Infusion +22
            87512, //Swim-Speed Infusion +23
            87527, //Swim-Speed Infusion +24
            87502, //Swim-Speed Infusion +25
            87538, //Swim-Speed Infusion +26
            87504, //Swim-Speed Infusion +27
            //87504, //Swim-Speed Infusion +28
            //87504, //Swim-Speed Infusion +29
            //87504, //Swim-Speed Infusion +30
        ];

        private Func<NotificationBadge> _getNotificationBadge;
        private Func<LoadingSpinner> _getSpinner;

        public GW2API(Gw2ApiManager gw2ApiManager, Data data, Paths paths, Func<NotificationBadge> notificationBadge, Func<LoadingSpinner> spinner)
        {

            Gw2ApiManager = gw2ApiManager;
            Data = data;
            Paths = paths;

            _getNotificationBadge = notificationBadge;
            _getSpinner = spinner;
        }

        public NotificationBadge NotificationBadge => _getNotificationBadge?.Invoke() is NotificationBadge badge ? badge : null;

        public LoadingSpinner Spinner => _getSpinner?.Invoke() is LoadingSpinner spinner ? spinner : null;

        public Gw2ApiManager Gw2ApiManager { get; }

        private Data Data { get; }

        public Paths Paths { get; }

        public void Cancel()
        {
            _cancellationTokenSource?.Cancel();
        }

        public bool IsCanceled()
        {
            return _cancellationTokenSource is not null && _cancellationTokenSource.IsCancellationRequested;
        }

        public async Task UpdateData()
        {
            if (Paths == null)
            {
                _logger.Info($"No Paths set for {nameof(UpdateData)}!");
                return;
            }

            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = new();

            _logger.Info($"{nameof(UpdateData)}: Fetch data ...");

            LocalizedString state = [];

            if (NotificationBadge is NotificationBadge notificationBadge)
            {
                notificationBadge.Visible = false;
            }

            Locale locale = GameService.Overlay.UserLocale.Value;
        }

        public async Task UpdateMappedIds()
        {
            try
            {
                _cancellationTokenSource?.Cancel();
                _cancellationTokenSource = new CancellationTokenSource();

                var mapCollection = StaticVersion.LoadFromFile($@"{Paths.ModuleDataPath}DataMap.json");
                var mapVersions = mapCollection.GetVersions();

                var raw_itemids = await Gw2ApiManager.Gw2ApiClient.V2.Items.IdsAsync(_cancellationTokenSource.Token);
                var invalidIds = new List<int>()
                {
                    11126, // Corrupted
                    63366, //Nameless Rune
                    90369, // Corrupted
                    100262, //Relic of Fireworks duplicate
                };

                if (_cancellationTokenSource.IsCancellationRequested)
                    return;

                var itemid_lists = raw_itemids.Except(invalidIds).ToList().ChunkBy(200);
                int count = 0;

                foreach (var ids in itemid_lists)
                {
                    if (_cancellationTokenSource.IsCancellationRequested)
                        return;

                    if (ids is not null)
                    {
                        IReadOnlyList<Item> items = null;
                        count++;

                        BuildsManager.Logger.Info($"Fetching chunk {count}/{itemid_lists.Count}");

                        try
                        {
                            items = await Gw2ApiManager.Gw2ApiClient.V2.Items.ManyAsync(ids, _cancellationTokenSource.Token);

                            foreach (var item in items)
                            {
                                try
                                {
                                    List<ByteIntMap> maps = [];

                                    switch (item)
                                    {
                                        case ItemArmor armor:
                                            if (Data.SkinDictionary.ContainsKey(armor.Id)) maps.Add(mapCollection.Armors);
                                            break;

                                        case ItemBack back:
                                            if (Data.SkinDictionary.ContainsKey(back.Id)) maps.Add(mapCollection.Backs);
                                            break;

                                        case ItemWeapon weapon:
                                            if (Data.SkinDictionary.ContainsKey(weapon.Id)) maps.Add(mapCollection.Weapons);
                                            break;

                                        case ItemTrinket trinket:
                                            if (Data.SkinDictionary.ContainsKey(trinket.Id)) maps.Add(mapCollection.Trinkets);
                                            break;

                                        case ItemConsumable consumable:
                                            if (consumable.Level == 80 && (consumable.Details.ApplyCount is not null || consumable.Rarity.Value is ItemRarity.Ascended))
                                            {
                                                maps.Add(consumable.Details.Type.Value switch
                                                {
                                                    ItemConsumableType.Food => mapCollection.Nourishments,
                                                    ItemConsumableType.Utility => mapCollection.Enhancements,
                                                    _ => null,
                                                });
                                            }

                                            break;

                                        case ItemUpgradeComponent upgrade:
                                            if (upgrade.Details.InfusionUpgradeFlags?.ToList()?.Contains(ItemInfusionFlag.Infusion) == true && _infusions.Contains(upgrade.Id))
                                            {
                                                maps.Add(mapCollection.Infusions);
                                            }
                                            else if (upgrade.Details.InfusionUpgradeFlags?.ToList()?.Contains(ItemInfusionFlag.Enrichment) == true)
                                            {
                                                maps.Add(mapCollection.Enrichments);
                                            }
                                            else if (upgrade.Rarity.Value is ItemRarity.Exotic)
                                            {
                                                bool isRune = upgrade.Details.Flags.ToList().Contains(ItemUpgradeComponentFlag.HeavyArmor) && !upgrade.Details.Flags.ToList().Contains(ItemUpgradeComponentFlag.Trinket) && !upgrade.Details.Flags.ToList().Contains(ItemUpgradeComponentFlag.Sword);
                                                bool isSigil = !upgrade.Details.Flags.ToList().Contains(ItemUpgradeComponentFlag.HeavyArmor) && !upgrade.Details.Flags.ToList().Contains(ItemUpgradeComponentFlag.Trinket) && upgrade.Details.Flags.ToList().Contains(ItemUpgradeComponentFlag.Sword);

                                                if (isRune)
                                                {
                                                    if (upgrade.GameTypes.FirstOrDefault(e => e.Value == ItemGameType.Pvp) is not null) maps.Add(mapCollection.PvpRunes);
                                                    if (upgrade.GameTypes.FirstOrDefault(e => e.Value == ItemGameType.Pve) is not null) maps.Add(mapCollection.PveRunes);
                                                }

                                                if (isSigil)
                                                {
                                                    if (upgrade.GameTypes.FirstOrDefault(e => e.Value == ItemGameType.Pvp) is not null) maps.Add(mapCollection.PvpSigils);
                                                    if (upgrade.GameTypes.FirstOrDefault(e => e.Value == ItemGameType.Pve) is not null) maps.Add(mapCollection.PveSigils);
                                                }
                                            }

                                            break;

                                        default:
                                            if (item.Type.ToString() is "Relic" or "Mwcc" && item.Rarity == ItemRarity.Exotic)
                                            {
                                                if (item.GameTypes.FirstOrDefault(e => e.Value == ItemGameType.Pvp) is not null) maps.Add(mapCollection.PvpRelics);
                                                if (item.GameTypes.FirstOrDefault(e => e.Value == ItemGameType.Pve) is not null) maps.Add(mapCollection.PveRelics);
                                            }
                                            else if (item.Type == ItemType.PowerCore)
                                            {
                                                maps.Add(mapCollection.PowerCores);
                                            }

                                            break;
                                    }

                                    foreach (var map in maps)
                                    {
                                        if (map is not null && map.Items.FirstOrDefault(x => x.Value == item.Id) is KeyValuePair<byte, int> sitem && sitem.Value <= 0 && map.Count < byte.MaxValue)
                                        {
                                            BuildsManager.Logger.Info($"Adding {item.Id} to {item.Type}");
                                            map.Add((byte)(map.Count + 1), item.Id);

                                            if (mapVersions.TryGetValue(map.Name, out SemVer.Version mapVersion))
                                            {
                                                if (mapVersion.ToString() == map.Version.ToString())
                                                {
                                                    map.Version = map.Version.Increment();

                                                    BuildsManager.Logger.Info($"Updating {item.Type} version from {mapVersion} to {map.Version}");
                                                }
                                            }
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    BuildsManager.Logger.Warn($"{item.Id}Exception {ex}");
                                }
                            }
                        }
                        catch (Exception ex)
                        {

                            BuildsManager.Logger.Warn($"Exception thrown for ids: {Environment.NewLine + string.Join($",{Environment.NewLine}", ids)} {Environment.NewLine + ex}");
                        }
                    }
                }

                var ring = (ItemTrinket)await Gw2ApiManager.Gw2ApiClient.V2.Items.GetAsync(91234, _cancellationTokenSource.Token);
                var legyWeapon = (ItemWeapon)await Gw2ApiManager.Gw2ApiClient.V2.Items.GetAsync(30698, _cancellationTokenSource.Token);
                var statIds = ring.Details.StatChoices.Concat(legyWeapon.Details.StatChoices);

                var apiStats = await Gw2ApiManager.Gw2ApiClient.V2.Itemstats.AllAsync(_cancellationTokenSource.Token);
                if (_cancellationTokenSource.IsCancellationRequested) return;

                foreach (var e in apiStats)
                {
                    if (statIds.Contains(e.Id))
                    {
                        var map = mapCollection.Stats;
                        if (map is not null && map.Items.FirstOrDefault(x => x.Value == e.Id) is KeyValuePair<byte, int> sitem && sitem.Value <= 0 && map.Count < byte.MaxValue)
                        {
                            BuildsManager.Logger.Info($"Adding {e.Id} to Stats.");
                            map.Add((byte)(map.Count + 1), e.Id);
                        }
                    }
                }

                var apiAmulets = await Gw2ApiManager.Gw2ApiClient.V2.Pvp.Amulets.AllAsync(_cancellationTokenSource.Token);
                if (_cancellationTokenSource.IsCancellationRequested) return;

                foreach (var e in apiAmulets)
                {
                    var map = mapCollection.PvpAmulets;
                    if (map is not null && map.Items.FirstOrDefault(x => x.Value == e.Id) is KeyValuePair<byte, int> sitem && sitem.Value <= 0 && map.Count < byte.MaxValue)
                    {
                        BuildsManager.Logger.Info($"Adding {e.Id} to Pvp Amulets.");
                        map.Add((byte)(map.Count + 1), e.Id);
                    }
                }

                BuildsManager.Logger.Info($@"Save to {Paths.ModuleDataPath}DataMap.json");
                mapCollection.Save($@"{Paths.ModuleDataPath}DataMap.json");
            }
            catch
            {
            }
        }
    }
}
