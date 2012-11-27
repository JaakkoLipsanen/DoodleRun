
using Flai;
using Flai.Advertisiments;
using Flai.Content;
using Flai.Extensions;
using Flai.Graphics;
using Flai.ScreenManagement;
using LineRunner.Model;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LineRunner.Screens
{
    public class GameplayScreen : GameScreen
    {
        #region Logic

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
        private Sprite _sunSprite;
        private Texture2D _cloudTexture;

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

            _sunSprite = new Sprite(gameContentManager.LoadTexture("Gameplay/Sun"), true);
            _cloudTexture = gameContentManager.LoadTexture("Gameplay/Cloud1");

            IFontProvider fontProvider = base.Services.GetService<IFontProvider>();
            _scoreFont = fontProvider["Crayon32"];
        }

        protected override void Update(UpdateContext updateContext, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            if (this.IsActive)
            {
                // If FPS drops even temporarily under 5fps, pause the game
                if (updateContext.DeltaSeconds > 0.2f)
                {
                    this.Pause();
                    return;
                }

                if (_player.IsAlive)
                {
                    _score += updateContext.GameTime.ElapsedGameTime.TotalMilliseconds / 20;
                }

                _player.Update(updateContext);
                this.HandleCollisions();
                this.UpdateCamera();
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
            // Main drawing
            graphicsContext.SpriteBatch.Begin(_camera2D.GetTransform(graphicsContext.GraphicsDevice));

            _level.DrawBackground(graphicsContext, _camera2D);
            _player.Draw(graphicsContext);
            _level.Draw(graphicsContext, _camera2D);

            graphicsContext.SpriteBatch.End();

            // Static ( = no camera ) drawing
            graphicsContext.SpriteBatch.Begin();

            graphicsContext.SpriteBatch.Draw(graphicsContext.BlankTexture, new Rectangle(400 - AdManager.AdSize.X / 2, 480 - AdManager.AdSize.Y - 4, AdManager.AdSize.X, AdManager.AdSize.Y), Color.Black * 0.7f);

            // Sun
         //   graphicsContext.SpriteBatch.Draw(_sunSprite, new Vector2(800, 0));

            graphicsContext.SpriteBatch.Draw(_cloudTexture, new Vector2(300, 70), Color.White, 0f, Vector2.Zero, 0.3f);

            // Score
            graphicsContext.SpriteBatch.DrawString(_scoreFont, this.Score, new Vector2(graphicsContext.GraphicsDevice.Viewport.Width - 100, 5), Color.Black);

            graphicsContext.SpriteBatch.End();
        }

        public void RestartGame()
        {
            _score = 0;

            _player.Reset();
            _level.Reset();
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
                            if (GraphicsHelper.PixelPerfectCollision(_player.PixelData, _player.Area.ToRectangle(), _level.GetPixelData(upperTile), new Rectangle((int)x, LineRunnerGlobals.UpperTileLevel - LineRunnerGlobals.TileSize, LineRunnerGlobals.TileSize, LineRunnerGlobals.TileSize), 3))
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
                            if (GraphicsHelper.PixelPerfectCollision(_player.PixelData, _player.Area.ToRectangle(), _level.GetPixelData(groundTile), new Rectangle((int)x, LineRunnerGlobals.GroundLevel - LineRunnerGlobals.TileSize, LineRunnerGlobals.TileSize, LineRunnerGlobals.TileSize), 3))
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
            _player.IsAlive = false;
            base.ScreenManager.AddScreen(new DeathScreen(this, this.Score));
        }
    }
}
