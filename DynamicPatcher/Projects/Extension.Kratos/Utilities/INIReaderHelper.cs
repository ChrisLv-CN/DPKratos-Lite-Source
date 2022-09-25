using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using DynamicPatcher;
using PatcherYRpp;
using Extension.Ext;
using Extension.INI;
using Extension.Script;
using Extension.Utilities;

namespace Extension.Utilities
{

    public abstract class KEnumParser<T> : IParser<T>, IParserRegister
    {
        public void Register()
        {
            Parsers.AddParser<T>(this);
        }

        public void Unregister()
        {
            Parsers.RemoveParser<T>();
        }

        public bool Parse(string val, ref T buffer)
        {
            if (!string.IsNullOrEmpty(val))
            {
                string t = val.Substring(0, 1).ToUpper();
                return ParseInitials(t, ref buffer);
            }
            return false;
        }

        public abstract bool ParseInitials(string t, ref T buffer);
    }

    public static class NoneStringCheck
    {
        public static string NotNONE(this string val)
        {
            if (val.IsNullOrEmptyOrNone())
            {
                return null;
            }
            return val;
        }

        public static bool IsNullOrEmptyOrNone(this string val)
        {
            return string.IsNullOrEmpty(val) || "none" == val.Trim().ToLower();
        }
    }

    public static partial class ExHelper
    {
        public static Regex Number = new Regex(@"^\d+$");
        public static Regex PercentFloat = new Regex(@"^\d?\.\d+$");
        public static Regex PercentNumber = new Regex(@"^\d+$");

        public static bool ReadPercent(this INIReader reader, string section, string key, ref double percent, bool allowNegative = false)
        {
            string chanceStr = null;

            if (reader.Read(section, key, ref chanceStr))
            {
                if (!string.IsNullOrEmpty(chanceStr))
                {
                    chanceStr = chanceStr.Trim();
                    // 写负数等于0
                    if (!allowNegative && chanceStr.IndexOf("-") > -1)
                    {
                        percent = 0;
                        Logger.LogWarning("read ini [{0}]{1} is wrong. value {2} type error.", section, key, chanceStr);
                        return true;
                    }
                    else
                    {
                        percent = PercentStrToDouble(chanceStr);
                        if (percent > 1)
                        {
                            percent = 1;
                        }
                        return true;
                    }
                }
            }
            return false;
        }

        public static double PercentStrToDouble(string chanceStr, double defVal = 1)
        {
            double result = defVal;

            if (PercentFloat.IsMatch(chanceStr))
            {
                // 小数格式
                result = Convert.ToDouble(chanceStr);
            }
            else if (chanceStr.IndexOf("%") > 0)
            {
                // 百分数格式
                string temp = chanceStr.Substring(0, chanceStr.IndexOf("%"));
                result = Convert.ToDouble(temp) / 100;
            }
            else if (PercentNumber.IsMatch(chanceStr))
            {
                // 数字格式
                result = Convert.ToDouble(chanceStr) / 100;
            }
            return result;
        }

        public static double[] ReadChanceList(this ISectionReader reader, string key, double[] defVal)
        {
            string[] texts = reader.GetList<string>(key);
            if (null != texts)
            {
                double[] result = new double[texts.Length];
                for (int i = 0; i < texts.Length; i++)
                {
                    result[i] = PercentStrToDouble(texts[i]);
                }
                return result;
            }
            return defVal;
        }

    }

}