using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Gw2Mumble;
using Blish_HUD.Modules;
using Blish_HUD.Modules.Managers;
using Blish_HUD.Settings;
using Characters.Res;
using Gw2Sharp.WebApi.V2.Models;
using Kenedia.Modules.Characters.Controls;
using Kenedia.Modules.Characters.Enums;
using Kenedia.Modules.Characters.Models;
using Kenedia.Modules.Characters.Services;
using Kenedia.Modules.Characters.Views;
using Kenedia.Modules.Core.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Resources;
using System.Threading.Tasks;
using static Kenedia.Modules.Characters.Services.TextureManager;
using File = System.IO.File;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using Version = SemVer.Version;
using CornerIcon = Kenedia.Modules.Core.Controls.CornerIcon;
using LoadingSpinner = Kenedia.Modules.Core.Controls.LoadingSpinner;
using Blish_HUD.Controls;
using Kenedia.Modules.Core.Res;
using Microsoft.Xna.Framework.Graphics;
using Kenedia.Modules.Core.Controls;
using Color = Microsoft.Xna.Framework.Color;
using Characters.Controls;
using Kenedia.Modules.Core.Extensions;
using System.Runtime;

namespace Kenedia.Modules.Characters
{
    [Export(typeof(Module))]
    public class Characters : BaseModule<Characters, MainWindow, SettingsModel>
    {
        public readonly Version BaseVersion;
        public readonly ResourceManager RM = new("Characters.Res.strings", System.Reflection.Assembly.GetExecutingAssembly());

        public readonly Dictionary<string, SearchFilter<Character_Model>> TagFilters = new();
        private readonly Ticks _ticks = new();

        private CornerIcon _cornerIcon;

        [ImportingConstructor]
        public Characters([Import("ModuleParameters")] ModuleParameters moduleParameters)
            : base(moduleParameters)
        {
            ModuleInstance = this;
            HasGUI = true;
            BaseVersion = Version.BaseVersion();
        }

        public Dictionary<string, SearchFilter<Character_Model>> SearchFilters { get; private set; }

        public TagList Tags { get; } = new TagList();

        public CharacterSwapping CharacterSwapping { get; } = new();

        public CharacterSorting CharacterSorting { get; } = new();

        public RunIndicator RunIndicator { get; private set; }

        public SettingsWindow SettingsWindow { get; private set; }

        private FramedContainer _framedContainer;

        public RadialMenu RadialMenu { get; private set; }
        public PotraitCapture PotraitCapture { get; private set; }

        public LoadingSpinner APISpinner { get; private set; }

        public TextureManager TextureManager { get; private set; }

        public ObservableCollection<Character_Model> CharacterModels { get; } = new();

        public Character_Model CurrentCharacterModel { get; set; }

        public Data Data { get; private set; }

        public OCR OCR { get; private set; }

        public string GlobalAccountsPath { get; set; }

        public string CharactersPath { get; set; }

        public string AccountImagesPath { get; set; }

        public string AccountPath { get; set; }

        public bool SaveCharacters { get; set; }

        public GW2API_Handler GW2APIHandler { get; private set; }

        public void UpdateFolderPaths(string accountName, bool api_handled = true)
        {
            Paths.AccountName = accountName;

            Characters mIns = ModuleInstance;
            string b = mIns.Paths.ModulePath;

            mIns.AccountPath = b + accountName;
            mIns.CharactersPath = b + accountName + @"\characters.json";
            mIns.AccountImagesPath = b + accountName + @"\images\";

            if (!Directory.Exists(mIns.AccountPath))
            {
                _ = Directory.CreateDirectory(mIns.AccountPath);
            }

            if (!Directory.Exists(mIns.AccountImagesPath))
            {
                _ = Directory.CreateDirectory(mIns.AccountImagesPath);
            }

            if (api_handled && CharacterModels.Count == 0)
            {
                _ = LoadCharacters();
            }
        }

