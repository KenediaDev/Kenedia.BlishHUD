using Blish_HUD.Modules.Managers;
using Gw2Sharp.WebApi.V2.Models;
using Kenedia.Modules.Core.Extensions;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Services;
using Kenedia.Modules.FashionManager.Models;
using Kenedia.Modules.FashionManager.Utility;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using File = System.IO.File;

namespace Kenedia.Modules.FashionManager.Services
{
    public class Color : Gw2Sharp.WebApi.V2.Models.Color
    {

        [DataMember]
        public LocalizedString Names { get; protected set; } = [];

        public new string Name
        {
            get => Names.Text;
            set => Names.Text = value;
        }

        public Color()
        {
        }

        public Color(Gw2Sharp.WebApi.V2.Models.Color color)
        {
            Id = color.Id;
            Name = color.Name;
            BaseRgb = color.BaseRgb;
            Cloth = color.Cloth;
            Leather = color.Leather;
            Metal = color.Metal;
            Fur = color.Fur;
            Item = color.Item;
            Categories = color.Categories;
        }
    }

    public abstract class DataEntry<TId, TValue> : IEnumerable<TValue>
    {
        protected Dictionary<TId, TValue> Dictionary = [];

        protected virtual string FilePath { get; set; }

        protected DataEntry(Gw2ApiManager gw2ApiManager, Paths paths)
        {
            Gw2ApiManager = gw2ApiManager;
            Paths = paths;
        }

        public Gw2ApiManager Gw2ApiManager { get; }

        public Paths Paths { get; }

        public TValue? this[TId id]
        {
            get => TryGetValue(id, out var value) ? value : default;
            set => Dictionary[id] = value;
        }

        public bool TryGetValue(TId id, out TValue value)
        {
            return Dictionary.TryGetValue(id, out value);
        }

        public IEnumerator<TValue> GetEnumerator()
        {
            return Dictionary.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public abstract Task<bool> LoadDataFromWebAsync(DataLoadProgress? progress = null, CancellationToken cancellationToken = default);

        public abstract Task<bool> LoadDataAsync(DataLoadProgress? progress = null, CancellationToken cancellationToken = default);

        public virtual Task<bool> Load(DataLoadProgress? progress = null, CancellationToken cancellationToken = default)
        {
            if (!File.Exists(FilePath))
            {
                return LoadDataFromWebAsync(progress, cancellationToken);
            }

            return LoadDataAsync(progress, cancellationToken);
        }
    }

    public class InfusionsDataEntry : DataEntry<int, InfusionSkin>
    {
        public InfusionsDataEntry(StaticHosting staticHosting, Gw2ApiManager gw2ApiManager, Paths paths) : base(gw2ApiManager, paths)
        {
            FilePath = Path.Combine(Paths.DataPath, "infusions.json");
            StaticHosting = staticHosting;
        }

        public StaticHosting StaticHosting { get; }

        public override async Task<bool> LoadDataAsync(DataLoadProgress progress = null, CancellationToken cancellationToken = default)
        {
            progress?.Report(new DataLoadReport("Infusions loading", 0, 0, 0));

            return true;
        }

        public override async Task<bool> LoadDataFromWebAsync(DataLoadProgress progress = null, CancellationToken cancellationToken = default)
        {
            progress?.Report(new DataLoadReport("Infusions loading", 0, 0, 0));

            return true;
        }
    }

    public class ColorDataEntry : DataEntry<int, Color>
    {
        public ColorDataEntry(Gw2ApiManager gw2ApiManager, Paths paths) : base(gw2ApiManager, paths)
        {
            FilePath = Path.Combine(Paths.DataPath, "miniatures.json");
        }

        public override async Task<bool> LoadDataAsync(DataLoadProgress? progress = null, CancellationToken cancellationToken = default)
        {
            if (!File.Exists(FilePath))
            {
                return false;
            }

            string json = File.ReadAllText(FilePath);
            Dictionary = JsonConvert.DeserializeObject<Dictionary<int, Color>>(json, SerializerSettings.Default);

            progress?.Report(new DataLoadReport("Colors loaded", Dictionary.Count, Dictionary.Count, Dictionary.Count / Dictionary.Count));

            return await Task.FromResult(true);
        }

