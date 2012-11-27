
using System;
using Flai;
using Flai.Architecture;
using Flai.Content;
using Flai.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LineRunner.Model
{
    public class Player : DrawableGameObject
    {
        #region Logic

        // All of the values are in pixels
        private const int Speed = 500;
        private const int JumpPower = 550;
        private const int Gravity = 5000;

        private Vector2 _position;
        private float _yVelocity = 0;

        private bool _isOnGround = true;
        private bool _isFloating = false;
        private bool _isRolling = false;
        private bool _jumpWhenPossible = false;

        private float _timeOnAir = 0f;

        public bool IsAlive { get; set; }

        public Vector2 Position
        {
            get { return _position; }
        }      

        public RectangleF Area
        {
            get
            {
                // _position is bottom-right corner of the player
                return new RectangleF(_position.X - _playerSprite.FrameSize.X, _position.Y - _playerSprite.FrameSize.Y, _playerSprite.FrameSize.X, _playerSprite.FrameSize.Y);
            }
        }

        public Color[] PixelData
        {
            get { return _playerSprite.PixelData; }
        }

        private float GravityMultiplier
        {
            get { return _isRolling ? 2f : (_isFloating ? (0.5f) : 1.2f); }
        }

        #endregion

        #region Visual

        private MultiAnimationSprite _playerSprite;

        #endregion

        public Player()
        {        
        }

        public void LoadContent(FlaiContentManager contentManager)
        {
            _playerSprite = new MultiAnimationSprite(contentManager.LoadTexture("Gameplay/PlayerSprite"), 3, 3, 0.1) { SavePixelData = true };
            _playerSprite.AddAnimation("Run", new Vector2i(0, 0), new Vector2i(2, 0));
            _playerSprite.AddAnimation("Roll", new Vector2i(0, 1), new Vector2i(2, 2));

            _playerSprite.SetAnimation("Run", true);

            _playerSprite.AnimationEnded += new EventHandler(playerSpriteAnimationEnded);

            this.Reset();
        }

        #region Update and HandleInput

        public override void Update(UpdateContext updateContext)
        {
            if (this.IsAlive)
            {
                _position.X += updateContext.DeltaSeconds * Player.Speed;

                // If player is not in ground, handle vertical movement
                if (!_isOnGround)
                {
                    float mult = 1;
                    if (Math.Abs(_yVelocity) < 100)
                    {
                        mult = 0.6f;
                    }

                    
                    _yVelocity += updateContext.DeltaSeconds * Player.Gravity * this.GravityMultiplier * mult;
                    _position.Y += _yVelocity * updateContext.DeltaSeconds;

                    if (_position.Y > LineRunnerGlobals.GroundLevel)
                    {
                        _position.Y = LineRunnerGlobals.GroundLevel;
                        _isOnGround = true;
                        _yVelocity = 0;
                        _timeOnAir = 0;

                        if (_jumpWhenPossible)
                        {
                            _jumpWhenPossible = false;
                            this.Jump();
                        }
                    }

                    _timeOnAir += updateContext.DeltaSeconds;
                }
                // If player is on ground, animate the sprite. TODO: Make running, jumping and rolling animation
                else
                {
                    _playerSprite.Update(updateContext);
                    _timeOnAir = 0;
                }
            }
        }

        public void HandleInput(UpdateContext updateContext)
        {
            Rectangle jumpInputRectangle = new Rectangle(400, 240, 400, 240);
            Rectangle rollInputRectangle = new Rectangle(0, 240, 400, 240);

            // If player is on ground and user presses bottom-right screen, jump
            if (_isOnGround && updateContext.InputState.IsNewTouchAt(jumpInputRectangle))
            {
                this.Jump();
            }
            // If player is on air and user holds on bottom-right screen, make the player jump a bit longer
            else if (!_isOnGround)
            {
                // If user taps jump again, and the player is falling already down, set _jumpWhenPossible to true
                if (updateContext.InputState.IsNewTouchAt(jumpInputRectangle))
                {
                    if (_yVelocity > 0 && !_isFloating)
                    {
                        _jumpWhenPossible = true;
                    }
                }
                else
                {
                    _isFloating = updateContext.InputState.IsTouchAt(jumpInputRectangle);
                }
            }

            if (updateContext.InputState.IsNewTouchAt(rollInputRectangle))
            {
                if (_playerSprite.CurrentAnimation != "Roll")
                {
                    _playerSprite.SetAnimation("Roll", false);
                    _isRolling = true;
                }
            }
        }

        #endregion

        #region Draw

        public override void Draw(GraphicsContext graphicsContext)
        {
            graphicsContext.SpriteBatch.Draw(_playerSprite, _position - _playerSprite.FrameSize);
        }

        #endregion

        public void Reset()
        {
            _position = new Vector2(0, LineRunnerGlobals.GroundLevel);
            _isOnGround = true;
            _yVelocity = 0;

            this.IsAlive = true;

            _playerSprite.SetAnimation("Run", true);
        }

        private void Jump()
        {
            _isOnGround = false;
            _yVelocity = -Player.JumpPower;

            _isRolling = false;
            _playerSprite.SetAnimation("Run", true);
        }

        private void playerSpriteAnimationEnded(object sender, EventArgs e)
        {
            if (_playerSprite.CurrentAnimation == "Roll")
            {
                _isRolling = false;
                _playerSprite.SetAnimation("Run", true);
            }
        }
    }
}
