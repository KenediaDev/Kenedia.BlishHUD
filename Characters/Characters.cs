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
using System.Threading;
using NotificationBadge = Kenedia.Modules.Core.Controls.NotificationBadge;
using AnchoredContainer = Kenedia.Modules.Core.Controls.AnchoredContainer;

// TODO if character name is in multiple accounts -> don't load
namespace Kenedia.Modules.Characters
{
    [Export(typeof(Module))]
    public class Characters : BaseModule<Characters, MainWindow, Settings, PathCollection>
    {
        public readonly ResourceManager RM = new("Kenedia.Modules.Characters.Res.strings", System.Reflection.Assembly.GetExecutingAssembly());

        private readonly Ticks _ticks = new();

        private AnchoredContainer _cornerContainer;
        private NotificationBadge _notificationBadge;
        private CornerIcon _cornerIcon;
        private bool _saveCharacters;
        private bool _loadedCharacters;
        private bool _mapsUpdated;

        private SemVer.Version _version;

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

        public LoadingSpinner ApiSpinner { get; private set; }

        public TextureManager TextureManager { get; private set; }

        public ObservableCollection<Character_Model> CharacterModels { get; } = new();

        private Character_Model _currentCharacterModel;
        private CancellationTokenSource _characterFileTokenSource;

