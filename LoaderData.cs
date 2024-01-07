using System;
using UnityEngine;

namespace BlunderLoader
{
	// Token: 0x02000006 RID: 6
	public struct LoaderData
	{
		// Token: 0x04000046 RID: 70
		public static bool UseLoadingScreenLocalPlay = true;

		// Token: 0x04000047 RID: 71
		public static bool CheckpointCollider;

		// Token: 0x04000048 RID: 72
		public static bool InvisibleCheckpoint = true;

		// Token: 0x04000049 RID: 73
		public static bool UseRespawningTilesFix = true;

		// Token: 0x0400004A RID: 74
		public static bool UseBlunderBranding = true;

		// Token: 0x0400004B RID: 75
		public static bool EnableUiLocalPlay = true;

		// Token: 0x0400004C RID: 76
		public static bool HideUiWhenPressingLocalPlay = false;

		// Token: 0x0400004D RID: 77
		public static bool EnableServerThings = true;

		// Token: 0x0400004E RID: 78
		public static bool UseSimulationTime = false;

		// Token: 0x0400004F RID: 79
		public static int Red = 255;

		// Token: 0x04000050 RID: 80
		public static int Green = 0;

		// Token: 0x04000051 RID: 81
		public static int Blue = 0;

		// Token: 0x04000052 RID: 82
		public static KeyCode[] keys;

		// Token: 0x04000053 RID: 83
		public static float width = 90f;

		// Token: 0x04000054 RID: 84
		public static float height = 20f;

		// Token: 0x04000055 RID: 85
		public static float x = 40f;

		// Token: 0x04000056 RID: 86
		public static float y = 40f;

		// Token: 0x04000057 RID: 87
		public static string CustomImagesStart = "custom_";

		// Token: 0x04000058 RID: 88
		public static string[] secretScenes = new string[]
		{
			"AutomationPlayground", "FallGuy_Background_Goop_Season_6", "FallGuy_Background_Goop_Season_7", "FallGuy_Background_Respawn_Season_6", "FallGuy_Background_Respawn_Symphony_1", "FallGuy_ButtonBasherArena", "FallGuy_VirusScanner_TestBed", "FallGuy_KrakenTentacle_TestBed", "FallGuy_Testbed_Lives_System", "FallGuy_Testbed_Boom_Blasters",
			"FallGuy_S9_Obstacle_Balance_Testbed", "FallGuy_Season_9_SlideTestingArea", "FallGuy_Obstacles_Symphony_2", "FallGuy_Obstacles_Offline_Playground", "FallGuy_Metrics_Testbed_01", "FallGuy_Firewall_TestBed", "fallguy_obstacles_symphony_2"
		};
	}
}
