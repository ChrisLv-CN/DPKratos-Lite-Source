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

    public partial class TechnoStatusScript
    {

        private IConfigWrapper<CrawlingFLHData> _crawlingFLHData;
        private CrawlingFLHData crawlingFLHData
        {
            get
            {
                if (null == _crawlingFLHData)
                {
                    // 获取Image
                    string imgSection = this.section;
                    string image = Ini.GetSection(Ini.RulesDependency, imgSection).Get<string>("Image");
                    if (!string.IsNullOrEmpty(image))
                    {
                        imgSection = image;
                    }
                    _crawlingFLHData = Ini.GetConfig<CrawlingFLHData>(Ini.ArtDependency, imgSection);
                }
                return _crawlingFLHData.Data;
            }
        }

        public void OnUpdate_CrawlingFLH()
        {
            if (isInfantry && !isFearless)
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
                            primary.Ref.FLH = crawlingFLHData.ElitePrimaryCrawlingFLH;
                        }
                        if (null != secondary && !secondary.IsNull)
                        {
                            secondary.Ref.FLH = crawlingFLHData.EliteSecondaryCrawlingFLH;
                        }
                    }
                    else
                    {
                        if (null != primary && !primary.IsNull)
                        {
                            primary.Ref.FLH = crawlingFLHData.PrimaryCrawlingFLH;
                        }
                        if (null != secondary && !secondary.IsNull)
                        {
                            secondary.Ref.FLH = crawlingFLHData.SecondaryCrawlingFLH;
                        }
                    }
                }
                else
                {
                    if (isElite)
                    {
                        if (null != primary && !primary.IsNull)
                        {
                            primary.Ref.FLH = crawlingFLHData.ElitePrimaryFireFLH;
                        }
                        if (null != secondary && !secondary.IsNull)
                        {
                            secondary.Ref.FLH = crawlingFLHData.EliteSecondaryFireFLH;
                        }
                    }
                    else
                    {
                        if (null != primary && !primary.IsNull)
                        {
                            primary.Ref.FLH = crawlingFLHData.PrimaryFireFLH;
                        }
                        if (null != secondary && !secondary.IsNull)
                        {
                            secondary.Ref.FLH = crawlingFLHData.SecondaryFireFLH;
                        }
                    }
                }
            }
        }
    }
}
