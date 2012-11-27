
using Flai;
using Flai.Advertisiments;
using System;

namespace LineRunner
{
    public static class LineRunnerGlobals
    {
        /// <summary>
        /// Ground level is at 320 pixel from top of the screen
        /// </summary>
        public const int GroundLevel = 320;
        public const int GroundLevelDrawOffset = 12;

        public const int TileSize = 28;
        public const int UpperTileLevel = (int)(GroundLevel - TileSize * 1.5);

        public static readonly Range GroundTileVerticalRange = 
            new Range(LineRunnerGlobals.GroundLevel - LineRunnerGlobals.TileSize, LineRunnerGlobals.GroundLevel);
        public static readonly Range UpperTileVerticalRange = 
            new Range(LineRunnerGlobals.UpperTileLevel - LineRunnerGlobals.TileSize, LineRunnerGlobals.UpperTileLevel);

        // Change this to false when on submitting this to marketplace
        public const bool ShowDebug = false;
        public static readonly TimeSpan ScreenFadeTime = TimeSpan.FromSeconds(0);
        public const string MogadeLeaderboardId = "5059b3be563d8a53c200afe1";

        public static readonly Vector2i BottomAdCenterPosition = new Vector2i(400, 480 - AdManager.AdSize.Y / 2 - 4);
    }
}
