using System;
using System.Collections.Generic;
using System.IO;

namespace RupPub
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                goto help;
            }
            else if (args.Length == 1)
            {
                if (args[0] == "?" || args[0] == "help")
                {
                    goto help;
                }
                else
                {
                    #region 当前目录的解压及创建压缩包

                    if (File.Exists(args[0]))
                    {
                        if (args[0].EndsWith(".zip"))
                        {
                            //解压到zip文件当前目录
                            ZipStorer.ExtractZipToDirWithOverrideBak(args[0], Path.GetDirectoryName(args[0]), null);
                        }
                        else if (args[0].EndsWith(".config"))
                        {
                            SafeUtil.AutoProtectSectionExchange(args[0], "appSettings");
                            SafeUtil.AutoProtectSectionExchange(args[0], "connectionStrings");
                            Console.WriteLine("已自动RSA处理配置文件！");
                        }
                    }
                    else
                    {
                        if (Directory.Exists(args[0]))
                        {
                            ZipStorer.CreateZipFromDir(Path.Combine((new DirectoryInfo(args[0])).Parent.FullName, DateTime.Now.ToString("yyyyMMddhhmmss") + ".zip"),
                                args[0], null);
                        }
                    }

                    #endregion
                    goto endProcess;
                }
            }
            else
            {
                string fDirPath = ".";
                string pDirPath = "", targetFileOrPubRulePath = "";
                bool isMultiTargetPub = false;
                List<MatchRule> ruleList = new List<MatchRule>();
                bool showInConsole = false;
                for (int i = 0, j = args.Length; i < j; i++)
                {
                    string arg = args[i];

                    if (string.Equals(arg, "/show", StringComparison.InvariantCultureIgnoreCase))
                    {
                        showInConsole = true;
                        continue;
                    }

                    if (arg == "/fdir")
                    {
                        fDirPath = (args[i + 1].Trim('"', '\''));

                        i++;
                        continue;
                    }

                    if (arg == "/pdir")
                    {
                        pDirPath = (args[i + 1].Trim('"', '\''));

                        i++;
                        continue;
                    }

                    if (arg == "/tz")
                    {
                        string[] argV = (args[i + 1].Trim('"', '\''))
                            .Split(new char[] { ',', '|' }, StringSplitOptions.RemoveEmptyEntries);
                        ruleList.Add(new TheTimeZoneRule(DateTime.Parse(argV[0]), DateTime.Parse(argV[1])));

                        i++;
                        continue;
                    }

                    if (arg == "/am")
                    {
                        string tArgV = (args[i + 1].Trim('"', '\''));
                        ruleList.Add(new AfterModifyTimeRule(DateTime.Parse(tArgV)));

                        i++;
                        continue;
                    }

                    if (arg == "/ske")
                    {
                        string sArgV = (args[i + 1].Trim('"', '\''));
                        ruleList.Add(new SkipFileExtRule(sArgV));

                        i++;
                        continue;
                    }

                    if (arg == "/ice")
                    {
                        string sArgV = (args[i + 1].Trim('"', '\''));
                        ruleList.Add(new FileExtIncludeRule(sArgV));

                        i++;
                        continue;
                    }

                    if (arg == "-h")
                    {
                        ruleList.Add(new SkipHideRule());
                        continue;
                    }

                    if (arg == "/skp" && !string.IsNullOrEmpty(fDirPath))
                    {
                        string sArgV = (args[i + 1].Trim('"', '\''));
                        ruleList.Add(new SkipFolderRule(new DirectoryInfo(fDirPath), sArgV));

                        i++;
                        continue;
                    }

                    //共用参数逻辑
                    if (arg == "/zfile" || arg == "/mtf")
                    {
                        targetFileOrPubRulePath = (args[i + 1].Trim('"', '\''));

                        i++;
                        continue;
                    }
                }

                DirectoryInfo di = new System.IO.DirectoryInfo(fDirPath);
                FileDirMaker dirMaker = new FileDirMaker(di, ruleList.ToArray());
                FileInfo[] allFiles = dirMaker.GetAllMatchFiles();
                if (showInConsole)
                {
                    (new ConsoleViewPub()).Pub(di, allFiles);
                    goto endProcess;
                }
                else
                {
                    bool pubed = false;
                    if (!string.IsNullOrEmpty(targetFileOrPubRulePath))
                    {
                        //Console.WriteLine(allFiles.Length);
                        if (isMultiTargetPub)
                        {
                            pubed = new MultiTargetPub(targetFileOrPubRulePath).Pub(di, allFiles);
                            Console.WriteLine("多目标发布完成");
                        }
                        else
                        {
                            pubed = (new ZipStorerPub(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, targetFileOrPubRulePath))).Pub(di, allFiles);
                            Console.WriteLine("Zip文件包发布完成");
                        }
                    }

                    if (!string.IsNullOrEmpty(pDirPath))
                    {
                        pubed = (new FileSystemPub(new DirectoryInfo(pDirPath))).Pub(di, allFiles);
                        Console.WriteLine("目录发布完成");
                    }

                    if (!pubed)
                    {
                        Console.WriteLine("参数处理错误，没有发布任何文件");
                    }
                    goto endProcess;
                }

            }

            /****************************
             *  /tz /am /ske /fdir /pdir /show ? help
             * **************************/
            help:
            Console.WriteLine("/tz \"2014-07-01,2014-07-15\" 时间区间，英文逗号分隔时间值。");
            Console.WriteLine("/am \"2014-07-01 08:00:00\" 在某个时间之后修改的。");
            Console.WriteLine("/ske \".pdb;.cs;.txt\" 忽略的文件扩展名列表");
            Console.WriteLine("/ice \".config;.xml\"  需要包含文件扩展名列表");
            Console.WriteLine("-h   忽略带隐藏属性的文件");
            Console.WriteLine("/skp \"obj;Reference;Libs\" 忽略的相对目录列表");
            Console.WriteLine("/zfile 生成的zip发布包文件名称");
            Console.WriteLine("/mtf  多目标发布规则文件");
            Console.WriteLine("/fdir \"获取文件目录路径\"");
            Console.WriteLine("/pdir \"发布文件目录路径\"");
            Console.WriteLine("/show 查看满足条件的所有文件");

            endProcess:
            Console.WriteLine();

        }
    }
}
