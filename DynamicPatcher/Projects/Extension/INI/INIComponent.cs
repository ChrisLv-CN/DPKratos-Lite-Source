using Extension.Utilities;
using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using Extension.EventSystems;
using Extension.Components;

namespace Extension.INI
{
    /// <summary>
    /// Component used to get ini value lazily
    /// </summary>
    [Serializable]
    public class INIComponent : Component, ISectionReader
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dependency">the ini filename or denpendency chain</param>
        /// <param name="section">the section name in ini</param>
        public INIComponent(string dependency, string section)
        {
            _dependency = dependency;
            _section = section;
        }

        public string Dependency
        {
            get => _dependency;
            set
            {
                _dependency = value;
                ResetBuffer();
            }
        }
        public string Section
        {
            get => _section;
            set
            {
                _section = value;
                ResetBuffer();
            }
        }

        /// <summary>
        /// get key value from ini
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="def"></param>
        /// <returns></returns>
        public T Get<T>(string key, T def = default, IParser<T> parser = null)
        {
            return GetReader().Get(key, def, parser);
        }

        /// <summary>
        /// get key values from ini
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="def"></param>
        /// <returns></returns>
        public T[] GetList<T>(string key, T[] def = default, IParser<T> parser = null)
        {
            return GetReader().GetList(key, def, parser);
        }

        protected INIBufferReader GetReader()
        {
            if (_ini == null)
            {
                _ini = new INIBufferReader(_dependency, _section);
            }

            return _ini;
        }

        public virtual void ResetBuffer()
        {
            _ini = null;
        }

        private string _dependency;
        private string _section;
        [NonSerialized]
        private INIBufferReader _ini;
    }

    public static class INIComponentHelpers
    {
        public static INIComponent CreateAiIniComponent(this Component component, string section)
        {
            INIComponent ini = new INIComponent(INIComponentManager.GetDependency(INIConstant.AiName), section);
            ini.AttachToComponent(component);
            return ini;
        }
        public static INIComponent CreateArtIniComponent(this Component component, string section)
        {
            INIComponent ini = new INIComponent(INIComponentManager.GetDependency(INIConstant.ArtName), section);
            ini.AttachToComponent(component);
            return ini;
        }
        public static INIComponent CreateRulesIniComponent(this Component component, string section)
        {
            INIComponent ini = new INIComponent(INIComponentManager.GetDependency(INIConstant.RulesName), section);
            ini.AttachToComponent(component);
            return ini;
        }
    }
}
