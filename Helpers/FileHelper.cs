﻿using System;
using System.IO;

namespace ABTesting.Helpers
{
    public enum TestFileType
    {
        Automatic = 0,
        Production = 1,
        Debug = 2
    }

    /// <summary>
    /// Thread-safe singleton class used to serialize reads/writes to the FairlyCertain data file.
    /// </summary>
    public sealed class FileHelper
    {
        private IFilePathProvider _filePathProvider = null;
        DateTime _lastSave = DateTime.Now;

        #region Singleton Implementation

        private static volatile FileHelper _instance;
        private static object syncRoot = new Object();
        
        private FileHelper(IFilePathProvider provider)
        {
            _filePathProvider = provider;
        }

        /// <summary>
        /// Returns an instance that automatically selects the correct file store to use.
        /// </summary>
        /// <returns></returns>
        public static FileHelper GetInstance()
        {
            lock (syncRoot)
            {
                if (_instance == null)
                {
                    return GetInstance(TestFileType.Automatic);
                }
                else
                {
                    return _instance;
                }
            }
        }

        public static FileHelper GetInstance(TestFileType fileType)
        {
            lock (syncRoot)
            {
                IFilePathProvider provider = GetProvider(fileType);
                if (_instance == null)
                {
                    _instance = new FileHelper(provider);
                }
                else
                {
                    if (!_instance._filePathProvider.GetType().Equals(provider.GetType()))
                    {
                        _instance = null;
                        _instance = new FileHelper(provider);
                    }
                }
            }

            return _instance;
        }

        #endregion


        public FileSystemWatcher GetFileWatcher()
        {
            string fullPath = _filePathProvider.GetFilePath();
            string directory = Path.GetDirectoryName(fullPath);
            string fileName = Path.GetFileName(fullPath);

            FileSystemWatcher watcher = new FileSystemWatcher(directory);
            watcher.Filter = fileName;
            watcher.NotifyFilter = NotifyFilters.LastWrite;

            return watcher;
        }
        
        private static IFilePathProvider GetProvider(TestFileType fileType)
        {
            IFilePathProvider provider = null;

            switch (fileType)
            {
                case TestFileType.Automatic:
                    provider = new FilePathProviders.AutomaticFilePathProvider();
                    break;

                case TestFileType.Production:
                    provider = new FilePathProviders.ProductionFilePathProvider();
                    break;

                case TestFileType.Debug:
                    provider = new FilePathProviders.DebugFilePathProvider();
                    break;
            }

            return provider;
        }
        
        public string FilePath
        {
            get
            {
                return _filePathProvider.GetFilePath();
            }
        }

        /// <summary>
        /// Write serialized test data to a file on the server.  Right now.
        /// </summary>
        public void Save(SerializableDictionary<string, Experiment> tests)
        {
            lock (syncRoot)
            {
                SerializationHelper.SerializeToFile(tests, _filePathProvider.GetFilePath());
            }
        }

        /// <summary>
        /// Attempt to populate ourself from a saved file.  Will leave us with in a clean, empty state if the file is missing or corrupt.
        /// </summary>
        public SerializableDictionary<string, Experiment> Load()
        {
            SerializableDictionary<string, Experiment> tests = new SerializableDictionary<string, Experiment>();

            string path = _filePathProvider.GetFilePath();
            if (File.Exists(path))
            {
                lock (syncRoot)
                {
                    if (File.Exists(path))
                    {
                        try
                        {
                            tests = (SerializableDictionary<string, Experiment>) SerializationHelper.DeSerializeFromFile(path, typeof(SerializableDictionary<string, Experiment>));
                        }
                        catch
                        {
                            // Don't sweat it if we can't parse the file.  It's not worth crashing the page load.
                        }
                    }

                    // No saved data yet (or bad xml)
                    Save(tests);
                }
            }

            return tests;
        }
    }
}
