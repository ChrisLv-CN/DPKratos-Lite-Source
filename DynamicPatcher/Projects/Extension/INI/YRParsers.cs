

using Extension.Utilities;
using PatcherYRpp;

namespace Extension.INI
{
    internal partial class YRParsers
    {
        public static void Register()
        {
            RegisterFindTypeParsers();

            RegisterBasicStructureParsers();
        }

        public static partial void RegisterFindTypeParsers();
        public static partial void RegisterBasicStructureParsers();
    }
}
