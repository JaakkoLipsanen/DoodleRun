
using System;

namespace LineRunner.Model
{
    public class Sector
    {
        private TileType[] _upperLevelTiles;
        private TileType[] _groundLevelTiles;

        public int Width
        {
            get { return _upperLevelTiles.Length; }
        }

        public Sector(TileType[] upperLevelTiles, TileType[] groundLevelTiles)
        {
            if (upperLevelTiles.Length != groundLevelTiles.Length || upperLevelTiles.Length < 4)
            {
                throw new ArgumentOutOfRangeException("Length of the tile arrays is invalid");
            }

            _upperLevelTiles = upperLevelTiles;
            _groundLevelTiles = groundLevelTiles;
        }

        public TileType GetUpperLevel(int x)
        {
            return _upperLevelTiles[x];
        }

        public TileType GetGroundLevel(int x)
        {
            return _groundLevelTiles[x];
        }

        public static Sector FromString(string upperLevel, string groundLevel)
        {
            if (upperLevel.Length != groundLevel.Length || upperLevel.Length < 4)
            {
                throw new ArgumentOutOfRangeException("Length of the strings is invalid");
            }

            TileType[] upperLevelTiles = new TileType[upperLevel.Length];
            TileType[] groundLevelTiles = new TileType[groundLevel.Length];

            for (int i = 0; i < upperLevel.Length; i++)
            {
                upperLevelTiles[i] = TileTypeHelper.FromCharacter(upperLevel[i]);
                groundLevelTiles[i] = TileTypeHelper.FromCharacter(groundLevel[i]);
            }

            return new Sector(upperLevelTiles, groundLevelTiles);
        }
    }
}
