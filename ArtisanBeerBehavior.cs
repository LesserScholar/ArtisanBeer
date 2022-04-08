using SandBox;
using SandBox.Conversation;
using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.AgentOrigins;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Overlay;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Locations;
using TaleWorlds.CampaignSystem.Settlements.Workshops;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;
using TaleWorlds.SaveSystem;

namespace ArtisanBeer
{
    public class ArtisanBeerBehavior : CampaignBehaviorBase
    {
        public override void RegisterEvents()
        {
            CampaignEvents.OnWorkshopChangedEvent.AddNonSerializedListener(this, OnWorkshopChangedEvent);
            CampaignEvents.DailyTickTownEvent.AddNonSerializedListener(this, DailyTick);
            CampaignEvents.LocationCharactersAreReadyToSpawnEvent.AddNonSerializedListener(this, LocationCharactersAreReadyToSpawn);
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, OnSessionLaunched);
        }
        ItemObject _artisanBeer;
        CharacterObject _artisanBrewer;
        private void OnSessionLaunched(CampaignGameStarter starter)
        {
            _artisanBeer = MBObjectManager.Instance.GetObject<ItemObject>("artisan_beer");
            _artisanBrewer = MBObjectManager.Instance.GetObject<CharacterObject>("artisan_brewer");
            AddDialogs(starter);
            AddGameMenus(starter);
        }
        Workshop _selectedWorkshop;
        private void AddGameMenus(CampaignGameStarter starter)
        {
            for (int i = 0; i < 5; i++)
            {
                AddWorkshopButton(starter, i);
            }
            starter.AddGameMenu("town_workshop", "Brewery", (MenuCallbackArgs args) => { },
                GameOverlays.MenuOverlayType.SettlementWithBoth);
            starter.AddGameMenuOption("town_workshop", "town_workshop_inventory", "Inventory", (MenuCallbackArgs args) =>
            {
                args.optionLeaveType = GameMenuOption.LeaveType.Trade;
                return true;
            }, (MenuCallbackArgs args) => {
                List<InquiryElement> list = new List<InquiryElement>();
                var artisanWorkshop = ArtisanWorkshop(_selectedWorkshop);
                for (int i = 0; i < artisanWorkshop.inventoryStock; i++)
                {
                    list.Add(new InquiryElement(_artisanBeer, _artisanBeer.Name.ToString(), new ImageIdentifier(_artisanBeer)));
                }
                MultiSelectionInquiryData data = new MultiSelectionInquiryData("Inventory", "Take items from the workshop inventory.",
                    list, true, 1000, "Take", "{=3sRdGQou}Leave", (List<InquiryElement> list) =>
                    {
                        var artisanWorkshop = ArtisanWorkshop(_selectedWorkshop);
                        artisanWorkshop.AddToStock(-list.Count);
                        MobileParty.MainParty.ItemRoster.AddToCounts(_artisanBeer, list.Count);
                    }, (List<InquiryElement> list) => { });
                InformationManager.ShowMultiSelectionInquiry(data);
            });
            starter.AddGameMenuOption("town_workshop", "town_workshop_management", "Manage Workshop", (MenuCallbackArgs args) =>
            {
                args.optionLeaveType = GameMenuOption.LeaveType.Submenu;
                return true;
            }, (MenuCallbackArgs args) =>
            {
                GameStateManager.Current.PushState(GameStateManager.Current.CreateState<ArtisanWorkshopManagementState>(_selectedWorkshop, ArtisanWorkshop(_selectedWorkshop)));
            });
            starter.AddGameMenuOption("town_workshop", "town_workshop_id", "{=3sRdGQou}Leave", (MenuCallbackArgs args) =>
            {
                args.optionLeaveType = GameMenuOption.LeaveType.Leave;
                return true;
            }, (MenuCallbackArgs args) => GameMenu.SwitchToMenu("town"), true);
        }

