using Gw2Sharp.WebApi.V2.Models;
using System;
using Kenedia.Modules.Core.Utility;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using System.Threading;
using Blish_HUD.Content;
using Blish_HUD;
using System.Runtime.Serialization;
using Kenedia.Modules.Core.DataModels;
using Kenedia.Modules.Core.Models;
using System.Diagnostics;
using DirectoryExtension = Kenedia.Modules.Core.Extensions.DirectoryExtension;
using System.Collections.ObjectModel;
using Kenedia.Modules.FashionManager.Utility;

namespace Kenedia.Modules.FashionManager.Models
{
    public class FashionSlotChangedEventArgs : EventArgs
    {
        public FashionSlot Slot { get; }

        public FashionSubSlot SubSlot { get; }

        public Skin Skin { get; }

        public FashionSlotChangedEventArgs(FashionSlot slot, FashionSubSlot subSlot, Skin skin)
        {
            Slot = slot;
            SubSlot = subSlot;
            Skin = skin;
        }
    }

    public delegate void FashionSlotChangedEventHandler(object sender, FashionSlotChangedEventArgs e);

    public class InfusionSkin : Skin
    {
        public InfusionSkin()
        {

        }
    }

    public abstract class FashionTemplateSlot
    {
        public Skin Skin { get; protected set; }

        public virtual void Set(FashionSubSlot subSlot, Skin skin)
        {

        }
    }

    public class ArmorFashionSlot : FashionTemplateSlot
    {
        public InfusionSkin Infusion { get; protected set; }

        public Color Dye1 { get; protected set; }

        public Color Dye2 { get; protected set; }

        public Color Dye3 { get; protected set; }

        public Color Dye4 { get; protected set; }

        public override void Set(FashionSubSlot subSlot, Skin skin)
        {
            switch (subSlot)
            {
                case FashionSubSlot.Item:
                    Skin = skin;
                    break;

                case FashionSubSlot.Infusion:
                    Infusion = skin as InfusionSkin;
                    break;

            }
        }
    }

    public class WeaponFashionSlot : FashionTemplateSlot
    {
        public InfusionSkin Infusion { get; protected set; }

        public override void Set(FashionSubSlot subSlot, Skin skin)
        {
            switch (subSlot)
            {
                case FashionSubSlot.Item:
                    Skin = skin;
                    break;

                case FashionSubSlot.Infusion:
                    Infusion = skin as InfusionSkin;
                    break;

            }
        }
    }

    public class GatheringFashionSlot : FashionTemplateSlot
    {
        public override void Set(FashionSubSlot subSlot, Skin skin)
        {
            switch (subSlot)
            {
                case FashionSubSlot.Item:
                    Skin = skin;
                    break;
            }
        }
    }

    public class BackFashionSlot : FashionTemplateSlot
    {
        public InfusionSkin Infusion { get; protected set; }

        public override void Set(FashionSubSlot subSlot, Skin skin)
        {
            switch (subSlot)
            {
                case FashionSubSlot.Item:
                    Skin = skin;
                    break;

                case FashionSubSlot.Infusion:
                    Infusion = skin as InfusionSkin;
                    break;

            }
        }
    }

    public class MountFashionSlot
    {
        public MountSkin Mount { get; protected set; }

        public void Set(FashionSubSlot subSlot, MountSkin skin)
        {
            switch (subSlot)
            {
                case FashionSubSlot.Item:
                    Mount = skin;
                    break;
            }
        }
    }

    [DataContract]
    public class FashionTemplate
    {
        private bool _triggerEvents = true;

        private AsyncTexture2D _thumbnail;
        private string _fashionCode;
        private string _name;
        private string _thumbnailFileName;
        private CancellationTokenSource _cancellationTokenSource;
        private SkinBack _back;
        private Races _races = Races.None;
        private Gender _gender = Gender.Unknown;
        private ObservableCollection<AsyncTexture2D> _gallery;

        public FashionTemplate()
        {

        }

        [JsonConstructor]
        public FashionTemplate(string name, string thumbnail, string templateCode, Races? race, Gender? gender, List<string> galleryPaths)
        {
            _name = name;
            _thumbnailFileName = thumbnail;
            _fashionCode = templateCode;
            _races = race ?? _races;
            _gender = gender ?? _gender;
            GalleryPaths = galleryPaths;
        }

        public string FileName => Common.MakeValidFileName(Name?.Trim());

        public string DirectoryPath => $@"{FashionManager.ModuleInstance.Paths.TemplatesPath}{FileName}\";

        [DataMember]
        public string Name { get => _name; set => Common.SetProperty(ref _name, value, OnNameChanged); }

        private void OnNameChanged(object sender, Core.Models.ValueChangedEventArgs<string> e)
        {

        }

        [DataMember]
        public string ThumbnailFileName { get => _thumbnailFileName; set => Common.SetProperty(ref _thumbnailFileName, value, SetThumbnail); }

        private void SetThumbnail(object sender, Core.Models.ValueChangedEventArgs<string> e)
        {
            throw new NotImplementedException();
        }

        [DataMember]
        public Races Race { get => _races; set => Common.SetProperty(ref _races, value, OnRaceChanged); }

        private void OnRaceChanged(object sender, Core.Models.ValueChangedEventArgs<Races> e)
        {
            throw new NotImplementedException();
        }

        [DataMember]
        public Gender Gender { get => _gender; set => Common.SetProperty(ref _gender, value, OnGenderChanged); }

        private void OnGenderChanged(object sender, Core.Models.ValueChangedEventArgs<Gender> e)
        {
            throw new NotImplementedException();
        }

        [DataMember]
        public string FashionCode { get => ParseFashionCode(); set => Common.SetProperty(ref _fashionCode, value); }

        private string ParseFashionCode()
        {
            return FashionChatCode.ParseChatCode(this);
        }

        public AsyncTexture2D Thumbnail
        {
            get
            {
                if (_thumbnail is null && System.IO.File.Exists($"{DirectoryPath}{ThumbnailFileName}"))
                {
                    GameService.Graphics.QueueMainThreadRender((graphicsDevice) => _thumbnail = TextureUtil.FromStreamPremultiplied(graphicsDevice, new FileStream(DirectoryPath + ThumbnailFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)));
                }

                return _thumbnail;
            }
        }

        [DataMember]
        public List<string> GalleryPaths { get; set; } = [];

        public ObservableCollection<AsyncTexture2D> Gallery
        {
            get
            {
                if (_gallery is null)
                {
                    _gallery = [];

                    foreach (string fileName in GalleryPaths)
                    {
                        if (System.IO.File.Exists($"{DirectoryPath}{fileName}"))
                            GameService.Graphics.QueueMainThreadRender((graphicsDevice) => _gallery.Add(TextureUtil.FromStreamPremultiplied(graphicsDevice, new FileStream(DirectoryPath + fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))));
                    }
                }

                return _gallery;
            }
        }

    }
}
