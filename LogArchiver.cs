using System;
using System.Collections.Generic;
using System.Configuration;

namespace LogFilesServiceCompressor
{
    public class LogArchiver
    {
        private int _bufferTimeDays;
        private Periodicity _compressSpan;
        private int _compressSpanFactor;
        private bool _removeOriginalFiles;
        private bool _useDateModified;
        private bool _limitResourceToSingleCore;
        public enum Periodicity
        {
            DAILY = 1,
            WEEKLY = 2,
            MONTHLY = 3
        }

        private Dictionary<string, Periodicity> compressSpanFactorDict { get; set; }
        public DateTime RunDate { get; set; }
        public Periodicity CompressSpan { get => _compressSpan; set => _compressSpan = value; }
        public int CompressSpanFactor { get => _compressSpanFactor; set => _compressSpanFactor = value; }
        public int BufferTimeDays { get => _bufferTimeDays;  set => _bufferTimeDays = value; }
        public bool RemoveOriginalFiles { get => _removeOriginalFiles; set => _removeOriginalFiles = value; }
        public bool LimitResourceToSingleCore { get => _limitResourceToSingleCore; set => _limitResourceToSingleCore = value; }
        public string SourceLogFiles { get; set; }
        public string ZipFileDirectory { get; set; }
        public bool UseDateModified { get => _useDateModified; set => _useDateModified = value; }

        public LogArchiver()
        {
            // Running Defaults
            // Read all this from the config file
            // SourceLogFiles = @"C:\apps\sample1\log";                
            // ZipFileDirectory = @"C:\apps\sample1\zips\LogZipFile.zip";
            // RunDate = new DateTime(2020, 10, 15, 0, 0, 0);
            // CompressSpan = LogCompressSpan.DAILY;
            // BufferTimeDays = 0; // days without compression
            // CompressSpanFactor = 10; 

            compressSpanFactorDict = new Dictionary<string, Periodicity> {
                {"DAILY",Periodicity.DAILY },
                {"WEEKLY",Periodicity.WEEKLY },
                {"MONTHLY",Periodicity.MONTHLY },
            };

            // this is the day that is running
            RunDate = DateTime.Now;
            #if DEBUG
            RunDate = new DateTime(2021, 04, 07, 2, 5, 0);
            #endif

            #region AppSettings
            // SourceLogFiles
            if (ConfigurationManager.AppSettings.Get("SourceLogFiles") != null && !ConfigurationManager.AppSettings.Get("SourceLogFiles").ToString().Equals(""))
                SourceLogFiles = ConfigurationManager.AppSettings.Get("SourceLogFiles").ToString();
            else
                return;

            if (ConfigurationManager.AppSettings.Get("ZipFileDestination") != null && !ConfigurationManager.AppSettings.Get("ZipFileDestination").ToString().Equals(""))
            {
                ZipFileDirectory = ConfigurationManager.AppSettings.Get("ZipFileDestination").ToString();
                ZipFileDirectory = ZipFileDirectory
                .Replace("YYYY", RunDate.Year.ToString())
                .Replace("MM", RunDate.Month.ToString("d2"));
            }
            else 
            {
                LogHelper.Error("Missing Configuration Parameter - ZipFileDestination");
                return;
            }

            // CompressSpan
            if (ConfigurationManager.AppSettings.Get("CompressSpan") != null && !ConfigurationManager.AppSettings.Get("CompressSpan").ToString().Equals(""))
                {
                    var compressSpanSetting = ConfigurationManager.AppSettings.Get("CompressSpan").ToString();
                    if (!compressSpanSetting.Equals(""))
                    {
                        compressSpanFactorDict.TryGetValue(compressSpanSetting, out _compressSpan);
                    }
                }
            else
            {
                LogHelper.Error("Missing Configuration Parameter - CompressSpan");
                return;
            }

            // BufferTimeDays
            if (ConfigurationManager.AppSettings.Get("BufferTimeDays") != null && !ConfigurationManager.AppSettings.Get("BufferTimeDays").ToString().Equals(""))
                int.TryParse(ConfigurationManager.AppSettings.Get("BufferTimeDays"), out _bufferTimeDays);
            else
            {
                LogHelper.Error("Missing Configuration Parameter - BufferTimeDays");
                return;
            }

            // CompressSpanFactor
            if (ConfigurationManager.AppSettings.Get("CompressSpanFactor") != null && !ConfigurationManager.AppSettings.Get("CompressSpanFactor").ToString().Equals(""))
                int.TryParse(ConfigurationManager.AppSettings.Get("CompressSpanFactor"), out _compressSpanFactor);
            else
            {
                LogHelper.Error("Missing Configuration Parameter - BufferTimeDays");
                return;
            }

            // RemoveOriginalFiles
            if (ConfigurationManager.AppSettings.Get("RemoveOriginalFiles") != null && !ConfigurationManager.AppSettings.Get("RemoveOriginalFiles").ToString().Equals(""))
                bool.TryParse(ConfigurationManager.AppSettings.Get("RemoveOriginalFiles"), out _removeOriginalFiles);
            else
            {
                LogHelper.Info("Missing Configuration Parameter - RemoveOriginalFiles, value will default to false");
                _removeOriginalFiles = false;
            }

            // LimitResourceToSingleCore
            if (ConfigurationManager.AppSettings.Get("LimitResourceToSingleCore") != null && !ConfigurationManager.AppSettings.Get("LimitResourceToSingleCore").ToString().Equals(""))
                bool.TryParse(ConfigurationManager.AppSettings.Get("LimitResourceToSingleCore"), out _limitResourceToSingleCore);
            else
            {
                LogHelper.Info("Missing Configuration Parameter - LimitResourceToSingleCore, value will default to true");
                _limitResourceToSingleCore = true;
            }

            // Use Last Modified time instead of created time
            // Issue observed with Windows Server 2012 (UseDateModified)
            if (ConfigurationManager.AppSettings.Get("UseDateModified") != null && !ConfigurationManager.AppSettings.Get("UseDateModified").ToString().Equals(""))
                bool.TryParse(ConfigurationManager.AppSettings.Get("UseDateModified"), out _useDateModified);
            else
            {
                LogHelper.Info("Missing Configuration Parameter - UseDateModified, value will default to false");
                _useDateModified = false;
            }
            #endregion

        }

        public bool ZipLogs()
        {
            ZipUtil.ZipFiles(SourceLogFiles, ZipFileDirectory, null,RemoveOriginalFiles,this, LimitResourceToSingleCore);
            Console.Out.WriteLine("Finished compressing files");
            return true;
        }

        //public void Start() {
        //    LogArchiver logArchiver = new LogArchiver();
        //}
        //public void Stop() {
        //    //ArrayList fileList = ZipUtil.GenerateFileListByDate(inputFolderPath, logArchiver);
        //    //ZipUtil.DeleteSourceFiles(fileList);
        //}
    }
}