        private void AddWorkshopButton(CampaignGameStarter starter, int i)
        {
            starter.AddGameMenuOption("town", "town_workshop" + i, "Go to Brewery",
                        (MenuCallbackArgs args) =>
                        {
                            args.optionLeaveType = GameMenuOption.LeaveType.Submenu;
                            if (i >= Settlement.CurrentSettlement.Town.Workshops.Length) return false;
                            Workshop workshop = Settlement.CurrentSettlement.Town.Workshops[i];
                            return workshop.WorkshopType.StringId == "brewery" &&
                                workshop.Owner == Hero.MainHero;
                        },
                        delegate (MenuCallbackArgs x)
                        {
                            GameMenu.SwitchToMenu("town_workshop");
                            _selectedWorkshop = Settlement.CurrentSettlement.Town.Workshops[i];
                        }, false, 9, false);
        }

        public Workshop FindCurrentWorkshop(Agent agent)
        {
            if (Settlement.CurrentSettlement != null && Settlement.CurrentSettlement.IsTown)
            {
                CampaignAgentComponent component = agent.GetComponent<CampaignAgentComponent>();
                AgentNavigator agentNavigator = (component != null) ? component.AgentNavigator : null;
                if (agentNavigator != null)
                {
                    foreach (Workshop workshop in Settlement.CurrentSettlement.GetComponent<Town>().Workshops)
                    {
                        if (workshop.Tag == agentNavigator.SpecialTargetTag)
                        {
                            return workshop;
                        }
                    }
                }
            }
            return null;
        }
        public ArtisanWorkshopState ConversationArtisanWorkshop()
        {
            var workshop = FindCurrentWorkshop(ConversationMission.OneToOneConversationAgent);
            if (workshop != null) return ArtisanWorkshop(workshop);
            return null;
        }
        public int ConversationArtisanWorkshopStock()
        {
            var artisanWorkshop = ConversationArtisanWorkshop();
            if (artisanWorkshop != null) return artisanWorkshop.inventoryStock;
            return 0;
        }

        private void AddDialogs(CampaignGameStarter starter)
        {
            {
                starter.AddPlayerLine("tavernkeeper_talk_ask_artisan_beer", "tavernkeeper_talk", "tavernkeeper_artisan_beer", "Do you sell artisan beer?", null, null);
                starter.AddDialogLine("tavernkeeper_talk_artisan_beer_a", "tavernkeeper_artisan_beer", "tavernkeeper_talk", "Bah. Greedy bastard at the brewery doesn't want to sell his stuff to me. Something about getting better rates selling directly to customers.", () =>
                {
                    foreach (var workshop in Settlement.CurrentSettlement.Town.Workshops)
                    {
                        if (workshop.WorkshopType.StringId == "brewery") return true;
                    }
                    return false;
                }, null);
                starter.AddDialogLine("tavernkeeper_talk_artisan_beer_b", "tavernkeeper_artisan_beer", "tavernkeeper_talk", "We don't have a brewery in town. You'll have to look somewhere else.", null, null);
            }
            {
                starter.AddDialogLine("artisan_brewer_talk_outofstock", "start", "end", "Howdy. Are you here to buy artisan beer? Unfortunately we are out of stock. Come back later.",
                    () => CharacterObject.OneToOneConversationCharacter == _artisanBrewer && ConversationArtisanWorkshopStock() <= 0, null);
                starter.AddDialogLine("artisan_brewer_talk_instock", "start", "artisan_brewer", "Howdy. Would you like to purchase some Artisan Beer? One mug is 200 denars.",
                    () => CharacterObject.OneToOneConversationCharacter == _artisanBrewer, null);
                starter.AddPlayerLine("artisan_brewer_buy", "artisan_brewer", "artisan_brewer_purchased", "Sure, I'll take one.", null, () =>
                {
                    Hero.MainHero.ChangeHeroGold(-200);
                    MobileParty.MainParty.ItemRoster.AddToCounts(_artisanBeer, 1);
                    var artisanWorkshop = ConversationArtisanWorkshop();
                    artisanWorkshop.AddToStock(-1);
                }, 100, (out TextObject explanation) =>
                {
                    if (Hero.MainHero.Gold < 200)
                    {
                        explanation = new TextObject("Not enough money.");
                        return false;
                    }
                    else
                    {
                        explanation = TextObject.Empty;
                        return true;
                    }
                });
                starter.AddDialogLine("artisan_brewer_thanks_for_business", "artisan_brewer_purchased", "end", "Thank you come again!", null, null);

                starter.AddPlayerLine("artisan_brewer_buy_refuse", "artisan_brewer", "artisan_brewer_declined", "Nah I'm good, thanks.", null, null);
                starter.AddDialogLine("artisan_brewer_your_loss", "artisan_brewer_declined", "end", "Your loss.", null, null);
            }
        }

