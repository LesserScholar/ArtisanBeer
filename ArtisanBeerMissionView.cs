using System;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.GauntletUI.Data;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.Missions;
using TaleWorlds.ObjectSystem;

namespace ArtisanBeer
{
    public class ArtisanBeerMissionView : MissionView
    {
        GauntletLayer _layer;
        IGauntletMovie _movie;
        ArtisanBeerMissionVM _dataSource;

        public override void OnMissionScreenInitialize()
        {
            base.OnMissionScreenInitialize();
            _dataSource = new ArtisanBeerMissionVM(Mission);
            _layer = new GauntletLayer(1);
            _movie = _layer.LoadMovie("ArtisanBeerHUD", _dataSource);
            MissionScreen.AddLayer(_layer);
        }
        public override void OnMissionScreenFinalize()
        {
            base.OnMissionScreenFinalize();
            MissionScreen.RemoveLayer(_layer);
            _movie = null;
            _layer = null;
            _dataSource = null;
        }

        public override void OnMissionScreenTick(float dt)
        {
            base.OnMissionScreenTick(dt);

            if (Input.IsKeyPressed(TaleWorlds.InputSystem.InputKey.Q))
            {
                _dataSource.DrinkBeer();
            }
        }

        public override void OnMissionModeChange(MissionMode oldMissionMode, bool atStart)
        {
            base.OnMissionModeChange(oldMissionMode, atStart);
            _dataSource?.OnMissionModeChanged(Mission);
        }
    }

    public class ArtisanBeerMissionVM : ViewModel {

        Mission _mission;
        int _soundIndex;

        public ArtisanBeerMissionVM(Mission mission)
        {
            _mission = mission;
            var itemRoster = MobileParty.MainParty.ItemRoster;
            var artisanBeerObject = MBObjectManager.Instance.GetObject<ItemObject>("artisan_beer");
            BeerAmount = itemRoster.GetItemNumber(artisanBeerObject);
            _soundIndex = SoundEvent.GetEventIdFromString("artisanbeer/drink");

            OnMissionModeChanged(mission);
        }

        public void OnMissionModeChanged(Mission mission)
        {
            IsVisible = mission.Mode is MissionMode.Battle or MissionMode.Stealth;
        }

        int _beerAmount;
        [DataSourceProperty]
        public int BeerAmount
        {
            get
            {
                return this._beerAmount;
            }
            set
            {
                if (value != this._beerAmount)
                {
                    this._beerAmount = value;
                    base.OnPropertyChangedWithValue(value, "BeerAmount");
                }
            }
        }
        bool _isVisible;
        [DataSourceProperty]
        public bool IsVisible
        {
            get
            {
                return this._isVisible;
            }
            set
            {
                if (value != this._isVisible)
                {
                    this._isVisible = value;
                    base.OnPropertyChangedWithValue(value, "IsVisible");
                }
            }
        }
        public void DrinkBeer()
        {
            if (!IsVisible) return;
            // Check you actually have artisan beer in inventory
            var itemRoster = MobileParty.MainParty.ItemRoster;
            var artisanBeerObject = MBObjectManager.Instance.GetObject<ItemObject>("artisan_beer");
            if (itemRoster.GetItemNumber(artisanBeerObject) <= 0) return;
            // Remove one beer
            itemRoster.AddToCounts(artisanBeerObject, -1);
            // Increase main character hp
            var ma = _mission.MainAgent;
            var oldHealth = ma.Health;
            ma.Health += 20;
            if (ma.Health > ma.HealthLimit) ma.Health = ma.HealthLimit;
            InformationManager.DisplayMessage(new InformationMessage(String.Format("We healed {0} hp", _mission.MainAgent.Health - oldHealth)));

            BeerAmount -= 1;

            SoundEvent.PlaySound2D(_soundIndex);
        }
    }
}