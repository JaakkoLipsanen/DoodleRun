using System;
using Flai;
using Flai.Content;
using Flai.Graphics;
using Flai.ScreenManagement;
using Flai.ScreenManagement.Screens;
using Flai.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LineRunner.Screens
{
    public class PauseScreen : GameScreen
    {
        // TODO: Restart button
        private TexturedButton _continueButton;
        private TexturedButton _mainMenuButton;

        private Texture2D _background;

        private BasicUiContainer _uiContainer = new BasicUiContainer();

        public override bool IsPopup
        {
            get { return true; }
        }

        public PauseScreen(GameplayScreen gamePlayScreen)
        {
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

            _continueButton = new TexturedButton(new Sprite(contentManager.LoadTexture("PauseScreen/Play")), leftButtonRect);
            _continueButton.Click += this.ExitScreen;
            _uiContainer.Add(_continueButton);

            _mainMenuButton = new TexturedButton(new Sprite(contentManager.LoadTexture("PauseScreen/MainMenu")), rightButtonRect);
            _mainMenuButton.Click += () =>
            {
                LoadingScreen.Load(this.ScreenManager, false, new MainMenuScreen());
            };
            _uiContainer.Add(_mainMenuButton);

            _background = contentManager.LoadTexture("PauseScreen/Background");
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
            _uiContainer.Draw(graphicsContext, true);

            graphicsContext.SpriteBatch.End();

            base.Draw(graphicsContext);
        }
    }
}