        public Character_Model CurrentCharacterModel
        {
            get => _currentCharacterModel;
            private set
            {
                if (_currentCharacterModel != value)
                {
                    if (_currentCharacterModel is not null)
                    {
                        _currentCharacterModel.Updated -= CurrentCharacterModel_Updated;
                        _currentCharacterModel.IsCurrentCharacter = false;
                    }

                    _currentCharacterModel = value;

                    if (_currentCharacterModel is not null)
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

        protected override async void OnLocaleChanged(object sender, Blish_HUD.ValueChangedEventArgs<Locale> eventArgs)
        {
            await GW2APIHandler.FetchLocale(eventArgs?.NewValue, !_mapsUpdated);
            _mapsUpdated = true;
            base.OnLocaleChanged(sender, eventArgs);
        }

        protected override void Initialize()
        {
            base.Initialize();

            Paths = new PathCollection(DirectoriesManager, Name);

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

            Settings.ShortcutKey.Value.Enabled = true;
            Settings.ShortcutKey.Value.Activated += ShortcutWindowToggle;

            Settings.RadialKey.Value.Enabled = true;
            Settings.RadialKey.Value.Activated += RadialMenuToggle;

            Tags.CollectionChanged += Tags_CollectionChanged;

            _version = Settings.Version.Value;

            Settings.Version.Value = ModuleVersion;
        }

        protected override void DefineSettings(SettingCollection settings)
        {
            base.DefineSettings(settings);

            Settings = new Settings(settings);
            Settings.ShowCornerIcon.SettingChanged += ShowCornerIcon_SettingChanged;
            Settings.UseBetaGamestate.SettingChanged += UseBetaGamestate_SettingChanged;

            Services.GameStateDetectionService.Enabled = Settings.UseBetaGamestate.Value;
        }

        private void UseBetaGamestate_SettingChanged(object sender, Blish_HUD.ValueChangedEventArgs<bool> e)
        {
            Services.GameStateDetectionService.Enabled = e.NewValue;
        }

        protected override async Task LoadAsync()
        {
            await base.LoadAsync();

            CharacterSwapping = new(Settings, Services.GameStateDetectionService, CharacterModels);
            CharacterSorting = new(Settings, Services.GameStateDetectionService, CharacterModels);

            CharacterSwapping.CharacterSorting = CharacterSorting;
            CharacterSorting.CharacterSwapping = CharacterSwapping;

            TextureManager = new TextureManager(Services.TexturesService);
            await Data.Load();

            if (Settings.LoadCachedAccounts.Value) _ = await LoadCharacters();

            Data.StaticInfo.BetaStateChanged += StaticInfo_BetaStateChanged;
        }

        private void StaticInfo_BetaStateChanged(object sender, bool e)
        {
            MainWindow?.AdjustForBeta();
        }

        protected override void OnModuleLoaded(EventArgs e)
        {
            base.OnModuleLoaded(e);

            GW2APIHandler = new GW2API_Handler(Gw2ApiManager, AddOrUpdateCharacters, () => ApiSpinner, Paths, Data, () => _notificationBadge);
            GW2APIHandler.AccountChanged += GW2APIHandler_AccountChanged;
            Gw2ApiManager.SubtokenUpdated += Gw2ApiManager_SubtokenUpdated;

            if (Settings.ShowCornerIcon.Value)
            {
                CreateCornerIcons();
            }

            CharacterModels.CollectionChanged += OnCharacterCollectionChanged;
            Services.InputDetectionService.ClickedOrKey += InputDetectionService_ClickedOrKey;
        }

        private void GW2APIHandler_AccountChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(Paths.AccountName) && Paths.AccountName != GW2APIHandler.Account?.Name)
            {
                Paths.AccountName = GW2APIHandler.Account?.Name;
                Logger.Info("Account changed. Wipe all account bound data of this session.");

                CharacterModels.Clear();
                MainWindow?.CharacterCards.Clear();
                MainWindow?.LoadedModels.Clear();
            }
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

                string name = player is not null ? player.Name : string.Empty;
                bool charSelection = Settings.UseBetaGamestate.Value ? Services.GameStateDetectionService.IsCharacterSelection : !GameService.GameIntegration.Gw2Instance.IsInGame;

                CurrentCharacterModel = !charSelection ? CharacterModels.FirstOrDefault(e => e.Name == name) : null;

                if (!_mapsUpdated && GameService.Gw2Mumble.CurrentMap.Id > 0 && Data.GetMapById(GameService.Gw2Mumble.CurrentMap.Id).Id == 0)
                {
                    OnLocaleChanged(this, new(Locale.Chinese, GameService.Overlay.UserLocale.Value));
                    _mapsUpdated = true;
                }

                if (CurrentCharacterModel is not null)
                {
                    CurrentCharacterModel?.UpdateCharacter(player);
                }

                Data?.StaticInfo?.CheckBeta();
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
                Name = Name,
                MainWindowEmblem = AsyncTexture2D.FromAssetId(156015),
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

            OCR.MainWindow = MainWindow;
            CharacterSwapping.HideMainWindow = MainWindow.Hide;
            CharacterSwapping.OCR = OCR;
            CharacterSorting.OCR = OCR;
            CharacterSorting.UpdateCharacterList = MainWindow.PerformFiltering;

            Services.ClientWindowService.ResolutionChanged += ClientWindowService_ResolutionChanged;

            GW2APIHandler.MainWindow = MainWindow;
        }

        private void ClientWindowService_ResolutionChanged(object sender, Blish_HUD.ValueChangedEventArgs<Point> e)
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
                var keys = new List<Keys>()
                {
                    Settings.LogoutKey.Value.PrimaryKey,
                    Keys.Enter,
                };

                if (GameService.Input.Keyboard.KeysDown.Except(keys).Count() > 0)
                {
                    CancelEverything();
                }
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
            Logger.Debug($"ReloadKey_Activated: {Name}");

            base.ReloadKey_Activated(sender, e);
            CreateCornerIcons();
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

            _cornerContainer = new()
            {
                Parent = GameService.Graphics.SpriteScreen,
                WidthSizingMode = SizingMode.AutoSize,
                HeightSizingMode = SizingMode.AutoSize,
                Anchor = _cornerIcon,
                AnchorPosition = AnchoredContainer.AnchorPos.Bottom,
                RelativePosition = new(0, -_cornerIcon.Height / 2),
                CaptureInput = CaptureType.Filter,
            };

            _notificationBadge = new NotificationBadge()
            {
                Location = new(_cornerIcon.Width - 15, 0),
                Parent = _cornerContainer,
                Size = new(20),
                Opacity = 0.6f,
                HoveredOpacity = 1f,
                CaptureInput = CaptureType.Filter,
                Anchor = _cornerIcon,
                Visible = false,
            };

            ApiSpinner = new LoadingSpinner()
            {
                Location = new Point(0, _notificationBadge.Bottom),
                Parent = _cornerContainer,
                Size = _cornerIcon.Size,
                BasicTooltipText = strings_common.FetchingApiData,
                Visible = false,
                CaptureInput = null,
            };
        }

