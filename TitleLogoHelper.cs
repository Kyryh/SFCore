﻿using UnityEngine;
using Logger = Modding.Logger;
using System.Collections.Generic;
using UObject = UnityEngine.Object;

namespace SFCore
{
    public class TitleLogoHelper
    {
        private static bool isntanceChanged = false;
        private static List<Sprite> customLogos = new List<Sprite>();

        static TitleLogoHelper()
        {
            On.MenuStyleTitle.ctor += OnMenuStyleTitleConstructor;
            On.MenuStyleTitle.SetTitle += OnMenuStyleTitleSetTitle;
        }

        public static void Initialize()
        {
        }

        public static int AddLogo(Sprite logo)
        {
            customLogos.Add(logo);
            UObject.DontDestroyOnLoad(logo);
            return customLogos.Count;
        }

        private static void OnMenuStyleTitleConstructor(On.MenuStyleTitle.orig_ctor orig, MenuStyleTitle self)
        {
            orig(self);
            isntanceChanged = false;
        }

        private static void OnMenuStyleTitleSetTitle(On.MenuStyleTitle.orig_SetTitle orig, MenuStyleTitle self, int index)
        {
            if (!isntanceChanged)
            {
                RuntimePlatform[] allPlatforms = new RuntimePlatform[]
                {
                    RuntimePlatform.OSXEditor,
                    RuntimePlatform.OSXPlayer,
                    RuntimePlatform.WindowsPlayer,
                    RuntimePlatform.WindowsEditor,
                    RuntimePlatform.IPhonePlayer,
                    RuntimePlatform.Android,
                    RuntimePlatform.LinuxPlayer,
                    RuntimePlatform.LinuxEditor,
                    RuntimePlatform.WebGLPlayer,
                    RuntimePlatform.WSAPlayerX86,
                    RuntimePlatform.WSAPlayerX64,
                    RuntimePlatform.WSAPlayerARM,
                    RuntimePlatform.TizenPlayer,
                    RuntimePlatform.PSP2,
                    RuntimePlatform.PS4,
                    RuntimePlatform.PSM,
                    RuntimePlatform.XboxOne,
                    RuntimePlatform.WiiU,
                    RuntimePlatform.tvOS,
                    RuntimePlatform.Switch
                };
                for (int i = 0; i < self.TitleSprites.Length; i++)
                {
                    self.TitleSprites[i].PlatformWhitelist = allPlatforms;
                }

                List<MenuStyleTitle.TitleSpriteCollection> tmpList = new List<MenuStyleTitle.TitleSpriteCollection>(self.TitleSprites);
                foreach (var s in customLogos)
                {
                    var tmpTSC = new MenuStyleTitle.TitleSpriteCollection();

                    tmpTSC.PlatformWhitelist = allPlatforms;
                    tmpTSC.Default = s;
                    tmpTSC.Chinese = null;
                    tmpTSC.Russian = null;
                    tmpTSC.Italian = null;
                    tmpTSC.Japanese = null;
                    tmpTSC.Spanish = null;
                    tmpTSC.Korean = null;
                    tmpTSC.French = null;
                    tmpTSC.BrazilianPT = null;

                    tmpList.Add(tmpTSC);
                }
                self.TitleSprites = tmpList.ToArray();

                isntanceChanged = true;
            }

            orig(self, index);
        }

        private static void Log(string message)
        {
            Logger.Log($"[SFCore]:[TitleLogoHelper] - {message}");
        }
        private static void Log(object message)
        {
            Logger.Log($"[SFCore]:[TitleLogoHelper] - {message.ToString()}");
        }
    }
}
