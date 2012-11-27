
using System;

namespace LineRunner.Model
{
    public enum TileType : byte
    {
        Air = 0,
        Block,
        Spike,
    }

    public static class TileTypeHelper
    {
        public static TileType FromCharacter(char c)
        {
            if (c == ' ')
            {
                return TileType.Air;
            }
            else if (c == '-')
            {
                return TileType.Block;
            }
            else if (c == 'I')
            {
                return TileType.Spike;
            }

            throw new ArgumentException(string.Format("{0} is not valid TileType character", c));
        }
    }
}
