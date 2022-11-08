using System;
using DynamicPatcher;
using PatcherYRpp;
using Extension.Ext;
using Extension.INI;
using Extension.Script;
using Extension.Utilities;

namespace Extension.Ext
{

    [Serializable]
    public class TechnoTypeData : INIConfig
    {
        // Ares
        public bool AllowCloakable;

        // Phobos
        public CoordStruct TurretOffset;

        public TechnoTypeData()
        {
            this.AllowCloakable = true;

            this.TurretOffset = default;
        }

        public override void Read(IConfigReader reader)
        {
            this.AllowCloakable = reader.Get("Cloakable.Allowed", this.AllowCloakable);

            string turretOffset = reader.Get<string>("TurretOffset", null);
            if (!turretOffset.IsNullOrEmptyOrNone())
            {
                if (ExHelper.Number.IsMatch(turretOffset))
                {
                    // 只有一个数字
                    this.TurretOffset.X = Convert.ToInt32(turretOffset);
                }
                else
                {
                    // 有几个数字写几个
                    string[] pos = turretOffset.Split(',');
                    if (null != pos && pos.Length > 0)
                    {
                        for (int i = 0; i < pos.Length; i++)
                        {
                            int value = Convert.ToInt32(pos[i].Trim());
                            switch (i)
                            {
                                case 0:
                                    TurretOffset.X = value;
                                    break;
                                case 1:
                                    TurretOffset.Y = value;
                                    break;
                                case 2:
                                    TurretOffset.Z = value;
                                    break;
                            }
                        }
                    }
                }
            }

        }

    }

}
