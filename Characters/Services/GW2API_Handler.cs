using Blish_HUD.Controls;
using Blish_HUD.Modules.Managers;
using Characters.Res;
using Gw2Sharp.Models;
using Gw2Sharp.WebApi.V2.Models;
using Kenedia.Modules.Characters.Enums;
using Kenedia.Modules.Characters.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using File = System.IO.File;

namespace Kenedia.Modules.Characters.Services
{
    public class GW2API_Handler
    {
        private Account _account;

        public Account Account
        {
            get => _account;
            set
            {
                if (value != null && (_account == null || _account.Name != value.Name))
                {
                    Characters.ModuleInstance.UpdateFolderPaths(value.Name);
                }

                _account = value;
            }
        }

        private void UpdateAccountsList(Account account, Gw2Sharp.WebApi.V2.IApiV2ObjectList<Character> characters)
        {
            Gw2ApiManager gw2ApiManager = Characters.ModuleInstance.Gw2ApiManager;
            try
            {
                string path = Characters.ModuleInstance.GlobalAccountsPath;
                List<AccountSummary> accounts = new();
                AccountSummary accountEntry;

                if (File.Exists(path))
                {
                    string content = File.ReadAllText(path);
                    accounts = JsonConvert.DeserializeObject<List<AccountSummary>>(content);
                    accountEntry = accounts.Find(e => e.AccountName == account.Name);

                    if (accountEntry != null)
                    {
                        accountEntry.AccountName = account.Name;
                        accountEntry.CharacterNames = new();
                        characters.ToList().ForEach(c => accountEntry.CharacterNames.Add(c.Name));
                    }
                    else
                    {
                        accounts.Add(accountEntry = new()
                        {
                            AccountName = account.Name,
                            CharacterNames = new(),
                        });
                        characters.ToList().ForEach(c => accountEntry.CharacterNames.Add(c.Name));
                    }
                }
                else
                {
                    accounts.Add(accountEntry = new()
                    {
                        AccountName = account.Name,
                        CharacterNames = new(),
                    });
                    characters.ToList().ForEach(c => accountEntry.CharacterNames.Add(c.Name));
                }

                string json = JsonConvert.SerializeObject(accounts, Formatting.Indented);
                File.WriteAllText(path, json);
            }
            catch { }
        }

        public async void CheckAPI()
        {
            Characters.ModuleInstance.APISpinner?.Show();

            try
            {
                Gw2ApiManager gw2ApiManager = Characters.ModuleInstance.Gw2ApiManager;

                if (gw2ApiManager.HasPermissions(new[] { TokenPermission.Account, TokenPermission.Characters }))
                {
                    Account account = await gw2ApiManager.Gw2ApiClient.V2.Account.GetAsync();
                    Account = account;

                    Gw2Sharp.WebApi.V2.IApiV2ObjectList<Character> characters = await gw2ApiManager.Gw2ApiClient.V2.Characters.AllAsync();
                    UpdateAccountsList(account, characters);

                    Characters.Logger.Info($"Fetching new API Data. Adjusting characters informatin to fresh data from the api.");

                    var character_Models = Characters.ModuleInstance.CharacterModels.ToList();
                    int pos = 0;

                    // Cleanup
                    for (int i = character_Models.Count - 1; i >= 0; i--)
                    {
                        Character_Model c = character_Models[i];
                        Character character = characters.ToList().Find(e => e.Name == c.Name);
                        if (character == null || character.Created != c.Created)
                        {
                            character_Models[i].Delete();
                        }
                    }

                    foreach (Character c in characters)
                    {
                        Character_Model character_Model = character_Models.Find(e => e.Name == c.Name);
                        bool found = character_Model != null;

                        character_Model ??= new();

                        // Update everything values
                        character_Model.Name = c.Name;
                        character_Model.Level = c.Level;
                        character_Model.Race = (RaceType)Enum.Parse(typeof(RaceType), c.Race);
                        character_Model.Profession = (ProfessionType)Enum.Parse(typeof(ProfessionType), c.Profession);
                        character_Model.Specialization = SpecializationType.None;
                        character_Model.Gender = c.Gender;
                        character_Model.Created = c.Created;
                        character_Model.LastModified = c.LastModified.UtcDateTime;
                        character_Model.LastLogin = c.LastModified.UtcDateTime > character_Model.LastLogin ? c.LastModified.UtcDateTime : character_Model.LastLogin;
                        character_Model.Position = pos;
                        character_Model.Level = c.Level;

                        foreach (CharacterCraftingDiscipline disc in c.Crafting.ToList())
                        {
                            CharacterCrafting craft = character_Model.Crafting.Find(e => e.Id == (int)disc.Discipline.Value);
                            bool craftFound = craft != null;

                            craft ??= new();
                            craft.Id = (int)disc.Discipline.Value;
                            craft.Rating = disc.Rating;
                            craft.Active = disc.Active;

                            if(!craftFound)
                            {
                                character_Model.Crafting.Add(craft);
                            }
                        }

                        if (!found)
                        {
                            character_Model.Initialize();
                            Characters.ModuleInstance.CharacterModels.Add(character_Model);
                        }

                        pos++;
                    }
                }
                else
                {
                    ScreenNotification.ShowNotification(strings.Error_InvalidPermissions, ScreenNotification.NotificationType.Error);
                    Characters.Logger.Error(strings.Error_InvalidPermissions);
                    Characters.ModuleInstance.APISpinner?.Hide();
                }
            }
            catch (Exception ex)
            {
                Characters.Logger.Warn(ex, strings.Error_FailedAPIFetch);
                Characters.ModuleInstance.APISpinner?.Hide();
            }

            Characters.ModuleInstance.APISpinner?.Hide();
        }
    }
}
