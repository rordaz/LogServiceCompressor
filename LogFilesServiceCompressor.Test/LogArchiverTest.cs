using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LogFilesServiceCompressor
{
    public class LogArchiverTest
    {
        private string _testDir = @"C:\Test";
        private string _testFiles = @"TestFiles\cantrbry.zip";
        private string _zipFileDirectory = @"C:\TestZip\cantrbry.zip";
        private string _zipMd5 = "38fe67eb84dd0c08591a158d42d611f0";
        private LogArchiver _lg = new LogArchiver();

        public LogArchiverTest()
        {
            CleanTestDirectory();
            CopyTestFiles();
            InitializeLogArchiver();
        }

        [SetUp]
        public void SetUp()
        {
            
        }

        [Test, Order(1)]
        public void ShouldFindFilesInSourceFolder()
        {
            var fileList = ZipUtil.GenerateFileListByDate(_testDir, _lg);
            Assert.AreEqual(11,fileList.Count);
        }

        [Test, Order(2)]
        public void ShouldCreateZipFile()
        {
            // compare the files added to the files versus the originals
            ZipUtil.ZipFiles(_testDir, _zipFileDirectory, _lg,null, true);
            // Check Contents of the zip file
            //Assert.AreEqual(_zipMd5, ComputeMD5(_zipFileDirectory));

            Assert.AreEqual(true, File.Exists(_zipFileDirectory));

        }

        [Test, Order(5)]
        public void ShouldValidateZipFile()
        {
            TestUtilities.UnZipFiles(_testFiles, _zipFileDirectory);
        }

        //[Test, Order(3)]
        //public void ShouldAddFilesToExistingZipFile()
        //{
        //    // compare the files added to the files versus the originals
        //    throw new NotImplementedException();
        //}

        //[Test, Order(4)]
        //public void ShouldDeleteOriginalFiles()
        //{
        //    throw new NotImplementedException();
        //}

        //[Test, Order(5)]
        //public void ShouldValidateZipFile()
        //{
        //    throw new NotImplementedException();
        //}

        private void CopyTestFiles()
        {
            TestUtilities.UnZipFiles(_testFiles, _testDir);
        }

        private void CleanTestDirectory()
        {
            if (Directory.Exists(_testDir))
                Directory.Delete(_testDir, true);
        }

        private void InitializeLogArchiver()
        {
            _lg.BufferTimeDays = -1;
            _lg.CompressSpan = LogArchiver.Periodicity.DAILY;
            _lg.CompressSpanFactor = 1;
            _lg.RemoveOriginalFiles = false;
            _lg.RunDate = DateTime.Now;
            _lg.SourceLogFiles = _testFiles;
            _lg.ZipFileDirectory = _zipFileDirectory;
        }

        private string ComputeMD5(string file)
        {
            string md5Value = String.Empty;
            byte[] md5hash;
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(file))
                {
                    md5hash = md5.ComputeHash(stream);
                }
            }
            return md5Value = BitConverter.ToString(md5hash).Replace("-", String.Empty).ToLowerInvariant();
        }


    }
}
