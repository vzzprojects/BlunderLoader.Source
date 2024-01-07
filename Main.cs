using System;
using BepInEx;
using BepInEx.IL2CPP;
using BlunderLoader.Fixes;
using BlunderLoader.HarmonyPatches;
using HarmonyLib;
using UnhollowerRuntimeLib;
using UnityEngine;

namespace BlunderLoader
{
	// Token: 0x02000008 RID: 8
	[BepInPlugin("matti.BlunderLoader", "BlunderLoader - mattiFG", "1.7.6")]
	public class Main : BasePlugin
	{
		// Token: 0x06000038 RID: 56 RVA: 0x00004A20 File Offset: 0x00002C20
		public override void Load()
		{
			base.Log.LogMessage("BlunderLoader - by mattiFG");
			ClassInjector.RegisterTypeInIl2Cpp<BlunderLoaderManager>();
			ClassInjector.RegisterTypeInIl2Cpp<Wormholes>();
			ClassInjector.RegisterTypeInIl2Cpp<RespawningTile>();
			ClassInjector.RegisterTypeInIl2Cpp<InstantiatedObject>();
			this.monoGO = new GameObject();
			this.monoGO.name = "BlunderLoader | Manager";
			Object.DontDestroyOnLoad(this.monoGO);
			this.monoGO.AddComponent<BlunderLoaderManager>();
			this.monoGO.hideFlags = 61;
			try
			{
				this.harmony.PatchAll(typeof(ServerPatches.IsGameServerPatch));
				this.harmony.PatchAll(typeof(ObstaclesPatches.InstantiateObjectPatch));
				this.harmony.PatchAll(typeof(ObstaclesPatches.DestroyIndexPatch));
				this.harmony.PatchAll(typeof(ObstaclesPatches.MovingPlatformEndServerPatch));
				this.harmony.PatchAll(typeof(ObstaclesPatches.MovingPlatformUpdateServerPatch));
				this.harmony.PatchAll(typeof(ObstaclesPatches.ConveyorBeltUpdatePatch));
				this.harmony.PatchAll(typeof(GeneralPatches));
				this.harmony.PatchAll(typeof(ObstaclesPatches));
				this.harmony.PatchAll(typeof(ObstaclesPatches.Checkpoints));
				this.harmony.PatchAll(typeof(ObstaclesPatches.LivesPatch));
			}
			catch (Exception ex)
			{
				Debug.LogError("BlunderLoader | Error trying harmony patch: " + ex.Message);
			}
		}

		// Token: 0x04000059 RID: 89
		private GameObject monoGO;

		// Token: 0x0400005A RID: 90
		public Harmony harmony = new Harmony("com.matti.BlunderLoader");
	}
}
