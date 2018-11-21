using Ccf.Ck.Libs.Logging;
using Ccf.Ck.Libs.Web.Bundling.Utils;
using System.IO;
using System.Text;

namespace Ccf.Ck.Libs.Web.Bundling.Primitives
{
    public class BundleFile
    {
        private string _ETag;
        private string _PhysicalPath;
        private Bundle _Parent;
        private FileSystemWatcher _FileWatcher;

        public BundleFile(Bundle parent)
        {
            _Parent = parent;
        }
        public string VirtualPath { get; set; }
        public string PhysicalPath
        {
            get
            {
                return _PhysicalPath;
            }
            set
            {
                _PhysicalPath = value;
                FileInfo fileInfo = new FileInfo(_PhysicalPath);
                _FileWatcher = new FileSystemWatcher(fileInfo.Directory.FullName, fileInfo.Name);
                _FileWatcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite;
                _FileWatcher.EnableRaisingEvents = true;
                _FileWatcher.IncludeSubdirectories = false;

                _FileWatcher.Renamed += FileWatcher_Renamed;
                _FileWatcher.Changed += FileWatcher_Changed;
            }
        }

        private void FileWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            FileSystemWatcher fileWatcher = sender as FileSystemWatcher;
            fileWatcher.EnableRaisingEvents = false;
            KraftLogger.LogError("FileWatcher_Changed PhysicalPath: " + PhysicalPath);
            KraftLogger.LogError("FileWatcher_Changed VirtualPath: " + VirtualPath);
            fileWatcher.Changed -= new FileSystemEventHandler(FileWatcher_Changed);
            fileWatcher.Dispose();
            _Parent?.RemoveFromCache();
        }

        private void FileWatcher_Renamed(object sender, RenamedEventArgs e)
        {
            FileSystemWatcher fileWatcher = sender as FileSystemWatcher;
            fileWatcher.EnableRaisingEvents = false;

            KraftLogger.LogError("FileWatcher_Renamed PhysicalPath: " + PhysicalPath);
            KraftLogger.LogError("FileWatcher_Renamed VirtualPath: " + VirtualPath);

            fileWatcher.Renamed -= new RenamedEventHandler(FileWatcher_Renamed);
            fileWatcher.Dispose();
            _Parent?.RemoveFromCache();
        }

        public StringBuilder Content { get; set; }
        public string ETag
        {
            get
            {
                if (Content == null || Content.Length == 0)
                {
                    //Enable this when there are no empty files (placeholders)
                    //throw new Exception($"The content for file: {PhysicalPath} was not loaded or is empty!");
                }
                _ETag = GeneralUtility.GenerateETag(Encoding.ASCII.GetBytes(Content.ToString()));
                return _ETag;
            }
        }

        internal void CleanUpEvents()
        {
            _FileWatcher.EnableRaisingEvents = false;
            _FileWatcher.Renamed -= new RenamedEventHandler(FileWatcher_Renamed);
            _FileWatcher.Changed -= new FileSystemEventHandler(FileWatcher_Changed);
            _FileWatcher.Dispose();
        }
    }
}
