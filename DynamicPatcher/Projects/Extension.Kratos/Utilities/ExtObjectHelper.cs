using System;
using System.Collections.Generic;
using System.Linq;
using DynamicPatcher;
using PatcherYRpp;
using Extension.Ext;
using Extension.INI;
using Extension.Script;
using Extension.Utilities;

namespace Extension.Utilities
{

    public static partial class ExHelper
    {

        public static TechnoExt ToTechnoExt(this IExtension extension)
        {
            return extension.Cast<TechnoExt>();
        }

        public static bool TryToTechnoExt(this IExtension extension, out TechnoExt technoExt)
        {
            return extension.TryConvert<TechnoExt>(out technoExt);
        }

        public static BulletExt ToBulletExt(this IExtension extension)
        {
            return extension.Cast<BulletExt>();
        }

        public static bool TryToBulletExt(this IExtension extension, out BulletExt bulletExt)
        {
            return extension.TryConvert<BulletExt>(out bulletExt);
        }

        public static bool TryConvert<T>(this IExtension extension, out T t) where T : IExtension
        {
            t = default;
            if (extension is T)
            {
                t = extension.Cast<T>();
                return true;
            }
            return false;
        }

        public static T Cast<T>(this IExtension extension) where T : IExtension
        {
            return (T)extension;
        }


    }

}