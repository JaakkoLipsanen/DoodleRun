
using Flai;
using Flai.Advertisiments;
using Flai.Content;
using Flai.Extensions;
using Flai.Graphics;
using Flai.Misc;
using Flai.ScreenManagement;
using LineRunner.Model;
using Microsoft.Devices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Diagnostics;

namespace LineRunner.Screens
{
    public class GameplayScreen : GameScreen
    {
        #region Logic

        private const int CloudCount = 5;
        private readonly Cloud[] _clouds = new Cloud[CloudCount];

        private Player _player;
        private Level _level;

        private Camera2D _camera2D;

        private double _score = 0;
        public int Score
        {
            get { return (int)_score; }
        }

        #endregion

        #region Visual

        private SpriteFont _scoreFont;
        private Texture2D _backgroundTexture;

        #endregion

        public GameplayScreen()
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

            _player = new Player();
            _level = new Level();
            _camera2D = new Camera2D(new Vector2(400, 240));

            FlaiContentManager gameContentManager = base.ContentProvider.DefaultManager;
            _level.LoadContent(gameContentManager);
            _player.LoadContent(gameContentManager);
            this.CreateBackgroundTexture(gameContentManager);

            IFontProvider fontProvider = base.Services.GetService<IFontProvider>();
            _scoreFont = fontProvider["Crayon32"];

            // Load clouds
            Cloud.LoadTextures(gameContentManager);
            this.InitializeClouds();

            IAdManager adManager = base.Services.GetService<IAdManager>();
            adManager.Visible = true;
            adManager.AdPosition = LineRunnerGlobals.AdCenterPosition;
        }

        protected override void Update(UpdateContext updateContext, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            if (this.IsActive)
            {
                if (_player.IsAlive)
                {
                    _score += updateContext.GameTime.ElapsedGameTime.TotalMilliseconds / 20;
                }

                _player.Update(updateContext);
                this.HandleCollisions();
                this.UpdateCamera();

                foreach (Cloud cloud in _clouds)
                {
                    cloud.Update(updateContext);
                }
            }
            else if (!_player.IsAlive)
            {
                _player.Fall(updateContext);
                foreach (Cloud cloud in _clouds)
                {
                    cloud.Update(updateContext);
                }
            }
        }

        protected override void HandleInput(UpdateContext updateContext)
        {
            if (updateContext.InputState.IsBackButtonPressed)
            {
                this.Pause();
                return;
            }

            _player.HandleInput(updateContext);
        }

        protected override void Draw(GraphicsContext graphicsContext)
        {
            // Draw background
            graphicsContext.SpriteBatch.Begin();
            graphicsContext.SpriteBatch.Draw(_backgroundTexture, Vector2.Zero);
            graphicsContext.SpriteBatch.End();

            // Main drawing
            graphicsContext.SpriteBatch.Begin(_camera2D.GetTransform(graphicsContext.GraphicsDevice));

            _player.Draw(graphicsContext);
            _level.Draw(graphicsContext, _camera2D);

            graphicsContext.SpriteBatch.End();

            // Draw clouds
            graphicsContext.SpriteBatch.Begin(SpriteSortMode.BackToFront);
            foreach (Cloud cloud in _clouds)
            {
                cloud.Draw(graphicsContext);
            }
            graphicsContext.SpriteBatch.End();

            // Draw score
            graphicsContext.SpriteBatch.Begin();
            graphicsContext.SpriteBatch.DrawString<int>(_scoreFont, this.Score, new Vector2(10, 5), Color.Black, 1f);
            graphicsContext.SpriteBatch.End();      
        }

        private void InitializeClouds()
        {
            for (int i = 0; i < _clouds.Length; i++)
            {
                int textureIndex = Global.Random.Next(0, Cloud.TextureCount);
                float scale = Global.Random.NextFloat(0.5f, 1.0f);
                float rotation = Global.Random.NextFloat(0, 0.05f) - 0.025f;
                Vector2 position = new Vector2(Global.Random.NextFloat(-20, 1500), Global.Random.NextFloat(20, 100));
                _clouds[i] = new Cloud(textureIndex, position, scale, rotation);
            }
        }

