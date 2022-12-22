using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.Zip;

namespace LogFilesServiceCompressor
{
    internal class TestUtilities
    {
        public static void UnZipFiles(string zipPathAndFile, string outputFolder, string password = null, bool deleteZipFile = false)
        {
            
            string testZipPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), zipPathAndFile);

            try
            {
                ZipInputStream s = new ZipInputStream(File.OpenRead(testZipPath));
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
                                    break;

                            }
                            streamWriter.Close();
                        }
                    }
                }
                s.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed UnZip Operation - exception: {0}", e.Message);
                LogHelper.Info("Failed UnZip Operation - exception: " + e.Message);
                throw;
            }
            finally { }
            
            
            if (deleteZipFile)
                File.Delete(zipPathAndFile);
        }

        public static bool CompareDirectories(ArrayList dirOriginalFiles, ArrayList dirUnpack)
        {
            bool result = false;
            //foreach (var item in dirOriginalFiles)
            //{

            //}
           result = dirOriginalFiles.Equals(dirUnpack);
            return result;
        }

        public string ComputeMD5(string file)
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
