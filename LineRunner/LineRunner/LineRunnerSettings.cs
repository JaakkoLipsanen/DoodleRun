using System.IO;
using Flai.Extensions;
using Flai.Misc;

namespace LineRunner
{
    public class LineRunnerSettings : Settings
    {
        private bool _highScorePostedToLeaderboards = true;
        private string _userName = "";
        private string _mogadeUserName = "";
        private bool _audioOn = false;
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

        public bool IsAudioOn
        {
            get { return _audioOn; }
            set
            {
                if (_audioOn != value)
                {
                    _audioOn = value;
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
            _audioOn = false;
        }

        #endregion

        #region IBinarySerializable Members

        protected override void WriteInner(BinaryWriter writer)
        {
            writer.WriteString(_userName);
            writer.Write(_mogadeUserName);
            writer.Write(_highScore);
            writer.Write(_highScorePostedToLeaderboards);
            writer.Write(_audioOn);
        }

        protected override void ReadInner(BinaryReader reader)
        {
            this.UserName = reader.ReadString();
            this.MogadeUserName = reader.ReadString();
            this.HighScore = reader.ReadInt32();
            this.HighScorePostedToLeaderboards = reader.ReadBoolean();
            this.IsAudioOn = reader.ReadBoolean();
        }

        #endregion
    }
}