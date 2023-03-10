using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Gw2Mumble;
using Blish_HUD.Modules;
using Blish_HUD.Modules.Managers;
using Blish_HUD.Settings;
using Blish_HUD.Controls;
using Gw2Sharp.WebApi.V2.Models;
using Kenedia.Modules.Characters.Controls;
using Kenedia.Modules.Characters.Enums;
using Kenedia.Modules.Characters.Models;
using Kenedia.Modules.Characters.Services;
using Kenedia.Modules.Characters.Views;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Res;
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
using CornerIcon = Kenedia.Modules.Core.Controls.CornerIcon;
using LoadingSpinner = Kenedia.Modules.Core.Controls.LoadingSpinner;
using Microsoft.Xna.Framework.Graphics;
using Kenedia.Modules.Core.Extensions;
using Gw2Sharp.Models;
using Gw2Sharp.WebApi.V2;
using Kenedia.Modules.Characters.Controls.SideMenu;
using Kenedia.Modules.Characters.Res;
using Gw2Sharp.WebApi;
using System.Collections.Specialized;
using System.Diagnostics;

namespace Kenedia.Modules.Characters
{
    [Export(typeof(Module))]
    public class Characters : BaseModule<Characters, MainWindow, Settings>
    {
        public readonly ResourceManager RM = new("Kenedia.Modules.Characters.Res.strings", System.Reflection.Assembly.GetExecutingAssembly());

        private readonly Ticks _ticks = new();

        private CornerIcon _cornerIcon;
        private bool _saveCharacters;
        private bool _loadedCharacters;
        private bool _mapsUpdated;

        [ImportingConstructor]
        public Characters([Import("ModuleParameters")] ModuleParameters moduleParameters)
            : base(moduleParameters)
        {
            ModuleInstance = this;
            HasGUI = true;
        }

        public SearchFilterCollection SearchFilters { get; } = new();

        public SearchFilterCollection TagFilters { get; } = new();

        public TagList Tags { get; } = new();

        public CharacterSwapping CharacterSwapping { get; private set; }

        public CharacterSorting CharacterSorting { get; private set; }

        public RunIndicator RunIndicator { get; private set; }

        public RadialMenu RadialMenu { get; private set; }

        public PotraitCapture PotraitCapture { get; private set; }

        public LoadingSpinner APISpinner { get; private set; }

        public TextureManager TextureManager { get; private set; }

        public ObservableCollection<Character_Model> CharacterModels { get; } = new();

        private Character_Model _currentCharacterModel;

        public Character_Model CurrentCharacterModel
        {
            get => _currentCharacterModel;
            private set
            {
                if (_currentCharacterModel != value)
                {
                    if (_currentCharacterModel != null)
                    {
                        _currentCharacterModel.Updated -= CurrentCharacterModel_Updated;
                        _currentCharacterModel.IsCurrentCharacter = false;
                    }

                    _currentCharacterModel = value;

                    if (_currentCharacterModel != null)
                    {
                        _currentCharacterModel.Updated += CurrentCharacterModel_Updated;
                        _currentCharacterModel.UpdateCharacter();
                        _currentCharacterModel.IsCurrentCharacter = true;

                        MainWindow?.SortCharacters();
                    }
                }
            }
        }

        public Data Data { get; private set; }

        public OCR OCR { get; private set; }

        public string GlobalAccountsPath { get; set; }

        public string CharactersPath => $@"{Paths.AccountPath}characters.json";

        public string AccountImagesPath => $@"{Paths.AccountPath}images\";

        public GW2API_Handler GW2APIHandler { get; private set; }

        public override IView GetSettingsView()
        {
            return new SettingsView(() => SettingsWindow?.ToggleWindow());
        }

        protected override async void OnLocaleChanged(object sender, ValueChangedEventArgs<Locale> eventArgs)
        {
            await GW2APIHandler.FetchLocale(eventArgs?.NewValue, !_mapsUpdated);
            _mapsUpdated = true;
            base.OnLocaleChanged(sender, eventArgs);
        }

        protected override void Initialize()
        {
            base.Initialize();

            Logger.Info($"Starting {Name} v." + ModuleVersion);

            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore,
            };

            Data = new Data(ContentsManager, Paths);

            GlobalAccountsPath = Paths.ModulePath + @"\accounts.json";

