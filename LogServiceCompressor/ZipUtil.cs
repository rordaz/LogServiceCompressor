using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace LogFilesServiceCompressor
{
    public static class ZipUtil
    {
        public static void ZipFiles(string inputFolderPath, string outputPathAndFile, string password = null, bool cleanDir = false,LogArchiver logArchiver = null, bool LimitResourceToSingleCore = true)
        {
            if (logArchiver == null)
            {
                return;    
            }
            ArrayList fileList = GenerateFileListByDate(inputFolderPath, logArchiver);
            if (fileList.Count == 0 )
            {
                return;
            }

            // Detect Trailing Slash in SourceLogFiles
            int TrimLength = 0;
            if (inputFolderPath.Substring(inputFolderPath.Length - 1).Equals("\\"))
                TrimLength = inputFolderPath.Length;
            else
                TrimLength = inputFolderPath.Length + 1;
            string outPath = outputPathAndFile;

            Process[] zipProcess = Process.GetProcessesByName("LogFilesServiceCompressor");

            if (LimitResourceToSingleCore && zipProcess.Length > 0)
            {
                LogHelper.Info("LimitResourceToSingleCore Enable");
                Console.Out.WriteLine("LimitResourceToSingleCore Enabled");
                ProcessThreadCollection threads;
                //Process[] zipProcess;
                // Retrieve the zipProcess processes.
                zipProcess = Process.GetProcessesByName("LogFilesServiceCompressor");
                // Get the ProcessThread collection for the first instance
                threads = zipProcess[0].Threads;
                // Set the properties on the first ProcessThread in the collection
                threads[0].IdealProcessor = 0;
                threads[0].ProcessorAffinity = (IntPtr)1;
            }
            else {
                LogHelper.Info("LimitResourceToSingleCore Disabled");
                Console.Out.WriteLine("LimitResourceToSingleCore Disabled");
            }
            
            Console.WriteLine("Ziping Files");
            LogHelper.Info("Ziping Files");

            // Does the file exist?
            if (File.Exists(outPath))
            {
                Console.WriteLine("Zip File already exist, proceed to add files");
                LogHelper.Info("Zip File already exist, proceed to add files");

                using (ZipFile logZipFile = new ZipFile(outPath))
                {
                    logZipFile.BeginUpdate();
                    foreach (string item in fileList)
                    {
                        logZipFile.Add(item, item.Remove(0, TrimLength)); 
                    }
                    logZipFile.CommitUpdate();
                    logZipFile.Close();
                }
            }
            else
            {
                Console.WriteLine("Zip File does not exist, creating a new zip files to add logs, outpath {0}", outPath);
                LogHelper.Info("Zip File does not exist, creating a new zip file to add logs");
                FileStream ostream;
                byte[] obuffer;
                try
                {
                    string zipDirectory = outPath.Substring(0, outPath.LastIndexOf("\\"));
                    if (!Directory.Exists(zipDirectory))
                    {
                        Console.WriteLine("Missing zip file directory, creating directory per config files");
                        LogHelper.Info("Missing zip file directory, creating directory per config files");
                        Directory.CreateDirectory(zipDirectory);
                    }    
                }
                catch (Exception e)
                {
                    Console.WriteLine("Failed Zip Operation - exception: {0}", e.Message);
                    LogHelper.Info("Failed Zip Operation - exception: "+ e.Message);
                    throw;
                }
                
                
                using (ZipOutputStream oZipStream = new ZipOutputStream(File.Create(outPath)))
                {
                    Console.WriteLine("Stream open");
                    LogHelper.Info("Stream open");
                    if (password != null && password != String.Empty)
                        oZipStream.Password = password;
                    oZipStream.SetLevel(7); // maximum compression
                    ZipEntry oZipEntry;
                    foreach (string item in fileList) // for each file, generate a zipentry
                    {
                        oZipEntry = new ZipEntry(item.Remove(0, TrimLength));
                        oZipStream.PutNextEntry(oZipEntry);

                        if (!item.EndsWith(@"/")) // if a file ends with '/' its a directory
                        {
                            ostream = File.OpenRead(item);
                            obuffer = new byte[ostream.Length];
                            ostream.Read(obuffer, 0, obuffer.Length);
                            oZipStream.Write(obuffer, 0, obuffer.Length);
                        }
                    }
                    oZipStream.Finish();
                    oZipStream.Close();
                    oZipStream.Dispose();
                    Console.WriteLine("Files successfully compressed: {0} Files - on {1}", fileList.Count.ToString(), DateTime.Now.ToString());
                    LogHelper.Info("Files successfully compressed " + fileList.Count.ToString() + " - on " + DateTime.Now.ToString());
                }
            }
        }

        private static ArrayList GenerateFileList(string Dir)
        {
            ArrayList fils = new ArrayList();
            bool Empty = true;
            foreach (string file in Directory.GetFiles(Dir)) // add each file in directory
            {
                fils.Add(file);
                Empty = false;
            }

            if (Empty)
            {
                if (Directory.GetDirectories(Dir).Length == 0)
                // if directory is completely empty, add it
                {
                    fils.Add(Dir + @"/");
                }
            }

            foreach (string dirs in Directory.GetDirectories(Dir)) // recursive
            {
                foreach (object obj in GenerateFileList(dirs))
                {
                    fils.Add(obj);
                }
            }
            return fils; // return file list
        }

        private static ArrayList GenerateFileListByDate(string Dir, LogArchiver logArchiver)
        {
            // Subtract the buffer Days
            DateTime EndDate = logArchiver.RunDate.Date;
            EndDate = EndDate.AddDays(-logArchiver.BufferTimeDays);
            int CompressionDays = 1 ;
            switch (logArchiver.CompressSpan)
            {
                case LogArchiver.Periodicity.DAILY:
                    CompressionDays = 1 * logArchiver.CompressSpanFactor;
                    break;
                case LogArchiver.Periodicity.WEEKLY:
                    CompressionDays = 7 * logArchiver.CompressSpanFactor;
                    break;
                case LogArchiver.Periodicity.MONTHLY:
                    CompressionDays = DateTime.DaysInMonth(logArchiver.RunDate.Year, logArchiver.RunDate.Month);
                    break;
                default:
                    CompressionDays = 1;
                    break;
            }

            DateTime StartDate = EndDate.AddDays(-CompressionDays);
            ArrayList fils = new ArrayList();
            bool Empty = true;
            Console.WriteLine("Getting list of Files created from {0} to {1}", StartDate, EndDate);
            LogHelper.Info("Getting list of Files created from " + StartDate + " to " + EndDate);
            try
            {
                foreach (var file in Directory.GetFiles(Dir, "*", SearchOption.AllDirectories))
                {
                    DateTime FileTimeStamp = DateTime.Now;
                    if (logArchiver.UseDateModified)
                        FileTimeStamp = File.GetLastWriteTime(file);
                    else
                        FileTimeStamp = File.GetCreationTime(file);

                    if (FileTimeStamp >= StartDate && FileTimeStamp < EndDate)
                    {
                        fils.Add(file);
                        Empty = false;
                    }
                }

                if (Empty)
                {
                    Console.WriteLine("No Files were found to process");
                    LogHelper.Info("No Files were found to process");
                    return fils;
                }
                Console.WriteLine("Found {0} Files", fils.Count);
                LogHelper.Info("Found " + fils.Count + " Files");
                return fils; // return file list
            }
            catch (Exception e)
            {
                Console.WriteLine("Error while accessing source folder - Message: {0}", e.Message);
                LogHelper.Info("Error while accessing source folder - Message: " + e.Message);
                return fils;
            }
            
        }

        public static void UnZipFiles(string zipPathAndFile, string outputFolder, string password, bool deleteZipFile)
        {
            ZipInputStream s = new ZipInputStream(File.OpenRead(zipPathAndFile));
            if (password != null && password != String.Empty)
                s.Password = password;
            ZipEntry theEntry;
            string tmpEntry = String.Empty;
            while ((theEntry = s.GetNextEntry()) != null)
            {
                string directoryName = outputFolder;
                string fileName = Path.GetFileName(theEntry.Name);
                // create directory 
                if (directoryName != "")
                {
                    Directory.CreateDirectory(directoryName);
                }
                if (fileName != String.Empty)
                {
                    if (theEntry.Name.IndexOf(".ini") < 0)
                    {
                        string fullPath = directoryName + "\\" + theEntry.Name;
                        fullPath = fullPath.Replace("\\ ", "\\");
                        string fullDirPath = Path.GetDirectoryName(fullPath);
                        if (!Directory.Exists(fullDirPath)) Directory.CreateDirectory(fullDirPath);
                        FileStream streamWriter = File.Create(fullPath);
                        int size = 2048;
                        byte[] data = new byte[2048];
                        while (true)
                        {
                            size = s.Read(data, 0, data.Length);
                            if (size > 0)
                            {
                                streamWriter.Write(data, 0, size);
                            }
                            else
                            {
                                break;
                            }
                        }
                        streamWriter.Close();
                    }
                }
            }
            s.Close();
            if (deleteZipFile)
                File.Delete(zipPathAndFile);
        }

        public static void CleanDirectory(string DirectoryPath)
        {

            if (Directory.Exists(DirectoryPath))
            {
                Console.WriteLine("Removing Original fies");
                LogHelper.Info("Removing Original fies");
                try
                {
                    Directory.Delete(DirectoryPath, true);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception Cleaning Directory: " + e.Message);
                    LogHelper.Error("Exception Cleaning Directory: " + e.Message);
                }
                Console.WriteLine("Removal of Original files Successfully");
                LogHelper.Info("Removal of Original files Successfully");
            }
        }

        public static void CleanDirectory(string DirectoryPath, bool Retry)
        {
            if (Retry)
            {
                int retryMax = 5;
                const int DelayOnRetry = 1000;
                for (int i = 1; i <= retryMax; ++i)
                {
                    try
                    {
                        // Do stuff with file
                        Directory.Delete(DirectoryPath, true);
                        break; // When done we can break loop
                    }
                    catch (IOException e) when (i <= retryMax)
                    {
                        // You may check error code to filter some exceptions, not every error
                        // can be recovered.

                        Thread.Sleep(DelayOnRetry);
                    }
                }
            }
            else
            {
                CleanDirectory(DirectoryPath);
            }



        }

        public static void ClearAttributes(string DirectoryPath)
        {
            if (Directory.Exists(DirectoryPath))
            {
                File.SetAttributes(DirectoryPath, FileAttributes.Normal);

                string[] subDirs = Directory.GetDirectories(DirectoryPath);
                foreach (string dir in subDirs)
                {
                    ClearAttributes(dir);
                }

                string[] files = files = Directory.GetFiles(DirectoryPath);
                foreach (string file in files)
                {
                    File.SetAttributes(file, FileAttributes.Normal);
                }
            }
        }

        public static void DeleteOriginalFiles(ArrayList fileList) 
        {
            int deleteCount = 0;
            // Not sure about the use of this, will disable for the moment
            //System.GC.Collect();
            //System.GC.WaitForPendingFinalizers();
            foreach (string item in fileList)
            {
                if (File.Exists(item))
                {
                    try
                    {
                        File.Delete(item);
                        //Console.Out.WriteLine("Deleted File: {0}", item);
                        LogHelper.Info("File Deleted: + " + item);
                        deleteCount++;
                    }
                    catch (IOException)
                    {
                        LogHelper.Error("Couldn't delete file: " + item);
                        Console.Out.WriteLine("Couldn't delete file: " + item);
                        continue;
                    }
                }
            }
            LogHelper.Info("Count Files Deleted: " + deleteCount.ToString());
        }
        public static void DeleteOriginalFiles(string inputFolderPath, LogArchiver logArchiver = null) {

            LogHelper.Info("Original files deletion in progress");
            Console.Out.WriteLine("Original files deletion in progress");
            if (logArchiver == null)
            {
                LogHelper.Info("Original files deletion aborted");
                return;
            }
            ArrayList fileList = GenerateFileListByDate(inputFolderPath, logArchiver);
            // delete the files
            DeleteOriginalFiles(fileList);
            LogHelper.Info("Original files deletion Completed");
            Console.Out.WriteLine("Original files deletion Completed");

        }
    }
}
