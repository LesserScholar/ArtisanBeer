using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Workshops;

namespace ArtisanBeer
{
    public class ArtisanBeerBehavior : CampaignBehaviorBase
    {
        public override void RegisterEvents()
        {
            CampaignEvents.OnWorkshopChangedEvent.AddNonSerializedListener(this, OnWorkshopChangedEvent);
            CampaignEvents.DailyTickTownEvent.AddNonSerializedListener(this, DailyTick);
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