            if (!File.Exists(Paths.ModulePath + @"\gw2.traineddata") || Settings.Version.Value != ModuleVersion)
            {
                using Stream target = File.Create(Paths.ModulePath + @"\gw2.traineddata");
                Stream source = ContentsManager.GetFileStream(@"data\gw2.traineddata");
                _ = source.Seek(0, SeekOrigin.Begin);
                source.CopyTo(target);
            }

            if (!File.Exists(Paths.ModulePath + @"\tesseract.dll") || Settings.Version.Value != ModuleVersion)
            {
                using Stream target = File.Create(Paths.ModulePath + @"\tesseract.dll");
                Stream source = ContentsManager.GetFileStream(@"data\tesseract.dll");
                _ = source.Seek(0, SeekOrigin.Begin);
                source.CopyTo(target);
            }

            CreateToggleCategories();

            Gw2ApiManager.SubtokenUpdated += Gw2ApiManager_SubtokenUpdated;

            Settings.ShortcutKey.Value.Enabled = true;
            Settings.ShortcutKey.Value.Activated += ShortcutWindowToggle;

            Settings.RadialKey.Value.Enabled = true;
            Settings.RadialKey.Value.Activated += RadialMenuToggle;

            Tags.CollectionChanged += Tags_CollectionChanged;

            Settings.Version.Value = ModuleVersion;

            GW2APIHandler = new GW2API_Handler(Gw2ApiManager, AddOrUpdateCharacters, () => APISpinner, Paths, Data);

            
        }

        protected override void DefineSettings(SettingCollection settings)
        {
            base.DefineSettings(settings);

            Settings = new Settings(settings);
            Settings.ShowCornerIcon.SettingChanged += ShowCornerIcon_SettingChanged;
        }

        protected override async Task LoadAsync()
        {
            await base.LoadAsync();
            await Data.Load();
        }

        protected override void OnModuleLoaded(EventArgs e)
        {
            base.OnModuleLoaded(e);
            CharacterSwapping = new(Settings, Services.GameState, CharacterModels);
            CharacterSorting = new(Settings, Services.GameState, CharacterModels);

            CharacterSwapping.CharacterSorting = CharacterSorting;
            CharacterSorting.CharacterSwapping = CharacterSwapping;

            TextureManager = new TextureManager(Services.TexturesService);

            if (Settings.ShowCornerIcon.Value)
            {
                CreateCornerIcons();
            }

            CharacterModels.CollectionChanged += OnCharacterCollectionChanged;

            if (Settings.LoadCachedAccounts.Value) _ = LoadCharacters();

            Services.InputDetectionService.ClickedOrKey += InputDetectionService_ClickedOrKey;
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            _ticks.Global += gameTime.ElapsedGameTime.TotalMilliseconds;
            _ticks.APIUpdate += gameTime.ElapsedGameTime.TotalSeconds;
            _ticks.Save += gameTime.ElapsedGameTime.TotalMilliseconds;
            _ticks.Tags += gameTime.ElapsedGameTime.TotalMilliseconds;
            _ticks.OCR += gameTime.ElapsedGameTime.TotalMilliseconds;

            if (_ticks.Global > 500)
            {
                _ticks.Global = 0;
                PlayerCharacter player = GameService.Gw2Mumble.PlayerCharacter;

                string name = player != null ? player.Name : string.Empty;
                bool charSelection = Settings.UseBetaGamestate.Value ? Services.GameState.IsCharacterSelection : !GameService.GameIntegration.Gw2Instance.IsInGame;

                CurrentCharacterModel = !charSelection ? CharacterModels.FirstOrDefault(e => e.Name == name) : null;

                if (!_mapsUpdated && GameService.Gw2Mumble.CurrentMap.Id > 0 && Data.GetMapById(GameService.Gw2Mumble.CurrentMap.Id).Id == 0)
                {
                    OnLocaleChanged(this, new(Locale.Chinese, GameService.Overlay.UserLocale.Value));
                    _mapsUpdated = true;
                }

                if (CurrentCharacterModel != null)
                {
                    CurrentCharacterModel?.UpdateCharacter(player);
                }
            }

            if (_ticks.APIUpdate > 300)
            {
                _ticks.APIUpdate = 0;

                _ = GW2APIHandler.CheckAPI();
            }

            if (_ticks.Save > 25 && _saveCharacters)
            {
                _ticks.Save = 0;

                SaveCharacterList();
                _saveCharacters = false;
            }
        }

