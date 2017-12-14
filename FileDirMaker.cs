using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RupPub
{
	public class FileDirMaker
	{
		public FileDirMaker(DirectoryInfo baseDi, MatchRule[] allMatchRules)
		{
			BaseDirectoryInfo = baseDi;
			AllRules = allMatchRules;
		}

		public DirectoryInfo BaseDirectoryInfo { get; set; }

		public MatchRule[] AllRules { get; set; }

		public FileInfo[] GetAllMatchFiles()
		{
			return getMatchFileInDirectory(BaseDirectoryInfo);
		}

		bool fileInfoMatchRules(FileInfo fi)
		{
			foreach (MatchRule rule in AllRules)
			{
				if (!rule.IsMatch(fi))
					return false;
			}
			return true;
		}

		FileInfo[] getMatchFileInDirectory(DirectoryInfo di)
		{
			List<FileInfo> fileList = new List<FileInfo>();
			foreach (FileInfo fi in di.GetFiles())
			{
				if (fileInfoMatchRules(fi))
					fileList.Add(fi);
			}
			foreach (DirectoryInfo cdi in di.GetDirectories())
			{
				if ((cdi.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden
					&& Array.Exists<MatchRule>(AllRules, r => r.GetType() == typeof(SkipHideRule)))
				{
					continue;
				}

				if (AllRules.Where(r => r.ApplyForDirectory).Any(r => r.SkipDirectory(cdi)))
					continue;

				FileInfo[] cflist = getMatchFileInDirectory(cdi);
				if (cflist.Length > 0)
				{
					fileList.AddRange(cflist);
				}
			}
			return fileList.ToArray();
		}
	}
}
