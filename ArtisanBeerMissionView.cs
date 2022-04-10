using System;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.GauntletUI.Data;
using TaleWorlds.Library;
using TaleWorlds.Localization;
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
            IsVisible = (mission.Mode is MissionMode.Battle or MissionMode.Stealth) && _beerAmount > 0;
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
            if (_beerAmount <= 0) return;

            var itemRoster = MobileParty.MainParty.ItemRoster;
            var artisanBeerObject = MBObjectManager.Instance.GetObject<ItemObject>("artisan_beer");
            itemRoster.AddToCounts(artisanBeerObject, -1);
            BeerAmount -= 1;

            var ma = _mission.MainAgent;
            var oldHealth = ma.Health;
            ma.Health += 15;
            if (ma.Health > ma.HealthLimit) ma.Health = ma.HealthLimit;
            var msg = new TextObject("{=yCLS6x8c04f1C}We healed {HEAL_AMOUNT} HP").SetTextVariable("HEAL_AMOUNT", _mission.MainAgent.Health - oldHealth);
            InformationManager.DisplayMessage(new InformationMessage(msg.ToString()));

            _mission.MakeSound(_soundIndex, Vec3.Zero, false, true, -1, -1);

        }
    }
}