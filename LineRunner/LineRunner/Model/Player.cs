
using Flai;
using Flai.Architecture;
using Flai.Content;
using Flai.Graphics;
using Microsoft.Devices;
using Microsoft.Xna.Framework;
using System;

namespace LineRunner.Model
{
    public class Player : DrawableGameObject
    {
        private static readonly TimeSpan JumpRollVibrationTime = TimeSpan.FromSeconds(0.0005f);

        #region Logic

        // All of the values are in pixels
        public const int Speed = 450;
        public const int JumpPower = 600;
        public const int Gravity = 5000;

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
            _playerSprite = new MultiAnimationSprite(contentManager.LoadTexture("Gameplay/PlayerSprite"), 6, 3) { SavePixelData = true };
            _playerSprite.AddAnimation("Run", new Vector2i(0, 0), new Vector2i(5, 0), 0.05);
            _playerSprite.AddAnimation("Roll", new Vector2i(0, 1), new Vector2i(5, 1), 0.08);
            _playerSprite.AddAnimation("Float", new Vector2i(0, 2), new Vector2i(0, 2), 0.1);
            _playerSprite.AddAnimation("RunToRoll", new Vector2i(1, 2), new Vector2i(2, 2), 0.05);
            _playerSprite.AddAnimation("RollToRun", new Vector2i(3, 2), new Vector2i(4, 2), 0.05);

            _playerSprite.SetAnimation("Run", true);

            _playerSprite.AnimationEnded += this.OnSpriteAnimationEnded;

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
                        else
                        {
                            if (_playerSprite.CurrentAnimation == "Float")
                            {
                                _playerSprite.SetAnimation("Run", true);
                            }
                        }
                    }

                    _timeOnAir += updateContext.DeltaSeconds;
                }
                // If player is on ground, animate the sprite. TODO: Make running, jumping and rolling animation
                else
                {
                    _timeOnAir = 0;
                }
            }

            _playerSprite.Update(updateContext);
        }

        public void HandleInput(UpdateContext updateContext)
        {
            if (!this.IsAlive)
            {
                return;
            }

            Rectangle jumpInputRectangle = new Rectangle(400, 0, 400, 480);
            Rectangle rollInputRectangle = new Rectangle(0, 0, 400, 480);

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
                if (!_isRolling && (_playerSprite.CurrentAnimation != "Roll" && _playerSprite.CurrentAnimation != "RunToRoll"))
                {
                    this.Roll();
                }             
            }
        }

        

        #endregion

        public override void Draw(GraphicsContext graphicsContext)
        {
            graphicsContext.SpriteBatch.Draw(_playerSprite, _position - _playerSprite.FrameSize);
        }

        public void Reset()
        {
            _position = new Vector2(0, LineRunnerGlobals.GroundLevel);
            _isOnGround = true;
            _isRolling = false;
            _yVelocity = 0;

            this.IsAlive = true;

            _playerSprite.Rotation = 0;
            _playerSprite.SetAnimation("Run", true);
        }

        private void Jump()
        {
            _isOnGround = false;
            _yVelocity = -Player.JumpPower;

            _isRolling = false;
            _playerSprite.SetAnimation("Float", true);

            VibrateController.Default.Start(Player.JumpRollVibrationTime);
        }

        private void Roll()
        {
            _isRolling = true;
            if (_playerSprite.CurrentAnimation == "Run")
            {
                _playerSprite.SetAnimation("RunToRoll", false);
            }
            else
            {
                _playerSprite.SetAnimation("Roll", false);
            }

            VibrateController.Default.Start(Player.JumpRollVibrationTime);
        }

        public void Fall(UpdateContext updateContext)
        {
            _yVelocity += updateContext.DeltaSeconds * Player.Gravity * this.GravityMultiplier * 0.5f;
            _position.X += Player.Speed * updateContext.DeltaSeconds;
            _position.Y += Math.Min(750, _yVelocity) * updateContext.DeltaSeconds;

            _playerSprite.Rotation += updateContext.DeltaSeconds * 5f;

            _playerSprite.SetAnimation("Float", false);
        }

        private void OnSpriteAnimationEnded(object sender, EventArgs e)
        {
            if (_playerSprite.CurrentAnimation == "Roll")
            {
                _isRolling = false;
                _playerSprite.SetAnimation("RollToRun", false);
            }
            else if (_playerSprite.CurrentAnimation == "RunToRoll")
            {
                if (_isRolling)
                {
                    _playerSprite.SetAnimation("Roll", false);
                }
            }
            else if (_playerSprite.CurrentAnimation == "RollToRun")
            {
                _playerSprite.SetAnimation("Run", true);
            }
        }
    }
}