        public void SwapTo(Character_Model character)
        {
            PlayerCharacter player = GameService.Gw2Mumble.PlayerCharacter;

            if (character.Name != player.Name || !GameService.GameIntegration.Gw2Instance.IsInGame)
            {

                CharacterSwapping.Start(character);
            }
        }

        public bool LoadCharacters()
        {
            PlayerCharacter player = GameService.Gw2Mumble.PlayerCharacter;

            if (player == null || string.IsNullOrEmpty(player.Name))
            {
                return false;
            }

            AccountSummary getAccount()
            {
                try
                {
                    string path = GlobalAccountsPath;
                    if (File.Exists(path))
                    {
                        string content = File.ReadAllText(path);
                        List<AccountSummary> accounts = JsonConvert.DeserializeObject<List<AccountSummary>>(content);
                        return accounts.Find(e => e.CharacterNames.Contains(player.Name));
                    }
                }
                catch { }

                return null;
            }

            AccountSummary account = getAccount();
            if (account != null)
            {
                Logger.Debug($"Found '{player.Name}' in a stored character list for '{account.AccountName}'. Loading characters of '{account.AccountName}'");
                UpdateFolderPaths(account.AccountName, false);
                return LoadCharacterFile();
            }

            return false;
        }

        public bool LoadCharacterFile()
        {
            try
            {
                if (File.Exists(CharactersPath))
                {
                    FileInfo infos = new(CharactersPath);
                    string content = File.ReadAllText(CharactersPath);
                    PlayerCharacter player = GameService.Gw2Mumble.PlayerCharacter;
                    List<Character_Model> characters = JsonConvert.DeserializeObject<List<Character_Model>>(content);

                    if (characters != null)
                    {
                        characters.ForEach(c =>
                        {
                            if (!CharacterModels.Contains(c))
                            {
                                foreach (string t in c.Tags)
                                {
                                    if (!Tags.Contains(t))
                                    {
                                        Tags.AddTag(t, false);
                                    }
                                }

                                CharacterModels.Add(c);
                                c.Initialize(CharacterSwapping, Paths.ModulePath);
                            }
                        });

                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                Logger.Warn(ex, "Failed to load the local characters from file '" + CharactersPath + "'.");
                File.Copy(CharactersPath, CharactersPath.Replace(".json", " [" + DateTimeOffset.Now.ToUnixTimeSeconds().ToString() + "].corruped.json"));
                return false;
            }
        }

        public void SaveCharacterList()
        {
            string json = JsonConvert.SerializeObject(CharacterModels, Formatting.Indented);

            // write string to file
            File.WriteAllText(CharactersPath, json);
        }

        protected override void Initialize()
        {
            base.Initialize();

            Logger.Info($"Starting  {Name} v." + BaseVersion);

            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore,
            };

            GlobalAccountsPath = Paths.ModulePath + @"\accounts.json";

            if (!File.Exists(Paths.ModulePath + @"\gw2.traineddata") || Settings.Version.Value != BaseVersion)
            {
                using Stream target = File.Create(Paths.ModulePath + @"\gw2.traineddata");
                Stream source = ContentsManager.GetFileStream(@"data\gw2.traineddata");
                _ = source.Seek(0, SeekOrigin.Begin);
                source.CopyTo(target);
            }

            if (!File.Exists(Paths.ModulePath + @"\tesseract.dll") || Settings.Version.Value != BaseVersion)
            {
                using Stream target = File.Create(Paths.ModulePath + @"\tesseract.dll");
                Stream source = ContentsManager.GetFileStream(@"data\tesseract.dll");
                _ = source.Seek(0, SeekOrigin.Begin);
                source.CopyTo(target);
            }

            Data = new Data();
            CreateToggleCategories();

            Gw2ApiManager.SubtokenUpdated += Gw2ApiManager_SubtokenUpdated;

            Settings.ShortcutKey.Value.Enabled = true;
            Settings.ShortcutKey.Value.Activated += ShortcutWindowToggle;
            
            Settings.RadialKey.Value.Enabled = true;
            Settings.RadialKey.Value.Activated += RadialMenuToggle;

            Tags.CollectionChanged += Tags_CollectionChanged;

            Settings.Version.Value = BaseVersion;

            CharacterSwapping.CharacterSorting = CharacterSorting;
            CharacterSorting.CharacterSwapping = CharacterSwapping;

            GW2APIHandler = new GW2API_Handler(CharacterSwapping, Paths.ModulePath);
        }

        private void RadialMenuToggle(object sender, EventArgs e)
        {
            if(Settings.EnableRadialMenu.Value) _ = (RadialMenu?.ToggleVisibility());
        }

        protected override void DefineSettings(SettingCollection settings)
        {
            base.DefineSettings(settings);

            Settings = new SettingsModel(settings);
            Settings.ShowCornerIcon.SettingChanged += ShowCornerIcon_SettingChanged;
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            _ticks.Global += gameTime.ElapsedGameTime.TotalMilliseconds;
            _ticks.APIUpdate += gameTime.ElapsedGameTime.TotalSeconds;
            _ticks.Save += gameTime.ElapsedGameTime.TotalMilliseconds;
            _ticks.Tags += gameTime.ElapsedGameTime.TotalMilliseconds;
            _ticks.OCR += gameTime.ElapsedGameTime.TotalMilliseconds;

            MouseState mouse = GameService.Input.Mouse.State;
            if (mouse.LeftButton == ButtonState.Pressed || mouse.RightButton == ButtonState.Pressed || GameService.Input.Keyboard.KeysDown.Count > 0)
            {
                CancelEverything();
            }

            PlayerCharacter player = GameService.Gw2Mumble.PlayerCharacter;

            if (_ticks.Global > 15000)
            {
                _ticks.Global = 0;
                CurrentCharacterModel = null;

                if (GameService.GameIntegration.Gw2Instance.IsInGame)
                {
                    player = GameService.Gw2Mumble.PlayerCharacter;
                    CurrentCharacterModel = CharacterModels.ToList().Find(e => e.Name == player.Name);

                    if (CurrentCharacterModel != null)
                    {
                        CurrentCharacterModel.Specialization = (SpecializationType)player.Specialization;
                        CurrentCharacterModel.Map = GameService.Gw2Mumble.CurrentMap.Id;
                        CurrentCharacterModel.LastLogin = DateTime.UtcNow;

                        MainWindow?.SortCharacters();
                    }
                }
            }

            if (_ticks.APIUpdate > 300)
            {
                _ticks.APIUpdate = 0;

                GW2APIHandler.CheckAPI();
            }

            if (_ticks.Save > 25 && SaveCharacters)
            {
                _ticks.Save = 0;

                SaveCharacterList();
                SaveCharacters = false;
            }
        }

        private void CancelEverything()
        {
            CharacterSwapping.Cancel();
            CharacterSorting.Cancel();
        }

        protected override async Task LoadAsync()
        {
            await base.LoadAsync();
        }

        protected override void OnModuleLoaded(EventArgs e)
        {
            // Base handler must be called
            base.OnModuleLoaded(e);

            TextureManager = new TextureManager();

            if (Settings.ShowCornerIcon.Value)
            {
                CreateCornerIcons();
            }

            PlayerCharacter player = GameService.Gw2Mumble.PlayerCharacter;
            player.SpecializationChanged += ForceUpdate;
            player.NameChanged += ForceUpdate;

            CurrentMap map = GameService.Gw2Mumble.CurrentMap;
            map.MapChanged += ForceUpdate;

            GameService.GameIntegration.Gw2Instance.IsInGameChanged += ForceUpdate;

            CharacterModels.CollectionChanged += OnCharacterCollectionChanged;

            if (Settings.LoadCachedAccounts.Value) _ = LoadCharacters();
        }

        protected override void Unload()
        {
            Settings?.Dispose();
            SettingsWindow?.Dispose();
            MainWindow?.Dispose();
            _cornerIcon?.Dispose();

            TextureManager?.Dispose();
            TextureManager = null;

            DeleteCornerIcons();

            CharacterModels.CollectionChanged -= OnCharacterCollectionChanged;

            PlayerCharacter player = GameService.Gw2Mumble.PlayerCharacter;
            player.SpecializationChanged -= ForceUpdate;
            player.NameChanged -= ForceUpdate;

            CurrentMap map = GameService.Gw2Mumble.CurrentMap;
            map.MapChanged -= ForceUpdate;

            GameService.GameIntegration.Gw2Instance.IsInGameChanged -= ForceUpdate;
            Tags.CollectionChanged -= Tags_CollectionChanged;

            base.Unload();
        }

        protected override void ReloadKey_Activated(object sender, EventArgs e)
        {
            base.ReloadKey_Activated(sender, e);
            GameService.Graphics.SpriteScreen.Visible = true;
            MainWindow?.ToggleWindow();
            SettingsWindow?.ToggleWindow();
        }

        private void OnCharacterCollectionChanged(object sender, EventArgs e)
        {
            MainWindow?.CreateCharacterControls(CharacterModels);
        }

        private void ShortcutWindowToggle(object sender, EventArgs e)
        {
            if (Control.ActiveControl is not Blish_HUD.Controls.TextBox)
            {
                MainWindow?.ToggleWindow();
            }
        }

        private void Gw2ApiManager_SubtokenUpdated(object sender, ValueEventArgs<IEnumerable<TokenPermission>> e)
        {
            GW2APIHandler.CheckAPI();
        }

        private void Tags_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UpdateTagsCollection();
        }

