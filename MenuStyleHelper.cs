﻿using System;
using System.Collections;
using SFCore.Utils;
using UnityEngine;
using Logger = Modding.Logger;
using System.Collections.Generic;
using UnityEngine.Audio;
using UObject = UnityEngine.Object;

namespace SFCore
{
    /// <summary>
    ///     Menu style helper class for easily adding custom menu styles.
    ///     The mod using this needs to handle the following:
    ///     1 language string per menu style
    ///     up to 1 title logo index per menu style
    /// </summary>
    public static class MenuStyleHelper
    {
        private static List<(string, GameObject, int, string, string[], MenuStyles.MenuStyle.CameraCurves, AudioMixerSnapshot)> queue = new List<(string, GameObject, int, string, string[], MenuStyles.MenuStyle.CameraCurves, AudioMixerSnapshot)>();
        private static List<Func<MenuStyles, (string, GameObject, int, string, string[], MenuStyles.MenuStyle.CameraCurves, AudioMixerSnapshot)>> callbackQueue = new List<Func<MenuStyles, (string, GameObject, int, string, string[], MenuStyles.MenuStyle.CameraCurves, AudioMixerSnapshot)>>();

        public delegate (string languageString, GameObject styleGo, int titleIndex, string unlockKey, string[] achievementKeys, MenuStyles.MenuStyle.CameraCurves cameraCurves, AudioMixerSnapshot musicSnapshot) Hook(MenuStyles self);
        /// <inheritdoc />
        /// <summary>
        ///     Hook to add custom run audio.
        /// </summary>
        /// <param name="self">active MenuStyles</param>
        /// <returns>
        ///     Tuple of:
        ///     - Language string of the name of the menu style
        ///     - GameObject of the menu style
        ///     - title logo index (use -1 if you're not sure)
        ///     - unlock key (use "" to have it unlocked by default)
        ///     - array of achievement keys needed to unlock the menu style (null to have it unlocked by default)
        ///     - CameraCurves of the colour correction to use when the style is used (null for default)
        ///     - AudioMixerSnapshot of the snapshot to use when the style is used (null for default)
        /// </returns>
        public static Hook AddMenuStyleHook;

        static MenuStyleHelper()
        {
            On.MenuStyles.Awake += OnMenuStylesAwake;
            On.MenuStyles.SetStyle += OnMenuStylesSetStyle;
        }

        /// <inheritdoc />
        /// <summary>
        ///     Hook to add custom run audio.
        /// </summary>
        /// <param name="languageString">Language string of the name of the menu style</param>
        /// <param name="styleGo">GameObject of the menu style</param>
        /// <param name="titleIndex">Title logo index</param>
        /// <param name="unlockKey">Unlock key</param>
        /// <param name="achievementKeys">Array of achievement keys needed to unlock the menu style</param>
        /// <param name="cameraCurves">CameraCurves of the colour correction to use when the style is used</param>
        /// <param name="musicSnapshot">AudioMixerSnapshot of the snapshot to use when the style is used</param>
        public static void AddMenuStyle(string languageString, GameObject styleGo, int titleIndex = -1, string unlockKey = "", string[] achievementKeys = null, MenuStyles.MenuStyle.CameraCurves cameraCurves = null, AudioMixerSnapshot musicSnapshot = null)
        {
            queue.Add((languageString, styleGo, titleIndex, unlockKey, achievementKeys, cameraCurves, musicSnapshot));
            UObject.DontDestroyOnLoad(styleGo);
            UObject.DontDestroyOnLoad(musicSnapshot);
        }
        /// <inheritdoc />
        /// <summary>
        ///     Hook to add custom run audio.
        /// </summary>
        /// <param name="callback">
        ///     Method that returns tuple of:
        ///     - Language string of the name of the menu style
        ///     - GameObject of the menu style
        ///     - title logo index (use -1 if you're not sure)
        ///     - unlock key (use "" to have it unlocked by default)
        ///     - array of achievement keys needed to unlock the menu style (null to have it unlocked by default)
        ///     - CameraCurves of the colour correction to use when the style is used
        ///     - AudioMixerSnapshot of the snapshot to use when the style is used (null for default)
        /// </param>
        public static void AddMenuStyle(Func<MenuStyles, (string, GameObject, int, string, string[], MenuStyles.MenuStyle.CameraCurves, AudioMixerSnapshot)> callback)
        {
            callbackQueue.Add(callback);
        }

        private static void OnMenuStylesAwake(On.MenuStyles.orig_Awake orig, MenuStyles self)
        {
            orig(self);
            AddCustomStyles(self);
        }

