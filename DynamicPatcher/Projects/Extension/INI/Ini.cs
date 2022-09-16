using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extension.INI
{
    /// <summary>
    /// Handy ini reading tool
    /// </summary>
    public static class Ini
    {

        /// <summary>
        /// Rules dependency chain
        /// </summary>
        public static string RulesDependency => GetDependency(INIConstant.RulesName);
        /// <summary>
        /// Art dependency chain
        /// </summary>
        public static string ArtDependency => GetDependency(INIConstant.ArtName);
        /// <summary>
        /// Ai dependency chain
        /// </summary>
        public static string AiDependency => GetDependency(INIConstant.AiName);


        /// <summary>
        /// Get dependency chain for ini
        /// </summary>
        /// <param name="iniName"></param>
        /// <returns></returns>
        public static string GetDependency(string iniName)
        {
            return INIComponentManager.GetDependency(iniName);
        }

        /// <summary>
        /// Get buffered value from ini
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dependency">Dependency chain for ini</param>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <param name="def"></param>
        /// <param name="parser"></param>
        /// <returns></returns>
        public static T Get<T>(string dependency, string section, string key, T def = default, IParser<T> parser = null)
        {
            if (INIComponentManager.FindLinkedBuffer(dependency, section).GetParsed(key, out T val, parser ?? Parsers.GetParser<T>()))
            {
                return val;
            }

            return def;
        }

        /// <summary>
        /// Get buffered values from ini
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dependency">Dependency chain for ini</param>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <param name="def"></param>
        /// <param name="parser"></param>
        /// <returns></returns>
        public static T[] GetList<T>(string dependency, string section, string key, T[] def = default, IParser<T> parser = null)
        {
            if (INIComponentManager.FindLinkedBuffer(dependency, section).GetParsedList(key, out T[] val, parser ?? Parsers.GetParser<T>()))
            {
                return val;
            }

            return def;
        }


        /// <summary>
        /// Read value from ini without buffering parsed value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dependency">Dependency chain for ini</param>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <param name="buffer"></param>
        /// <param name="parser"></param>
        /// <returns></returns>
        public static bool Read<T>(string dependency, string section, string key, ref T buffer, IParser<T> parser = null)
        {
            return new INIBufferReader(dependency, section).Read(section, key, ref buffer, parser);
        }

        /// <summary>
        /// Read values from ini without buffering parsed values
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dependency">Dependency chain for ini</param>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <param name="buffer">The buffer to fill parsed values in length <paramref name="count"/></param>
        /// <param name="count"></param>
        /// <param name="parser"></param>
        /// <returns></returns>
        public static bool ReadArray<T>(string dependency, string section, string key, ref T[] buffer, int count = -1, IParser<T> parser = null)
        {
            return new INIBufferReader(dependency, section).ReadArray(section, key, ref buffer, count, parser);
        }

        /// <summary>
        /// Read collection from ini without buffering parsed collection
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TCollection"></typeparam>
        /// <param name="dependency">Dependency chain for ini</param>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <param name="buffer"></param>
        /// <param name="parser"></param>
        /// <returns></returns>
        public static bool ReadCollection<T, TCollection>(string dependency, string section, string key, ref TCollection buffer, IParser<T> parser = null) where TCollection : ICollection<T>
        {
            return new INIBufferReader(dependency, section).ReadCollection(section, key, ref buffer, parser);
        }


        /// <summary>
        /// Get lazy reader for ini section
        /// </summary>
        /// <param name="dependency">dependency chain for ini</param>
        /// <param name="section"></param>
        /// <returns></returns>
        public static ISectionReader GetSection(string dependency, string section)
        {
            return new INIComponent(dependency, section);
        }

        /// <summary>
        /// Get lazy config wrapper for ini section
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dependency">Dependency chain for ini</param>
        /// <param name="section"></param>
        /// <returns></returns>
        public static IConfigWrapper<T> GetConfig<T>(string dependency, string section) where T : INIConfig, new()
        {
            return new INIComponentWith<T>(dependency, section);
        }

        public static void ClearBuffer()
        {
            INIComponentManager.ClearBuffer();
        }
    }
}