        protected override void Unload()
        {
            TextureManager = null;
            DeleteCornerIcons();

            CharacterModels.CollectionChanged -= OnCharacterCollectionChanged;
            Tags.CollectionChanged -= Tags_CollectionChanged;
            Services.ClientWindowService.ResolutionChanged -= ClientWindowService_ResolutionChanged;

            base.Unload();
        }

        protected override void LoadGUI()
        {
            base.LoadGUI();
            RadialMenu = new RadialMenu(Settings, CharacterModels, GameService.Graphics.SpriteScreen, () => CurrentCharacterModel, Data, TextureManager)
            {
                Visible = false,
                ZIndex = int.MaxValue / 2
            };

            PotraitCapture = new PotraitCapture(Services.ClientWindowService, Services.SharedSettings, TextureManager)
            {
                Parent = GameService.Graphics.SpriteScreen,
                Visible = false,
                ZIndex = (int.MaxValue / 2) - 1,
                AccountImagePath = () => AccountImagesPath,
            };

            OCR = new(Services.ClientWindowService, Services.SharedSettings, Settings, Paths.ModulePath, CharacterModels);
            RunIndicator = new(CharacterSorting, CharacterSwapping, Settings.ShowStatusWindow, TextureManager, Settings.ShowChoyaSpinner);

            var settingsBg = AsyncTexture2D.FromAssetId(155997);
            Texture2D cutSettingsBg = settingsBg.Texture.GetRegion(0, 0, settingsBg.Width - 482, settingsBg.Height - 390);
            SettingsWindow = new SettingsWindow(
                settingsBg,
                new Rectangle(30, 30, cutSettingsBg.Width + 10, cutSettingsBg.Height),
                new Rectangle(30, 35, cutSettingsBg.Width - 5, cutSettingsBg.Height - 15),
                SharedSettingsView,
                OCR,
                Settings)
            {
                Parent = GameService.Graphics.SpriteScreen,
                Title = "❤",
                Subtitle = "❤",
                SavesPosition = true,
                Id = $"{Name} SettingsWindow",
                Version = ModuleVersion,
            };

            Texture2D bg = TextureManager.GetBackground(Backgrounds.MainWindow);
            Texture2D cutBg = bg.GetRegion(25, 25, bg.Width - 100, bg.Height - 325);

            MainWindow = new(
            bg,
            new Rectangle(25, 25, cutBg.Width + 10, cutBg.Height),
            new Rectangle(35, 14, cutBg.Width - 10, cutBg.Height - 10),
            Settings,
            TextureManager,
            CharacterModels,
            SearchFilters,
            TagFilters,
            OCR.ToggleContainer,
            () => PotraitCapture.ToggleVisibility(),
            async () => await GW2APIHandler.CheckAPI(),
            () => AccountImagesPath,
            Tags,
            () => CurrentCharacterModel,
            Data,
            CharacterSorting)
            {
                Parent = GameService.Graphics.SpriteScreen,
                Title = "❤",
                Subtitle = "❤",
                SavesPosition = true,
                Id = $"{Name} MainWindow",
                CanResize = true,
                Size = Settings.WindowSize.Value,
                SettingsWindow = (SettingsWindow)SettingsWindow,
                Version = ModuleVersion,
            };

            SideMenuToggles _toggles;
            MainWindow.SideMenu.AddTab(_toggles = new SideMenuToggles(TextureManager, TagFilters, SearchFilters, () => MainWindow?.FilterCharacters(), Tags, Data)
            {
                Width = MainWindow.SideMenu.Width,
                Icon = AsyncTexture2D.FromAssetId(440021),
            });

            MainWindow.SideMenu.AddTab(new SideMenuBehaviors(RM, TextureManager, Settings, () => MainWindow?.SortCharacters())
            {
                Icon = AsyncTexture2D.FromAssetId(156909),
            });
            MainWindow.SideMenu.TogglesTab = _toggles;
            _ = MainWindow.SideMenu.SwitchTab(_toggles);
            MainWindow?.CreateCharacterControls();

            PotraitCapture.OnImageCaptured = () =>
            {
                MainWindow.CharacterEdit.LoadImages(null, null);
                MainWindow.CharacterEdit.ShowImages(true);
            };

            CharacterSwapping.HideMainWindow = MainWindow.Hide;
            CharacterSwapping.OCR = OCR;
            CharacterSorting.OCR = OCR;
            CharacterSorting.UpdateCharacterList = MainWindow.PerformFiltering;

            Services.ClientWindowService.ResolutionChanged += ClientWindowService_ResolutionChanged;

            GW2APIHandler.MainWindow = MainWindow;
        }

