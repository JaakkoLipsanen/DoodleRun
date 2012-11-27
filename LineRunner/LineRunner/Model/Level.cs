using System;
using System.Collections.Generic;
using Flai;
using Flai.Content;
using Flai.Extensions;
using Flai.Graphics;
using Flai.Misc;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LineRunner.Model
{
    public class Level
    {
        #region Logic

        private readonly ISectorProvider _sectorProvider = new HardCodedSectorProvider();
        private readonly List<int> _sectorOrder = new List<int>();

        private int _currentSectorIndex = 0;
        private float _currentSectorX = 0f;

        public int CurrentSector
        {
            get { return _sectorOrder[_currentSectorIndex]; }
        }

        #endregion

        #region Visual

        private Color _phoneAccentColor;
        private Texture2D _groundTexture;
        private Texture2D _backgroundTexture;

        private readonly Dictionary<TileType, SpriteSheet> _tileSpriteSheets = new Dictionary<TileType, SpriteSheet>();
        private readonly Dictionary<TileType, Color[]> _tilePixelDatas = new Dictionary<TileType, Color[]>();

        #endregion

        public Level()
        {
            this.Reset();
        }

        public void LoadContent(FlaiContentManager contentManager)
        {
            _backgroundTexture = contentManager.LoadTexture("Gameplay/Background");
            _groundTexture = contentManager.LoadTexture("Gameplay/Ground");

            SpriteSheet spikeSpriteSheet = new SpriteSheet(contentManager.LoadTexture("Gameplay/Spike"), 3, 1);
            _tileSpriteSheets.Add(TileType.Spike, spikeSpriteSheet);

            SpriteSheet blockSpriteSheet = new SpriteSheet(contentManager.LoadTexture("Gameplay/Block"), 1, 1);
            _tileSpriteSheets.Add(TileType.Block, blockSpriteSheet);

            _tilePixelDatas.Add(TileType.Spike, contentManager.LoadTexture("Gameplay/SpikeCollision").GetData<Color>());
            _tilePixelDatas.Add(TileType.Block, blockSpriteSheet.Texture.GetPartialData<Color>(blockSpriteSheet.GetSourceRectangle(0, 0)));

            _phoneAccentColor = DeviceInfo.PhoneAccentColor;
        }

        #region Draw

        public void DrawBackground(GraphicsContext graphicsContext, Camera2D camera)
        {
            graphicsContext.SpriteBatch.DrawCentered(_backgroundTexture, camera.Position);
            this.DrawTiles(graphicsContext, camera);
        }

        public void Draw(GraphicsContext graphicsContext, Camera2D camera)
        {           
            this.DrawGround(graphicsContext, camera);          
        }

        private void DrawGround(GraphicsContext graphicsContext, Camera2D camera)
        {
            RectangleF cameraArea = camera.GetArea(graphicsContext.GraphicsDevice);

            float firstX = cameraArea.Left - camera.Position.X % 800;
            graphicsContext.SpriteBatch.Draw(_groundTexture, new Vector2(firstX, LineRunnerGlobals.GroundLevel - LineRunnerGlobals.GroundLevelDrawOffset), Color.White);
            graphicsContext.SpriteBatch.Draw(_groundTexture, new Vector2(firstX + 800, LineRunnerGlobals.GroundLevel - LineRunnerGlobals.GroundLevelDrawOffset), Color.White);
        }

        private void DrawTiles(GraphicsContext graphicsContext, Camera2D camera)
        {
            RectangleF cameraArea = camera.GetArea(graphicsContext.GraphicsDevice);
            this.UpdateSectors(cameraArea);

            float tempSectorX = _currentSectorX;
            int tempSectorIndex = _currentSectorIndex;

            while (tempSectorX < cameraArea.Right)
            {
                Sector currentSector = this.GetSector(tempSectorIndex);
                int sectorWidth = currentSector.Width;

                int start = 0;
                if (tempSectorX < cameraArea.Left)
                {
                    start = (int)((cameraArea.Left - tempSectorX) / LineRunnerGlobals.TileSize);
                }

                int end = sectorWidth;
                int sectorEnd = (int)(tempSectorX +  this.TileToPixels(sectorWidth)) + 1;
                if (sectorEnd > cameraArea.Right)
                {
                    end -= (int)((sectorEnd - cameraArea.Right) / LineRunnerGlobals.TileSize);
                }
                for (int x = start; x < end; x++)
                {
                    float xPosition = tempSectorX + x * LineRunnerGlobals.TileSize;

                    TileType upperTile = currentSector.GetUpperLevel(x);
                    if (upperTile != TileType.Air)
                    {
                        Rectangle rect = new Rectangle((int)Math.Round(xPosition), LineRunnerGlobals.UpperTileLevel - LineRunnerGlobals.TileSize, LineRunnerGlobals.TileSize, LineRunnerGlobals.TileSize);
                        this.DrawTile(graphicsContext, rect, _tileSpriteSheets[upperTile], _phoneAccentColor);
                    }

                    TileType groundTile = currentSector.GetGroundLevel(x);
                    if (groundTile != TileType.Air)
                    {
                        Rectangle rect = new Rectangle((int)Math.Round(xPosition), LineRunnerGlobals.GroundLevel - LineRunnerGlobals.TileSize, LineRunnerGlobals.TileSize, LineRunnerGlobals.TileSize);
                        this.DrawTile(graphicsContext, rect, _tileSpriteSheets[groundTile], Color.White);
                    }
                }

                tempSectorX += sectorWidth * LineRunnerGlobals.TileSize;
                tempSectorIndex++;
            }
        }

        private void DrawTile(GraphicsContext graphicsContext, Rectangle area, SpriteSheet spriteSheet, Color color)
        {
            // Draw random tile from the tile sprite sheet
            int x = SimplexNoise.GetPseudoRandom(area.Left, -area.Right, new RangeInt(0, spriteSheet.SheetSize.X - 1));
            graphicsContext.SpriteBatch.Draw(spriteSheet.Texture, area, spriteSheet.GetSourceRectangle(x, 0), color);
        }

        #endregion

        public TileType GetTile(Vector2 position)
        {
            if (position.X < _currentSectorX)
            {
                return TileType.Air;
            }

            // Ground level
            if (LineRunnerGlobals.GroundTileVerticalRange.Contains(position.Y))
            {
                float sectorX = _currentSectorX;
                int sectorIndex = _currentSectorIndex;
                while (sectorIndex < _sectorOrder.Count)
                {
                    Sector sector = this.GetSector(sectorIndex);
                    if (sectorX + this.TileToPixels(sector.Width) > position.X)
                    {
                        int tileIndex = (int)((position.X - sectorX) / LineRunnerGlobals.TileSize);
                        return sector.GetGroundLevel(tileIndex);
                    }

                    sectorX += this.TileToPixels(sector.Width);
                    sectorIndex++;
                }
            }
            // Upper level
            else if (LineRunnerGlobals.UpperTileVerticalRange.Contains(position.Y))
            {
                float sectorX = _currentSectorX;
                int sectorIndex = _currentSectorIndex;
                while (sectorIndex < _sectorOrder.Count)
                {
                    Sector sector = this.GetSector(sectorIndex);
                    if (sectorX + this.TileToPixels(sector.Width) > position.X)
                    {
                        int tileIndex = (int)((position.X - sectorX) / LineRunnerGlobals.TileSize);
                        return sector.GetUpperLevel(tileIndex);
                    }

                    sectorX += this.TileToPixels(sector.Width);
                    sectorIndex++;
                }
            }

            return TileType.Air;
        }

        public Color[] GetPixelData(TileType tileType)
        {
            if (tileType == TileType.Air)
            {
                throw new ArgumentException("tileType");
            }

            return _tilePixelDatas[tileType];
        }

        public void Reset()
        {
            this.ResetSectors();

            _currentSectorIndex = 0;
            _currentSectorX = 0f;
        }

        private void ResetSectors()
        {
            _sectorOrder.Clear();

            // Sector 0 is an empty sector
            _sectorOrder.Add(0);

            this.RandomizeSectors();
        }

        private void RandomizeSectors()
        {
            const int SectorCount = 100;
            const int MinimumSpaceBetweenSectors = 0;

            Random random = new Random(Global.Random.Next());
            while (_sectorOrder.Count < SectorCount)
            {
                int sector = random.Next(1, _sectorProvider.SectorCount);
                bool flag = false;
                for (int i = Math.Max(0, _sectorOrder.Count - MinimumSpaceBetweenSectors); i < _sectorOrder.Count; i++)
                {
                    if (_sectorOrder[i] == sector)
                    {
                        flag = true;
                        break;
                    }
                }

                if (!flag)
                {
                    _sectorOrder.Add(sector);
                }
            }
        }

        private void UpdateSectors(RectangleF cameraArea)
        {
            // If current sector is completely on left side of the camera, move to next sector
            int sectorWidthInPixels = this.TileToPixels(this.GetSector(_currentSectorIndex).Width * LineRunnerGlobals.TileSize);
            while (_currentSectorX + sectorWidthInPixels  < cameraArea.Left)
            {
                _currentSectorX += this.GetSector(_currentSectorIndex).Width * LineRunnerGlobals.TileSize;
                _currentSectorIndex++;
                if (_currentSectorIndex >= _sectorOrder.Count)
                {
                    _sectorOrder.Clear();
                    this.RandomizeSectors();
                }
            }

            // If current sector is too close to the end of the sector order list, update the list
            float tempSectorX = _currentSectorX;
            int tempSectorIndex = _currentSectorIndex;
            while (tempSectorX + this.GetSector(tempSectorIndex).Width * LineRunnerGlobals.TileSize < cameraArea.Right)
            {
                tempSectorIndex++;
                if (tempSectorIndex >= _sectorOrder.Count)
                {
                    // Not sure if this is right. Possibly _currentSectorIndex - 1
                    _sectorOrder.RemoveRange(0, _currentSectorIndex);
                    this.RandomizeSectors();

                    tempSectorIndex -= _currentSectorIndex;
                    _currentSectorIndex = 0;
                }

                tempSectorX += this.GetSector(tempSectorIndex).Width * LineRunnerGlobals.TileSize;
            }
        }

        private Sector GetSector(int index)
        {
            return _sectorProvider[_sectorOrder[index]];
        }

        private int TileToPixels(int tiles)
        {
            return tiles * LineRunnerGlobals.TileSize;
        }
    }
}
