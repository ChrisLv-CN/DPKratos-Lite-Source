using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using static PatcherYRpp.INIClass;

namespace Extension.INI
{
    public sealed class INIFileReader : INIReader
    {
        Pointer<CCINIClass> IniFile;
        byte[] readBuffer = new byte[2048];

        public INIFileReader(Pointer<CCINIClass> pINI)
        {
            IniFile = pINI;
        }

        private string GetString()
        {
            string str = Encoding.GetString(readBuffer);
            str = str.Substring(0, str.IndexOf('\0'));
            str = str.Trim();

            return str;
        }

        private int ReadBuffer(string section, string key)
        {
            return IniFile.Ref.ReadString(section, key, "", readBuffer, readBuffer.Length);
        }

        public override bool HasSection(string section)
        {
            for (var pSection = IniFile.Ref.Sections.First; pSection.IsNotNull; pSection = pSection.Cast<YRNode<INISection>>().Ref.Next)
            {
                if (section == pSection.Ref.Name)
                {
                    return true;
                }
            }

            return false;
        }

        protected override bool ReadString(string section, string key, out string str)
        {
            if (ReadBuffer(section, key) > 0)
            {
                str = GetString();
                return true;
            }

            str = null;
            return false;
        }
    }
}