        private void ClientWindowService_ResolutionChanged(object sender, ValueChangedEventArgs<Point> e)
        {
            MainWindow?.CheckOCRRegion();
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

        private void Gw2ApiManager_SubtokenUpdated(object sender, ValueEventArgs<IEnumerable<TokenPermission>> e)
        {
            _ = GW2APIHandler.CheckAPI();
        }

        private void ShortcutWindowToggle(object sender, EventArgs e)
        {
            if (Control.ActiveControl is not TextBox)
            {
                MainWindow?.ToggleWindow();
            }
        }

        private void RadialMenuToggle(object sender, EventArgs e)
        {
            if (Settings.EnableRadialMenu.Value && RadialMenu?.HasDisplayedCharacters() == true) _ = (RadialMenu?.ToggleVisibility());
        }

        private void CurrentCharacterModel_Updated(object sender, EventArgs e)
        {
            MainWindow?.SortCharacters();
        }

        private void InputDetectionService_ClickedOrKey(object sender, double e)
        {
            if (GameService.GameIntegration.Gw2Instance.Gw2HasFocus && (!Settings.CancelOnlyOnESC.Value || GameService.Input.Keyboard.KeysDown.Contains(Keys.Escape)))
            {
                CancelEverything();
            }
        }

        private void CancelEverything()
        {
            MouseState mouse = GameService.Input.Mouse.State;

            if (CharacterSwapping.Cancel()) Logger.Info($"Cancel any automated action. Left Mouse Down: {mouse.LeftButton == ButtonState.Pressed} | Right Mouse Down: {mouse.RightButton == ButtonState.Pressed} | Keyboard Keys pressed {string.Join("|", GameService.Input.Keyboard.KeysDown.Select(k => k.ToString()).ToArray())}");
            if (CharacterSorting.Cancel()) Logger.Info($"Cancel any automated action. Left Mouse Down: {mouse.LeftButton == ButtonState.Pressed} | Right Mouse Down: {mouse.RightButton == ButtonState.Pressed} | Keyboard Keys pressed {string.Join("|", GameService.Input.Keyboard.KeysDown.Select(k => k.ToString()).ToArray())}");
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
            MainWindow?.CreateCharacterControls();
        }

        private void Tags_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
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

        private void CreateToggleCategories()
        {
            var filters = new List<SearchFilter<Character_Model>>();

            foreach (KeyValuePair<ProfessionType, Data.Profession> e in Data.Professions)
            {
                SearchFilters.Add(e.Value.Name, new((c) => Settings.DisplayToggles.Value["Profession"].Check && c.Profession == e.Key));
                SearchFilters.Add($"Core {e.Value.Name}", new((c) => Settings.DisplayToggles.Value["Profession"].Check && c.Profession == e.Key));
            }

            foreach (KeyValuePair<SpecializationType, Data.Specialization> e in Data.Specializations)
            {
                SearchFilters.Add(e.Value.Name, new((c) => Settings.DisplayToggles.Value["Profession"].Check && c.Specialization == e.Key));
            }

            foreach (KeyValuePair<int, Data.CraftingProfession> e in Data.CrafingProfessions)
            {
                SearchFilters.Add(e.Value.Name, new((c) => c.Crafting.Find(p => Settings.DisplayToggles.Value["CraftingProfession"].Check && p.Id == e.Value.Id && (!Settings.DisplayToggles.Value["OnlyMaxCrafting"].Check || p.Rating >= e.Value.MaxRating)) != null));
            }

            foreach (KeyValuePair<RaceType, Data.Race> e in Data.Races)
            {
                SearchFilters.Add(e.Value.Name, new((c) => Settings.DisplayToggles.Value["Race"].Check && c.Race == e.Key));
            }

            SearchFilters.Add("Birthday", new((c) => c.HasBirthdayPresent));
            SearchFilters.Add("Hidden", new((c) => !c.Show));
            SearchFilters.Add("Female", new((c) => c.Gender == Gender.Female));
            SearchFilters.Add("Male", new((c) => c.Gender == Gender.Male));
        }

        private void AddOrUpdateCharacters(IApiV2ObjectList<Character> characters)
        {
            if (!_loadedCharacters)
            {
                Logger.Info($"This is our first API data fetched for this character/session. Trying to load local data first.");
                _ = LoadCharacters();
            }

            Logger.Info($"Update characters based on fresh data from the api.");

            var freshList = characters.Select(c => new { c.Name, c.Created }).ToList();
            var oldList = CharacterModels.Select(c => new { c.Name, c.Created }).ToList();

            bool updateMarkedCharacters = false;
            for (int i = CharacterModels.Count - 1; i >= 0; i--)
            {
                Character_Model c = CharacterModels[i];
                if (!freshList.Contains(new { c.Name, c.Created }))
                {

                    if (Settings.AutomaticCharacterDelete.Value)
                    {
                        Logger.Info($"{c.Name} created on {c.Created} no longer exists. Delete them!");
                        c.Delete();
                        //CharacterModels.RemoveAt(i);
                    }
                    else if (!c.MarkedAsDeleted)
                    {
                        Logger.Info($"{c.Name} created on {c.Created} does not exist in the api data. Mark them as potentially deleted!");
                        c.MarkedAsDeleted = true;
                        updateMarkedCharacters = true;
                    }
                }
            }

            if (updateMarkedCharacters) MainWindow.UpdateMissingNotification();

            int pos = 0;
            foreach (var c in characters)
            {
                if (!oldList.Contains(new { c.Name, c.Created }))
                {
                    Logger.Info($"{c.Name} created on {c.Created} does not exist yet. Create them!");
                    CharacterModels.Add(new(c, CharacterSwapping, Paths.ModulePath, RequestCharacterSave, CharacterModels, Data) { Position = pos });
                }
                else
                {
                    //Logger.Info($"{c.Name} created on {c.Created} does exist already. Update it!");
                    var character = CharacterModels.FirstOrDefault(e => e.Name == c.Name);
                    character?.UpdateCharacter(c);
                    if (character != null)
                    {
                        character.Position = pos;
                    }
                }

                pos++;
            }

            if (!File.Exists(CharactersPath) || Settings.ImportVersion.Value < OldCharacterModel.ImportVersion)
            {
                string p = CharactersPath.Replace(@"kenedia\", "");

                if (File.Exists(p))
                {
                    Logger.Info($"This is the first start of {Name} since import version {OldCharacterModel.ImportVersion}. Importing old data from {p}!");
                    OldCharacterModel.Import(p, CharacterModels, AccountImagesPath, Paths.AccountName, Tags);
                }

                Settings.ImportVersion.Value = OldCharacterModel.ImportVersion;
            }

            SaveCharacterList();
            MainWindow?.CreateCharacterControls();
            MainWindow?.PerformFiltering();
        }

        private bool LoadCharacters()
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
                Paths.AccountName = account.AccountName;

                _loadedCharacters = true;
                Settings.LoadAccountSettings(Paths.AccountName);

                if (!Directory.Exists(AccountImagesPath))
                {
                    _ = Directory.CreateDirectory(AccountImagesPath);
                }
            }

            if (account != null)
            {
                Logger.Debug($"Found '{player.Name}' in a stored character list for '{account.AccountName}'. Loading characters of '{account.AccountName}'");
                return LoadCharacterFile();
            }

            return false;
        }