        private void LocationCharactersAreReadyToSpawn(Dictionary<string, int> unusedUsablePointCount)
        {
            Location locationWithId = Settlement.CurrentSettlement.LocationComplex.GetLocationWithId("center");
            if (!(CampaignMission.Current.Location == locationWithId && CampaignTime.Now.IsDayTime)) return;

            Settlement settlement = PlayerEncounter.LocationEncounter.Settlement;
            foreach (Workshop workshop in settlement.Town.Workshops)
            {
                if (workshop.IsRunning && workshop.WorkshopType.StringId == "brewery")
                {
                    int num;
                    unusedUsablePointCount.TryGetValue(workshop.Tag, out num);
                    if (num > 0f)
                    {
                        string actionSetCode = "as_human_villager_drinker_with_mug";
                        string value = "artisan_beer_drinking_animation";
                        var agentData = new AgentData(
                            new SimpleAgentOrigin(_artisanBrewer)).Monster(Campaign.Current.HumanMonsterSettlement);
                        LocationCharacter locationCharacter = new LocationCharacter(agentData,
                            new LocationCharacter.AddBehaviorsDelegate(SandBoxManager.Instance.AgentBehaviorManager.AddWandererBehaviors),
                            workshop.Tag, true, LocationCharacter.CharacterRelations.Friendly, actionSetCode, true, false, null, false, false, true)
                        {
                            PrefabNamesForBones = {
                                {
                                    agentData.AgentMonster.MainHandItemBoneIndex,
                                    value
                                }
                            }
                        };
                        locationWithId.AddCharacter(locationCharacter);
                    }
                }
            }

        }

        private void DailyTick(Town town)
        {
            foreach (var workshop in town.Workshops)
            {
                if (workshop.WorkshopType.StringId == "brewery")
                {
                    workshop.ChangeGold(-TaleWorlds.Library.MathF.Round(workshop.Expense * 0.15f));

                    var artisanWorkshop = ArtisanWorkshop(workshop);
                    artisanWorkshop.AddToStock(artisanWorkshop.dailyProductionAmount);
                }
            }
        }

        private void OnWorkshopChangedEvent(Workshop workshop, Hero oldOwningHero, WorkshopType type)
        {

        }

        public ArtisanWorkshopState ArtisanWorkshop(Workshop workshop)
        {
            string id = workshop.Settlement.StringId + "_" + workshop.Tag;
            if (artisanWorkshops.TryGetValue(id, out var state))
            {
                return state;
            }
            else
            {
                state = new ArtisanWorkshopState() { dailyProductionAmount = 1, inventoryCapacity = 10, inventoryStock = 0 };
                artisanWorkshops.Add(id, state);
                return state;
            }
        }

        public Dictionary<string, ArtisanWorkshopState> artisanWorkshops = new Dictionary<string, ArtisanWorkshopState>();
        public override void SyncData(IDataStore dataStore)
        {
            dataStore.SyncData("artisanWorkshops", ref artisanWorkshops);
        }
    }

    public class ArtisanWorkshopState
    {
        [SaveableField(1)]
        public int inventoryStock;
        [SaveableField(2)]
        public int inventoryCapacity;

        [SaveableField(3)]
        public int dailyProductionAmount;

        public void AddToStock(int amount)
        {
            inventoryStock += amount;
            if (inventoryStock > inventoryCapacity) inventoryStock = inventoryCapacity;
            if (inventoryStock < 0) InformationManager.DisplayMessage(new InformationMessage("Artisan workshop negative inventory stock."));
        }
    }

    public class ArtisanWorkshopSaveableTypeDefiner : SaveableTypeDefiner
    {
        public ArtisanWorkshopSaveableTypeDefiner() : base(536_882_256) { }

        protected override void DefineClassTypes()
        {
            AddClassDefinition(typeof(ArtisanWorkshopState), 1);
        }
        protected override void DefineContainerDefinitions()
        {
            ConstructContainerDefinition(typeof(Dictionary<string, ArtisanWorkshopState>));
        }
    }
}