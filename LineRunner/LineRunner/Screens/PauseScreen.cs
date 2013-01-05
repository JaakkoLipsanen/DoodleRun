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

        protected override void Activate(bool instancePreserved)
        {
            if (instancePreserved)
            {
                return;
            }

            FlaiContentManager contentManager = base.ContentProvider.DefaultManager;
            Rectangle leftButtonRect = new Rectangle(400 + (400 - (200 + 128)), 190, 128, 128);
            Rectangle rightButtonRect = new Rectangle(200, 190, 128, 128);

            _continueButton = new TexturedButton(leftButtonRect, new Sprite(contentManager.LoadTexture("PauseScreen/Play")));
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
