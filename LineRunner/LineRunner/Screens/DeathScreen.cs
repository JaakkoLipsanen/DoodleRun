using System;
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

namespace LineRunner.Screens
{
    public class DeathScreen : GameScreen
    {
        private TexturedButton _restartButton;
        private TexturedButton _mainMenuButton;

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

        protected override void Activate(bool instancePreserved)
        {
            if (instancePreserved)
            {
                return;
            }

            FlaiContentManager contentManager = base.ContentProvider.DefaultManager;
            Rectangle leftButtonRect = new Rectangle(200, 190, 128, 128);
            Rectangle rightButtonRect = new Rectangle(400 + (400 - (200 + 128)), 190, 128, 128);

            _restartButton = new TexturedButton(leftButtonRect, new Sprite(contentManager.LoadTexture("PauseScreen/Restart")));
            _restartButton.Tap += (o, e) =>
            {
                _gamePlayScreen.RestartGame();
                base.ExitScreen();
            };
            _uiContainer.Add(_restartButton);

            _mainMenuButton = new TexturedButton(rightButtonRect, new Sprite(contentManager.LoadTexture("PauseScreen/MainMenu")));
            _mainMenuButton.Tap += (o, e) =>
            {
                LoadingScreen.Load(base.ScreenManager, false, new MainMenuScreen());
            };
            _uiContainer.Add(_mainMenuButton);

            IFontProvider fontProvider = base.Services.GetService<IFontProvider>();
            _font = fontProvider["Crayon32"];

            this.SaveScore();
        }

        protected override void Update(UpdateContext updateContext, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            _uiContainer.Update(updateContext);
            base.Update(updateContext, otherScreenHasFocus, coveredByOtherScreen);
        }

        protected override void Draw(GraphicsContext graphicsContext)
        {
            graphicsContext.SpriteBatch.Begin();

            graphicsContext.SpriteBatch.Draw(graphicsContext.BlankTexture, new Rectangle(100, 100, 800 - 100 * 2, 480 - 100 * 2), Color.Black * 0.5f);
            graphicsContext.SpriteBatch.DrawStringCentered(_font, string.Format("Score: {0}", _score), new Vector2(400, 150), Color.White);

            _uiContainer.Draw(graphicsContext, true);

            graphicsContext.SpriteBatch.End();

            base.Draw(graphicsContext);
        }

        private void SaveScore()
        {
            LineRunnerSettings settings = base.Services.GetService<ISettingsManager<LineRunnerSettings>>().Settings;

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