        private void UpdateTagsCollection()
        {
            foreach (Character_Model c in CharacterModels)
            {
                c.UpdateTags(Tags);
            }
        }

        private void ForceUpdate(object sender, EventArgs e)
        {
            _ticks.Global = 2000000;
            if (CurrentCharacterModel != null)
            {
                CurrentCharacterModel.LastLogin = DateTime.UtcNow;
            }

            CurrentCharacterModel = null;
        }

        private void CreateCornerIcons()
        {
            DeleteCornerIcons();

            _cornerIcon = new CornerIcon()
            {
                Icon = AsyncTexture2D.FromAssetId(156678),
                HoverIcon = AsyncTexture2D.FromAssetId(156679),
                SetLocalizedTooltip = () => string.Format(strings.Toggle, $"{Name}"),
                Parent = GameService.Graphics.SpriteScreen,
                Visible = Settings.ShowCornerIcon.Value,
                ClickAction = () => MainWindow?.ToggleWindow(),
            };

            APISpinner = new LoadingSpinner()
            {
                Location = new Point(_cornerIcon.Left, _cornerIcon.Bottom + 3),
                Parent = GameService.Graphics.SpriteScreen,
                Size = new Point(_cornerIcon.Width, _cornerIcon.Height),
                BasicTooltipText = strings_common.FetchingApiData,
                Visible = false,
            };

            _cornerIcon.Moved += CornerIcon_Moved;
        }

