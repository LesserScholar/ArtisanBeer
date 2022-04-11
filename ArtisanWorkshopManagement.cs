using System;
using TaleWorlds.Core;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.View.Screen;

#if BANNERLORD_172
using SandBox.View;
using TaleWorlds.CampaignSystem.Settlements.Workshops;
using TaleWorlds.ScreenSystem;
#else
using TaleWorlds.CampaignSystem;
using TaleWorlds.Engine.Screens;
#endif

namespace ArtisanBeer
{
    public class ArtisanWorkshopManagementState : GameState
    {
        public Workshop workshop;
        public ArtisanWorkshopState artisanWorkshop;
        public ArtisanWorkshopManagementState(Workshop workshop, ArtisanWorkshopState artisanWorkshop)
        {
            this.workshop = workshop;
            this.artisanWorkshop = artisanWorkshop;
        }
        public ArtisanWorkshopManagementState() { throw new ArgumentException(); }
    }

    [GameStateScreen(typeof(ArtisanWorkshopManagementState))]
    public class WorkshopManagementScreen : ScreenBase, IGameStateListener
    {
        ArtisanWorkshopManagementState _artisanWorkshopManagementState;
        public WorkshopManagementScreen(ArtisanWorkshopManagementState artisanWorkshopState)
        {
            _artisanWorkshopManagementState = artisanWorkshopState;
            _artisanWorkshopManagementState.Listener = this;
        }

        GauntletLayer _layer;
        WorkshopManagementVM _dataSource;

        void IGameStateListener.OnActivate()
        {
            _layer = new GauntletLayer(1, "GauntletLayer", true);
            _dataSource = new WorkshopManagementVM(_artisanWorkshopManagementState);
            _layer.LoadMovie("WorkshopManagement", _dataSource);
            _layer.Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("PartyHotKeyCategory"));
            _layer.InputRestrictions.SetInputRestrictions(true, InputUsageMask.All);
            _layer.IsFocusLayer = true;
            ScreenManager.TrySetFocus(_layer);
            AddLayer(_layer);
        }

        void IGameStateListener.OnDeactivate()
        {
            _layer.InputRestrictions.ResetInputRestrictions();
            _layer.IsFocusLayer = false;
            RemoveLayer(_layer);
            ScreenManager.TryLoseFocus(_layer);
            _dataSource = null;
        }

        protected override void OnFrameTick(float dt)
        {
            base.OnFrameTick(dt);
            if (_layer.Input.IsKeyReleased(TaleWorlds.InputSystem.InputKey.Escape))
            {
                _dataSource.ExecuteCancel();
            }
            if (_layer.Input.IsKeyReleased(TaleWorlds.InputSystem.InputKey.Enter))
            {
                _dataSource.ExecuteDone();
            }
        }

        void IGameStateListener.OnFinalize() { }
        void IGameStateListener.OnInitialize() { }
    }

    public class WorkshopManagementVM : ViewModel
    {
        ArtisanWorkshopManagementState _artisanWorkshopManagementState;

        public WorkshopManagementVM(ArtisanWorkshopManagementState artisanWorkshopManagementState)
        {
            this._artisanWorkshopManagementState = artisanWorkshopManagementState;
            _artisanBeerProduction = _artisanWorkshopManagementState.artisanWorkshop.dailyProductionAmount;
            UpdateRegularBeerProductionLabel();
        }

        void UpdateRegularBeerProductionLabel()
        {
            int effiency = 120 - _artisanBeerProduction * 20;
            RegularBeerProduction = String.Format("{0}%", effiency);
        }

        int _artisanBeerProduction;
        [DataSourceProperty]
        public int ArtisanBeerProduction
        {
            get
            {
                return this._artisanBeerProduction;
            }
            set
            {
                if (value != this._artisanBeerProduction)
                {
                    this._artisanBeerProduction = value;
                    base.OnPropertyChangedWithValue(value, "ArtisanBeerProduction");
                    UpdateRegularBeerProductionLabel();
                }
            }
        }

        string _regularBeerProduction;
        [DataSourceProperty]
        public string RegularBeerProduction
        {
            get
            {
                return _regularBeerProduction;
            }
            set
            {
                if (value != this._regularBeerProduction)
                {
                    this._regularBeerProduction = value;
                    base.OnPropertyChangedWithValue(value, "RegularBeerProduction");
                }
            }
        }
        [DataSourceProperty]
        public string CancelLabel => GameTexts.FindText("str_cancel", null).ToString();
        [DataSourceProperty]
        public string DoneLabel => GameTexts.FindText("str_done", null).ToString();

        [DataSourceProperty]
        public string ArtisanBeerLabel => new TextObject("{=wYej8AeYdGwLx}Artisan Beer").ToString();
        [DataSourceProperty]
        public string RegularBeerLabel => new TextObject("{=elyqcMdr6iAEp}Regular Beer").ToString();
        [DataSourceProperty]
        public string TitleText => new TextObject("{=Ted1fuXNB0sps}Brewery Management").ToString();
        [DataSourceProperty]
        public string ProductionRatioLabel => new TextObject("{=sYp2rcJbLmxjU}Production Ratio").ToString();

        public void ExecuteCancel()
        {
            GameStateManager.Current.PopState();
        }

        public void ExecuteDone()
        {
            _artisanWorkshopManagementState.artisanWorkshop.dailyProductionAmount = ArtisanBeerProduction;
            GameStateManager.Current.PopState();
        }
    }
}