        private void DeleteCornerIcons()
        {
            _cornerIcon?.Dispose();
            _cornerIcon = null;

            _cornerContainer?.Dispose();
            _cornerContainer = null;
        }

        private void ShowCornerIcon_SettingChanged(object sender, Blish_HUD.ValueChangedEventArgs<bool> e)
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
                SearchFilters.Add(e.Value.Name, new((c) => c.Crafting.Find(p => Settings.DisplayToggles.Value["CraftingProfession"].Check && p.Id == e.Value.Id && (!Settings.DisplayToggles.Value["OnlyMaxCrafting"].Check || p.Rating >= e.Value.MaxRating)) is not null));
            }

            foreach (KeyValuePair<RaceType, Data.Race> e in Data.Races)
            {
                SearchFilters.Add(e.Value.Name, new((c) => Settings.DisplayToggles.Value["Race"].Check && c.Race == e.Key));
            }

            SearchFilters.Add("Birthday", new((c) => c.HasBirthdayPresent));
            SearchFilters.Add("Hidden", new((c) => !c.Show || (!Data.StaticInfo.IsBeta && c.Beta)));
            SearchFilters.Add("Female", new((c) => c.Gender == Gender.Female));
            SearchFilters.Add("Male", new((c) => c.Gender == Gender.Male));
        }

        private async void AddOrUpdateCharacters(IApiV2ObjectList<Character> characters)
        {
            if (!_loadedCharacters && Settings.LoadCachedAccounts.Value)
            {
                Logger.Info($"This is our first API data fetched for this character/session. Trying to load local data first.");
                if (await LoadCharacters() == null)
                {
                    Logger.Info($"Checking the cache.");
                }
            }

            Logger.Info($"Update characters for '{Paths.AccountName}' based on fresh data from the api.");
            if (Paths.AccountName is not null && characters.Count > 0)
            {
                var freshList = characters.Select(c => new { c.Name, c.Created }).ToList();
                var oldList = CharacterModels.Select(c => new { c.Name, c.Created }).ToList();

                bool updateMarkedCharacters = false;
                for (int i = CharacterModels.Count - 1; i >= 0; i--)
                {
                    Character_Model c = CharacterModels[i];
                    if (c is not null)
                    {
                        if (!freshList.Contains(new { c.Name, c.Created }))
                        {
                            if (Settings.AutomaticCharacterDelete.Value)
                            {
                                Logger?.Info($"{c?.Name} created on {c?.Created} no longer exists. Delete them!");
                                c?.Delete();
                                //CharacterModels.RemoveAt(i);
                            }
                            else if (!c.MarkedAsDeleted)
                            {
                                Logger?.Info($"{c.Name} created on {c.Created} does not exist in the api data. Mark them as potentially deleted!");
                                c.MarkedAsDeleted = true;
                                updateMarkedCharacters = true;
                            }
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
                        if (character is not null)
                        {
                            character.Position = pos;
                        }
                    }

                    pos++;
                }

                //if (!File.Exists(CharactersPath) || Settings.ImportVersion.Value < OldCharacterModel.ImportVersion)
                //{
                //    string p = CharactersPath.Replace(@"kenedia\", "");

                //    if (File.Exists(p))
                //    {
                //        Logger.Info($"This is the first start of {Name} since import version {OldCharacterModel.ImportVersion}. Importing old data from {p}!");
                //        OldCharacterModel.Import(p, CharacterModels, AccountImagesPath, Paths.AccountName, Tags);
                //    }

                //    Settings.ImportVersion.Value = OldCharacterModel.ImportVersion;
                //}

                MainWindow?.CreateCharacterControls();
                MainWindow?.PerformFiltering();

                _ = SaveCharacterList();
            }
        }

        private async Task<bool?> LoadCharacters()
        {
            PlayerCharacter player = GameService.Gw2Mumble.PlayerCharacter;

            if ((player == null || string.IsNullOrEmpty(player.Name)) && string.IsNullOrEmpty(Paths.AccountName))
            {
                Logger.Info($"Player name is currently null or empty. Can not check for the account.");
                return null;
            }

            AccountSummary getAccount()
            {
                try
                {
                    string path = GlobalAccountsPath;
                    if (File.Exists(path))
                    {
                        string content = File.ReadAllText(path);
                        List<AccountSummary> accounts = JsonConvert.DeserializeObject<List<AccountSummary>>(content, SerializerSettings.Default);
                        return accounts.Find(e => e.CharacterNames.Contains(player.Name));
                    }
                }
                catch (Exception)
                { }

                return null;
            }

            AccountSummary account = getAccount();

            if (account is not null || !string.IsNullOrEmpty(Paths.AccountName))
            {
                Paths.AccountName ??= account.AccountName;

                _loadedCharacters = true;
                Settings.LoadAccountSettings(Paths.AccountName);

                if (!Directory.Exists(AccountImagesPath))
                {
                    _ = Directory.CreateDirectory(AccountImagesPath);
                }

                Logger.Info($"Found '{player.Name ?? "Unkown Player name."}' in a stored character list for '{Paths.AccountName}'. Loading characters of '{Paths.AccountName}'");
                return await LoadCharacterFile();
            }

            return false;
        }

        private async Task<bool> LoadCharacterFile()
        {
            try
            {
                _characterFileTokenSource?.Cancel();
                _characterFileTokenSource = new CancellationTokenSource();

                if (File.Exists(CharactersPath) && await FileExtension.WaitForFileUnlock(CharactersPath, 2500, _characterFileTokenSource.Token))
                {
                    FileInfo infos = new(CharactersPath);
                    string content = File.ReadAllText(CharactersPath);
                    PlayerCharacter player = GameService.Gw2Mumble.PlayerCharacter;
                    List<Character_Model> characters = JsonConvert.DeserializeObject<List<Character_Model>>(content, SerializerSettings.Default);
                    var names = CharacterModels.Select(c => c.Name).ToList();

                    if (characters is not null)
                    {
                        characters.ForEach(c =>
                        {
                            if (!names.Contains(c.Name))
                            {
                                Tags.AddTags(c.Tags);
                                CharacterModels.Add(new(c, CharacterSwapping, Paths.ModulePath, RequestCharacterSave, CharacterModels, Data) { Beta = _version >= new SemVer.Version(1, 0, 20) && c.Beta });
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

        private async Task SaveCharacterList()
        {
            try
            {
                _characterFileTokenSource?.Cancel();
                _characterFileTokenSource = new CancellationTokenSource();

                if (await FileExtension.WaitForFileUnlock(CharactersPath, 2500, _characterFileTokenSource.Token))
                {
                    string json = JsonConvert.SerializeObject(CharacterModels, SerializerSettings.Default);

                    // write string to file
                    File.WriteAllText(CharactersPath, json);
                }
                else
                {
                    if (!_characterFileTokenSource.IsCancellationRequested) Logger.Info("Failed to save the characters file '" + CharactersPath + "'.");
                }
            }
            catch (Exception ex)
            {
                Logger.Warn("Failed to save the characters file '" + CharactersPath + "'.");
                Logger.Warn($"{ex}");
            }
        }

        private void RequestCharacterSave()
        {
            _saveCharacters = true;
        }
    }
}