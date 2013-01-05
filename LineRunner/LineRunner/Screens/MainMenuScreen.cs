using Flai;
using Flai.Advertisiments;
using Flai.Content;
using Flai.Extensions;
using Flai.Graphics;
using Flai.Misc;
using Flai.Mogade;
using Flai.ScreenManagement;
using Flai.ScreenManagement.Screens;
using Flai.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using Mogade;
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

        protected override void Activate(bool instancePreserved)
        {
            if (instancePreserved)
            {
                return;
            }

            TextButton playButton = new TextButton(new Vector2(400f, 160), "Play") { Font = "Crayon64", Color = new Color(226, 105, 105) };
            playButton.Tap += (o, e) =>
                {
                    if (LineRunnerGlobals.IsFirstLaunch && !LineRunnerGlobals.HasShownHelpScreen)
                    {
                        LoadingScreen.Load(base.ScreenManager, false, new GameplayScreen(), new HelpScreen());
                    }
                    else
                    {
                        LoadingScreen.Load(base.ScreenManager, false, new GameplayScreen());
                    }
                };
            _uiContainer.Add(playButton);

            TextButton leaderboardButton = new TextButton(new Vector2(400f, 280), "Leaderboard") { Font = "Crayon64", Color = new Color(75, 148, 80) };
            leaderboardButton.Tap += (o, e) =>
            {
                LoadingScreen.Load(base.ScreenManager, false, new LeaderboardScreen());
            };
            _uiContainer.Add(leaderboardButton);

            TextButton changeUsernameButton = new TextButton(new Vector2(400f, 400), "Change Username") { Font = "Crayon64", Color = Color.SteelBlue }; // Color.RoyalBlue };
            changeUsernameButton.Tap += (o, e) =>
                {
                    this.ShowChangeUsernameDialog();
                };
            _uiContainer.Add(changeUsernameButton);

            TextButton helpButton = new TextButton(new Vector2(50, 450), "Help") { Font = "Crayon32", Color = Color.Black };
            helpButton.Tap += (o, e) =>
                {
                    base.ScreenManager.AddScreen(new HelpScreen());
                };
            _uiContainer.Add(helpButton);

            TextButton rateButton = new TextButton(new Vector2(740, 455), "Rate") { Font = "Crayon32", Color = Color.Black };
            rateButton.Tap += (o, e) =>
            {
                ApplicationInfo.OpenApplicationReviewPage();
            };
            _uiContainer.Add(rateButton);

            FlaiContentManager contentManager = base.ContentProvider.DefaultManager;
            _backgroundTexture = contentManager.LoadTexture("Gameplay/Background");

            if (!this.CheckIfFirstLaunch())
            {
                this.UpdateUsername();
            }

             // Save settings if needed
            ISettingsManager<LineRunnerSettings> settings = base.Services.GetService<ISettingsManager<LineRunnerSettings>>();
            settings.Save();

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
            }
        }

        protected override void HandleInput(UpdateContext updateContext)
        {
            if (updateContext.InputState.IsBackButtonPressed)
            {
                base.ScreenManager.Game.Exit();
                return;
            }

          /*  if (updateContext.InputState.IsNewTouchAt(_rateInputArea))
            {
                ApplicationInfo.OpenApplicationReviewPage();
            }
            else if (updateContext.InputState.IsNewTouchAt(_helpInputArea))
            {
                base.ScreenManager.AddScreen(new HelpScreen());
               // LoadingScreen.Load(base.ScreenManager, false, new HelpScreen());
            } */
        }

        protected override void Draw(GraphicsContext graphicsContext)
        {
            graphicsContext.SpriteBatch.Begin();

            graphicsContext.SpriteBatch.Draw(_backgroundTexture, Vector2.Zero);
            _uiContainer.Draw(graphicsContext, true);

            graphicsContext.SpriteBatch.DrawStringCentered(base.FontProvider["Crayon32"], ApplicationInfo.Version, new Vector2(770, 25), Color.Black);
       /*    graphicsContext.SpriteBatch.DrawString(base.FontProvider["Crayon32"], "Rate", new Vector2(720, 435), Color.Black);
            graphicsContext.SpriteBatch.DrawString(base.FontProvider["Crayon32"], "Help", new Vector2(10, 435), Color.Black); */

            graphicsContext.SpriteBatch.End();
        }

        private bool CheckIfFirstLaunch()
        {
            LineRunnerSettings settings = base.GetSettings<LineRunnerSettings>();
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

        private void ShowChangeUsernameDialog()
        {
            if (!Guide.IsVisible)
            {
                LineRunnerSettings settings = base.GetSettings<LineRunnerSettings>();
                Guide.BeginShowKeyboardInput(PlayerIndex.One, "Change username", "Change the username that will be used on the global leaderboards. The username must be at least 4 characters long ", settings.UserName, (result) =>
                {
                    string input = (Guide.EndShowKeyboardInput(result) ?? "").Trim();
                    if (!string.IsNullOrEmpty(input) && input.Length >= 4 && settings.UserName != input)
                    {
                        settings.UserName = input;
                        if (settings.MogadeUserName == "")
                        {
                            settings.MogadeUserName = settings.UserName;
                            this.UpdateHighscore();
                        }
                        else
                        {
                            this.UpdateUsername();
                        }
                    }
                }, null);
            }
        }

        private void GetFirstLaunchUserName(IAsyncResult result)
        {
            LineRunnerSettings settings = base.GetSettings<LineRunnerSettings>();

            string input = (Guide.EndShowKeyboardInput(result) ?? "").Trim();
            if (!string.IsNullOrEmpty(input) && input.Length >= 4 && settings.UserName != input)
            {
                settings.UserName = input;
                settings.MogadeUserName = input;

                base.Services.GetService<ISettingsManager>().Save();
            }
        }

        private void UpdateUsername()
        {
            LineRunnerSettings settings = base.GetSettings<LineRunnerSettings>();
            if (settings.UserName != settings.MogadeUserName)
            {
                // WHY. THE. FUCK. THIS DOESNT WORK IF mogadeManager is IMogadeManager? Fucking DLL's. Fucking fuck. FUUUUUUCK
                MogadeManager mogadeManager = base.Services.GetService<IMogadeManager>() as MogadeManager;
                mogadeManager.Rename(settings.MogadeUserName, settings.UserName, (response) =>
                {
                    if (response.Success)
                    {
                        settings.MogadeUserName = settings.UserName;
                        this.UpdateHighscore();

                        base.Services.GetService<ISettingsManager>().Save();
                    }
                });
            }
            else
            {
                this.UpdateHighscore();
            }
        }

        private void UpdateHighscore()
        {
            LineRunnerSettings settings = base.GetSettings<LineRunnerSettings>();
            if (!settings.HighScorePostedToLeaderboards && settings.CanPostScoresToLeaderboard)
            {
                IMogadeManager mogadeManager = base.Services.GetService<IMogadeManager>();
                mogadeManager.SaveScore(LineRunnerGlobals.MogadeLeaderboardId, new Score() { UserName = settings.MogadeUserName, Points = settings.HighScore, Dated = DateTime.Now }, (response) =>
                {
                    settings.HighScorePostedToLeaderboards = response.Success;
                });
            }
        }
    }
}
