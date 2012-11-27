using Flai;
using Flai.Advertisiments;
using Flai.Content;
using Flai.Extensions;
using Flai.Graphics;
using Flai.Mogade;
using Flai.ScreenManagement;
using Flai.ScreenManagement.Screens;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using Mogade;
using System;
using System.Collections.Generic;

// TODO: FIND NEWER MOGADE.DLL WHICH CONTAINS RENAME METHOD!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
namespace LineRunner.Screens
{
    public class LeaderboardScreen : GameScreen
    {
        #region Logic

        const int ScoresVerticalLevel = 240;
        const int ScoreSpacing = 33;

        private readonly Camera2D _camera = new Camera2D(new Vector2(400, 240));
        private readonly Button[] _scopeButtons = new Button[3];

        private Button _loadScoresButton;

        private IMogadeManager _mogadeManager;
        private MogadeScrollLeaderboard _leaderboard;

        private bool _isLoadingFirstPage = true;
        private bool _hasLoadingFailed = false;

        private Range _cameraRange = new Range(240, 240);
        private float _verticalVelocity = 0;

        private bool _isLoadingRanks = false;
        private Dictionary<LeaderboardScope, int> _ranks = new Dictionary<LeaderboardScope, int>();

        #endregion

        #region Visual

        private Texture2D _backgroundTexture;
        private Texture2D _topArrowTexture;

        private SpriteFont _titleFont;
        private SpriteFont _scoreFont;

        #endregion

        private bool ShowGotoTopButton
        {
            get
            {
                return _cameraRange.Min != _cameraRange.Max && _camera.Position.Y > _cameraRange.Min;
            }
        }

        public LeaderboardScreen()
        {
            base.EnabledGestures = GestureType.Flick | GestureType.Tap;

            this.TransitionOnTime = LineRunnerGlobals.ScreenFadeTime;
            this.TransitionOffTime = LineRunnerGlobals.ScreenFadeTime;
        }

        protected override void Activate(bool instancePreserved)
        {
            if (instancePreserved)
            {
                return;
            }

            // Initialize the mogade leaderboard
            _mogadeManager = base.Services.GetService<IMogadeManager>();
            _leaderboard = new MogadeScrollLeaderboard(_mogadeManager, LineRunnerGlobals.MogadeLeaderboardId);
            _leaderboard.LoadNextPage(response =>
            {
                _isLoadingFirstPage = false;
                this.LoadPageResponse(response);
            });

            // Load assets
            FlaiContentManager contentManager = base.ContentProvider.DefaultManager;

            _titleFont = base.FontProvider.GetFont("Crayon96");
            _scoreFont = contentManager.LoadFont("Crayon32");

            _backgroundTexture = contentManager.LoadTexture("Leaderboard/Background");
            _topArrowTexture = contentManager.LoadTexture("Leaderboard/TopArrow");

            // Initialize buttons
            this.InitializeButtons();

            // Load ranks
            this.LoadRanks();

            // Set ads disabled
            IAdManager adManager = base.Services.GetService<IAdManager>();
            adManager.Visible = false;
        }

        #region Update and Handle Input

