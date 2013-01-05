using Flai;
using Flai.Content;
using Flai.Extensions;
using Flai.Graphics;
using Flai.Misc;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace LineRunner.Model
{
    public class Cloud
    {
        #region Static and Const

        // Default speed is 30 units per second
        private const float DefaultSpeed = 50;
        public const int TextureCount = 4;

        private static bool TexturesLoaded = false;
        private static readonly Texture2D[] Textures = new Texture2D[Cloud.TextureCount];

        #endregion

        private Vector2 _position;
        private float _scale;
        private float _rotation;
        private int _textureIndex;

        private bool IsOnScreen
        {
            get { return _position.X + Cloud.Textures[_textureIndex].Width * _scale >= 0; }
        }

        public Cloud(int textureIndex, Vector2 position, float scale, float rotation)
        {
            _textureIndex = textureIndex;
            _position = position;
            _scale = scale;
            _rotation = rotation;
        }

        public void Update(UpdateContext updateContext)
        {
            _position.X -= Cloud.DefaultSpeed * _scale * updateContext.DeltaSeconds;
            if (!this.IsOnScreen)
            {
                this.Reinitialize();
            }
        }

        public void Draw(GraphicsContext graphicsContext)
        {
            if (_position.X + Cloud.Textures[_textureIndex].Width > 0 && _position.X < 800)
            {
                graphicsContext.SpriteBatch.Draw(Cloud.Textures[_textureIndex], _position, null, Color.White, _rotation, Vector2.Zero, _scale, SpriteEffects.None, 1f - _scale);
            }
        }

        private void Reinitialize()
        {
            _textureIndex = Global.Random.Next(0, TextureCount);
            _position = new Vector2(800 + Global.Random.Next(5, 800), Global.Random.Next(20, 100));
            _scale = Global.Random.NextFloat(0.5f, 1f);
            _rotation = Global.Random.NextFloat(0, 0.05f) - 0.025f;
        }

        public static void LoadTextures(FlaiContentManager contentManager)
        {
            if (!Cloud.TexturesLoaded)
            {
                for (int i = 0; i < TextureCount; i++)
                {
                    Cloud.Textures[i] = contentManager.LoadTexture("Gameplay/Cloud" + (i + 1));
                }
            }
        }
    }
}
