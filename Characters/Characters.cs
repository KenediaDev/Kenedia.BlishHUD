using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Controls.Extern;
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
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Resources;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using static Kenedia.Modules.Characters.Services.TextureManager;
using static Kenedia.Modules.Characters.Utility.WindowsUtil.WindowsUtil;
using File = System.IO.File;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using Version = SemVer.Version;

namespace Kenedia.Modules.Characters
{
    [Export(typeof(Module))]
    public class Characters : Module
    {
        public const uint WMCOMMAND = 0x0111;
        public const uint WMPASTE = 0x0302;

        public static readonly Logger Logger = Logger.GetLogger<Characters>();
        public readonly Version BaseVersion;
        public readonly ResourceManager RM = new("Characters.Res.strings", System.Reflection.Assembly.GetExecutingAssembly());

        public readonly Dictionary<string, SearchFilter<Character_Model>> TagFilters = new();
        private readonly Ticks _ticks = new();

        private CornerIcon _cornerIcon;
        private bool _dataLoaded;
        private RECT _windowRectangle;
        private Point _clientRes = Point.Zero;
        private RECT _clientRectangle;

        [ImportingConstructor]
        public Characters([Import("ModuleParameters")] ModuleParameters moduleParameters)
            : base(moduleParameters)
        {
            ModuleInstance = this;

            BaseVersion = Version.BaseVersion();
        }
        public event EventHandler LanguageChanged;

        public event EventHandler ResolutionChanged;

        public event EventHandler DataLoaded_Event;

        public Dictionary<string, SearchFilter<Character_Model>> SearchFilters { get; private set; }

        public static VirtualKeyShort[] ModKeyMapping { get; private set; }

        public TagList Tags { get; set; } = new TagList();

        public RunIndicator RunIndicator { get; private set; }

        public MainWindow MainWindow { get; private set; }

        public SettingsWindow SettingsWindow { get; private set; }

        public CharacterPotraitCapture PotraitCapture { get; private set; }

        public LoadingSpinner APISpinner { get; private set; }

        public SettingsModel Settings { get; private set; }

        public TextureManager TextureManager { get; private set; }

        public SettingEntry<Blish_HUD.Input.KeyBinding> ReloadKey { get; set; }

        public ObservableCollection<Character_Model> CharacterModels { get; set; } = new();

        public Character_Model CurrentCharacterModel { get; set; }

        public string BasePath { get; set; }

        public Data Data { get; private set; }

        public OCR OCR { get; private set; }

        public string GlobalAccountsPath { get; set; }

        public string CharactersPath { get; set; }

        public string AccountInfoPath { get; set; }

        public string AccountImagesPath { get; set; }

        public string GlobalImagesPath { get; set; }

        public string AccountPath { get; set; }

        public bool RunOCR { get; set; }

        public bool SaveCharacters { get; set; }

        public bool FetchingAPI { get; set; }

        public GW2API_Handler GW2APIHandler { get; private set; } = new GW2API_Handler();

        public bool DataLoaded
        {
            get => _dataLoaded;
            set
            {
                _dataLoaded = value;
                if (value)
                {
                    ModuleInstance.OnDataLoaded();
                }
            }
        }

        public RECT WindowRectangle { get => _windowRectangle; set => _windowRectangle = value; }

        public RECT ClientRectangle { get => _clientRectangle; set => _clientRectangle = value; }

        public int TitleBarHeight { get; private set; }

        public int SideBarWidth { get; private set; }

        internal static Characters ModuleInstance { get; private set; }

        internal SettingsManager SettingsManager => ModuleParameters.SettingsManager;

        internal ContentsManager ContentsManager => ModuleParameters.ContentsManager;

        internal DirectoriesManager DirectoriesManager => ModuleParameters.DirectoriesManager;

        internal Gw2ApiManager Gw2ApiManager => ModuleParameters.Gw2ApiManager;

