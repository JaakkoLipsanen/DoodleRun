using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.IO.IsolatedStorage;
using Flai.IO;

namespace Flai.Misc
{
    public abstract class Settings : ISettings
    {
        protected bool _needsUpdate = false;
        private bool _firstLaunch = true;

        public bool NeedsUpdate
        {
            get { return _needsUpdate; }
        }

        public bool FirstLaunch
        {
            get { return _firstLaunch; }
            set
            {
                if (_firstLaunch != value)
                {
                    _firstLaunch = value;
                    _needsUpdate = true;
                }
            }
        }

        #region IBinarySerializable Members

        public void Write(BinaryWriter writer)
        {
            this.WriteInner(writer);

            // Write FirstLaunch value, which is always false
            writer.Write(false);
            _needsUpdate = false;
        }
        protected abstract void WriteInner(BinaryWriter writer);

        public void Read(BinaryReader reader)
        {
            this.ReadInner(reader);

            // Read FirstLaunch value
            _firstLaunch = reader.ReadBoolean();
        }
        protected abstract void ReadInner(BinaryReader reader);

        #endregion

        #region ISettings Members

        public void ResetToDefault()
        {
            _firstLaunch = true;
            this.ResetToDefaultInner();
        }
        protected abstract void ResetToDefaultInner();

        #endregion
    }

    public interface ISettings : IBinarySerializable
    {
        bool NeedsUpdate { get; }
        void ResetToDefault();
    }

    public interface ISettingsManager
    {
        ISettings Settings { get; }
        void Save();
    }

    public interface ISettingsManager<T> : ISettingsManager
        where T : ISettings
    {
        new T Settings { get; }
    }

    public class SettingsManager<T> : ISettingsManager<T>
        where T : ISettings, new()
    {
        private const string FileName = "Settings.dat";
        /*  private const string TempFileName = "Settings.dat.tmp";
            private const string BackupFileName = "Settings.dat.bak"; */

        private readonly FlaiServiceContainer _services;
        private readonly IsolatedStorageFile _isolatedStorage;

        private T _settings;
        public T Settings
        {
            get { return _settings; }
        }

        ISettings ISettingsManager.Settings
        {
            get { return _settings; }
        }

        public SettingsManager(FlaiServiceContainer services)
        {
            _isolatedStorage = IsolatedStorageFile.GetUserStoreForApplication();
            _services = services;

            services.Add<ISettingsManager>(this);
            services.Add<ISettingsManager<T>>(this);
            this.LoadFromFile();
        }

        private void LoadFromFile()
        {
            try
            {
                // Check if FileName exists. If it does not and BackupFileName does exist, rename BackupFileName to FileName
                /* bool exists = _isolatedStorage.FileExists(FileName);
                 if (!exists && _isolatedStorage.FileExists(BackupFileName))
                 {
                     _isolatedStorage.MoveFile(BackupFileName, FileName);
                     exists = true;
                 } */

                if (_isolatedStorage.FileExists(FileName))
                {
                    using (IsolatedStorageFileStream stream = _isolatedStorage.OpenFile(FileName, FileMode.Open, FileAccess.Read))
                    {
                        using (BinaryReader reader = new BinaryReader(stream))
                        {
                            _settings = reader.ReadGeneric<T>();
                        }
                    }
                }
            }
            catch { }

            if (_settings == null)
            {
                _settings = new T();
                _settings.ResetToDefault();
            }

            _services.Add<T>(this.Settings);
        }

        public void Save()
        {
            if (_settings.NeedsUpdate)
            {
                try
                {
                    using (IsolatedStorageFileStream stream = _isolatedStorage.OpenFile(FileName, FileMode.Create, FileAccess.Write))
                    {
                        using (BinaryWriter writer = new BinaryWriter(stream))
                        {
                            writer.Write<T>(this.Settings);
                        }
                    }
                }
                catch { }
            }
        }

        // SAFE FILE READING/WRITING. not using atm because not sure does this work and multiple calls to IsolatedStorage can be slow, thus increasing the risk that the whole writing process fails
        /*
         * 
         * private void LoadEntityFromFile()
        {
            try
            {
                // Check if FileName exists. If it does not and BackupFileName does exist, rename BackupFileName to FileName
                bool exists = _isolatedStorage.FileExists(FileName);
                if (!exists && _isolatedStorage.FileExists(BackupFileName))
                {
                    _isolatedStorage.MoveFile(BackupFileName, FileName);
                    exists = true;
                }
 
                if (exists)
                {
                    using (IsolatedStorageFileStream stream = _isolatedStorage.OpenFile(FileName, FileMode.Open, FileAccess.Read))
                    {
                        using (BinaryReader reader = new BinaryReader(stream))
                        {
                            _settings = reader.ReadGeneric<T>();
                        }
                    }
                }
            }
            catch { }
 
            if (_settings == null)
            {
                _settings = new T();
                _settings.ResetToDefault();
            }
 
            _services.Add<T>(this.Settings);
        }
 
        public void Save()
        {
            try
            {
                // Write everything to TempFileName
                using (IsolatedStorageFileStream stream = _isolatedStorage.OpenFile(TempFileName, FileMode.Create, FileAccess.Write))
                {
                    using (BinaryWriter writer = new BinaryWriter(stream))
                    {
                        writer.Write<T>(this.Settings);
                    }
                }
 
                // If settings file already exists, rename it to BackupFileName
                if (_isolatedStorage.FileExists(FileName))
                {
                    _isolatedStorage.MoveFile(FileName, BackupFileName);
                }
 
                // Rename TempFileName, where you just wrote the settings, to FileName
                _isolatedStorage.MoveFile(TempFileName, FileName);
 
                // Delete the backup file
                _isolatedStorage.DeleteFile(BackupFileName);
 
            }
            catch
            {
            }
        }
         * 
         * */
    }
}