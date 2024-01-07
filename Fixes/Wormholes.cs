using System;
using System.Collections;
using FG.Common;
using Il2CppSystem.Collections.Generic;
using Levels.Wormholes;
using UnityEngine;

namespace BlunderLoader.Fixes
{
	// Token: 0x0200000C RID: 12
	public class Wormholes : MonoBehaviour
	{
		// Token: 0x0600003F RID: 63 RVA: 0x00004CBF File Offset: 0x00002EBF
		public void SetUp()
		{
			this.targets = this.wormhole._targets;
			if (this.targets.Count > 0)
			{
				this.nextWormhole = this.targets[0];
			}
		}

		// Token: 0x06000040 RID: 64 RVA: 0x00004CF4 File Offset: 0x00002EF4
		public void OnTriggerEnter(Collider collider)
		{
			if (!this.finished && !this.isTeleporting && (collider.gameObject.tag == "Player" || collider.gameObject.tag == "CarryObject" || collider.gameObject.tag == "Ball"))
			{
				this.wormhole.HandleMPGNetObjectEnterWormhole(collider.GetComponent<MPGNetObject>());
			}
		}

		// Token: 0x06000041 RID: 65 RVA: 0x00004D67 File Offset: 0x00002F67
		private IEnumerator Teleported()
		{
			float counter = 0f;
			while (counter < 1f)
			{
				counter += Time.deltaTime;
				yield return null;
			}
			this.finished = false;
			this.isTeleporting = false;
			this.wormhole.CurrentTarget.gameObject.transform.Find("Visuals").gameObject.GetComponent<Wormholes>().isTeleporting = false;
			Debug.Log("Resetting wormhole");
			yield break;
		}

		// Token: 0x04000062 RID: 98
		public COMMON_Wormhole wormhole;

		// Token: 0x04000063 RID: 99
		public bool finished;

		// Token: 0x04000064 RID: 100
		public List<COMMON_Wormhole> targets = new List<COMMON_Wormhole>();

		// Token: 0x04000065 RID: 101
		private COMMON_Wormhole nextWormhole;

		// Token: 0x04000066 RID: 102
		private bool isTeleporting;
	}
}
