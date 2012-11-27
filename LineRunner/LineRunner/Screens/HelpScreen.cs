using Flai.ScreenManagement;
using Flai;
using Flai.ScreenManagement.Screens;

namespace LineRunner.Screens
{
    public class HelpScreen : GameScreen
    {
        public HelpScreen()
        {
            base.TransitionOnTime = LineRunnerGlobals.ScreenFadeTime;
            base.TransitionOffTime = LineRunnerGlobals.ScreenFadeTime;
        }

        protected override void HandleInput(UpdateContext updateContext)
        {
            if (updateContext.InputState.IsBackButtonPressed)
            {
                LoadingScreen.Load(base.ScreenManager, false, new MainMenuScreen());
            }
        }
    }
}
