using System;
using Flai;
using Flai.Advertisiments;
using Flai.Graphics;
using Flai.Misc;
using Flai.Mogade;
using LineRunner.Screens;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LineRunner
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class LineRunnerGame : FlaiGame
    {
        private const string MogadeGameKey = "5055ef9a563d8a1c11006e79";
        private const string MogadeSecret = "9qEsN7BWbU@<F8TufoN<:";

        private const string PubCenterAppId = "424abe1e-b9b6-44aa-a9f5-f6fb7bbf5453";
        private const string PubCenterAdUnitId = "10046142"; // Same ad-unit as in boxStrike
        private const string AdDuplexAppId = "22767";

        private readonly MogadeManager _mogadeManager;
        private readonly SettingsManager<LineRunnerSettings> _settingsManager;
        private readonly AdManager _adManager;

        public LineRunnerGame()
        {
            base.ClearColor = Color.Black;

            _settingsManager = new SettingsManager<LineRunnerSettings>(base.Services);
            _mogadeManager = new MogadeManager(base.Services, LineRunnerGame.MogadeGameKey, LineRunnerGame.MogadeSecret);
            _adManager = new AdManager(base.Services)
            {
                Enabled = true,
                Visible = false,
                AdPosition = LineRunnerGlobals.AdCenterPosition,
            };
            base.Components.Add(_adManager);

            if (LineRunnerGlobals.ShowDebug)
            {
                base.Components.Add(new DebugInformationComponent(base.Services));
            }

            base.TargetFrameRate = 60;
            base.IsFixedTimeStep = false;

            LineRunnerGlobals.IsFirstLaunch = _settingsManager.Settings.FirstLaunch;
        }

        protected override void InitializeInner()
        {
            _adManager.InitializeMicrosoftAds(LineRunnerGame.PubCenterAppId, LineRunnerGame.PubCenterAdUnitId);
         // _adManager.InitializeAdDuplexAds(LineRunnerGame.AdDuplexAppId);

            _soundEffectManager.Enabled = false;
        }

        protected override void LoadContentInner()
        {
            BasicHouseAd houseAd = new BasicHouseAd("2c125639-a3cd-4588-ba99-bc78cc2fc2f9");
            houseAd.LoadContent(new Sprite(base.Content.LoadTexture("BoxStrikePromo")));
            _adManager.InitializeHouseAds(houseAd);
        }

        protected override void UpdateInner(UpdateContext updateContext)
        {
            _screenManager.Update(updateContext);
        }

        protected override void DrawInner(GraphicsContext graphicsContext)
        {
            _screenManager.Draw(graphicsContext);
        }

        protected override void AddInitialScreens()
        {
            _screenManager.AddScreen(new MainMenuScreen());
        }

        protected override void InitializeGraphicsSettings()
        {
            _graphicsDeviceManager.IsFullScreen = true;
            _graphicsDeviceManager.PreferMultiSampling = false;
            _graphicsDeviceManager.PreferredBackBufferWidth = 800;
            _graphicsDeviceManager.PreferredBackBufferHeight = 480;
            _graphicsDeviceManager.PreferredBackBufferFormat = SurfaceFormat.Color;
            _graphicsDeviceManager.SupportedOrientations =
                DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight;
        }

        protected override void OnPreparingDeviceSettings(object sender, PreparingDeviceSettingsEventArgs e)
        {
            e.GraphicsDeviceInformation.PresentationParameters.PresentationInterval = PresentInterval.One;
        }

        protected override void OnDeactivated(object sender, EventArgs args)
        {
            _settingsManager.Save();
            _soundEffectManager.Enabled = false;
        }

        protected override void OnExiting(object sender, EventArgs args)
        {
            _settingsManager.Save();
            _soundEffectManager.Enabled = false;
        }
    }
}
