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
        private static bool m_stop;
        private static string directoryToSearch = @"C:\Users\JosephM\Downloads";
        private static string fileToSearch = @"test";


        static void Main(string[] args)
        {
            string containingText = "";

            string[] files = Directory.GetFiles(directoryToSearch);
            DirectoryInfo dirInfo = new DirectoryInfo(directoryToSearch);

            SearchDirectory(dirInfo);

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
    }
}
