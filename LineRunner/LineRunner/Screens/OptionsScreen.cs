using Flai;
using Flai.Content;
using Flai.Graphics;
using Flai.Misc;
using Flai.ScreenManagement;
using Flai.ScreenManagement.Screens;
using Flai.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;

namespace LineRunner.Screens
{
    public class OptionsScreen : GameScreen
    {
        private  LineRunnerSettings _settings;
        private Texture2D _backgroundTexture;

        private readonly BasicUiContainer _uiContainer = new BasicUiContainer();

        public OptionsScreen()
        {
            base.TransitionOnTime = LineRunnerGlobals.ScreenFadeTime;
            base.TransitionOffTime = LineRunnerGlobals.ScreenFadeTime;

            
        }

        protected override void LoadContent(bool instancePreserved)
        {
            if(instancePreserved)
            {
                return;
            }

            _settings = base.Services.GetService<ISettingsManager<LineRunnerSettings>>().Settings;

            FlaiContentManager contentManager = base.ContentProvider.DefaultManager;
            _backgroundTexture = contentManager.LoadTexture("Gameplay/Background");

            _uiContainer.Add(new TextBlock("Settings", new Vector2(400, 60)) { Font = "Crayon64", Color = Color.Black });
            _uiContainer.Add(new TextBlock("Score Location", new Vector2(400, 150)) { Color = Color.Black, Font = "Crayon48" });
            TextButton changeScoreLocationButton = new TextButton(_settings.ScoreLocation.ToString(), new Vector2(400, 200)) { Color = Color.Black, Font = "Crayon32" };
            changeScoreLocationButton.Click += () =>
                {
                    if (_settings.ScoreLocation == ScoreLocation.Down)
                    {
                        _settings.ScoreLocation = ScoreLocation.Left;
                        changeScoreLocationButton.Text = "Left";
                    }
                    else if (_settings.ScoreLocation == ScoreLocation.Left)
                    {
                        _settings.ScoreLocation = ScoreLocation.Right;
                        changeScoreLocationButton.Text = "Right";
                    }
                    else if (_settings.ScoreLocation == ScoreLocation.Right)
                    {
                        _settings.ScoreLocation = ScoreLocation.Down;
                        changeScoreLocationButton.Text = "Down";
                    }
                };
            _uiContainer.Add(changeScoreLocationButton);

            _uiContainer.Add(new TextBlock("Vibration", new Vector2(400, 280)) { Color = Color.Black, Font = "Crayon48" });
            TextButton changeVibrationButton = new TextButton(_settings.VibrationEnabled ? "Enabled" : "Disabled", new Vector2(400, 330)) { Color = Color.Black, Font = "Crayon32", InflateAmount = 16 };
            changeVibrationButton.Click += () =>
                {
                    _settings.VibrationEnabled = !_settings.VibrationEnabled;
                    changeVibrationButton.Text = _settings.VibrationEnabled ? "Enabled" : "Disabled";
                };
            _uiContainer.Add(changeVibrationButton);

            TextButton changeUserNameButton = new TextButton("Change Username", new Vector2(400, 410)) { Color = Color.Black, Font = "Crayon48", InflateAmount = 4 };
            changeUserNameButton.Click += this.ShowChangeUsernameDialog;
            _uiContainer.Add(changeUserNameButton);
        }

        protected override void Update(UpdateContext updateContext, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            if (updateContext.InputState.IsBackButtonPressed)
            {
                LoadingScreen.Load(base.ScreenManager, false, new MainMenuScreen());
            }

            _uiContainer.Update(updateContext);
            if (this.IsActive)
            {
                if (updateContext.InputState.IsBackButtonPressed)
                {
                    LoadingScreen.Load(base.ScreenManager, false, new MainMenuScreen());
                }
            }
        }

        protected override void Draw(GraphicsContext graphicsContext)
        {
            graphicsContext.SpriteBatch.Begin();
            graphicsContext.SpriteBatch.Draw(_backgroundTexture, graphicsContext.ScreenArea);

            _uiContainer.Draw(graphicsContext, true);
            graphicsContext.SpriteBatch.End();
        }

        private void ShowChangeUsernameDialog()
        {
            if (!Guide.IsVisible)
            {
                LineRunnerSettings settings = base.Services.GetService<ISettingsManager<LineRunnerSettings>>().Settings;
                Guide.BeginShowKeyboardInput(PlayerIndex.One, "Change username", "Change the username that will be used on the global leaderboards. The username must be at least 4 characters long ", settings.UserName, (result) =>
                {
                    string input = (Guide.EndShowKeyboardInput(result) ?? "").Trim();
                    if (!string.IsNullOrEmpty(input) && input.Length >= 4 && settings.UserName != input)
                    {
                        settings.UserName = input;
                        if (settings.MogadeUserName == "")
                        {
                            settings.MogadeUserName = settings.UserName;
                            settings.UpdateHighscore(base.Services);
                        }
                        else
                        {
                            settings.UpdateUsername(base.Services);
                        }
                    }
                }, null);
            }
        }

    }
}