        public void UpdateFolderPaths(string accountName, bool api_handled = true)
        {
            Characters mIns = ModuleInstance;
            string b = mIns.BasePath;

            mIns.AccountPath = b + @"\" + accountName;
            mIns.CharactersPath = b + @"\" + accountName + @"\characters.json";
            mIns.AccountInfoPath = b + @"\" + accountName + @"\account.json";
            mIns.AccountImagesPath = b + @"\" + accountName + @"\images\";

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

        public void OnLanguageChanged(object sender, EventArgs e)
        {
            CreateToggleCategories();
            LanguageChanged?.Invoke(this, EventArgs.Empty);
        }

        public void OnResolutionChanged(bool fireEvent = true)
        {
            IntPtr hWnd = GameService.GameIntegration.Gw2Instance.Gw2WindowHandle;
            _ = GetClientRect(hWnd, out _clientRectangle);

            TitleBarHeight = WindowRectangle.Bottom - WindowRectangle.Top - (ClientRectangle.Bottom - ClientRectangle.Top);
            SideBarWidth = WindowRectangle.Right - WindowRectangle.Left - (ClientRectangle.Right - ClientRectangle.Left);

            if (fireEvent)
            {
                ResolutionChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public void SwapTo(Character_Model character)
        {
            Blish_HUD.Gw2Mumble.PlayerCharacter player = GameService.Gw2Mumble.PlayerCharacter;

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
                                        Tags.Add(t);
                                    }
                                }

                                CharacterModels.Add(c);
                                c.Initialize();
                            }
                        });

