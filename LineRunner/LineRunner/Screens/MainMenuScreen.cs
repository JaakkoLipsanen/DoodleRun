using Flai;
using Flai.Advertisiments;
using Flai.Content;
using Flai.Graphics;
using Flai.Misc;
using Flai.ScreenManagement;
using Flai.ScreenManagement.Screens;
using Flai.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using System;

namespace LineRunner.Screens
{
    public class MainMenuScreen : GameScreen
    {
        #region Logic

        private BasicUiContainer _uiContainer = new BasicUiContainer();

        private readonly Rectangle _helpInputArea = new Rectangle(0, 420, 100, 60);
        private readonly Rectangle _rateInputArea = new Rectangle(680, 430, 120, 50);

        #endregion

        #region Visual

        private Texture2D _backgroundTexture;

        #endregion

        public MainMenuScreen()
        {
            base.TransitionOffTime = LineRunnerGlobals.ScreenFadeTime;
            base.TransitionOnTime = LineRunnerGlobals.ScreenFadeTime;

            base.EnabledGestures = GestureType.Tap;
        }

        protected override void LoadContent(bool instancePreserved)
        {
            if (instancePreserved)
            {
                return;
            }

            TextButton playButton = new TextButton("Play", new Vector2(400f, 160)) { Font = "Crayon64", Color = new Color(226, 105, 105) };
            playButton.Click += () =>
                {
                    if (LineRunnerGlobals.IsFirstLaunch && !LineRunnerGlobals.HasShownHelpScreen)
                    {
                        LoadingScreen.Load(this.ScreenManager, false, new GameplayScreen(), new HelpScreen());
                    }
                    else
                    {
                        LoadingScreen.Load(this.ScreenManager, false, new GameplayScreen());
                    }
                };
            _uiContainer.Add(playButton);

            TextButton leaderboardButton = new TextButton("Leaderboard", new Vector2(400f, 280)) { Font = "Crayon64", Color = new Color(75, 148, 80) };
            leaderboardButton.Click += () => LoadingScreen.Load(this.ScreenManager, false, new LeaderboardScreen());
            _uiContainer.Add(leaderboardButton);

            TextButton changeUsernameButton = new TextButton("Options", new Vector2(400f, 400)) { Font = "Crayon64", Color = Color.SteelBlue }; // Color.RoyalBlue };
            changeUsernameButton.Click += () =>
                {
                    LoadingScreen.Load(base.ScreenManager, false, new OptionsScreen()); // this.ShowChangeUsernameDialog();
                };
            _uiContainer.Add(changeUsernameButton);

            TextButton helpButton = new TextButton("Help", new Vector2(50, 450)) { Font = "Crayon32", Color = Color.Black };
            helpButton.Click += () =>
                {
                    base.ScreenManager.AddScreen(new HelpScreen());
                };
            _uiContainer.Add(helpButton);

            TextButton rateButton = new TextButton("Rate", new Vector2(740, 455)) { Font = "Crayon32", Color = Color.Black };
            rateButton.Click += ApplicationInfo.OpenApplicationReviewPage;
            _uiContainer.Add(rateButton);

            TextButton buyPaidVersionButton = new TextButton("Buy Doodle Run+", new Vector2(140, 30)) { Color = Color.Black, Font = "Crayon32" };
            buyPaidVersionButton.Click += () => ApplicationInfo.OpenMarketplaceApplicationPage("8687dd13-25fd-4e1e-8f95-31ec3fda6cd0");

            _uiContainer.Add(buyPaidVersionButton);

            FlaiContentManager contentManager = base.ContentProvider.DefaultManager;
            _backgroundTexture = contentManager.LoadTexture("Gameplay/Background");

            if (!this.CheckIfFirstLaunch())
            {
                LineRunnerSettings settings = base.Services.GetService<ISettingsManager<LineRunnerSettings>>().Settings;
                settings.UpdateUsername(base.Services);
            }

             // Save settings if needed
            ISettingsManager<LineRunnerSettings> settingsManager = base.Services.GetService<ISettingsManager<LineRunnerSettings>>();
            settingsManager.Save();

            // Set ads hidden
            IAdManager adManager = base.Services.GetService<IAdManager>();
            adManager.Visible = false;
        }

        protected override void Update(UpdateContext updateContext, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            if (this.IsActive)
            {
                if (base.ScreenRunningTime.TotalSeconds > 0.4f)
                {
                    _uiContainer.Update(updateContext);
                }

                if (updateContext.InputState.IsBackButtonPressed)
                {
                    base.ScreenManager.Game.Exit();
                    return;
                }
            }
        }

        protected override void Draw(GraphicsContext graphicsContext)
        {
            graphicsContext.SpriteBatch.Begin();

            graphicsContext.SpriteBatch.Draw(_backgroundTexture, Vector2.Zero);
            _uiContainer.Draw(graphicsContext, true);

            graphicsContext.SpriteBatch.DrawStringCentered(graphicsContext.FontContainer["Crayon32"], ApplicationInfo.Version, new Vector2(770, 25), Color.Black);

            graphicsContext.SpriteBatch.End();
        }

        private bool CheckIfFirstLaunch()
        {
            LineRunnerSettings settings = base.Services.GetService<ISettingsManager<LineRunnerSettings>>().Settings;
            if (settings.FirstLaunch && string.IsNullOrEmpty(settings.UserName))
            {
                settings.FirstLaunch = false;
                if (!Guide.IsVisible)
                {
                    Guide.BeginShowKeyboardInput(
                        PlayerIndex.One,
                        "Please enter your username",
                        "Please enter the username you want to use for global leaderboards. The username can be changed at anytime",
                        "",
                        this.GetFirstLaunchUserName,
                        null);
                }

                return true;
            }

            return false;
        }

        private void GetFirstLaunchUserName(IAsyncResult result)
        {
            LineRunnerSettings settings = base.Services.GetService<ISettingsManager<LineRunnerSettings>>().Settings;

            string input = (Guide.EndShowKeyboardInput(result) ?? "").Trim();
            if (!string.IsNullOrEmpty(input) && input.Length >= 4 && settings.UserName != input)
            {
                settings.UserName = input;
                settings.MogadeUserName = input;

                base.Services.GetService<ISettingsManager>().Save();
            }
        }
    }
}
