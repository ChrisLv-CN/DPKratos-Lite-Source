using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using DynamicPatcher;
using PatcherYRpp;
using Extension.Ext;
using Extension.INI;
using Extension.Utilities;

namespace Extension.Script
{

    [Serializable]
    [GlobalScriptable(typeof(TechnoExt))]
    [UpdateAfter(typeof(TechnoStatusScript))]
    public class CrawlingFLHScript : TechnoScriptable
    {
        public CrawlingFLHScript(TechnoExt owner) : base(owner) { }

        private IConfigWrapper<CrawlingFLHData> crawlingFLHData;

        public override void Awake()
        {
            if (!pTechno.CastIf<InfantryClass>(AbstractType.Infantry, out Pointer<InfantryClass> pInf)
                || pInf.Ref.Type.Ref.Fearless)
            {
                GameObject.RemoveComponent(this);
                return;
            }
            // 获取Image
            string imgSection = this.section;
            string image = Ini.GetSection(Ini.RulesDependency, imgSection).Get<string>("Image");
            if (!string.IsNullOrEmpty(image))
            {
                imgSection = image;
            }
            crawlingFLHData = Ini.GetConfig<CrawlingFLHData>(Ini.ArtDependency, imgSection);
        }

        public override void OnUpdate()
        {
            if (!pTechno.IsDeadOrInvisible())
            {
                if (null != crawlingFLHData)
                {
                    Pointer<WeaponStruct> primary = pTechno.Ref.GetWeapon(0);
                    Pointer<WeaponStruct> secondary = pTechno.Ref.GetWeapon(1);
                    // Logger.Log("CrawlingFLHData = {0}", crawlingFLHData);

                    bool isElite = pTechno.Ref.Veterancy.IsElite();
                    if (pTechno.Convert<InfantryClass>().Ref.Crawling)
                    {
                        if (isElite)
                        {
                            if (null != primary && !primary.IsNull)
                            {
                                primary.Ref.FLH = crawlingFLHData.Data.ElitePrimaryCrawlingFLH;
                            }
                            if (null != secondary && !secondary.IsNull)
                            {
                                secondary.Ref.FLH = crawlingFLHData.Data.EliteSecondaryCrawlingFLH;
                            }
                        }
                        else
                        {
                            if (null != primary && !primary.IsNull)
                            {
                                primary.Ref.FLH = crawlingFLHData.Data.PrimaryCrawlingFLH;
                            }
                            if (null != secondary && !secondary.IsNull)
                            {
                                secondary.Ref.FLH = crawlingFLHData.Data.SecondaryCrawlingFLH;
                            }
                        }
                    }
                    else
                    {
                        if (isElite)
                        {
                            if (null != primary && !primary.IsNull)
                            {
                                primary.Ref.FLH = crawlingFLHData.Data.ElitePrimaryFireFLH;
                            }
                            if (null != secondary && !secondary.IsNull)
                            {
                                secondary.Ref.FLH = crawlingFLHData.Data.EliteSecondaryFireFLH;
                            }
                        }
                        else
                        {
                            if (null != primary && !primary.IsNull)
                            {
                                primary.Ref.FLH = crawlingFLHData.Data.PrimaryFireFLH;
                            }
                            if (null != secondary && !secondary.IsNull)
                            {
                                secondary.Ref.FLH = crawlingFLHData.Data.SecondaryFireFLH;
                            }
                        }
                    }
                }
            }
        }
    }
}
