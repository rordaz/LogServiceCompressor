using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace LogFilesServiceCompressor
{
    class LogArchiverTest
    {
        private string _testDir = @"C:\Test";
        private string _testFiles = @"LogFilesServiceCompressor.Test\TestFiles\cantrbry.zip";

        [SetUp]
        public void SetUp()
        {
            CleanTestDirectory();
            CopyTestFiles();
        }

        [Test, Order(1)]
        public void ShouldFindFilesInSourceFolder()
        {
            throw new NotImplementedException();
        }

        [Test, Order(2)]
        public void ShouldAddFilesToNewZipFile()
        {
            // compare the files added to the files versus the originals
            throw new NotImplementedException();
        }

        [Test, Order(3)]
        public void ShouldAddFilesToExistingZipFile()
        {
            // compare the files added to the files versus the originals
            throw new NotImplementedException();
        }

        [Test, Order(4)]
        public void ShouldDeleteOriginalFiles()
        {
            throw new NotImplementedException();
        }

        [Test, Order(5)]
        public void ShouldValidateZipFile()
        {
            throw new NotImplementedException();
        }

        private void CopyTestFiles()
        {
            TestUtilities.UnZipFiles(_testFiles, _testDir);
        }

        private void CleanTestDirectory()
        {
            if (Directory.Exists(_testDir))
                Directory.Delete(_testDir, true);
        }


    }
}
