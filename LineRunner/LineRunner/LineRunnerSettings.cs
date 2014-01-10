using System;
using System.IO;
using Flai;
using Flai.Misc;
using Flai.Mogade;
using Mogade;

namespace LineRunner
{
    public enum ScoreLocation
    {
        Left,
        Right,
        Down,
    }

    public class LineRunnerSettings : Settings
    {
        private ScoreLocation _scoreLocation = ScoreLocation.Down;
        private bool _vibrationEnabled = true;
        private bool _highScorePostedToLeaderboards = true;
        private string _userName = "";
        private string _mogadeUserName = "";
        private int _highScore = 0;

        public string UserName
        {
            get { return _userName; }
            set
            {
                if (value == null || value.Length < 4)
                {
                    value = string.Empty;
                }
                else if (20 < value.Length)
                {
                    value = value.Substring(0, 20);
                }

                if (_userName != value)
                {
                    _userName = value;
                    _needsUpdate = true;
                }
            }
        }

        public string MogadeUserName
        {
            get { return _mogadeUserName; }
            set
            {
                if (_mogadeUserName != value)
                {
                    _mogadeUserName = value;
                    _needsUpdate = true;
                }
            }
        }

        public int HighScore
        {
            get { return _highScore; }
            set
            {
                if (value > _highScore)
                {
                    _highScore = value;
                    _needsUpdate = true;
                }
            }
        }

        public ScoreLocation ScoreLocation
        {
            get { return _scoreLocation; }
            set
            {
                if (value != _scoreLocation)
                {
                    _scoreLocation = value;
                    _needsUpdate = true;
                }
            }
        }

        public bool VibrationEnabled
        {
            get { return _vibrationEnabled; }
            set
            {
                if (value != _vibrationEnabled)
                {
                    _vibrationEnabled = value;
                    _needsUpdate = true;
                }
            }
        }

        public bool HighScorePostedToLeaderboards
        {
            get { return _highScorePostedToLeaderboards; }
            set 
            {
                if (_highScorePostedToLeaderboards != value)
                {
                    _highScorePostedToLeaderboards = value;
                    _needsUpdate = true;
                }
            }
        }

        public bool CanPostScoresToLeaderboard
        {
            get { return !string.IsNullOrEmpty(this.UserName); }
        }

        #region ISettings Members

        protected override void ResetToDefaultInner()
        {
            _userName = "";
            _mogadeUserName = "";
            _highScorePostedToLeaderboards = true;
            _highScore = 0;
            _scoreLocation = ScoreLocation.Down;
            _vibrationEnabled = true;
        }

        #endregion

        #region IBinarySerializable Members

        protected override void WriteInner(BinaryWriter writer)
        {
            writer.WriteString(_userName);
            writer.Write(_mogadeUserName);
            writer.Write(_highScore);
            writer.Write(_highScorePostedToLeaderboards);
            writer.Write((int)_scoreLocation);
            writer.Write(_vibrationEnabled);
        }

        protected override void ReadInner(BinaryReader reader)
        {
            this.UserName = reader.ReadString();
            this.MogadeUserName = reader.ReadString();
            this.HighScore = reader.ReadInt32();
            this.HighScorePostedToLeaderboards = reader.ReadBoolean();

            try
            {
                this.ScoreLocation = (ScoreLocation)reader.ReadInt32();
                this.VibrationEnabled = reader.ReadBoolean();
            }
            catch
            {
                this.ScoreLocation = ScoreLocation.Down;
                this.VibrationEnabled = true;
            }
        }

        #endregion

        public void UpdateUsername(FlaiServiceContainer services)
        {
            if (this.UserName != this.MogadeUserName)
            {
                // WHY. THE. FUCK. THIS DOESNT WORK IF mogadeManager is IMogadeManager? Fucking DLL's. Fucking fuck. FUUUUUUCK
                MogadeManager mogadeManager = services.GetService<IMogadeManager>() as MogadeManager;
                mogadeManager.Rename(this.MogadeUserName, this.UserName, (response) =>
                {
                    if (response.Success)
                    {
                        this.MogadeUserName = this.UserName;
                        this.UpdateHighscore(services);

                        services.GetService<ISettingsManager>().Save();
                    }
                });
            }
            else
            {
                this.UpdateHighscore(services);
            }
        }

        public void UpdateHighscore(FlaiServiceContainer services)
        {
            if (!this.HighScorePostedToLeaderboards && this.CanPostScoresToLeaderboard)
            {
                IMogadeManager mogadeManager = services.GetService<IMogadeManager>();
                mogadeManager.SaveScore(LineRunnerGlobals.MogadeLeaderboardId, new Score() { UserName = this.MogadeUserName, Points = this.HighScore, Dated = DateTime.Now }, (response) =>
                {
                    this.HighScorePostedToLeaderboards = response.Success;
                });
            }
        }
    }    
}