        public void RestartGame()
        {
            _score = 0;

            _player.Reset();
            _level.Reset();

            this.InitializeClouds();
            this.UpdateCamera();
        }

        private void HandleCollisions()
        {
            RectangleF playerRectangle = _player.Area;
            Range playerVerticalPositionRange = new Range(playerRectangle.Top, playerRectangle.Bottom);

            bool intersectsWithUpperLevel = LineRunnerGlobals.UpperTileVerticalRange.Intersects(playerVerticalPositionRange);
            bool intersectsWithGroundLevel = LineRunnerGlobals.GroundTileVerticalRange.Intersects(playerVerticalPositionRange);
            if (intersectsWithGroundLevel || intersectsWithUpperLevel )
            {
                for (float x = playerRectangle.Left - (playerRectangle.Left % LineRunnerGlobals.TileSize); x <= playerRectangle.Right - (playerRectangle.Right % LineRunnerGlobals.TileSize); x += LineRunnerGlobals.TileSize)
                {
                    if (intersectsWithUpperLevel)
                    {
                        
                        TileType upperTile = _level.GetTile(new Vector2(x, LineRunnerGlobals.UpperTileLevel - LineRunnerGlobals.TileSize / 2f));
                        if (upperTile != TileType.Air)
                        {
                            if (GraphicsHelper.PixelPerfectCollision(_player.PixelData, _player.Area.ToRectangle(), _level.GetPixelData(upperTile), new Rectangle((int)x, LineRunnerGlobals.UpperTileLevel - LineRunnerGlobals.TileSize, LineRunnerGlobals.TileSize, LineRunnerGlobals.TileSize), 4))
                            {
                                this.OnPlayerDie();
                                break;
                            }
                        }
                    }

                    if (intersectsWithGroundLevel)
                    {
                        TileType groundTile = _level.GetTile(new Vector2(x, LineRunnerGlobals.GroundLevel - LineRunnerGlobals.TileSize / 2f));
                        if (groundTile != TileType.Air)
                        {
                            if (GraphicsHelper.PixelPerfectCollision(_player.PixelData, _player.Area.ToRectangle(), _level.GetPixelData(groundTile), new Rectangle((int)x, LineRunnerGlobals.GroundLevel - LineRunnerGlobals.TileSize, LineRunnerGlobals.TileSize, LineRunnerGlobals.TileSize), 4))
                            {
                                this.OnPlayerDie();
                                break;
                            }
                        }
                    }
                }
            }
        }

        private void UpdateCamera()
        {
            _camera2D.Position = new Vector2(_player.Position.X + 300, 240);
        }

        private void Pause()
        {
            base.ScreenManager.AddScreen(new PauseScreen(this));
        }

        private void OnPlayerDie()
        {
            VibrateController.Default.Start(TimeSpan.FromSeconds(0.1f));
            _player.IsAlive = false;
            base.ScreenManager.AddScreen(new DeathScreen(this, this.Score));
        }

        private void CreateBackgroundTexture(FlaiContentManager gameContentManager)
        {
            IGraphicsContext graphicsContext = base.Services.GetService<IGraphicsContext>();
            RenderTarget2D backgroundRenderTarget = new RenderTarget2D(graphicsContext.GraphicsDevice, graphicsContext.ScreenSize.X, graphicsContext.ScreenSize.Y);
            Texture2D emptyBackgroundTexture = gameContentManager.LoadTexture("Gameplay/Background");
            Texture2D sunTexture = gameContentManager.LoadTexture("Gameplay/Sun");

            // Draw
            graphicsContext.GraphicsDevice.SetRenderTarget(backgroundRenderTarget);
            graphicsContext.SpriteBatch.Begin();

            graphicsContext.SpriteBatch.DrawFullscreen(emptyBackgroundTexture, Color.White);
            graphicsContext.SpriteBatch.DrawCentered(sunTexture, new Vector2(800, 0));

            graphicsContext.SpriteBatch.End();
            graphicsContext.GraphicsDevice.SetRenderTarget(null);

            // Set render target to backgGroundTexture
            _backgroundTexture = backgroundRenderTarget;
        }
    }
}
