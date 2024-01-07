using System;
using System.IO;
using BepInEx;
using FGClient;
using FGClient.OfflinePlayground;
using HarmonyLib;
using UnityEngine;

namespace BlunderLoader.HarmonyPatches
{
	// Token: 0x0200000E RID: 14
	internal class GeneralPatches
	{
		// Token: 0x06000044 RID: 68 RVA: 0x00004D91 File Offset: 0x00002F91
		[HarmonyPatch(typeof(OfflinePlaygroundManager), "Start")]
		[HarmonyPrefix]
		public static bool NoIntroCams(ref OfflinePlaygroundManager __instance)
		{
			__instance._playerInput = __instance._fallGuysCharacter.GetComponentInChildren<FallGuysCharacterControllerInput>();
			__instance._playerInput.AcceptInput = false;
			__instance.ApplyOutfit();
			__instance.SetupPlayerUpdateManagerAndRegister(__instance._fallGuysCharacter, true);
			return false;
		}

		// Token: 0x06000045 RID: 69 RVA: 0x00004DCA File Offset: 0x00002FCA
		[HarmonyPatch(typeof(OfflinePlaygroundManager), "SetupPlayerUpdateManagerAndRegister")]
		[HarmonyPostfix]
		public static void GiveManagerToMain(ref OfflinePlaygroundManager __instance)
		{
			BlunderLoaderManager.Instance.clientPlayerUpdateManager = __instance._playerUpdateManager;
		}

		// Token: 0x06000046 RID: 70 RVA: 0x00004DE0 File Offset: 0x00002FE0
		[HarmonyPatch(typeof(LoadingGameScreenViewModel), "InitImage")]
		[HarmonyPostfix]
		public static void ImageLoadingScreen(ref LoadingGameScreenViewModel __instance)
		{
			__instance.IsLoading = true;
			if (__instance._round.LoadingScreenName.StartsWith(LoaderData.CustomImagesStart))
			{
				try
				{
					string text = Path.Combine(Paths.PluginPath, "BlunderLoader\\Media\\" + __instance._round.LoadingScreenName);
					if (File.Exists(text))
					{
						byte[] array = File.ReadAllBytes(text);
						Texture2D texture2D = new Texture2D(2, 2);
						if (ImageConversion.LoadImage(texture2D, array))
						{
							__instance.CurrentScreen = Sprite.Create(texture2D, new Rect(0f, 0f, (float)texture2D.width, (float)texture2D.height), new Vector2(0.5f, 0.5f), 100f, 0U, 1);
						}
					}
				}
				catch
				{
				}
			}
		}
	}
}
