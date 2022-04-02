using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.AgentOrigins;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Locations;
using TaleWorlds.CampaignSystem.Settlements.Workshops;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;

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
                starter.AddDialogLine("artisan_brewer_talk", "start", "artisan_brewer", "Howdy. Would you like to purchase some Artisan Beer? One mug is 200 denars.",
                    () => CharacterObject.OneToOneConversationCharacter == _artisanBrewer, null);
                starter.AddPlayerLine("artisan_brewer_buy", "artisan_brewer", "artisan_brewer_purchased", "Sure, I'll take one.", null, () =>
                {
                    Hero.MainHero.ChangeHeroGold(-200);
                    MobileParty.MainParty.ItemRoster.AddToCounts(_artisanBeer, 1);
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
                }
            }
        }

        private void OnWorkshopChangedEvent(Workshop workshop, Hero oldOwningHero, WorkshopType type)
        {

        }

        public override void SyncData(IDataStore dataStore)
        {
        }
    }
}