
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using DynamicPatcher;
using PatcherYRpp;
using PatcherYRpp.Utilities;
using Extension.Ext;
using Extension.EventSystems;
using Extension.INI;
using Extension.Script;
using Extension.Utilities;

namespace Extension.Ext
{

    public class Kratos
    {
        static Kratos()
        {
            bool disable = false;
            if (Ini.Read<bool>(Ini.GetDependency("uimd.ini"), "UISettings", "DisableKratosVersionText", ref disable))
            {
                disableVersionText = disable;
            }

            string firePath = System.IO.Directory.GetCurrentDirectory() + "\\DynamicPatcher\\antimodify";
            if (File.Exists(firePath))
            {
                using (FileStream fs = new FileStream(firePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    using (StreamReader reader = new StreamReader(fs, Encoding.UTF8))
                    {
                        antiModifyConfigJson = reader.ReadToEnd();
                    }
                }
            }
        }

        public const string Label = "Kratos";
        public const string PrivateKey = "<RSAKeyValue><Modulus>qyW2KqgmlGR3PgxsgKPbkr1dJb5IDTa3dE4IYKUwuwXnQ6PBhJRyfNqpzle/8jlfewuPbKzG+W82G2gSr886NF6lA7+G54R1WXmBfQ3ayWVEzMbpqMmVIJz4ob5SEyeDypXa6n1jXiJ3xs4pbxeDTp/xNhY+OBzXIPagM56fV80=</Modulus><Exponent>AQAB</Exponent><P>3rsWjNAO5X92O2yoIunEWlEGj3K3v92LD1/jZ4MPpT26G1KIf/tvD7gIZtiV5W2lb319X/MXKUOR0VmineReZw==</P><Q>xLYkqGPDvwWYs3W6rQW9dk4vFAoLgKSNhTa0d0o9vVXj3BKu/g3SHGeyAt0a0Z0sYDl8xAAa27UK+gTa9enPqw==</Q><DP>3MpNMk1VVv8hbwrpKaLeOEG15SFtMU8meJkvTf9H1R/5ivgnw+a1U7m06m6tOP+WTYzyadkKDqeitwrJ/wbQ5Q==</DP><DQ>odxWUZC1surB+Xj4AALnIP+4UT7qtBGyuViIKlgWcULJWX6uKzHoH6NboGua87vmlc730LddYkeYHp38JbkcPQ==</DQ><InverseQ>Ap0ZLRvQeiv0/4wg4ARLO5foWVxrR2s4YOH13QT04Oz5/ifAqf17hcps0av4NcCrmHPLdg/y+57G6Flt54lTDQ==</InverseQ><D>j1nV/GK9dbu0uO8VDqawqnccBxobJZ44CR23HKQgbfYi39/QnmjsgDFy21daedueYuSq7D7HpnOFIW2JgWKUf5uXYxihQDHWBcmYPBWy9VDHZhGRC61FmIvU+ioS/sfW17ieIyCI7QtVcoPNAp+xK/IJZN6sAl2JzrCgqIbk7Q0=</D></RSAKeyValue>";

        public static RSACryptoServiceProvider RSA = new RSACryptoServiceProvider();

        private static string version;
        public static string Version
        {
            get
            {
                if (string.IsNullOrEmpty(version))
                {
                    version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
                    string[] v = version.Split('.');
                    if (v.Length > 2)
                    {
                        version = v[2] + "." + v[3];
                    }
                }
                return version;
            }
        }

        private static bool disableVersionText;
        private static bool disableAntiModifyMod;

        private static TimerStruct showTextTimer = new TimerStruct(150);

        private static string antiModifyConfigJson = null;
        private static string defaultAntiININames = "MXtjyYKhlvoaKU26ygcEftf0hrjRe3qztELtINQ8Bek+yGGp2dRvKVy+ysR3Zd5+yfoadpbq6Z8f7A4nbvMZRGy2zSkl2wgWa903CUA2VDc1NSp4oFi5y00tTXOKwylRt8tOSvn6150KBlqqD0hPfgivLJwwoN3MOcJqIp+88Fk=";
        private static string defaultLicense = "mezJeGWA4PL+Nan91lGmqrFbw6fRvMoZgqPCwpGjjQnzvd8Uo9nfvWCeHsQdyh9/CM4FBF6J2cnYOVqsRNjqY/tWjMRHgyaEw49ovazlyGuF2mVqSD0q+ihLZIdEQHhT10EJLdU+k/UaxPKBsb/00xU55H5hbJxKW+LelFVO+lw=";
        private static int antiModifyMessageIndex = 7;
        private static TimerStruct antiModifyDelay;
        private static string[] supers = new string[]
        {
            "NukeSpecial",
            "LightningStormSpecial",
            "GeneticConverterSpecial",
            "PsychicDominatorSpecial",
            "IronCurtainSpecial"
        };
        private static GiftBoxData giftBoxData = new GiftBoxData(new string[] { "STARDUSTB" });

        public static void SendActiveMessage(object sender, EventArgs args)
        {
            string message = "Lite version " + Version + " is active, have fun.";
            MessageListClass.Instance.PrintMessage(Label, message, ColorSchemeIndex.Red, 150, true);
            EventSystem.GScreen.RemovePermanentHandler(EventSystem.GScreen.GScreenRenderEvent, SendActiveMessage);
            if (!disableAntiModifyMod && NotAllowedList())
            {
                EventSystem.GScreen.AddPermanentHandler(EventSystem.GScreen.GScreenRenderEvent, NotAllowed);
                EventSystem.GScreen.AddPermanentHandler(EventSystem.GScreen.GScreenRenderEvent, HappyMode);
            }
        }

        public static void DrawVersionText(object sender, EventArgs args)
        {
            string text = "Kratos-Lite Ver." + Version;
            RectangleStruct textRect = Drawing.GetTextDimensions(text, new Point2D(0, 0), 0, 2, 0);
            RectangleStruct sidebarRect = Surface.Sidebar.Ref.GetRect();
            int x = sidebarRect.Width / 2 - textRect.Width / 2;
            int y = sidebarRect.Height - textRect.Height;
            Point2D pos = new Point2D(x, y);

            Surface.Sidebar.Ref.DrawText(text, Pointer<Point2D>.AsPointer(ref pos), Drawing.TooltipColor);
            // Surface.Current.Ref.DrawText(text, Pointer<Point2D>.AsPointer(ref pos), Drawing.TooltipColor);
            // Surface.Primary.Ref.DrawText(text, Pointer<Point2D>.AsPointer(ref pos), Drawing.TooltipColor);

            if (disableVersionText && showTextTimer.Expired())
            {
                EventSystem.GScreen.RemovePermanentHandler(EventSystem.GScreen.SidebarRenderEvent, DrawVersionText);
            }
        }

        public static unsafe void NotAllowed(object sender, EventArgs args)
        {
            string text = "yOu ArE nOt PeRmItTeD tO mAkE a MoD oN oThEr PeOpLe'S wOrK !!!11!!!";
            RectangleStruct textRect = Drawing.GetTextDimensions(text, new Point2D(0, 0), 0, 2, 0);
            RectangleStruct rect = Surface.Current.Ref.GetRect();
            int x = rect.Width / 2 - textRect.Width / 2;
            int y = rect.Height / 2 - textRect.Height / 2;
            Point2D pos = new Point2D(x, y);
            Surface.Current.Ref.DrawText(text, Pointer<Point2D>.AsPointer(ref pos), Drawing.TooltipColor);
        }

        private static unsafe bool NotAllowedList()
        {
            bool notAllowed = true;
            try
            {
                string inis = null;
                string developer = null;
                if (!antiModifyConfigJson.IsNullOrEmptyOrNone())
                {
                    JObject jsonObject = JObject.Parse(antiModifyConfigJson);
                    inis = (string)jsonObject.GetValue("inis");
                    developer = (string)jsonObject.GetValue("developer");
                }
                if (inis.IsNullOrEmptyOrNone())
                {
                    inis = defaultAntiININames;
                }
                else if (inis == defaultAntiININames)
                {
                    return true;
                }
                byte[] bytes = Convert.FromBase64String(inis);
                RSA.FromXmlString(PrivateKey);
                byte[] code = RSA.Decrypt(bytes, false);
                string realNames = Encoding.UTF8.GetString(code);
                string[] iniNames = realNames.Split(',');
                notAllowed = iniNames == null || iniNames.Length < 1 || iniNames.Contains(CCINIClass.INI_Ruels_FileName.ToString().ToLower());
                if (notAllowed && !developer.IsNullOrEmptyOrNone())
                {
                    if (developer == defaultLicense)
                    {
                        return true;
                    }
                    byte[] bytes1 = Convert.FromBase64String(developer);
                    byte[] code1 = RSA.Decrypt(bytes1, false);
                    string realID = Encoding.UTF8.GetString(code1);
                    byte[] bytes2 = Convert.FromBase64String(defaultLicense);
                    byte[] code2 = RSA.Decrypt(bytes2, false);
                    string realLicense = Encoding.UTF8.GetString(code2);
                    notAllowed = realID != realLicense;
                }
            }
            catch (Exception e)
            {
                Logger.PrintException(e);
            }
            return notAllowed;
        }

        public static unsafe void HappyMode(object sender, EventArgs args)
        {
            string message;
            switch (antiModifyMessageIndex)
            {
                case 7:
                    message = "Detected that you are modifying \"Mental Omega\" without authorization.";
                    VocClass.Speak("EVA_NuclearSiloDetected");
                    break;
                case 6:
                    message = "Self-Destruction countdown...";
                    VocClass.Speak("Mis_A12_EvaCountdown");
                    break;
                case 5:
                    message = antiModifyMessageIndex.ToString();
                    int nukeSiren = VocClass.FindIndex("NukeSiren");
                    if (nukeSiren > -1)
                    {
                        VocClass.PlayGlobal(nukeSiren, 0x2000, 1.0f);
                    }
                    break;
                default:
                    message = antiModifyMessageIndex.ToString();
                    break;
            }
            if (antiModifyDelay.Expired())
            {
                antiModifyDelay.Start(90);
                if (antiModifyMessageIndex > 0)
                {
                    MessageListClass.Instance.PrintMessage(Label, message, ColorSchemeIndex.Red, 450, true);
                }
                if (antiModifyMessageIndex == 0)
                {
                    MessageListClass.Instance.PrintMessage(Label, "Happy Mode Active!!!", ColorSchemeIndex.Red, -1, true);
                }
                antiModifyMessageIndex--;
            }
            if (antiModifyMessageIndex < 0 && 0.005d.Bingo())
            {
                // var func = (delegate* unmanaged[Thiscall]<int, IntPtr, void>)ASM.FastCallTransferStation;
                // func(0x7DC720, IntPtr.Zero);
                HouseClass.Array.FindIndex((pHouse, i) =>
                {
                    if (!pHouse.IsNull && pHouse.Ref.ControlledByPlayer())
                    {
                        Pointer<TechnoClass> pTarget = pHouse.GetTechnoRandom();
                        if (!pTarget.IsNull)
                        {
                            int typeIndex = MathEx.Random.Next(5);
                            CoordStruct location = pTarget.Ref.Base.Base.GetCoords();
                            switch (typeIndex)
                            {
                                case 0:
                                    int idx = MathEx.Random.Next(supers.Count());
                                    FireSuperEntity superEntity = new FireSuperEntity();
                                    superEntity.Supers = new string[] { supers[idx] };
                                    FireSuperManager.Launch(pHouse, location, superEntity);
                                    break;
                                case 1:
                                    pTarget.Ref.FirepowerMultiplier = 4.0;
                                    pTarget.Ref.Berzerk = true;
                                    pTarget.Ref.BerzerkDurationLeft = 750;
                                    if (pTarget.CastToFoot(out var pFoot))
                                    {
                                        pTarget.Convert<MissionClass>().Ref.ForceMission(Mission.Hunt);
                                    }
                                    break;
                                case 2:
                                    pTarget.Ref.EMPLockRemaining = 450;
                                    Pointer<AnimTypeClass> pSparkles = RulesClass.Global().EMPulseSparkles;
                                    if (!pSparkles.IsNull)
                                    {
                                        Pointer<AnimClass> pAnim = YRMemory.Create<AnimClass>(pSparkles, pTarget.Ref.Base.Location);
                                        pAnim.Ref.Loops = 0xFF;
                                        pAnim.Ref.SetOwnerObject(pTarget.Convert<ObjectClass>());
                                        if (pTarget.Ref.Base.Base.WhatAmI() == AbstractType.Building)
                                        {
                                            pAnim.Ref.ZAdjust = -1024;
                                        }
                                        if (pTarget.TryGetStatus(out var paint))
                                        {
                                            paint.pExtraSparkleAnim.Pointer = pAnim;
                                        }
                                    }
                                    break;
                                case 3:
                                    if (pTarget.TryGetStatus(out var gift))
                                    {
                                        gift.GiftBoxState.Enable(giftBoxData);
                                    }
                                    break;
                                case 4:
                                    pTarget.Ref.SetOwningHouse(HouseClass.FindSpecial());
                                    break;
                            }
                        }
                    }
                    return false;
                });
            }
        }
    }

}
