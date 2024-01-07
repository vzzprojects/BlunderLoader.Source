using System;
using FGClient;
using HarmonyLib;

namespace BlunderLoader.HarmonyPatches
{
	// Token: 0x02000010 RID: 16
	public class ServerPatches
	{
		// Token: 0x02000033 RID: 51
		[HarmonyPatch(typeof(ClientGameStateView), "IsGameServer", 1)]
		internal static class IsGameServerPatch
		{
			// Token: 0x060000AA RID: 170 RVA: 0x000065B9 File Offset: 0x000047B9
			private static bool Prefix(ref bool __result)
			{
				__result = BlunderLoaderManager.Instance.server;
				return false;
			}
		}
	}
}
