using Flai;
using Flai.Graphics;
using Flai.ScreenManagement;
using Flai.ScreenManagement.Screens;

namespace LineRunner.Screens
{
    public class OptionsScreen : GameScreen
    {
        public OptionsScreen()
        {
            base.TransitionOnTime = LineRunnerGlobals.ScreenFadeTime;
            base.TransitionOffTime = LineRunnerGlobals.ScreenFadeTime;
        }

        protected override void Update(UpdateContext updateContext, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {

        }

        protected override void HandleInput(UpdateContext updateContext)
        {
            if (updateContext.InputState.IsBackButtonPressed)
            {
                LoadingScreen.Load(base.ScreenManager, false, new MainMenuScreen());
            }
        }

        protected override void Draw(GraphicsContext graphicsContext)
        {

        }
    }
}
