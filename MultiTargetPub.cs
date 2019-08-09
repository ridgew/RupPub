using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace RupPub
{
    public class MultiTargetPub : PackagePub
    {
        public MultiTargetPub(string pubRulefile)
        {

        }

        public override bool Pub(DirectoryInfo baseDi, FileInfo[] allFiles)
        {
            return false;
        }
    }
}