        private static void AddCustomStyles(MenuStyles self)
        {
            List<MenuStyles.MenuStyle> tmpList = new List<MenuStyles.MenuStyle>(self.styles);
            var tmpMenuStyle = tmpList[0];
            foreach (var (languageString, styleGo, titleIndex, unlockKey, achievementKeys, cameraCurves, musicSnapshot) in queue)
            {
                var tmpCameraCurves = cameraCurves ?? tmpMenuStyle.cameraColorCorrection;
                var tmpMusicSnapshot = musicSnapshot ?? tmpMenuStyle.musicSnapshot;
                styleGo.transform.SetParent(self.transform);
                var tmpStyle = new MenuStyles.MenuStyle
                {
                    enabled = true,
                    displayName = languageString,
                    styleObject = styleGo,
                    cameraColorCorrection = tmpCameraCurves,
                    musicSnapshot = tmpMusicSnapshot,
                    titleIndex = titleIndex,
                    unlockKey = unlockKey,
                    achievementKeys = achievementKeys,
                    initialAudioVolumes = tmpMenuStyle.initialAudioVolumes
                };
                tmpList.Add(tmpStyle);
            }
            foreach (var callback in callbackQueue)
            {
                var (languageString, styleGo, titleIndex, unlockKey, achievementKeys, cameraCurves, musicSnapshot) =
                    callback(self);
                var tmpCameraCurves = cameraCurves ?? tmpMenuStyle.cameraColorCorrection;
                var tmpMusicSnapshot = musicSnapshot ?? tmpMenuStyle.musicSnapshot;
                styleGo.transform.SetParent(self.transform);
                var tmpStyle = new MenuStyles.MenuStyle
                {
                    enabled = true,
                    displayName = languageString,
                    styleObject = styleGo,
                    cameraColorCorrection = tmpCameraCurves,
                    musicSnapshot = tmpMusicSnapshot,
                    titleIndex = titleIndex,
                    unlockKey = unlockKey,
                    achievementKeys = achievementKeys,
                    initialAudioVolumes = tmpMenuStyle.initialAudioVolumes
                };
                tmpList.Add(tmpStyle);
            }
            foreach (var callback in AddMenuStyleHook.GetInvocationList())
            {
                if (callback == null)
                    continue;

                var (languageString, styleGo, titleIndex, unlockKey, achievementKeys, cameraCurves, musicSnapshot) =
                    ((string, GameObject, int, string, string[], MenuStyles.MenuStyle.CameraCurves,
                        AudioMixerSnapshot))callback.DynamicInvoke(self);
                var tmpCameraCurves = cameraCurves ?? tmpMenuStyle.cameraColorCorrection;
                var tmpMusicSnapshot = musicSnapshot ?? tmpMenuStyle.musicSnapshot;
                styleGo.transform.SetParent(self.transform);
                var tmpStyle = new MenuStyles.MenuStyle
                {
                    enabled = true,
                    displayName = languageString,
                    styleObject = styleGo,
                    cameraColorCorrection = tmpCameraCurves,
                    musicSnapshot = tmpMusicSnapshot,
                    titleIndex = titleIndex,
                    unlockKey = unlockKey,
                    achievementKeys = achievementKeys,
                    initialAudioVolumes = tmpMenuStyle.initialAudioVolumes
                };
                tmpList.Add(tmpStyle);
            }
            self.styles = tmpList.ToArray();

            int tmpInt = Platform.Current.EncryptedSharedData.GetInt("menuStyle", 0);
            if (tmpInt >= self.styles.Length)
                Platform.Current.EncryptedSharedData.SetInt("menuStyle", 0);
        }

        private static IEnumerator OnMenuStylesFade(On.MenuStyles.orig_Fade orig, MenuStyles self, int styleindex, int fadetype, bool fade, AudioSource[] audiosources)
        {
            var tmp = orig(self, styleindex, fadetype, fade, audiosources);
            GameCameras.instance.colorCorrectionCurves.UpdateTextures();
            return tmp;
        }

        private static void OnMenuStylesSetStyle(On.MenuStyles.orig_SetStyle orig, MenuStyles self, int index, bool fade, bool save)
        {
            if (index < 0 || index >= 10)
            {
                orig(self, index, fade, false);
                Platform.Current.EncryptedSharedData.SetInt("menuStyle", 0);
            }
            else
            {
                orig(self, index, fade, save);
            }
            //GameManager.instance.StartCoroutine(UpdateTexturesWhileSwitching(self));
        }

        private static IEnumerator UpdateTexturesWhileSwitching(MenuStyles self)
        {
            while (self.GetAttr<MenuStyles, Coroutine>("fadeRoutine") != null)
            {
                GameCameras.instance.colorCorrectionCurves.UpdateTextures();
                yield return null;
            }
            for (float elapsed = 0f; elapsed < 0.251f; elapsed += Time.deltaTime)
            {
                GameCameras.instance.colorCorrectionCurves.UpdateTextures();
                yield return null;
            }
            yield break;
        }

        private static void Log(string message)
        {
            Logger.Log($"[SFCore]:[MenuStyleHelper] - {message}");
        }
        private static void Log(object message)
        {
            Logger.Log($"[SFCore]:[MenuStyleHelper] - {message.ToString()}");
        }
    }
}