        protected override void HandleInput(UpdateContext updateContext)
        {
            if (updateContext.InputState.IsBackButtonPressed)
            {
                LoadingScreen.Load(base.ScreenManager, false, new MainMenuScreen());
            }

            bool isScreenTouched = false;
            foreach (TouchLocation location in updateContext.InputState.TouchState)
            {
                // If the screen is being touched, reset vertical velocity to zero
                if (location.State == TouchLocationState.Moved || location.State == TouchLocationState.Pressed)
                {
                    isScreenTouched = true;
                    _verticalVelocity = 0;
                }

                // If finger is moved on the screen vertically, move the camera accordingly
                if (location.State == TouchLocationState.Moved)
                {
                    TouchLocation previousLocation;
                    if (location.TryGetPreviousLocation(out previousLocation))
                    {
                        float yDelta = location.Position.Y - previousLocation.Position.Y;
                        _camera.Position -= Vector2.UnitY * yDelta;
                    }
                }
            }

            if (!isScreenTouched)
            {
                // Check if there are new flick gestures
                foreach (GestureSample gesture in updateContext.InputState.Gestures)
                {
                    if (gesture.GestureType == GestureType.Flick)
                    {
                        _verticalVelocity = -Math.Min(4000, gesture.Delta.Y * 4);
                    }
                }

                // If the vertical velocit is not zero, update the camera position
                if (_verticalVelocity != 0)
                {
                    float velocityDecrease = _verticalVelocity * updateContext.DeltaSeconds * -10;

                    Sign oldSign = FlaiMath.GetSign(_verticalVelocity);
                    _verticalVelocity += velocityDecrease;

                    if (oldSign != FlaiMath.GetSign(_verticalVelocity))
                    {
                        _verticalVelocity = 0;
                    }

                    _camera.Position += Vector2.UnitY * _verticalVelocity * updateContext.DeltaSeconds;
                }

                // If the camera position is out of the camera range, smoothstep it to camera ranges minimum or maximum value
                if (_camera.Position.Y < _cameraRange.Min)
                {
                    _camera.Position = new Vector2(_camera.Position.X, MathHelper.SmoothStep(_camera.Position.Y, _cameraRange.Min, 20 * updateContext.DeltaSeconds));
                }
                else if (_camera.Position.Y > _cameraRange.Max)
                {
                    _camera.Position = new Vector2(_camera.Position.X, MathHelper.SmoothStep(_camera.Position.Y, _cameraRange.Max, 20 * updateContext.DeltaSeconds));
                }
            }

            // Don't let the camera position to get infintely over the camera range
            const int Elasticity = 120;
            if (_camera.Position.Y < _cameraRange.Min - Elasticity)
            {
                _camera.Position = new Vector2(_camera.Position.X, _cameraRange.Min - Elasticity);
            }
            else if (_camera.Position.Y > _cameraRange.Max + Elasticity)
            {
                _camera.Position = new Vector2(_camera.Position.X, _cameraRange.Max + Elasticity);
            }

            // Handle button input
            IGraphicsContext graphicsContext = base.Services.GetService<IGraphicsContext>();
            RectangleF cameraArea = _camera.GetArea(graphicsContext.GraphicsDevice);
            Vector2 cameraTopLeft = new Vector2(cameraArea.Left, cameraArea.Top);

            foreach (GestureSample gesture in updateContext.InputState.Gestures)
            {
                if (gesture.GestureType == GestureType.Tap)
                {
                    // Scope buttons
                    for (int i = 0; i < _scopeButtons.Length; i++)
                    {
                        if (_scopeButtons[i].HandleTap(cameraTopLeft + gesture.Position))
                        {
                            // If the button is tapped, deactivate all other buttons
                            for (int j = 0; j < _scopeButtons.Length; j++)
                            {
                                if (j != i)
                                {
                                    _scopeButtons[j].Deactivate();
                                }
                            }

                            break;
                        }
                    }

                    // Load more scores -button
                    if (!_leaderboard.IsLoading && _leaderboard.CanLoadMoreScores)
                    {
                        _loadScoresButton.HandleTap(cameraTopLeft + gesture.Position);
                    }

                    // Go to top -button 
                    if (gesture.Position.X > 720 && gesture.Position.Y > 400 && this.ShowGotoTopButton)
                    {
                        _camera.Position = new Vector2(_camera.Position.X, _cameraRange.Min);
                    }
                }
            }

            base.HandleInput(updateContext);
        }

        #endregion

        #region Draw

