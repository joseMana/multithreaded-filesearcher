using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiThreadedFileSearcher
{
    class Program
    {
        public delegate void FoundInfoEventHandler(FoundInfoEventArgs e);
        public static event FoundInfoEventHandler FoundInfo;

        private static bool m_stop;
        private static string directoryToSearch = @"C:\Users\JosephM\Downloads";
        private static string fileToSearch = @"test";
        private static Byte[] containingBytes = null;

        static void Main(string[] args)
        {
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
                catch (Exception ex)
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
            if (!m_stop)
            {
                try
                {
                    FileSystemInfo[] infos = dirInfo.GetFileSystemInfos(fileToSearch);

                    foreach (FileSystemInfo info in infos)
                    {
                        if (m_stop)
                        {
                            break;
                        }

                        if (MatchesRestrictions(info))
                        {
                            // We have found a matching FileSystemInfo, so let's raise an event:
                            if (FoundInfo != null)
                            {
                                FoundInfo(new FoundInfoEventArgs(info));
                            }
                        }
                    }

                    DirectoryInfo[] subDirInfos = dirInfo.GetDirectories();
                    foreach (DirectoryInfo subDirInfo in subDirInfos)
                    {
                        if (m_stop)
                        {
                            break;
                        }

                        // Recursion:
                        SearchDirectory(subDirInfo);
                    }
                }
                catch (Exception)
                {
                }
            }
        }
        private static Boolean MatchesRestrictions(FileSystemInfo info)
        {
            Boolean matches = true;

            if (matches)
            {
                matches = false;
                if (info is FileInfo)
                {
                    matches = FileContainsBytes(info.FullName, containingBytes);
                }
            }

            return matches;
        }

        private static Boolean FileContainsBytes(String path, Byte[] compare)
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
                    while (!m_stop);

                    fs.Close();
                }
                catch (Exception)
                {
                }
            }

            return contains;
        }
    }
    public class FoundInfoEventArgs
    {
        // ----- Variables -----

        private FileSystemInfo m_info;


        // ----- Constructor -----

        public FoundInfoEventArgs(FileSystemInfo info)
        {
            m_info = info;
        }


        // ----- Public Properties -----

        public FileSystemInfo Info
        {
            get { return m_info; }
        }
    }
}