        private bool LoadCharacterFile()
        {
            try
            {
                if (File.Exists(CharactersPath))
                {
                    FileInfo infos = new(CharactersPath);
                    string content = File.ReadAllText(CharactersPath);
                    PlayerCharacter player = GameService.Gw2Mumble.PlayerCharacter;
                    List<Character_Model> characters = JsonConvert.DeserializeObject<List<Character_Model>>(content);
                    var names = CharacterModels.Select(c => c.Name).ToList();

                    if (characters != null)
                    {
                        characters.ForEach(c =>
                        {
                            if (!names.Contains(c.Name))
                            {
                                Tags.AddTags(c.Tags);
                                CharacterModels.Add(new(c, CharacterSwapping, Paths.ModulePath, RequestCharacterSave, CharacterModels, Data));
                                names.Add(c.Name);
                            }
                        });

                        Logger.Info("Loaded local characters from file '" + CharactersPath + "'.");
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                Logger.Warn(ex, "Failed to load the local characters from file '" + CharactersPath + "'.");
                File.Copy(CharactersPath, CharactersPath.Replace(".json", " [" + DateTimeOffset.Now.ToUnixTimeSeconds().ToString() + "].corrupted.json"));
                return false;
            }
        }

        private void SaveCharacterList()
        {
            string json = JsonConvert.SerializeObject(CharacterModels, Formatting.Indented);

            // write string to file
            File.WriteAllText(CharactersPath, json);
        }

        private void RequestCharacterSave()
        {
            _saveCharacters = true;
        }
    }
}