        protected override void Draw(GraphicsContext graphicsContext)
        {
            Color textColor = Color.Black;

            RectangleF cameraArea = _camera.GetArea(graphicsContext.GraphicsDevice);

            // Draw the background STILL BACKGROUND
            graphicsContext.SpriteBatch.Begin();
            graphicsContext.SpriteBatch.Draw(_backgroundTexture, Vector2.Zero);
            graphicsContext.SpriteBatch.End();

            graphicsContext.SpriteBatch.Begin(_camera.GetTransform(graphicsContext.GraphicsDevice));

            // Draw the background MOVING BACKGROUND
            /* float backgroundTextureFirstY = cameraArea.Top - FlaiMath.RealModulus(cameraArea.Top, 480);
               graphicsContext.SpriteBatch.Draw(_backgroundTexture, new Vector2(0, 0));
               graphicsContext.SpriteBatch.Draw(_backgroundTexture, new Vector2(0, backgroundTextureFirstY + 480)); */

            // Draw buttons
            for (int i = 0; i < _scopeButtons.Length; i++)
            {
                _scopeButtons[i].Draw(graphicsContext, textColor);
            }

            // Draw title
            graphicsContext.SpriteBatch.DrawStringCentered(base.FontProvider["Crayon64"], "Leaderboards", new Vector2(400, 60), textColor);

            // Loading
            if (_isLoadingFirstPage)
            {
                graphicsContext.SpriteBatch.DrawStringCentered(base.FontProvider["Crayon32"], "Loading", new Vector2(400, 280), textColor);
            }
            // Loading failed
            else if (_hasLoadingFailed)
            {
                graphicsContext.SpriteBatch.DrawStringCentered(_scoreFont, "Loading scores failed. Please try again later", new Vector2(400, 240), textColor);
            }
            // Loading completed succesfully
            else
            {
                SpriteFont rankFont = base.FontProvider["Crayon32"];
                Vector2 position = new Vector2(400, ScoresVerticalLevel - 40);

                // Rank
                if (_ranks.ContainsKey(_leaderboard.Scope))
                {                   
                    int rank = _ranks[_leaderboard.Scope];  
  
                    // User doesn't have rank on the leaderboard
                    if (rank == 0)
                    {
                        graphicsContext.SpriteBatch.DrawStringCentered(rankFont, "You do not have a rank on this leaderboard", position, textColor);
                    }
                    // User has a rank on the leaderboard
                    else
                    {
                        graphicsContext.SpriteBatch.DrawStringCentered(rankFont, "Your rank is #", position, textColor); // '#' doesnt work on the "DK Crayon Crumble" font!

                        Vector2 stringSize = rankFont.MeasureString("Your rank is #");
                        position += new Vector2(stringSize.X, -stringSize.Y) / 2;

                        graphicsContext.SpriteBatch.DrawString<int>(rankFont, rank, position, textColor);
                    }
                }
                else
                {
                    // Username is invalid
                    if (!this.IsUsernameValid())
                    {
                        graphicsContext.SpriteBatch.DrawStringCentered(rankFont, "Please choose a username to submit scores to leaderboards", position, textColor);
                    }
                    // If user name is fine, try to load rank again
                    else if (!_hasLoadingFailed && !_isLoadingRanks)
                    {
                        this.LoadRanks();
                    }
                }

                // Load more scores -button
                if (!_leaderboard.IsLoading && _leaderboard.CanLoadMoreScores)
                {
                    _loadScoresButton.Draw(graphicsContext, textColor);
                }

                // Draw the scores
                int start = (int)Math.Max(0, (cameraArea.Top - ScoresVerticalLevel) / ScoreSpacing);
                int end = (int)Math.Min(_leaderboard.Scores.Count, (cameraArea.Bottom - ScoresVerticalLevel) / ScoreSpacing + 2);
                for (int i = start; i < end; i++)
                {
                    Score score = _leaderboard.Scores[i];
                    graphicsContext.SpriteBatch.DrawString<int, string, string>(_scoreFont, i + 1, ". ", score.UserName, new Vector2(140, ScoresVerticalLevel + ScoreSpacing * i), textColor);
                    graphicsContext.SpriteBatch.DrawString<int>(_scoreFont, score.Points, new Vector2(640, ScoresVerticalLevel + ScoreSpacing * i), textColor);
                }
            }

            graphicsContext.SpriteBatch.End();

            // Without camera
            graphicsContext.SpriteBatch.Begin();

            // Go to top -button
            if (this.ShowGotoTopButton)
            {
                graphicsContext.SpriteBatch.DrawCentered(_topArrowTexture, new Vector2(800 - _topArrowTexture.Width / 2 - 16, 480 - _topArrowTexture.Height / 2 - 16), Color.Black * 0.4f);
            }

            graphicsContext.SpriteBatch.End();
            base.Draw(graphicsContext);
        }

