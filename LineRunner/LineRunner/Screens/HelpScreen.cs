using Flai;
using Flai.Content;
using Flai.Graphics;
using Flai.ScreenManagement;
using LineRunner.Model;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using System;
using System.Collections.Generic;

namespace LineRunner.Screens
{
    public class HelpScreen : GameScreen
    {
        public override bool IsPopup
        {
            get
            {
                return true;
            }
        }
        #region Logic

        private int _screen = 1;

        private readonly Sector _sector = Sector.FromString(
                "         -        --        ---       ",
                "   III       II         I         I   ");

        #endregion

        #region Visual

        private bool _firstFrame = true;
        private bool _exit = false;

        private SpriteFont _font;

        private Texture2D _backgroundTexture;
        private Texture2D _sunTexture;

        private Dictionary<TileType, Texture2D> _tileTextures = new Dictionary<TileType, Texture2D>();
        private Texture2D _groundTexture;

        private SpriteSheet _playerSpriteSheet;

        private readonly Texture2D[] _cloudTextures = new Texture2D[Cloud.TextureCount];

        #endregion

        public HelpScreen()
        {
            LineRunnerGlobals.HasShownHelpScreen = true;
            base.TransitionOnTime = LineRunnerGlobals.ScreenFadeTime;
            base.TransitionOffTime = LineRunnerGlobals.ScreenFadeTime;

            base.EnabledGestures = GestureType.Tap;
        }

        protected override void Activate(bool instancePreserved)
        {
            if (instancePreserved)
            {
                return;
            }

            FlaiContentManager contentManager = base.ContentProvider.DefaultManager;

            _font = contentManager.LoadFont("Crayon24");

            _backgroundTexture = contentManager.LoadTexture("Gameplay/Background");
            _sunTexture = contentManager.LoadTexture("Gameplay/Sun");

            _tileTextures.Add(TileType.Block, contentManager.LoadTexture("Gameplay/Block"));
            _tileTextures.Add(TileType.Spike, contentManager.LoadTexture("Gameplay/Spike"));
            _groundTexture = contentManager.LoadTexture("Gameplay/Ground");

            _playerSpriteSheet = new SpriteSheet(contentManager.LoadTexture("Gameplay/PlayerSprite"), 6, 3);

            for (int i = 0; i < _cloudTextures.Length; i++)
            {
                _cloudTextures[i] = contentManager.LoadTexture("Gameplay/Cloud" + (i + 1));
            }
        }

        protected override void HandleInput(UpdateContext updateContext)
        {
            if (_exit || updateContext.InputState.IsBackButtonPressed)
            {
                this.ExitScreen();
            }

            if (!_firstFrame && updateContext.InputState.HasGesture(GestureType.Tap)) // updateContext.InputState.IsTouchAt(new Rectangle(0, 0, 800, 480), TouchLocationState.Pressed))
            {
                if (_screen == 2)
                {
                    _exit = true;
                }
                else
                {
                    _screen++;
                }
            }

            _firstFrame = false;
        }