        private void DeleteCornerIcons()
        {
            if (_cornerIcon != null) _cornerIcon.Moved -= CornerIcon_Moved;
            _cornerIcon?.Dispose();
            _cornerIcon = null;

            APISpinner?.Dispose();
            APISpinner = null;
        }

        private void CornerIcon_Moved(object sender, MovedEventArgs e)
        {
            if (APISpinner != null) APISpinner.Location = new Point(_cornerIcon.Left, _cornerIcon.Bottom + 3);
        }

        private void ShowCornerIcon_SettingChanged(object sender, ValueChangedEventArgs<bool> e)
        {
            if (e.NewValue)
            {
                CreateCornerIcons();
            }
            else
            {
                DeleteCornerIcons();
            }
        }

        protected override void LoadGUI()
        {
            base.LoadGUI();
            RadialMenu = new RadialMenu(Settings, CharacterModels, GameService.Graphics.SpriteScreen)
            {
                Visible = false,
                ZIndex = int.MaxValue  / 2
            };

            PotraitCapture = new PotraitCapture(Services.ClientWindowService, Services.SharedSettings) { Parent = GameService.Graphics.SpriteScreen, Visible = false, ZIndex = int.MaxValue - 1 };
            OCR = new(Services.ClientWindowService, Services.SharedSettings, Settings, Paths.ModulePath, CharacterModels);
            RunIndicator = new(CharacterSorting, CharacterSwapping);

            var settingsBg = AsyncTexture2D.FromAssetId(155997);
            Texture2D cutSettingsBg = settingsBg.Texture.GetRegion(0, 0, settingsBg.Width - 482, settingsBg.Height - 390);
            SettingsWindow = new(
                settingsBg,
                new Rectangle(30, 30, cutSettingsBg.Width + 10, cutSettingsBg.Height),
                new Rectangle(30, 35, cutSettingsBg.Width - 5, cutSettingsBg.Height - 15),
                SharedSettingsView,
                OCR)
            {
                Parent = GameService.Graphics.SpriteScreen,
                Title = "❤",
                Subtitle = "❤",
                SavesPosition = true,
                Id = $"CharactersSettingsWindow",
            };

            Texture2D bg = TextureManager.GetBackground(Backgrounds.MainWindow);
            Texture2D cutBg = bg.GetRegion(25, 25, bg.Width - 100, bg.Height - 325);

            MainWindow = new(
            bg,
            new Rectangle(25, 25, cutBg.Width + 10, cutBg.Height),
            new Rectangle(35, 14, cutBg.Width - 10, cutBg.Height - 10),
            CharacterSorting,
            Settings)
            {
                Parent = GameService.Graphics.SpriteScreen,
                Title = "❤",
                Subtitle = "❤",
                SavesPosition = true,
                Id = $"CharactersWindow",
                CanResize = true,
                Size = Settings.WindowSize.Value,
            };
        }