        private bool IsUsernameValid()
        {
            LineRunnerSettings settings = base.GetSettings<LineRunnerSettings>();

            return settings.CanPostScoresToLeaderboard;
        }

        #endregion

        private void LoadPageResponse(Response<LeaderboardScores> response)
        {
            if (!response.Success)
            {
                _hasLoadingFailed = true;
            }
            else
            {
                _hasLoadingFailed = false;
                _cameraRange = new Range(_cameraRange.Min, Math.Max(_cameraRange.Min, ScoresVerticalLevel + ScoreSpacing * _leaderboard.Scores.Count - 120));
                _loadScoresButton.Position = new Vector2i(400, ScoresVerticalLevel + ScoreSpacing * _leaderboard.Scores.Count + 40);
            }
        }

        private void LoadRanks()
        {
            LineRunnerSettings settings = base.GetSettings<LineRunnerSettings>();

            _isLoadingRanks = true;
            _leaderboard.GetRanks(settings.MogadeUserName, new LeaderboardScope[] { LeaderboardScope.Daily, LeaderboardScope.Weekly, LeaderboardScope.Overall }, (response) =>
            {
                _isLoadingRanks = false;
                if (response.Success)
                {
                    _ranks[LeaderboardScope.Daily] = response.Data.Daily;
                    _ranks[LeaderboardScope.Weekly] = response.Data.Weekly;
                    _ranks[LeaderboardScope.Overall] = response.Data.Overall;
                }
            });

        }

        private void InitializeButtons()
        {
            // Dont use "Yesterday"-scope
            LeaderboardScope[] scopes = new LeaderboardScope[] { LeaderboardScope.Daily, LeaderboardScope.Weekly, LeaderboardScope.Overall };

            const int ButtonVerticalLevel = 140;
            const int ButtonWidth = 200;
            const int ButtonHeight = 100;

            // Initialize scope buttons
            for (int i = 0; i < 3; i++)
            {
                Vector2i center = new Vector2i(200 + i * 200, ButtonVerticalLevel);
                int buttonIndex = i;
                _scopeButtons[buttonIndex] = new Button(center, new Vector2i(ButtonWidth, ButtonHeight), scopes[buttonIndex].ToString(), () =>
                {
                    if (_leaderboard.Scope != scopes[buttonIndex])
                    {
                        _leaderboard.Scope = scopes[buttonIndex];
                        _leaderboard.LoadNextPage(this.LoadPageResponse);
                    }
                });
            }
            _scopeButtons[0].Activate();
            _loadScoresButton = new Button(Vector2i.Zero, new Vector2i(400, 100), "Load More Scores", () => _leaderboard.LoadNextPage(this.LoadPageResponse));
        }

        #region "Button" -class

        private class Button
        {
            private bool _visible = true;
            private bool _isActive = false;
            private string _text;
            private Action _action;
            private Vector2i _position;
            private Vector2i _size;

            public Rectangle Area
            {
                get { return new Rectangle(_position.X - _size.X / 2, _position.Y - _size.Y / 2, _size.X, _size.Y); }
            }

            public bool IsActive
            {
                get { return _isActive; }
            }

            public Vector2i Position
            {
                get { return _position; }
                set { _position = value; }
            }

            public bool Visible
            {
                get { return _visible; }
                set { _visible = value; }
            }

            public Button(Vector2i position, Vector2i size, string text, Action action)
            {
                _position = position;
                _size = size;
                _text = text;
                _action = action;
            }

            public void Activate()
            {
                _isActive = true;
                _action();
            }

            public void Deactivate()
            {
                _isActive = false;
            }

            public void Draw(GraphicsContext graphicsContext, Color color)
            {
                if (_visible)
                {
                    const string Font = "Crayon32";
                    graphicsContext.SpriteBatch.DrawStringCentered(graphicsContext.FontContainer[Font], _text, _position, color * (_isActive ? 1f : 0.4f));
                }
            }

            public bool HandleTap(Vector2 tapPosition)
            {
                if (this.Area.Contains(tapPosition))
                {
                    this.Activate();
                    return true;
                }

                return false;
            }
        }

        #endregion
    }
}
