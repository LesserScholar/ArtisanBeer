using System;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.Missions;
using TaleWorlds.ObjectSystem;

namespace ArtisanBeer
{
    public class SubModule : MBSubModuleBase
    {
        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();

        }

        public override void OnMissionBehaviorInitialize(Mission mission)
        {
            base.OnMissionBehaviorInitialize(mission);
            mission.AddMissionBehavior(new ArtisanBeerMissionView());
        }
    }

    public class ArtisanBeerMissionView : MissionView
    {
        public override void OnMissionScreenTick(float dt)
        {
            base.OnMissionScreenTick(dt);

            if (Input.IsKeyPressed(TaleWorlds.InputSystem.InputKey.Q))
            {
                DrinkBeer();
            }
        }
        private void DrinkBeer()
        {
            if (!(Mission.Mode is MissionMode.Battle or MissionMode.Stealth)) return;
            // Check you actually have artisan beer in inventory
            var itemRoster = MobileParty.MainParty.ItemRoster;
            var artisanBeerObject = MBObjectManager.Instance.GetObject<ItemObject>("artisan_beer");
            if (itemRoster.GetItemNumber(artisanBeerObject) <= 0) return;
            // Remove one beer
            itemRoster.AddToCounts(artisanBeerObject, -1);
            // Increase main character hp
            var ma = Mission.MainAgent;
            var oldHealth = ma.Health;
            ma.Health += 20;
            if (ma.Health > ma.HealthLimit) ma.Health = ma.HealthLimit;
            InformationManager.DisplayMessage(new InformationMessage(String.Format("We healed {0} hp", Mission.MainAgent.Health - oldHealth)));
        }
    }
}