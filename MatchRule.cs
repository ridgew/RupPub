using System;
using System.IO;
using System.Collections.Generic;

namespace RupPub
{
    public abstract class MatchRule
    {
        public abstract bool IsMatch(FileInfo fi);
    }

    public class AfterModifyTimeRule : MatchRule
    {
        public AfterModifyTimeRule(DateTime theTime)
        {
            Time = theTime;
        }

        public DateTime Time { get; set; }

        public override bool IsMatch(FileInfo fi)
        {
            return fi.LastWriteTime > Time;
        }
    }

    public class TheTimeZoneRule : MatchRule
    {

        public TheTimeZoneRule(DateTime beginTime, DateTime endTime)
        {
            BeginTime = beginTime;
            EndTime = endTime;
        }

        public DateTime BeginTime { get; set; }

        public DateTime EndTime { get; set; }

        public override bool IsMatch(FileInfo fi)
        {
            return fi.LastWriteTime >= BeginTime && fi.LastWriteTime <= EndTime;
        }
    }

    /// <summary>
    /// 忽略的相关文件扩展名
    /// </summary>
    public class SkipFileExtRule : MatchRule
    {
        public SkipFileExtRule(string extSkipList)
        {
            if (string.IsNullOrEmpty(extSkipList))
            {
                throw new InvalidDataException("请传递有效的扩展名字符！");
            }
            skipExtArr = extSkipList.Split(new char[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries);
        }

        string[] skipExtArr = new string[0];
        /// <summary>
        /// 忽略的相关扩展名
        /// </summary>
        public string[] SkipExtensions
        {
            get { return skipExtArr; }
        }

        public override bool IsMatch(FileInfo fi)
        {
            string ext = fi.Extension;
            return !Array.Exists<string>(SkipExtensions,
                s => s.Equals(ext, StringComparison.InvariantCultureIgnoreCase));
        }
    }

    /// <summary>
    /// 获取特定文件扩展名的规则
    /// </summary>
    public class FileExtIncludeRule : MatchRule
    {
        public FileExtIncludeRule(string extIncludeList)
        {
            if (string.IsNullOrEmpty(extIncludeList))
            {
                throw new InvalidDataException("请传递有效的扩展名字符！");
            }
            includeExtArr = extIncludeList.Split(new char[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries);
        }

        string[] includeExtArr = new string[0];
        /// <summary>
        /// 必须包含的相关扩展名
        /// </summary>
        public string[] IncludeExtensions
        {
            get { return includeExtArr; }
        }

        public override bool IsMatch(FileInfo fi)
        {
            string ext = fi.Extension;
            return Array.Exists<string>(includeExtArr, s => s.Equals(ext, StringComparison.InvariantCultureIgnoreCase));
        }
    }

    public class SkipFolderRule : MatchRule
    {
        public SkipFolderRule(DirectoryInfo baseFolder, string pathSkipList)
        {
            BaseFolder = baseFolder;

            if (string.IsNullOrEmpty(pathSkipList))
            {
                throw new InvalidDataException("请传递有效的扩展名字符！");
            }
            skipPathArr = pathSkipList.Split(new char[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries);
        }

        public DirectoryInfo BaseFolder { get; set; }

        string[] skipPathArr = new string[0];
        /// <summary>
        /// 忽略的相关路径名称
        /// </summary>
        public string[] SkipPathes
        {
            get { return skipPathArr; }
        }

        public override bool IsMatch(FileInfo fi)
        {
            string fullPath = fi.DirectoryName.TrimEnd(Path.DirectorySeparatorChar);
            return !Array.Exists<string>(SkipPathes,
                s =>
                {
                    if (fullPath.EndsWith(s, StringComparison.InvariantCultureIgnoreCase))
                        return true;
                    return (fullPath.IndexOf(Path.Combine(BaseFolder.FullName, s.Replace("/", "\\").TrimStart(Path.DirectorySeparatorChar)),
                        StringComparison.InvariantCultureIgnoreCase) != -1);
                });
        }
    }

    public class SkipHideRule : MatchRule
    {
        public override bool IsMatch(FileInfo fi)
        {
            bool isHide = (fi.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden;
            //if (isHide) Console.WriteLine(fi.FullName);
            return !isHide;
        }
    }

}
