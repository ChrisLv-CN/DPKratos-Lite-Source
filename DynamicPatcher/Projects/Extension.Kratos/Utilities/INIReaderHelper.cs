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
        public static Regex Brackets = new Regex(@"(?is)(?<=\()[^\)]+(?=\))");

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

        public static double GetPercent(this ISectionReader reader, string key, double defVal, bool allowNegative = false)
        {
            double percent = defVal;
            string chanceStr = reader.Get<string>(key, null);
            if (!chanceStr.IsNullOrEmptyOrNone())
            {
                chanceStr = chanceStr.Trim();
                // 写负数等于0
                if (!allowNegative && chanceStr.IndexOf("-") > -1)
                {
                    percent = 0;
                    Logger.LogWarning($"read ini [{reader.Section}]{key} is wrong. value {chanceStr} type error.");
                }
                else
                {
                    percent = PercentStrToDouble(chanceStr);
                }
            }
            return percent;
        }

        public static double[] GetPercentList(this ISectionReader reader, string key, double[] defVal, bool isChance = false)
        {
            string[] texts = reader.GetList<string>(key);
            if (null != texts)
            {
                double[] result = new double[texts.Length];
                for (int i = 0; i < texts.Length; i++)
                {
                    double percent = PercentStrToDouble(texts[i]);
                    if (isChance)
                    {
                        if (percent < 0)
                        {
                            percent = 0;
                        }
                        else if (percent > 1)
                        {
                            percent = 1;
                        }
                    }
                    result[i] = percent;
                }
                return result;
            }
            return defVal;
        }

        public static double GetChance(this ISectionReader reader, string key, double defVal, bool allowNegative = false)
        {
            double chance = reader.GetPercent(key, defVal, allowNegative);
            if (chance < 0)
            {
                chance = 0;
            }
            else if (chance > 1)
            {
                chance = 1;
            }
            return chance;
        }

        public static double[] GetChanceList(this ISectionReader reader, string key, double[] defVal, bool isChance = false)
        {
            return reader.GetPercentList(key, defVal, true);
        }

        public static int GetDir16(this ISectionReader reader, string key, int defVal)
        {
            string dirStr = reader.Get<string>(key, null);
            if (!dirStr.IsNullOrEmptyOrNone())
            {
                if (Number.IsMatch(dirStr))
                {
                    int dir = Convert.ToInt32(dirStr);
                    if (dir > 15)
                    {
                        return dir % 16;
                    }
                }
                else if (Enum.IsDefined(typeof(Direction), dirStr))
                {
                    int dir = (int)Enum.Parse(typeof(Direction), dirStr);
                    return dir * 2;
                }
            }
            return defVal;
        }

        public static ColorStruct[] GetColorList(this ISectionReader reader, string key, ColorStruct[] defVal)
        {
            string listStr = reader.Get<string>(key, null);
            if (!listStr.IsNullOrEmptyOrNone())
            {
                List<ColorStruct> colorList = null;
                // 查找(括起来的内容)
                MatchCollection mc = Brackets.Matches(listStr);
                foreach (Match m in mc)
                {
                    string rgb = m.Value;
                    if (!rgb.IsNullOrEmptyOrNone())
                    {
                        string[] rgbs = ParserExtension.SplitValue(rgb);
                        if (null != rgbs && rgbs.Length >= 3)
                        {
                            ColorStruct color = new ColorStruct();
                            for (int i = 0; i < 3; i++)
                            {
                                int value = Convert.ToInt32(rgbs[i].Trim());
                                switch (i)
                                {
                                    case 0:
                                        color.R = (byte)value;
                                        break;
                                    case 1:
                                        color.G = (byte)value;
                                        break;
                                    case 2:
                                        color.B = (byte)value;
                                        break;
                                }
                            }
                            if (null == colorList)
                            {
                                colorList = new List<ColorStruct>();
                            }
                            colorList.Add(color);
                        }
                    }
                }
                if (null != colorList && colorList.Any())
                {
                    return colorList.ToArray();
                }
            }
            return defVal;
        }

    }

}