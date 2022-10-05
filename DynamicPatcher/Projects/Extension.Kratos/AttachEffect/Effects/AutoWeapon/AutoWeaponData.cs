using System;
using DynamicPatcher;
using PatcherYRpp;
using Extension.Ext;
using Extension.INI;
using Extension.Script;
using Extension.Utilities;

namespace Extension.Ext
{

    public partial class AttachEffectData
    {
        public AutoWeaponData AutoWeaponData;

        private void ReadAutoWeaponData(IConfigReader reader)
        {
            AutoWeaponData data = new AutoWeaponData(reader);
            if (data.Enable)
            {
                this.AutoWeaponData = data;
                this.Enable = true;
            }
        }
    }

    [Serializable]
    public class AutoWeaponEntity
    {
        public bool Enable;

        public int WeaponIndex; // 使用单位自身的武器
        public string[] WeaponTypes; // 武器类型
        public int RandomTypesNum; // 随机使用几个武器
        public CoordStruct FireFLH; // 开火相对位置
        public CoordStruct TargetFLH; // 目标相对位置
        public CoordStruct MoveTo; // 以开火位置为坐标0点，计算TargetFLH

        public AutoWeaponEntity()
        {
            this.Enable = false;

            this.WeaponIndex = -1;
            this.WeaponTypes = null;
            this.RandomTypesNum = 0;
            this.FireFLH = default;
            this.TargetFLH = default;
            this.MoveTo = default;
        }

        public AutoWeaponEntity Clone()
        {
            AutoWeaponEntity data = new AutoWeaponEntity();
            data.Enable = this.Enable;
            data.WeaponIndex = this.WeaponIndex;
            data.WeaponTypes = this.WeaponTypes;
            data.RandomTypesNum = this.RandomTypesNum;
            data.FireFLH = this.FireFLH;
            data.TargetFLH = this.TargetFLH;
            data.MoveTo = this.MoveTo;
            return data;
        }

        public void Read(IConfigReader reader, string title)
        {
            this.WeaponIndex = reader.Get(title + "WeaponIndex", this.WeaponIndex);
            this.WeaponTypes = reader.GetList(title + "Types", this.WeaponTypes);
            this.Enable = WeaponIndex >= 0 || (null != WeaponTypes && WeaponTypes.Length > 0);

            this.RandomTypesNum = reader.Get(title + "RandomTypesNum", this.RandomTypesNum);
            this.FireFLH = reader.Get(title + "FireFLH", this.FireFLH);
            this.TargetFLH = reader.Get(title + "TargetFLH", this.TargetFLH);
            this.MoveTo = reader.Get(title + "MoveTo", this.MoveTo);
        }

    }

    [Serializable]
    public class AutoWeaponData : EffectData
    {

        public const string TITLE = "AutoWeapon.";

        public AutoWeaponEntity Data; // 普通
        public AutoWeaponEntity EliteData; // 精英

        public bool FireOnce; // 发射后销毁
        public bool FireToTarget; // 朝附加对象的目标开火，如果附加的对象没有目标，不开火
        public bool IsOnTurret; // 相对炮塔或者身体
        public bool IsOnWorld; // 相对世界

        // 攻击者标记
        public bool IsAttackerMark; // 允许附加对象和攻击者进行交互
        public bool ReceiverAttack; // 武器由AE的接受者发射
        public bool ReceiverOwnBullet; // 武器所属是AE的接受者


        public AutoWeaponData()
        {
            this.Data = null;
            this.EliteData = null;

            this.FireOnce = false;
            this.FireToTarget = false;
            this.IsOnTurret = true;
            this.IsOnWorld = false;

            this.IsAttackerMark = false;
            this.ReceiverAttack = true;
            this.ReceiverOwnBullet = true;
        }

        public AutoWeaponData(IConfigReader reader) : this()
        {
            Read(reader);
        }

        public override void Read(IConfigReader reader)
        {
            base.Read(reader, TITLE);

            AutoWeaponEntity data = new AutoWeaponEntity();
            data.Read(reader, TITLE);
            if (data.Enable)
            {
                this.Data = data;
            }

            AutoWeaponEntity elite = null != this.Data ? Data.Clone() : new AutoWeaponEntity();
            elite.Read(reader, TITLE + "Elite");
            if (elite.Enable)
            {
                this.EliteData = elite;
            }

            this.Enable = null != this.Data || null != this.EliteData;


            this.FireOnce = reader.Get(TITLE + "FireOnce", this.FireOnce);
            this.FireToTarget = reader.Get(TITLE + "FireToTarget", this.FireOnce);
            this.IsOnTurret = reader.Get(TITLE + "IsOnTurret", this.FireOnce);
            this.IsOnWorld = reader.Get(TITLE + "IsOnWorld", this.FireOnce);

            this.IsAttackerMark = reader.Get(TITLE + "IsAttackerMark", this.FireOnce);
            this.ReceiverAttack = reader.Get(TITLE + "ReceiverAttack", this.FireOnce);
            this.ReceiverOwnBullet = reader.Get(TITLE + "ReceiverOwnBullet", this.FireOnce);
        }

    }


}
