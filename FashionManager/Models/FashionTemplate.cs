using Gw2Sharp.WebApi.V2.Models;
using System;
using Kenedia.Modules.Core.Utility;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;
using System.Threading;
using Blish_HUD.Content;
using Blish_HUD;
using System.Runtime.Serialization;
using Kenedia.Modules.Core.DataModels;
using System.Collections.ObjectModel;
using Kenedia.Modules.FashionManager.Utility;

namespace Kenedia.Modules.FashionManager.Models
{

    [DataContract]
    public class FashionTemplate
    {
        private bool _triggerEvents = true;
        private string _fashionCode;
        private string _name;
        private string _thumbnailFileName;
        private CancellationTokenSource _cancellationTokenSource;
        private SkinBack _back;
        private Races _races = Races.None;
        private Gender _gender = Gender.Unknown;

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
                if (field is null && System.IO.File.Exists($"{DirectoryPath}{ThumbnailFileName}"))
                {
                    GameService.Graphics.QueueMainThreadRender((graphicsDevice) => field = TextureUtil.FromStreamPremultiplied(graphicsDevice, new FileStream(DirectoryPath + ThumbnailFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)));
                }

                return field;
            }

            private set;
        }

        [DataMember]
        public List<string> GalleryPaths { get; set; } = [];

        public ObservableCollection<AsyncTexture2D> Gallery
        {
            get
            {
                if (field is null)
                {
                    field = [];

                    foreach (string fileName in GalleryPaths)
                    {
                        if (System.IO.File.Exists($"{DirectoryPath}{fileName}"))
                            GameService.Graphics.QueueMainThreadRender((graphicsDevice) => field.Add(TextureUtil.FromStreamPremultiplied(graphicsDevice, new FileStream(DirectoryPath + fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))));
                    }
                }

                return field;
            }
        }
    }
}
