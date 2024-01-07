using System;
using System.Collections;
using BepInEx.IL2CPP.Utils.Collections;
using Levels.Obstacles;
using UnityEngine;

namespace BlunderLoader.Fixes
{
	// Token: 0x0200000B RID: 11
	public class RespawningTile : MonoBehaviour
	{
		// Token: 0x0600003C RID: 60 RVA: 0x00004C58 File Offset: 0x00002E58
		public void OnTriggerEnter(Collider collider)
		{
			if (collider.gameObject.tag == "Player" && !this.despawned)
			{
				this.tile.HandleTileTriggerDespawning();
				base.StartCoroutine(CollectionExtensions.WrapToIl2Cpp(this.respawn()));
				this.despawned = true;
			}
		}

		// Token: 0x0600003D RID: 61 RVA: 0x00004CA8 File Offset: 0x00002EA8
		private IEnumerator respawn()
		{
			float counter = 0f;
			while (counter < 5f)
			{
				counter += Time.deltaTime;
				yield return null;
			}
			this.despawned = false;
			this.tile.OnTriggerRespawnRoutine();
			yield break;
		}

		// Token: 0x04000060 RID: 96
		public COMMON_RespawningTile tile;

		// Token: 0x04000061 RID: 97
		private bool despawned;
	}
}
