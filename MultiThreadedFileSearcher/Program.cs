using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Threading;

namespace MultiThreadedFileSearcher
{
    class Program
    {
        public delegate void FileFoundInfoEventHandler(FileFoundInfoEventArgs e);
        public static event FileFoundInfoEventHandler FoundInfo;

        private static string directoryToSearch = @"C:\Users\JosephM\";
        private static List<string> fileName = new List<string> { "details*" };
        private static string fileToSearch = @"";
        private static Byte[] containingBytes = null;
        private static bool stop;
        private static bool includeSubDirsChecked = true;

        static void Main(string[] args)
        {
            FoundInfo += new FileFoundInfoEventHandler(Searcher_FoundInfo);

            Boolean success = true;

            try
            {
                containingBytes = Encoding.ASCII.GetBytes(fileToSearch);
            }
            catch (Exception)
            {
                success = false;
            }

            if (success)
            {
                // Get the directory info for the search directory:
                DirectoryInfo dirInfo = null;
                try
                {
                    dirInfo = new DirectoryInfo(directoryToSearch);
                }
                catch
                {
                    success = false;
                }

                if (success)
                {
                    // Search the directory (maybe recursively),
                    // and raise events if something was found:
                    SearchDirectory(dirInfo);
                }
            }

            Console.ReadLine();
        }
        private static void SearchDirectory(DirectoryInfo dirInfo)
        {
            if (!stop)
            {
                try
                {
                    foreach (String fileName in fileName)
                    {
                        FileSystemInfo[] infos = dirInfo.GetFileSystemInfos(fileName);

                        foreach (FileSystemInfo info in infos)
                        {
                            if (stop)
                            {
                                break;
                            }
                            if (IsMatchingRestrictions(info))
                            {
                                // We have found a matching FileSystemInfo, so let's raise an event:
                                FoundInfo?.Invoke(new FileFoundInfoEventArgs(info));
                            }
                        }
                    }

                    if (includeSubDirsChecked)
                    {
                        DirectoryInfo[] subDirInfos = dirInfo.GetDirectories();
                        foreach (DirectoryInfo subDirInfo in subDirInfos)
                        {
                            if (stop)
                            {
                                break;
                            }

                            // Recursion:
                            new Thread(() => { SearchDirectory(subDirInfo); }).Start();
                        }
                    }
                }
                catch (Exception)
                {
                }
            }
        }
        private static Boolean IsMatchingRestrictions(FileSystemInfo info)
        {
            Boolean matches = true;

            if (matches && !string.IsNullOrEmpty(fileToSearch))
            {
                matches = false;
                if (info is FileInfo)
                {
                    matches = IsFileContainsBytes(info.FullName, containingBytes);
                }
            }

            return matches;
        }
        private static Boolean IsFileContainsBytes(String path, Byte[] compare)
        {
            Boolean contains = false;

            Int32 blockSize = 4096;
            if ((compare.Length >= 1) && (compare.Length <= blockSize))
            {
                Byte[] block = new Byte[compare.Length - 1 + blockSize];

                try
                {
                    FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);

                    // Read the first bytes from the file into "block":
                    Int32 bytesRead = fs.Read(block, 0, block.Length);

                    do
                    {
                        // Search "block" for the sequence "compare":
                        Int32 endPos = bytesRead - compare.Length + 1;
                        for (Int32 i = 0; i < endPos; i++)
                        {
                            // Read "compare.Length" bytes at position "i" from the buffer,
                            // and compare them with "compare":
                            Int32 j;
                            for (j = 0; j < compare.Length; j++)
                            {
                                if (block[i + j] != compare[j])
                                {
                                    break;
                                }
                            }

                            if (j == compare.Length)
                            {
                                // "block" contains the sequence "compare":
                                contains = true;
                                break;
                            }
                        }

                        // Search completed?
                        if (contains || (fs.Position >= fs.Length))
                        {
                            break;
                        }
                        else
                        {
                            // Copy the last "compare.Length - 1" bytes to the beginning of "block":
                            for (Int32 i = 0; i < (compare.Length - 1); i++)
                            {
                                block[i] = block[blockSize + i];
                            }

                            // Read the next "blockSize" bytes into "block":
                            bytesRead = compare.Length - 1 + fs.Read(block, compare.Length - 1, blockSize);
                        }
                    }
                    while (!stop);

                    fs.Close();
                }
                catch (Exception)
                {
                }
            }

            return contains;
        }
        private static void Searcher_FoundInfo(FileFoundInfoEventArgs e)
        {
            stop = true;
        }
    }
}
