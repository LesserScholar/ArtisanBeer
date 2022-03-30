using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.AgentOrigins;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Locations;
using TaleWorlds.CampaignSystem.Settlements.Workshops;
using TaleWorlds.Core;

namespace ArtisanBeer
{
    public class ArtisanBeerBehavior : CampaignBehaviorBase
    {
        public override void RegisterEvents()
        {
            CampaignEvents.OnWorkshopChangedEvent.AddNonSerializedListener(this, OnWorkshopChangedEvent);
            CampaignEvents.DailyTickTownEvent.AddNonSerializedListener(this, DailyTick);
            CampaignEvents.LocationCharactersAreReadyToSpawnEvent.AddNonSerializedListener(this, LocationCharactersAreReadyToSpawn);
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
                        CharacterObject caravanMaster = Settlement.CurrentSettlement.Culture.CaravanMaster;

                        LocationCharacter locationCharacter = new LocationCharacter(new AgentData(
                            new SimpleAgentOrigin(caravanMaster)).Monster(Campaign.Current.HumanMonsterSettlement),
                            new LocationCharacter.AddBehaviorsDelegate(SandBoxManager.Instance.AgentBehaviorManager.AddWandererBehaviors),
                            workshop.Tag, true, LocationCharacter.CharacterRelations.Neutral, null, true, false, null, false, false, true);
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