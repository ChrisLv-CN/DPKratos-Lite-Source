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
    public class CrawlingFLHData : INIConfig
    {
        public CoordStruct PrimaryFireFLH;
        public CoordStruct PrimaryCrawlingFLH;
        public CoordStruct ElitePrimaryFireFLH;
        public CoordStruct ElitePrimaryCrawlingFLH;
        public CoordStruct SecondaryFireFLH;
        public CoordStruct SecondaryCrawlingFLH;
        public CoordStruct EliteSecondaryFireFLH;
        public CoordStruct EliteSecondaryCrawlingFLH;

        public override void Read(IConfigReader reader)
        {
            this.PrimaryFireFLH = reader.Get<CoordStruct>("PrimaryFireFLH", default);
            this.ElitePrimaryFireFLH = reader.Get<CoordStruct>("ElitePrimaryFireFLH", PrimaryFireFLH);

            CoordStruct primaryCrawlingDefalut = PrimaryFireFLH;
            primaryCrawlingDefalut.Z = 20;
            this.PrimaryCrawlingFLH = reader.Get<CoordStruct>("PrimaryCrawlingFLH", primaryCrawlingDefalut);
            CoordStruct elitePrimaryCrawlingDefalut = ElitePrimaryFireFLH;
            elitePrimaryCrawlingDefalut.Z = 20;
            this.ElitePrimaryCrawlingFLH = reader.Get<CoordStruct>("ElitePrimaryCrawlingFLH", elitePrimaryCrawlingDefalut);

            this.SecondaryFireFLH = reader.Get<CoordStruct>("SecondaryFireFLH", default);
            this.EliteSecondaryFireFLH = reader.Get<CoordStruct>("EliteSecondaryFireFLH", SecondaryFireFLH);

            CoordStruct secondaryCrawlingDefalut = SecondaryFireFLH;
            secondaryCrawlingDefalut.Z = 20;
            this.SecondaryCrawlingFLH = reader.Get<CoordStruct>("SecondaryCrawlingFLH", secondaryCrawlingDefalut);
            CoordStruct eliteSecondaryCrawlingDefalut = EliteSecondaryFireFLH;
            eliteSecondaryCrawlingDefalut.Z = 20;
            this.EliteSecondaryCrawlingFLH = reader.Get<CoordStruct>("EliteSecondaryFireFLH", eliteSecondaryCrawlingDefalut);
        }

        public override string ToString()
        {
            return String.Format("{{\"PrimaryFireFLH\":{0}, \"PrimaryCrawlingFLH\":{1}, \"ElitePrimaryFireFLH\":{2}, \"ElitePrimaryCrawlingFLH\":{3}, \"SecondaryFireFLH\":{4}, \"SecondaryCrawlingFLH\":{5}, \"EliteSecondaryFireFLH\":{6}, \"EliteSecondaryCrawlingFLH\":{7}}}",
                PrimaryFireFLH, PrimaryCrawlingFLH,
                ElitePrimaryFireFLH, ElitePrimaryCrawlingFLH,
                SecondaryFireFLH, SecondaryCrawlingFLH,
                EliteSecondaryFireFLH, EliteSecondaryCrawlingFLH
            );
        }

    }


}
