using System;
using System.IO;

namespace RupPub
{
    public abstract class PackagePub
    {
        public abstract void Pub(DirectoryInfo baseDi, FileInfo[] allFiles);
    }

    public class ConsoleViewPub : PackagePub
    {
        public override void Pub(DirectoryInfo baseDi, FileInfo[] allFiles)
        {
            DirectoryInfo di = baseDi;
            foreach (FileInfo fi in allFiles)
            {
                Console.WriteLine(string.Format("{0} \t\t\t\t\t {1:yyyy-MM-dd hh:mm:ss}",
                    fi.FullName.Replace(di.FullName, ""),
                    fi.LastWriteTime).TrimStart('\\'));
            }
        }
    }

    public class FileSystemPub : PackagePub
    {
        public FileSystemPub(DirectoryInfo pubDir)
        {
            PubDirectory = pubDir;
        }

        public DirectoryInfo PubDirectory { get; set; }

        public override void Pub(DirectoryInfo baseDi, FileInfo[] allFiles)
        {
            string targetPath = null;
            int baseLen = baseDi.FullName.Length;
            foreach (FileInfo fi in allFiles)
            {
                targetPath = Path.Combine(PubDirectory.FullName,
                    fi.FullName.Substring(baseLen).TrimStart(Path.DirectorySeparatorChar));

                if (File.Exists(targetPath)) File.SetAttributes(targetPath, FileAttributes.Normal);

                string tDirPath = Path.GetDirectoryName(targetPath);
                if (!Directory.Exists(tDirPath)) Directory.CreateDirectory(tDirPath);

                fi.CopyTo(targetPath, true);
                File.SetLastWriteTime(targetPath, fi.LastWriteTime);
            }
        }
    }

    public class ZipStorerPub : PackagePub
    {
        public ZipStorerPub(string zipPath)
        {
            ZipFilePath = zipPath;
        }

        public string ZipFilePath { get; set; }

        public override void Pub(DirectoryInfo baseDi, FileInfo[] allFiles)
        {
            using (ZipStorer zip = ZipStorer.Create(ZipFilePath, ""))
            {
                int trimLen = baseDi.FullName.Length;
                foreach (FileInfo fi in allFiles)
                {
                    zip.AddFile(System.IO.ZipStorer.Compression.Deflate, fi.FullName, 
                        fi.FullName.Substring(trimLen).TrimStart(Path.DirectorySeparatorChar), "");
                }
            }
        }
    }
}