                        DataLoaded = true;
                        return true;
                    }
                }

                DataLoaded = true;
                return false;
            }
            catch (Exception ex)
            {
                Logger.Warn(ex, "Failed to load the local characters from file '" + CharactersPath + "'.");
                File.Copy(CharactersPath, CharactersPath.Replace(".json", " [" + DateTimeOffset.Now.ToUnixTimeSeconds().ToString() + "].corruped.json"));
                DataLoaded = true;
                return false;
            }
        }

        public void SaveCharacterList()
        {
            string json = JsonConvert.SerializeObject(CharacterModels, Formatting.Indented);

            // write string to file
            File.WriteAllText(CharactersPath, json);
        }

        protected override void Update(GameTime gameTime)
        {
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

            if (_ticks.OCR >= 50)
            {
                RECT newRect = new();
                _ = GetWindowRect(GameService.GameIntegration.Gw2Instance.Gw2WindowHandle, ref newRect);

                if (newRect.Left != _windowRectangle.Left || newRect.Top != _windowRectangle.Top || newRect.Right != _windowRectangle.Right || newRect.Bottom != _windowRectangle.Bottom)
                {
                    _windowRectangle = newRect;
                    OnResolutionChanged(true);
                }
            }

            PlayerCharacter player = GameService.Gw2Mumble.PlayerCharacter;

            if (MainWindow == null && player != null && !string.IsNullOrEmpty(player.Name))
            {
                CreateUI();
            }

            GameState.Run(gameTime);

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

        protected override void DefineSettings(SettingCollection settings)
        {
            Settings = new SettingsModel(settings);
            Settings.ShowCornerIcon.SettingChanged += ShowCornerIcon_SettingChanged;

#if DEBUG
            ReloadKey = settings.DefineSetting(
                nameof(ReloadKey),
                new Blish_HUD.Input.KeyBinding(ModifierKeys.Alt, Keys.R));

            ReloadKey.Value.Enabled = true;
            ReloadKey.Value.Activated += ReloadKey_Activated;
#endif
        }

        protected override void Initialize()
        {
            Logger.Info($"Starting  {Name} v." + BaseVersion);

            ModKeyMapping = new VirtualKeyShort[5];
            ModKeyMapping[(int)ModifierKeys.Ctrl] = VirtualKeyShort.CONTROL;
            ModKeyMapping[(int)ModifierKeys.Alt] = VirtualKeyShort.MENU;
            ModKeyMapping[(int)ModifierKeys.Shift] = VirtualKeyShort.LSHIFT;

            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore,
            };

            // string path = DirectoriesManager.GetFullDirectoryPath("characters") + @"\" + API_Account.Name;
            BasePath = DirectoriesManager.GetFullDirectoryPath("characters");
            GlobalAccountsPath = BasePath + @"\accounts.json";

            if (!File.Exists(BasePath + @"\gw2.traineddata") || Settings.Version.Value != BaseVersion)
            {
                using Stream target = File.Create(BasePath + @"\gw2.traineddata");
                Stream source = ContentsManager.GetFileStream(@"data\gw2.traineddata");
                _ = source.Seek(0, SeekOrigin.Begin);
                source.CopyTo(target);
            }

            if (!File.Exists(BasePath + @"\tesseract.dll") || Settings.Version.Value != BaseVersion)
            {
                using Stream target = File.Create(BasePath + @"\tesseract.dll");
                Stream source = ContentsManager.GetFileStream(@"data\tesseract.dll");
                _ = source.Seek(0, SeekOrigin.Begin);
                source.CopyTo(target);
            }

            Data = new Data();
            CreateToggleCategories();

            Gw2ApiManager.SubtokenUpdated += Gw2ApiManager_SubtokenUpdated;

            Settings.ShortcutKey.Value.Enabled = true;
            Settings.ShortcutKey.Value.Activated += ShortcutWindowToggle;
            Tags.CollectionChanged += Tags_CollectionChanged;

            Settings.Version.Value = BaseVersion;
        }

        private void CancelEverything()
        {
            CharacterSwapping.Cancel();
            CharacterSorting.Cancel();
        }

        protected override async Task LoadAsync()
        {
            await Task.Delay(0);
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

            Blish_HUD.Gw2Mumble.PlayerCharacter player = GameService.Gw2Mumble.PlayerCharacter;
            player.SpecializationChanged += ForceUpdate;
            player.NameChanged += ForceUpdate;

            Blish_HUD.Gw2Mumble.CurrentMap map = GameService.Gw2Mumble.CurrentMap;
            map.MapChanged += ForceUpdate;
            _clientRes = GameService.Graphics.Resolution;

            GameService.GameIntegration.Gw2Instance.IsInGameChanged += ForceUpdate;
            GameService.Overlay.UserLocale.SettingChanged += UserLocale_SettingChanged;

            OnResolutionChanged(false);

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

            if (_cornerIcon != null)
            {
                _cornerIcon.Click -= ToggleModule;
            }

            CharacterModels.CollectionChanged -= OnCharacterCollectionChanged;
            GameService.Overlay.UserLocale.SettingChanged -= UserLocale_SettingChanged;

            Blish_HUD.Gw2Mumble.PlayerCharacter player = GameService.Gw2Mumble.PlayerCharacter;
            player.SpecializationChanged -= ForceUpdate;
            player.NameChanged -= ForceUpdate;

            Blish_HUD.Gw2Mumble.CurrentMap map = GameService.Gw2Mumble.CurrentMap;
            map.MapChanged -= ForceUpdate;

            GameService.GameIntegration.Gw2Instance.IsInGameChanged -= ForceUpdate;
            Tags.CollectionChanged -= Tags_CollectionChanged;

            ModuleInstance = null;
        }

        private void OnCharacterCollectionChanged(object sender, EventArgs e)
        {
            MainWindow?.CreateCharacterControls(CharacterModels);
        }

        private void OnDataLoaded()
        {
            DataLoaded_Event?.Invoke(this, EventArgs.Empty);
            if (MainWindow == null)
            {
                CreateUI();
            }
        }

        private void ShortcutWindowToggle(object sender, EventArgs e)
        {
            if (Control.ActiveControl is not TextBox)
            {
                MainWindow?.ToggleWindow();
            }
        }

        private void Gw2ApiManager_SubtokenUpdated(object sender, ValueEventArgs<IEnumerable<TokenPermission>> e)
        {
            GW2APIHandler.CheckAPI();
        }

        private void ToggleWindow_Activated(object sender, EventArgs e)
        {
            MainWindow?.ToggleWindow();
        }

        private void ReloadKey_Activated(object sender, EventArgs e)
        {
            RebuildUI();
            MainWindow?.ToggleWindow();
            SettingsWindow?.ToggleWindow();
        }

        private void Tags_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (DataLoaded)
            {
                UpdateTagsCollection();
            }
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

        private void CornerIcon_Click(object sender, Blish_HUD.Input.MouseEventArgs e)
        {
            MainWindow?.ToggleWindow();
            if (MainWindow == null)
            {
                ScreenNotification.ShowNotification("New API Request sent...");
                GW2APIHandler.CheckAPI();
            }
        }

        private void Characters_DataLoaded_Event(object sender, EventArgs e)
        {
        }

        private void ToggleModule(object sender, Blish_HUD.Input.MouseEventArgs e)
        {
            MainWindow?.ToggleWindow();
        }

        private void CreateCornerIcons()
        {
            _cornerIcon = new CornerIcon()
            {
                Icon = AsyncTexture2D.FromAssetId(156678),
                HoverIcon = AsyncTexture2D.FromAssetId(156679),
                BasicTooltipText = string.Format(strings.Toggle, $"{Name}"),
                Parent = GameService.Graphics.SpriteScreen,
                Visible = Settings.ShowCornerIcon.Value,
            };

            APISpinner = new LoadingSpinner()
            {
                Location = new Point(_cornerIcon.Left, _cornerIcon.Bottom + 3),
                Parent = GameService.Graphics.SpriteScreen,
                Size = new Point(_cornerIcon.Width, _cornerIcon.Height),
                BasicTooltipText = "Fetching API data ...",
                Visible = false,
            };

            _cornerIcon.Click += CornerIcon_Click;
            _cornerIcon.Moved += CornerIcon_Moved;
        }

        private void CornerIcon_Moved(object sender, MovedEventArgs e)
        {
            APISpinner.Location = new Point(_cornerIcon.Left, _cornerIcon.Bottom + 3);
        }

        private void ShowCornerIcon_SettingChanged(object sender, ValueChangedEventArgs<bool> e)
        {
            if (e.NewValue && _cornerIcon == null)
            {
                CreateCornerIcons();
            }
            else if (_cornerIcon != null && !e.NewValue)
            {
                _cornerIcon.Moved -= CornerIcon_Moved;
                _cornerIcon.Click -= CornerIcon_Click;
                _cornerIcon.Dispose();
                _cornerIcon = null;

                APISpinner.Dispose();
                APISpinner = null;
            }
        }

        private void UserLocale_SettingChanged(object sender, ValueChangedEventArgs<Gw2Sharp.WebApi.Locale> e)
        {
            _cornerIcon.BasicTooltipText = string.Format(strings.Toggle, $"{Name}");
            OnLanguageChanged(null, null);
        }

        private void RebuildUI()
        {

            bool shown = MainWindow != null && MainWindow.Visible;
            bool settingsShown = SettingsWindow != null && SettingsWindow.Visible;

            SettingsWindow?.Dispose();
            MainWindow?.Dispose();
            PotraitCapture?.Dispose();
            OCR?.Dispose();
            RunIndicator?.Dispose();

            CreateUI(true);

            if (shown) MainWindow?.Show();
            if (settingsShown) SettingsWindow?.Show();
        }

        private void CreateUI(bool force = false)
        {
            if (SettingsWindow == null || force)
            {
                SettingsWindow?.Dispose();
                var settingsBg = AsyncTexture2D.FromAssetId(155997);
                Microsoft.Xna.Framework.Graphics.Texture2D cutSettingsBg = settingsBg.Texture.GetRegion(0, 0, settingsBg.Width - 482, settingsBg.Height - 390);
                SettingsWindow = new(
                    settingsBg,
                    new Rectangle(30, 30, cutSettingsBg.Width + 10, cutSettingsBg.Height),
                    new Rectangle(30, 35, cutSettingsBg.Width - 5, cutSettingsBg.Height - 15))
                {
                    Parent = GameService.Graphics.SpriteScreen,
                    Title = "❤",
                    Subtitle = "❤",
                    SavesPosition = true,
                    Id = $"CharactersSettingsWindow",
                };
            }

            if (MainWindow == null || force)
            {
                MainWindow?.Dispose();
                Microsoft.Xna.Framework.Graphics.Texture2D bg = TextureManager.GetBackground(Backgrounds.MainWindow);
                Microsoft.Xna.Framework.Graphics.Texture2D cutBg = bg.GetRegion(25, 25, bg.Width - 100, bg.Height - 325);

                MainWindow = new(
                bg,
                new Rectangle(25, 25, cutBg.Width + 10, cutBg.Height),
                new Rectangle(35, 14, cutBg.Width - 10, cutBg.Height - 10))
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

            if (PotraitCapture == null || force)
            {
                PotraitCapture?.Dispose();
                PotraitCapture = new CharacterPotraitCapture() { Parent = GameService.Graphics.SpriteScreen, Visible = false, ZIndex = 999 };
            }

            if (OCR == null || force)
            {
                OCR?.Dispose();
                OCR = new();
            }

            if (RunIndicator == null || force)
            {
                RunIndicator?.Dispose();
                RunIndicator = new();
            }
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