        protected override void UnloadGUI()
        {
            base.UnloadGUI();

            RadialMenu?.Dispose();
            SettingsWindow?.Dispose();
            MainWindow?.Dispose();
            PotraitCapture?.Dispose();
            OCR?.Dispose();
            RunIndicator?.Dispose();
        }

        public override IView GetSettingsView()
        {
            return new SettingsView();
        }

        private void CreateToggleCategories()
        {
            var filters = new List<SearchFilter<Character_Model>>();

            SearchFilters = new Dictionary<string, SearchFilter<Character_Model>>();

            foreach (KeyValuePair<Gw2Sharp.Models.ProfessionType, Data.Profession> e in Data.Professions)
            {
                SearchFilters.Add(e.Value.Name, new((c) => Settings.DisplayToggles.Value["Profession"].Check && c.Profession == e.Key));
                SearchFilters.Add($"Core {e.Value.Name}", new((c) => Settings.DisplayToggles.Value["Profession"].Check && c.Profession == e.Key));
            }

            foreach (KeyValuePair<SpecializationType, Data.Specialization> e in Data.Specializations)
            {
                SearchFilters.Add(e.Value.Name, new((c) => Settings.DisplayToggles.Value["Profession"].Check && c.Specialization == e.Key));
            }

            foreach (KeyValuePair<int, Data.CrafingProfession> e in Data.CrafingProfessions)
            {
                SearchFilters.Add(e.Value.Name, new((c) => c.Crafting.Find(p => Settings.DisplayToggles.Value["CraftingProfession"].Check && p.Id == e.Value.Id && (!Settings.DisplayToggles.Value["OnlyMaxCrafting"].Check || p.Rating >= e.Value.MaxRating)) != null));
            }

            foreach (KeyValuePair<Gw2Sharp.Models.RaceType, Data.Race> e in Data.Races)
            {
                SearchFilters.Add(e.Value.Name, new((c) => Settings.DisplayToggles.Value["Race"].Check && c.Race == e.Key));
            }

            SearchFilters.Add("Birthday", new((c) => c.HasBirthdayPresent));
            SearchFilters.Add("Hidden", new((c) => !c.Show));
            SearchFilters.Add("Female", new((c) => c.Gender == Gender.Female));
            SearchFilters.Add("Male", new((c) => c.Gender == Gender.Male));
        }
    }
}