        protected override void Draw(GraphicsContext graphicsContext)
        {
            graphicsContext.SpriteBatch.Begin();

            graphicsContext.SpriteBatch.Draw(_backgroundTexture, Vector2.Zero);
            graphicsContext.SpriteBatch.Draw(_sunTexture, new Vector2(800 - _sunTexture.Width / 2f, -_sunTexture.Height / 2f));

            if (_screen == 1)
            {
                this.DrawSector(graphicsContext, 120);

                graphicsContext.SpriteBatch.Draw(_playerSpriteSheet.Texture, new Vector2(100, LineRunnerGlobals.GroundLevel - 30) - _playerSpriteSheet.FrameSize, _playerSpriteSheet.GetSourceRectangle(0, 2), Color.White);
                graphicsContext.SpriteBatch.Draw(_groundTexture, new Vector2(0, LineRunnerGlobals.GroundLevel - LineRunnerGlobals.GroundLevelDrawOffset));
                this.DrawClouds(graphicsContext, new Vector2(80, 100), new Vector2(300, 30), new Vector2(656, 120), new Vector2(360, 140));

                graphicsContext.SpriteBatch.DrawStringCentered(_font, "Try to avoid spikes and squares by jumping and rolling", new Vector2(400, 400), Color.Black);
            }
            else if (_screen == 2)
            {
                this.DrawSector(graphicsContext, 120);

                graphicsContext.SpriteBatch.Draw(_playerSpriteSheet.Texture, new Vector2(100, LineRunnerGlobals.GroundLevel - 30) - _playerSpriteSheet.FrameSize, _playerSpriteSheet.GetSourceRectangle(0, 2), Color.White);
                graphicsContext.SpriteBatch.Draw(_groundTexture, new Vector2(0, LineRunnerGlobals.GroundLevel - LineRunnerGlobals.GroundLevelDrawOffset));
                this.DrawClouds(graphicsContext, new Vector2(80, 100), new Vector2(300, 30), new Vector2(656, 120), new Vector2(360, 140));

                graphicsContext.SpriteBatch.Draw(graphicsContext.BlankTexture, new Rectangle(0, 0, 360, 480), Color.Red * 0.25f);
                graphicsContext.SpriteBatch.Draw(graphicsContext.BlankTexture, new Rectangle(440, 0, 360, 480), Color.Green * 0.25f);

                graphicsContext.SpriteBatch.DrawStringCentered(_font, "Tap on the left side to roll", new Vector2(180, 400), Color.Black);
                graphicsContext.SpriteBatch.DrawStringCentered(_font, "Tap on the right side to jump", new Vector2(619.5f, 400), Color.Black);

                graphicsContext.SpriteBatch.DrawStringCentered(_font, "Hold to jump higher", new Vector2(620f, 440), Color.Black);
            }

            graphicsContext.SpriteBatch.End();
        }

        private void DrawSector(GraphicsContext graphicsContext, float xOffset)
        {
            int start = Math.Max(0, (int)(xOffset / LineRunnerGlobals.TileSize));
            int end = Math.Min(_sector.Width, (int)(800 + xOffset) / LineRunnerGlobals.TileSize + 1);
            for (int x = start; x < end; x++)
            {
                float xPosition = x * LineRunnerGlobals.TileSize - xOffset; 

                TileType upperTile = _sector.GetUpperLevel(x);
                if (upperTile != TileType.Air)
                {
                    Rectangle area = new Rectangle((int)Math.Round(xPosition), LineRunnerGlobals.UpperTileLevel - LineRunnerGlobals.TileSize, LineRunnerGlobals.TileSize, LineRunnerGlobals.TileSize);
                   graphicsContext.SpriteBatch.Draw(_tileTextures[upperTile], area, Color.Green);
                }

                TileType groundTile = _sector.GetGroundLevel(x);
                if (groundTile != TileType.Air)
                {
                    Rectangle area = new Rectangle((int)Math.Round(xPosition), LineRunnerGlobals.GroundLevel - LineRunnerGlobals.TileSize, LineRunnerGlobals.TileSize, LineRunnerGlobals.TileSize);
                    graphicsContext.SpriteBatch.Draw(_tileTextures[groundTile], area, Color.HotPink);
                }
            }
        }

        private void DrawClouds(GraphicsContext graphicsContext, Vector2 cloud1, Vector2 cloud2, Vector2 cloud3, Vector2 cloud4)
        {
            graphicsContext.SpriteBatch.Draw(_cloudTextures[0 % _cloudTextures.Length], cloud1, null, Color.White, 0f, Vector2.Zero, 0.6f, SpriteEffects.None, 0f);
            graphicsContext.SpriteBatch.Draw(_cloudTextures[1 % _cloudTextures.Length], cloud2, null, Color.White, 0f, Vector2.Zero, 0.9f, SpriteEffects.None, 0f);
            graphicsContext.SpriteBatch.Draw(_cloudTextures[2 % _cloudTextures.Length], cloud3, null, Color.White, 0f, Vector2.Zero, 0.75f, SpriteEffects.None, 0f);
            graphicsContext.SpriteBatch.Draw(_cloudTextures[3 % _cloudTextures.Length], cloud4, null, Color.White, 0f, Vector2.Zero, 0.6f, SpriteEffects.None, 0f);
        }
    }
}
