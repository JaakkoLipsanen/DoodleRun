using Flai;
using Flai.Content;
using Flai.Graphics;
using Flai.Misc;
using Flai.Mogade;
using Flai.ScreenManagement;
using Flai.ScreenManagement.Screens;
using Flai.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mogade;
using System;

namespace LineRunner.Screens
{
    public class DeathScreen : GameScreen
    {
        private TexturedButton _restartButton;
        private TexturedButton _mainMenuButton;

        private Texture2D _background;

        private readonly BasicUiContainer _uiContainer = new BasicUiContainer();
        
        private readonly GameplayScreen _gamePlayScreen;
        private readonly int _score;

        private SpriteFont _font;

        public override bool IsPopup
        {
            get { return true; }
        }

        public DeathScreen(GameplayScreen gamePlayScreen, int score)
        {
            _gamePlayScreen = gamePlayScreen;
            _score = score;

            base.TransitionOffTime = LineRunnerGlobals.ScreenFadeTime;
            base.TransitionOnTime = LineRunnerGlobals.ScreenFadeTime;
        }

        protected override void LoadContent(bool instancePreserved)
        {
            if (instancePreserved)
            {
                return;
            }

            FlaiContentManager contentManager = base.ContentProvider.DefaultManager;
            
            RectangleF leftButtonRect = new RectangleF(400 + (400 - (200 + 128)), 190, 128, 128);
            RectangleF rightButtonRect = new RectangleF(200, 190, 128, 128);

            _restartButton = new TexturedButton(leftButtonRect, new Sprite(contentManager.LoadTexture("PauseScreen/Play")));
            _restartButton.Click += () =>
            {
                _gamePlayScreen.RestartGame();
                this.ExitScreen();
            };
            _uiContainer.Add(_restartButton);

            _mainMenuButton = new TexturedButton(rightButtonRect, new Sprite(contentManager.LoadTexture("PauseScreen/MainMenu")));
            _mainMenuButton.Click += () => LoadingScreen.Load(this.ScreenManager, false, new MainMenuScreen());
            _uiContainer.Add(_mainMenuButton);

            _background = contentManager.LoadTexture("PauseScreen/Background");

            IFontContainer fontProvider = base.Services.GetService<IFontContainer>();
            _font = fontProvider["Crayon32"];

            this.SaveScore();
        }

        protected override void Update(UpdateContext updateContext, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            if (updateContext.InputState.IsBackButtonPressed)
            {
                LoadingScreen.Load(base.ScreenManager, false, new MainMenuScreen());
            }

            _uiContainer.Update(updateContext);
            base.Update(updateContext, otherScreenHasFocus, coveredByOtherScreen);
        }

        protected override void Draw(GraphicsContext graphicsContext)
        {
            graphicsContext.SpriteBatch.Begin();

            graphicsContext.SpriteBatch.DrawCentered(_background, new Vector2(400, 240));
            graphicsContext.SpriteBatch.DrawStringCentered(_font, string.Format("Score: {0}", _score), new Vector2(400, 160), Color.White);

            _uiContainer.Draw(graphicsContext, true);

            graphicsContext.SpriteBatch.End();

            base.Draw(graphicsContext);
        }

        private void SaveScore()
        {
            LineRunnerSettings settings = base.Services.GetService<SettingsManager<LineRunnerSettings>>().Settings;

            bool isHighScore = false;
            // If the score is new high score, post it to mogade
            if (_score > settings.HighScore)
            {
                settings.HighScore = _score;
                settings.HighScorePostedToLeaderboards = false;
                isHighScore = true;
            }

            // If the previous highscore was not sent to mogade, try to send it again
            if (settings.CanPostScoresToLeaderboard)
            {
                IMogadeManager mogadeManager = base.Services.GetService<IMogadeManager>();
                mogadeManager.SaveScore(
                      LineRunnerGlobals.MogadeLeaderboardId,
                      new Score() { UserName = settings.MogadeUserName, Points = _score, Dated = DateTime.Now }, (response) =>
                      {
                          if (isHighScore)
                          {
                              settings.HighScorePostedToLeaderboards = response.Success;
                          }
                      });

                // If the current highscore has not been sent to mogade, try to send it again
                if (!isHighScore && !settings.HighScorePostedToLeaderboards)
                {
                    mogadeManager.SaveScore(LineRunnerGlobals.MogadeLeaderboardId,
                        new Score() { UserName = settings.MogadeUserName, Points = settings.HighScore, Dated = DateTime.Now }, (response) =>
                        {
                            settings.HighScorePostedToLeaderboards = response.Success;
                        });
                }
            }
        }
    }
}
