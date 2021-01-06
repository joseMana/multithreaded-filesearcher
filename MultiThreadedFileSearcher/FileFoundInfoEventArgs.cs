using System.IO;

namespace MultiThreadedFileSearcher
{
    public class FileFoundInfoEventArgs
    {
        private FileSystemInfo _info;

        public FileFoundInfoEventArgs(FileSystemInfo info)
        {
            _info = info;
        }

        public FileSystemInfo Info
        {
            get { return _info; }
        }
    }
}
