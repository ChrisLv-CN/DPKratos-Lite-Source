using Extension.Components;
using PatcherYRpp;
using System;

namespace Extension.INI
{
    /// <summary>
    /// Component used to get ini value lazily
    /// </summary>
    [Serializable]
    public class INIComponentWith<T> : INIComponent, IConfigWrapper<T> where T : INIConfig, new()
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dependency">the ini filename or denpendency chain</param>
        /// <param name="section">the section name in ini</param>
        public INIComponentWith(string dependency, string section) : base(dependency, section)
        {
        }

        public T Data
        {
            get
            {
                if (m_Config == null)
                {
                    var buffer = INIComponentManager.FindLinkedBuffer(Dependency, Section);
                    m_Config = INIComponentManager.FindConfig<T>(buffer, GetReader());
                    m_Buffer = buffer;
                }

                if (m_Buffer.Expired)
                {
                    m_Config = null;
                    return Data;
                }

                return m_Config;
            }
        }

        public override void ResetBuffer()
        {
            base.ResetBuffer();

            m_Config = null;
        }

        [NonSerialized] private T m_Config;
        [NonSerialized] private INILinkedBuffer m_Buffer;
    }

    public static class INIComponentWithHelpers
    {
        public static INIComponentWith<T> CreateAiIniComponentWith<T>(this Component component, string section) where T : INIConfig, new()
        {
            var ini = new INIComponentWith<T>(INIComponentManager.GetDependency(INIConstant.AiName), section);
            ini.AttachToComponent(component);
            return ini;
        }
        public static INIComponentWith<T> CreateArtIniComponentWith<T>(this Component component, string section) where T : INIConfig, new()
        {
            var ini = new INIComponentWith<T>(INIComponentManager.GetDependency(INIConstant.ArtName), section);
            ini.AttachToComponent(component);
            return ini;
        }
        public static INIComponentWith<T> CreateRulesIniComponentWith<T>(this Component component, string section) where T : INIConfig, new()
        {
            var ini = new INIComponentWith<T>(INIComponentManager.GetDependency(INIConstant.RulesName), section);
            ini.AttachToComponent(component);
            return ini;
        }
    }
}
