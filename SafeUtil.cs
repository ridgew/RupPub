using System.Configuration;

namespace RupPub
{
    public class SafeUtil
    {
        public static bool AutoProtectSectionExchange(string exePath, string sectionName)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(exePath);
            ConfigurationSection section = config.GetSection(sectionName);
            if (section != null)
            {
                if (section.SectionInformation.IsProtected)
                {
                    section.SectionInformation.UnprotectSection();
                }
                else
                {
                    section.SectionInformation.ProtectSection("RsaProtectedConfigurationProvider");
                }
                config.Save();
                return true;
            }
            return false;
        }
    }
}
