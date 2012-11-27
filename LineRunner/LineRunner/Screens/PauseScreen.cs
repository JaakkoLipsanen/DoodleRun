using System;
using Flai;
using Flai.Content;
using Flai.Graphics;
using Flai.ScreenManagement;
using Flai.ScreenManagement.Screens;
using Flai.Ui;
using Microsoft.Xna.Framework;

namespace LineRunner.Screens
{
    public class PauseScreen : GameScreen
    {
        // TODO: Restart button
        private TexturedButton _continueButton;
        private TexturedButton _mainMenuButton;

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

        protected override void Activate(bool instancePreserved)
        {
            if (instancePreserved)
            {
                return;
            }

            FlaiContentManager contentManager = base.ContentProvider.DefaultManager;
            Rectangle leftButtonRect = new Rectangle(200, 190, 128, 128);
            Rectangle rightButtonRect = new Rectangle(400 + (400 - (200 + 128)), 190, 128, 128);

            _continueButton = new TexturedButton(leftButtonRect, new Sprite(contentManager.LoadTexture("PauseScreen/Continue")));
            _continueButton.Tap += (o, e) =>
            {
                base.ExitScreen();
            };
            _uiContainer.Add(_continueButton);

            _mainMenuButton = new TexturedButton(rightButtonRect, new Sprite(contentManager.LoadTexture("PauseScreen/MainMenu")));
            _mainMenuButton.Tap += (o, e) =>
            {
                LoadingScreen.Load(base.ScreenManager, false, new MainMenuScreen());
            };
            _uiContainer.Add(_mainMenuButton);
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
            _uiContainer.Draw(graphicsContext, true);

            graphicsContext.SpriteBatch.End();

            base.Draw(graphicsContext);
        }
    }
}