        public override async Task<bool> LoadDataFromWebAsync(DataLoadProgress? progress = null, CancellationToken cancellationToken = default)
        {
            var allids = await Gw2ApiManager.Gw2ApiClient.V2.Colors.IdsAsync(cancellationToken);

            progress?.Report(new DataLoadReport("Colors loading", 0, allids.Count, 0));

            var colors = new List<Color>();
            var idBatches = allids.ToList().ChunkBy(200);

            foreach (var ids in idBatches)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return false;
                }

                var colorChunk = await Gw2ApiManager.Gw2ApiClient.V2.Colors.ManyAsync(ids, cancellationToken);

                progress?.Report(new DataLoadReport("Colors loading", colors.Count, allids.Count, colors.Count / (double)allids.Count));

                if (colorChunk is not null)
                {
                    colors.AddRange(colorChunk.Select(x => new Color(x)));
                }
            }

            progress?.Report(new DataLoadReport("Colors loaded", colors.Count, allids.Count, colors.Count / (double)allids.Count));
            Dictionary = colors.ToDictionary(c => c.Id, c => c);

            string json = JsonConvert.SerializeObject(Dictionary, SerializerSettings.Default);
            File.WriteAllText(FilePath, json);

            return true;
        }
    }

    public class ArmorSkinsDataEntry : DataEntry<int, SkinArmor>
    {
        public ArmorSkinsDataEntry(Gw2ApiManager gw2ApiManager, Paths paths) : base(gw2ApiManager, paths)
        {
            FilePath = Path.Combine(Paths.DataPath, "armorskins.json");
        }

        public override async Task<bool> LoadDataAsync(DataLoadProgress? progress = null, CancellationToken cancellationToken = default)
        {
            if (!File.Exists(FilePath))
            {
                return false;
            }

            string json = File.ReadAllText(FilePath);
            Dictionary = JsonConvert.DeserializeObject<Dictionary<int, SkinArmor>>(json, SerializerSettings.Default);

            progress?.Report(new DataLoadReport("Armor Skins loaded", Dictionary.Count, Dictionary.Count, Dictionary.Count / Dictionary.Count));

            return await Task.FromResult(true);
        }

        public override async Task<bool> LoadDataFromWebAsync(DataLoadProgress? progress = null, CancellationToken cancellationToken = default)
        {
            var allids = await Gw2ApiManager.Gw2ApiClient.V2.Skins.IdsAsync(cancellationToken);

            progress?.Report(new DataLoadReport("Armor Skins loading", 0, allids.Count, 0));

            var skins = new List<SkinArmor>();
            var idBatches = allids.ToList().ChunkBy(200);

            foreach (var ids in idBatches)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return false;
                }

                var skinChunk = await Gw2ApiManager.Gw2ApiClient.V2.Skins.ManyAsync(ids, cancellationToken);

                progress?.Report(new DataLoadReport("Armor Skins loading", skins.Count, allids.Count, skins.Count / (double)allids.Count));

                if (skinChunk is not null)
                {
                    skins.AddRange(skinChunk.OfType<SkinArmor>());
                }
            }

            progress?.Report(new DataLoadReport("Armor Skins loaded", skins.Count, allids.Count, skins.Count / (double)allids.Count));
            Dictionary = skins.ToDictionary(c => c.Id, c => c);

            string json = JsonConvert.SerializeObject(Dictionary, SerializerSettings.Default);
            File.WriteAllText(FilePath, json);

            return true;
        }
    }

    public class WeaponSkinsDataEntry : DataEntry<int, SkinWeapon>
    {
        public WeaponSkinsDataEntry(Gw2ApiManager gw2ApiManager, Paths paths) : base(gw2ApiManager, paths)
        {
            FilePath = Path.Combine(Paths.DataPath, "weaponskins.json");
        }

        public override async Task<bool> LoadDataAsync(DataLoadProgress? progress = null, CancellationToken cancellationToken = default)
        {
            if (!File.Exists(FilePath))
            {
                return false;
            }

            string json = File.ReadAllText(FilePath);
            Dictionary = JsonConvert.DeserializeObject<Dictionary<int, SkinWeapon>>(json, SerializerSettings.Default);

            progress?.Report(new DataLoadReport("Weapon Skins loaded", Dictionary.Count, Dictionary.Count, Dictionary.Count / Dictionary.Count));

            return await Task.FromResult(true);
        }

        public override async Task<bool> LoadDataFromWebAsync(DataLoadProgress? progress = null, CancellationToken cancellationToken = default)
        {
            var allids = await Gw2ApiManager.Gw2ApiClient.V2.Skins.IdsAsync(cancellationToken);

            progress?.Report(new DataLoadReport("Weapon Skins loading", 0, allids.Count, 0));

            var skins = new List<SkinWeapon>();
            var idBatches = allids.ToList().ChunkBy(200);

            foreach (var ids in idBatches)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return false;
                }

                var skinChunk = await Gw2ApiManager.Gw2ApiClient.V2.Skins.ManyAsync(ids, cancellationToken);

                progress?.Report(new DataLoadReport("Weapon Skins loading", skins.Count, allids.Count, skins.Count / (double)allids.Count));

                if (skinChunk is not null)
                {
                    skins.AddRange(skinChunk.OfType<SkinWeapon>());
                }
            }

            progress?.Report(new DataLoadReport("Weapon Skins loaded", skins.Count, allids.Count, skins.Count / (double)allids.Count));
            Dictionary = skins.ToDictionary(c => c.Id, c => c);

            string json = JsonConvert.SerializeObject(Dictionary, SerializerSettings.Default);
            File.WriteAllText(FilePath, json);

            return true;
        }
    }

    public class BackSkinsDataEntry : DataEntry<int, SkinBack>
    {
        public BackSkinsDataEntry(Gw2ApiManager gw2ApiManager, Paths paths) : base(gw2ApiManager, paths)
        {
            FilePath = Path.Combine(Paths.DataPath, "backskins.json");
        }

        public override async Task<bool> LoadDataAsync(DataLoadProgress? progress = null, CancellationToken cancellationToken = default)
        {
            if (!File.Exists(FilePath))
            {
                return false;
            }

            string json = File.ReadAllText(FilePath);
            Dictionary = JsonConvert.DeserializeObject<Dictionary<int, SkinBack>>(json, SerializerSettings.Default);

            progress?.Report(new DataLoadReport("Back Skins loaded", Dictionary.Count, Dictionary.Count, Dictionary.Count / (double) Dictionary.Count));

            return await Task.FromResult(true);
        }

        public override async Task<bool> LoadDataFromWebAsync(DataLoadProgress? progress = null, CancellationToken cancellationToken = default)
        {
            var allids = await Gw2ApiManager.Gw2ApiClient.V2.Skins.IdsAsync(cancellationToken);

            progress?.Report(new DataLoadReport("Back Skins loading", 0, allids.Count, 0));

            var skins = new List<SkinBack>();
            var idBatches = allids.ToList().ChunkBy(200);

            foreach (var ids in idBatches)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return false;
                }

                var skinChunk = await Gw2ApiManager.Gw2ApiClient.V2.Skins.ManyAsync(ids, cancellationToken);

                progress?.Report(new DataLoadReport("Back Skins loading", skins.Count, allids.Count, skins.Count / (double)allids.Count));

                if (skinChunk is not null)
                {
                    skins.AddRange(skinChunk.OfType<SkinBack>());
                }
            }

            progress?.Report(new DataLoadReport("Back Skins loaded", skins.Count, allids.Count, skins.Count / (double)allids.Count));
            Dictionary = skins.ToDictionary(c => c.Id, c => c);

            string json = JsonConvert.SerializeObject(Dictionary, SerializerSettings.Default);
            File.WriteAllText(FilePath, json);

            return true;
        }
    }

    public class GatheringSkinsDataEntry : DataEntry<int, SkinGathering>
    {
        public GatheringSkinsDataEntry(Gw2ApiManager gw2ApiManager, Paths paths) : base(gw2ApiManager, paths)
        {
            FilePath = Path.Combine(Paths.DataPath, "gatheringskins.json");
        }

        public override async Task<bool> LoadDataAsync(DataLoadProgress? progress = null, CancellationToken cancellationToken = default)
        {
            if (!File.Exists(FilePath))
            {
                return false;
            }

            string json = File.ReadAllText(FilePath);
            Dictionary = JsonConvert.DeserializeObject<Dictionary<int, SkinGathering>>(json, SerializerSettings.Default);

            progress?.Report(new DataLoadReport("Gathering Skins loaded", Dictionary.Count, Dictionary.Count, Dictionary.Count / (double) Dictionary.Count));

            return await Task.FromResult(true);
        }

        public override async Task<bool> LoadDataFromWebAsync(DataLoadProgress? progress = null, CancellationToken cancellationToken = default)
        {
            var allids = await Gw2ApiManager.Gw2ApiClient.V2.Skins.IdsAsync(cancellationToken);

            progress?.Report(new DataLoadReport("Gathering Skins loading", 0, allids.Count, 0));

            var skins = new List<SkinGathering>();
            var idBatches = allids.ToList().ChunkBy(200);

            foreach (var ids in idBatches)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return false;
                }

                var skinChunk = await Gw2ApiManager.Gw2ApiClient.V2.Skins.ManyAsync(ids, cancellationToken);

                progress?.Report(new DataLoadReport("Gathering Skins loading", skins.Count, allids.Count, skins.Count / (double)allids.Count));

                if (skinChunk is not null)
                {
                    skins.AddRange(skinChunk.OfType<SkinGathering>());
                }
            }

            progress?.Report(new DataLoadReport("Gathering Skins loaded", skins.Count, allids.Count, skins.Count / (double)allids.Count));
            Dictionary = skins.ToDictionary(c => c.Id, c => c);

            string json = JsonConvert.SerializeObject(Dictionary, SerializerSettings.Default);
            File.WriteAllText(FilePath, json);

            return true;
        }
    }

    public class MiniaturesDataEntry : DataEntry<int, Mini>
    {
        public MiniaturesDataEntry(Gw2ApiManager gw2ApiManager, Paths paths) : base(gw2ApiManager, paths)
        {
            FilePath = Path.Combine(Paths.DataPath, "miniatures.json");
        }

        public override async Task<bool> LoadDataAsync(DataLoadProgress? progress = null, CancellationToken cancellationToken = default)
        {
            if (!File.Exists(FilePath))
            {
                return false;
            }

            string json = File.ReadAllText(FilePath);
            Dictionary = JsonConvert.DeserializeObject<Dictionary<int, Mini>>(json, SerializerSettings.Default);

            progress?.Report(new DataLoadReport("Miniatures loaded", Dictionary.Count, Dictionary.Count, Dictionary.Count / (double)Dictionary.Count));

            return await Task.FromResult(true);
        }

        public override async Task<bool> LoadDataFromWebAsync(DataLoadProgress? progress = null, CancellationToken cancellationToken = default)
        {
            var allids = await Gw2ApiManager.Gw2ApiClient.V2.Minis.IdsAsync(cancellationToken);

            progress?.Report(new DataLoadReport("Miniatures loading", 0, allids.Count, 0));

            var miniatures = new List<Mini>();
            var idBatches = allids.ToList().ChunkBy(200);

            foreach (var ids in idBatches)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return false;
                }

                var miniaturesChunk = await Gw2ApiManager.Gw2ApiClient.V2.Minis.ManyAsync(ids, cancellationToken);

                progress?.Report(new DataLoadReport("Miniatures loading", miniatures.Count, allids.Count, miniatures.Count / (double)allids.Count));

                if (miniaturesChunk is not null)
                {
                    miniatures.AddRange(miniaturesChunk.OfType<Mini>());
                }
            }

            progress?.Report(new DataLoadReport("Miniatures loaded", miniatures.Count, allids.Count, miniatures.Count / (double)allids.Count));
            Dictionary = miniatures.ToDictionary(c => c.Id, c => c);

            string json = JsonConvert.SerializeObject(Dictionary, SerializerSettings.Default);
            File.WriteAllText(FilePath, json);

            return true;
        }
    }

    public class NoveltiesDataEntry : DataEntry<int, Novelty>
    {
        public NoveltiesDataEntry(Gw2ApiManager gw2ApiManager, Paths paths) : base(gw2ApiManager, paths)
        {
            FilePath = Path.Combine(Paths.DataPath, "noveltys.json");
        }

        public override async Task<bool> LoadDataAsync(DataLoadProgress? progress = null, CancellationToken cancellationToken = default)
        {
            if (!File.Exists(FilePath))
            {
                return false;
            }

            string json = File.ReadAllText(FilePath);
            Dictionary = JsonConvert.DeserializeObject<Dictionary<int, Novelty>>(json, SerializerSettings.Default);

            progress?.Report(new DataLoadReport("Noveltys loaded", Dictionary.Count, Dictionary.Count, Dictionary.Count / (double)Dictionary.Count));

            return await Task.FromResult(true);
        }

        public override async Task<bool> LoadDataFromWebAsync(DataLoadProgress? progress = null, CancellationToken cancellationToken = default)
        {
            var allids = await Gw2ApiManager.Gw2ApiClient.V2.Novelties.IdsAsync(cancellationToken);

            progress?.Report(new DataLoadReport("Noveltys loading", 0, allids.Count, 0));

            var noveltys = new List<Novelty>();
            var idBatches = allids.ToList().ChunkBy(200);

            foreach (var ids in idBatches)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return false;
                }

                var noveltysChunk = await Gw2ApiManager.Gw2ApiClient.V2.Novelties.ManyAsync(ids, cancellationToken);

                progress?.Report(new DataLoadReport("Noveltys loading", noveltys.Count, allids.Count, noveltys.Count / (double)allids.Count));

                if (noveltysChunk is not null)
                {
                    noveltys.AddRange(noveltysChunk.OfType<Novelty>());
                }
            }

            progress?.Report(new DataLoadReport("Noveltys loaded", noveltys.Count, allids.Count, noveltys.Count / (double)allids.Count));
            Dictionary = noveltys.ToDictionary(c => c.Id, c => c);

            string json = JsonConvert.SerializeObject(Dictionary, SerializerSettings.Default);
            File.WriteAllText(FilePath, json);

            return true;
        }
    }

    public class GlidersDataEntry : DataEntry<int, Glider>
    {
        public GlidersDataEntry(Gw2ApiManager gw2ApiManager, Paths paths) : base(gw2ApiManager, paths)
        {
            FilePath = Path.Combine(Paths.DataPath, "gliders.json");
        }

        public override async Task<bool> LoadDataAsync(DataLoadProgress? progress = null, CancellationToken cancellationToken = default)
        {
            if (!File.Exists(FilePath))
            {
                return false;
            }

            string json = File.ReadAllText(FilePath);
            Dictionary = JsonConvert.DeserializeObject<Dictionary<int, Glider>>(json, SerializerSettings.Default);

            progress?.Report(new DataLoadReport("Gliders loaded", Dictionary.Count, Dictionary.Count, Dictionary.Count / (double)Dictionary.Count));

            return await Task.FromResult(true);
        }

        public override async Task<bool> LoadDataFromWebAsync(DataLoadProgress? progress = null, CancellationToken cancellationToken = default)
        {
            var allids = await Gw2ApiManager.Gw2ApiClient.V2.Gliders.IdsAsync(cancellationToken);

            progress?.Report(new DataLoadReport("Gliders loading", 0, allids.Count, 0));

            var gliders = new List<Glider>();
            var idBatches = allids.ToList().ChunkBy(200);

            foreach (var ids in idBatches)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return false;
                }

                var glidersChunk = await Gw2ApiManager.Gw2ApiClient.V2.Gliders.ManyAsync(ids, cancellationToken);

                progress?.Report(new DataLoadReport("Gliders loading", gliders.Count, allids.Count, gliders.Count / (double)allids.Count));

                if (glidersChunk is not null)
                {
                    gliders.AddRange(glidersChunk.OfType<Glider>());
                }
            }

            progress?.Report(new DataLoadReport("Gliders loaded", gliders.Count, allids.Count, gliders.Count / (double)allids.Count));
            Dictionary = gliders.ToDictionary(c => c.Id, c => c);

            string json = JsonConvert.SerializeObject(Dictionary, SerializerSettings.Default);
            File.WriteAllText(FilePath, json);

            return true;
        }
    }

    public class OutfitsDataEntry : DataEntry<int, Outfit>
    {
        public OutfitsDataEntry(Gw2ApiManager gw2ApiManager, Paths paths) : base(gw2ApiManager, paths)
        {
            FilePath = Path.Combine(Paths.DataPath, "outfits.json");
        }

        public override async Task<bool> LoadDataAsync(DataLoadProgress? progress = null, CancellationToken cancellationToken = default)
        {
            if (!File.Exists(FilePath))
            {
                return false;
            }

            string json = File.ReadAllText(FilePath);
            Dictionary = JsonConvert.DeserializeObject<Dictionary<int, Outfit>>(json, SerializerSettings.Default);

            progress?.Report(new DataLoadReport("Outfits loaded", Dictionary.Count, Dictionary.Count, Dictionary.Count / (double)Dictionary.Count));

            return await Task.FromResult(true);
        }

        public override async Task<bool> LoadDataFromWebAsync(DataLoadProgress? progress = null, CancellationToken cancellationToken = default)
        {
            var allids = await Gw2ApiManager.Gw2ApiClient.V2.Outfits.IdsAsync(cancellationToken);

            progress?.Report(new DataLoadReport("Outfits loading", 0, allids.Count, 0));

            var outfits = new List<Outfit>();
            var idBatches = allids.ToList().ChunkBy(200);

            foreach (var ids in idBatches)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return false;
                }

                var outfitsChunk = await Gw2ApiManager.Gw2ApiClient.V2.Outfits.ManyAsync(ids, cancellationToken);

                progress?.Report(new DataLoadReport("Outfits loading", outfits.Count, allids.Count, outfits.Count / (double)allids.Count));

                if (outfitsChunk is not null)
                {
                    outfits.AddRange(outfitsChunk.OfType<Outfit>());
                }
            }

            progress?.Report(new DataLoadReport("Outfits loaded", outfits.Count, allids.Count, outfits.Count / (double)allids.Count));
            Dictionary = outfits.ToDictionary(c => c.Id, c => c);

            string json = JsonConvert.SerializeObject(Dictionary, SerializerSettings.Default);
            File.WriteAllText(FilePath, json);

            return true;
        }
    }

    public class MailCarriersDataEntry : DataEntry<int, MailCarrier>
    {
        public MailCarriersDataEntry(Gw2ApiManager gw2ApiManager, Paths paths) : base(gw2ApiManager, paths)
        {
            FilePath = Path.Combine(Paths.DataPath, "mailCarriers.json");
        }

        public override async Task<bool> LoadDataAsync(DataLoadProgress? progress = null, CancellationToken cancellationToken = default)
        {
            if (!File.Exists(FilePath))
            {
                return false;
            }

            string json = File.ReadAllText(FilePath);
            Dictionary = JsonConvert.DeserializeObject<Dictionary<int, MailCarrier>>(json, SerializerSettings.Default);

            progress?.Report(new DataLoadReport("MailCarriers loaded", Dictionary.Count, Dictionary.Count, Dictionary.Count / (double)Dictionary.Count));

            return await Task.FromResult(true);
        }

        public override async Task<bool> LoadDataFromWebAsync(DataLoadProgress? progress = null, CancellationToken cancellationToken = default)
        {
            var allids = await Gw2ApiManager.Gw2ApiClient.V2.MailCarriers.IdsAsync(cancellationToken);

            progress?.Report(new DataLoadReport("MailCarriers loading", 0, allids.Count, 0));

            var mailCarriers = new List<MailCarrier>();
            var idBatches = allids.ToList().ChunkBy(200);

            foreach (var ids in idBatches)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return false;
                }

                var mailCarriersChunk = await Gw2ApiManager.Gw2ApiClient.V2.MailCarriers.ManyAsync(ids, cancellationToken);

                progress?.Report(new DataLoadReport("MailCarriers loading", mailCarriers.Count, allids.Count, mailCarriers.Count / (double)allids.Count));

                if (mailCarriersChunk is not null)
                {
                    mailCarriers.AddRange(mailCarriersChunk.OfType<MailCarrier>());
                }
            }

            progress?.Report(new DataLoadReport("MailCarriers loaded", mailCarriers.Count, allids.Count, mailCarriers.Count / (double)allids.Count));
            Dictionary = mailCarriers.ToDictionary(c => c.Id, c => c);

            string json = JsonConvert.SerializeObject(Dictionary, SerializerSettings.Default);
            File.WriteAllText(FilePath, json);

            return true;
        }
    }

    public class FinishersDataEntry : DataEntry<int, Finisher>
    {
        public FinishersDataEntry(Gw2ApiManager gw2ApiManager, Paths paths) : base(gw2ApiManager, paths)
        {
            FilePath = Path.Combine(Paths.DataPath, "finishers.json");
        }

        public override async Task<bool> LoadDataAsync(DataLoadProgress? progress = null, CancellationToken cancellationToken = default)
        {
            if (!File.Exists(FilePath))
            {
                return false;
            }

            string json = File.ReadAllText(FilePath);
            Dictionary = JsonConvert.DeserializeObject<Dictionary<int, Finisher>>(json, SerializerSettings.Default);
            progress?.Report(new DataLoadReport("Finishers loaded", Dictionary.Count, Dictionary.Count, Dictionary.Count / (double)Dictionary.Count));

            return await Task.FromResult(true);
        }

        public override async Task<bool> LoadDataFromWebAsync(DataLoadProgress? progress = null, CancellationToken cancellationToken = default)
        {
            var allids = await Gw2ApiManager.Gw2ApiClient.V2.Finishers.IdsAsync(cancellationToken);

            progress?.Report(new DataLoadReport("Finishers loading", 0, allids.Count, 0));

            var finishers = new List<Finisher>();
            var idBatches = allids.ToList().ChunkBy(200);

            foreach (var ids in idBatches)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return false;
                }

                var finishersChunk = await Gw2ApiManager.Gw2ApiClient.V2.Finishers.ManyAsync(ids, cancellationToken);

                progress?.Report(new DataLoadReport("Finishers loading", finishers.Count, allids.Count, finishers.Count / (double)allids.Count));

                if (finishersChunk is not null)
                {
                    finishers.AddRange(finishersChunk.OfType<Finisher>());
                }
            }

            progress?.Report(new DataLoadReport("Finishers loaded", finishers.Count, allids.Count, finishers.Count / (double)allids.Count));
            Dictionary = finishers.ToDictionary(c => c.Id, c => c);

            string json = JsonConvert.SerializeObject(Dictionary, SerializerSettings.Default);
            File.WriteAllText(FilePath, json);

            return true;
        }
    }

    public class MountsDataEntry : DataEntry<int, MountSkin>
    {
        public MountsDataEntry(Gw2ApiManager gw2ApiManager, Paths paths) : base(gw2ApiManager, paths)
        {
            FilePath = Path.Combine(Paths.DataPath, "mounts.json");
        }

        public override async Task<bool> LoadDataAsync(DataLoadProgress? progress = null, CancellationToken cancellationToken = default)
        {
            if (!File.Exists(FilePath))
            {
                return false;
            }

            string json = File.ReadAllText(FilePath);
            Dictionary = JsonConvert.DeserializeObject<Dictionary<int, MountSkin>>(json, SerializerSettings.Default);

            progress?.Report(new DataLoadReport("Mounts loaded", Dictionary.Count, Dictionary.Count, Dictionary.Count / (double)Dictionary.Count));

            return await Task.FromResult(true);
        }

        public override async Task<bool> LoadDataFromWebAsync(DataLoadProgress? progress = null, CancellationToken cancellationToken = default)
        {
            var allids = await Gw2ApiManager.Gw2ApiClient.V2.Mounts.Skins.IdsAsync(cancellationToken);

            progress?.Report(new DataLoadReport("Mounts loading", 0, allids.Count, 0));

            var mounts = new List<MountSkin>();
            var idBatches = allids.ToList().ChunkBy(200);

            foreach (var ids in idBatches)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return false;
                }

                var mountsChunk = await Gw2ApiManager.Gw2ApiClient.V2.Mounts.Skins.ManyAsync(ids, cancellationToken);

                progress?.Report(new DataLoadReport("Mounts loading", mounts.Count, allids.Count, mounts.Count / (double)allids.Count));

                if (mountsChunk is not null)
                {
                    mounts.AddRange(mountsChunk.OfType<MountSkin>());
                }
            }

            progress?.Report(new DataLoadReport("Mounts loaded", mounts.Count, allids.Count, mounts.Count / (double)allids.Count));
            Dictionary = mounts.ToDictionary(c => c.Id, c => c);

            string json = JsonConvert.SerializeObject(Dictionary, SerializerSettings.Default);
            File.WriteAllText(FilePath, json);

            return true;
        }
    }

    public class Data
    {
        private CancellationTokenSource? _tokenSource;

        public Data(Gw2ApiManager gw2ApiManager, StaticHosting staticHosting, Paths paths)
        {
            Gw2ApiManager = gw2ApiManager;
            StaticHosting = staticHosting;
            Paths = paths;

            Colors = new ColorDataEntry(gw2ApiManager, paths);
            ArmorSkins = new ArmorSkinsDataEntry(gw2ApiManager, paths);
            WeaponSkins = new WeaponSkinsDataEntry(gw2ApiManager, paths);
            BackSkins = new BackSkinsDataEntry(gw2ApiManager, paths);
            GatheringSkins = new GatheringSkinsDataEntry(gw2ApiManager, paths);
            Miniatures = new MiniaturesDataEntry(gw2ApiManager, paths);
            Novelties = new NoveltiesDataEntry(gw2ApiManager, paths);
            Gliders = new GlidersDataEntry(gw2ApiManager, paths);
            Outfits = new OutfitsDataEntry(gw2ApiManager, paths);
            MailCarriers = new MailCarriersDataEntry(gw2ApiManager, paths);
            Finishers = new FinishersDataEntry(gw2ApiManager, paths);
            Mounts = new MountsDataEntry(gw2ApiManager, paths);
            Infusions = new InfusionsDataEntry(staticHosting, gw2ApiManager, paths);
        }

        public Gw2ApiManager Gw2ApiManager { get; }

        public StaticHosting StaticHosting { get; }

        public Paths Paths { get; }

        public ColorDataEntry Colors { get; }

        public ArmorSkinsDataEntry ArmorSkins { get; }

        public WeaponSkinsDataEntry WeaponSkins { get; }

        public BackSkinsDataEntry BackSkins { get; }

        public GatheringSkinsDataEntry GatheringSkins { get; }

        public MiniaturesDataEntry Miniatures { get; }

        public NoveltiesDataEntry Novelties { get; }

        public GlidersDataEntry Gliders { get; }

        public OutfitsDataEntry Outfits { get; }

        public MailCarriersDataEntry MailCarriers { get; }

        public FinishersDataEntry Finishers { get; }

        public MountsDataEntry Mounts { get; }

        public InfusionsDataEntry Infusions { get; }

        public async Task<bool> LoadDataFromGw2ApiAsync()
        {
            Debug.WriteLine($"LoadDataFromGw2ApiAsync");

            _tokenSource?.Cancel();
            _tokenSource = new CancellationTokenSource();

            bool result = false;

            var progress = new DataLoadProgress();

            var tasks = new List<(string name, Task<bool> task)>
            {
                (nameof(Colors), Colors.Load(progress, _tokenSource.Token)),
                (nameof(ArmorSkins), ArmorSkins.Load(progress, _tokenSource.Token)),
                (nameof(WeaponSkins), WeaponSkins.Load(progress, _tokenSource.Token)),
                (nameof(BackSkins), BackSkins.Load(progress, _tokenSource.Token)),
                (nameof(GatheringSkins), GatheringSkins.Load(progress, _tokenSource.Token)),
                (nameof(Miniatures), Miniatures.Load(progress, _tokenSource.Token)),
                (nameof(Novelties), Novelties.Load(progress, _tokenSource.Token)),
                (nameof(Gliders), Gliders.Load(progress, _tokenSource.Token)),
                (nameof(Outfits), Outfits.Load(progress, _tokenSource.Token)),
                (nameof(MailCarriers), MailCarriers.Load(progress, _tokenSource.Token)),
                (nameof(Finishers), Finishers.Load(progress, _tokenSource.Token)),
                (nameof(Mounts), Mounts.Load(progress, _tokenSource.Token)),
                (nameof(Infusions), Infusions.Load(progress, _tokenSource.Token))
            };

            //After each task is completed print me a result with a message
            foreach (var task in tasks)
            {
                result = await task.task;
                Debug.WriteLine($"{task.name} result: {result}");
            }

            return false;
        }
    }
}
