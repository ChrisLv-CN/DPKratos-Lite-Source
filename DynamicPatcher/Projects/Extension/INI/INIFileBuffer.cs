using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static PatcherYRpp.INIClass;

namespace Extension.INI
{
    internal class INIFileBuffer
    {
        public INIFileBuffer(string name)
        {
            m_Name = name;

            InitCache();
        }


        public INIBuffer GetSection(string section)
        {
            if (!m_Sections.TryGetValue(section, out INIBuffer buffer))
            {
                buffer = new INIBuffer(m_Name, section);

                if (m_SectionCache.TryGetValue(section, out var pairs))
                {
                    foreach (var pair in pairs)
                    {
                        buffer.Unparsed[pair.key] = pair.value;
                    }
                }

                m_Sections[section] = buffer;
            }

            return buffer;
        }

        private void InitCache()
        {
            // read ini section
            var pINI = YRMemory.Allocate<CCINIClass>().Construct();
            var pFile = YRMemory.Allocate<CCFileClass>().Construct(m_Name);
            INIReader reader = new INIFileReader(pINI);
            pINI.Ref.ReadCCFile(pFile);
            YRMemory.Delete(pFile);

            var pSection = pINI.Ref.Sections.First;
            while (pSection.IsNotNull)
            {
                string section = pSection.Ref.Name;
                if (section != null)
                {
                    // read all pairs as <string, string> first
                    int keyCount = pINI.Ref.GetKeyCount(section);
                    List<(string key, string value)> pairs = new(keyCount);
                    for (int i = 0; i < keyCount; i++)
                    {
                        string key = pINI.Ref.GetKeyName(section, i);
                        string val = null;
                        reader.Read(section, key, ref val);
                        pairs.Add((key, val));
                    }

                    m_SectionCache[section] = pairs.ToArray();
                }

                pSection = pSection.Cast<YRNode<INISection>>().Ref.Next;
            }

            YRMemory.Delete(pINI);
        }


        private string m_Name;
        private Dictionary<string, (string key, string value)[]> m_SectionCache = new();
        private Dictionary<string, INIBuffer> m_Sections = new();
    }
}
