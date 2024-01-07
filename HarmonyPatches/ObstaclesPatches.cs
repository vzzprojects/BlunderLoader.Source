using System;
using BlunderLoader.Fixes;
using FG.Common;
using FG.Common.AI;
using FG.Common.Network;
using HarmonyLib;
using Levels;
using Levels.Obstacles;
using Levels.Progression;
using Levels.WallGuys;
using Levels.Wormholes;
using UnityEngine;

namespace BlunderLoader.HarmonyPatches
{
	// Token: 0x0200000F RID: 15
	public class ObstaclesPatches
	{
		// Token: 0x06000048 RID: 72 RVA: 0x00004EB4 File Offset: 0x000030B4
		[HarmonyPatch(typeof(WallGuysSegmentGenerator), "Awake")]
		[HarmonyPrefix]
		public static bool Awake(ref WallGuysSegmentGenerator __instance)
		{
			__instance._collider = __instance.GetComponent<BoxCollider>();
			__instance._random = FGRandom.Create(__instance._objectId, 0);
			__instance.CreateSegmentObstacles();
			__instance._collider.enabled = false;
			return false;
		}

		// Token: 0x02000020 RID: 32
		[HarmonyPatch(typeof(TrolleyBotObstacle), "Awake")]
		internal static class TrolleyBotObstaclePatch
		{
			// Token: 0x06000097 RID: 151 RVA: 0x00006152 File Offset: 0x00004352
			private static bool Prefix(ref TrolleyBotObstacle __instance)
			{
				__instance.SetupIndependentTransforms();
				return false;
			}
		}

		// Token: 0x02000021 RID: 33
		[HarmonyPatch(typeof(COMMON_Button), "OnCollisionStay")]
		internal static class ButtonsPatch
		{
			// Token: 0x06000098 RID: 152 RVA: 0x0000615C File Offset: 0x0000435C
			public static bool PreFix(COMMON_Button __instance, Collision col)
			{
				COMMON_Button.ButtonState currentButtonState = __instance._currentButtonState;
				if (currentButtonState != null)
				{
					if (currentButtonState == 3)
					{
						__instance.TryApplyResetLaunchForce(col);
					}
				}
				else if (CollisionExtensions.IsCollisionFromAbove(col, __instance.CachedTransform, 10f))
				{
					__instance.LastTriggeringFGCC = col.gameObject.GetComponent<FallGuysCharacterController>();
					__instance._localLastTriggeringFGCC = __instance.LastTriggeringFGCC;
					__instance.PressButton(__instance.GameState.SimulationFixedTime);
				}
				return false;
			}
		}

		// Token: 0x02000022 RID: 34
		[HarmonyPatch(typeof(COMMON_PrefabSpawnerBase), "InstantiateObject")]
		internal static class InstantiateObjectPatch
		{
			// Token: 0x06000099 RID: 153 RVA: 0x000061C4 File Offset: 0x000043C4
			public static bool Prefix(COMMON_PrefabSpawnerBase __instance, COMMON_PrefabSpawnerBase.SpawnerEntry entry, Vector3 spawnPosition)
			{
				COMMON_PrefabSpawnerBase.__c__DisplayClass68_0 _c__DisplayClass68_ = new COMMON_PrefabSpawnerBase.__c__DisplayClass68_0();
				_c__DisplayClass68_.entry = entry;
				Quaternion initialRotation = __instance.GetInitialRotation(_c__DisplayClass68_.entry);
				_c__DisplayClass68_.spawnScale = _c__DisplayClass68_.entry.value.transform.localScale;
				GameObject gameObject = Object.Instantiate<GameObject>(entry.value, spawnPosition, initialRotation);
				gameObject.transform.localScale = _c__DisplayClass68_.spawnScale;
				__instance.OnInstantiateObject(gameObject, entry);
				_c__DisplayClass68_.spawnScale = Vector3.zero;
				__instance._currentInstances.Add(gameObject);
				InstantiatedObject instantiatedObject = gameObject.AddComponent<InstantiatedObject>();
				instantiatedObject._manager = __instance;
				instantiatedObject._killzone = BlunderLoaderManager.Instance.playerEliminationVolume.transform;
				if (gameObject.GetComponent<MPGNetObject>() != null)
				{
					BlunderLoaderManager.Instance.HandleMPGNetObject(gameObject.GetComponent<MPGNetObject>());
				}
				return false;
			}
		}

		// Token: 0x02000023 RID: 35
		[HarmonyPatch(typeof(LivesSystemManager), "_livesSystemEnabled", 1)]
		internal static class LivesPatch
		{
			// Token: 0x0600009A RID: 154 RVA: 0x00006284 File Offset: 0x00004484
			private static bool Prefix(ref bool __result)
			{
				__result = true;
				return false;
			}
		}

		// Token: 0x02000024 RID: 36
		[HarmonyPatch(typeof(CameraDirector), "Update")]
		internal static class CameraServerPatch
		{
			// Token: 0x0600009B RID: 155 RVA: 0x0000628A File Offset: 0x0000448A
			public static bool Prefix(CameraDirector __instance)
			{
				return false;
			}
		}

		// Token: 0x02000025 RID: 37
		[HarmonyPatch(typeof(COMMON_MovingPlatform), "HandleStateEnd")]
		internal static class MovingPlatformEndServerPatch
		{
			// Token: 0x0600009C RID: 156 RVA: 0x0000628D File Offset: 0x0000448D
			public static bool Prefix(COMMON_MovingPlatform __instance)
			{
				bool destroyOnEnd = __instance._destroyOnEnd;
				return false;
			}
		}

		// Token: 0x02000026 RID: 38
		[HarmonyPatch(typeof(COMMON_MovingPlatform), "HandleState")]
		internal static class MovingPlatformUpdateServerPatch
		{
			// Token: 0x0600009D RID: 157 RVA: 0x00006298 File Offset: 0x00004498
			public static bool Prefix(COMMON_MovingPlatform __instance)
			{
				bool flag = __instance._platformState != __instance._previousPlatformState;
				switch (__instance._platformState)
				{
				case 0:
					__instance.WaitForDelay(__instance._initialDelay, 1);
					break;
				case 1:
					__instance.HandleStateExtendToEnd(flag);
					break;
				case 2:
					__instance.HandleStateEndDelay(flag);
					break;
				case 3:
					__instance.platformIsMoving = true;
					__instance.MoveToPosition(__instance._localEndPosition, __instance._localStartPosition, 4, __instance._returnDuration);
					break;
				case 4:
					__instance.HandleStateStartDelay();
					break;
				case 5:
					if (__instance._destroyOnEnd)
					{
						Object.Destroy(__instance.gameObject);
					}
					break;
				}
				__instance._previousPlatformState = __instance._platformState;
				return false;
			}
		}

		// Token: 0x02000027 RID: 39
		[HarmonyPatch(typeof(COMMON_BullRMIController), "RegisterRemoteMethods")]
		internal static class BullRMIMethodsPatch
		{
			// Token: 0x0600009E RID: 158 RVA: 0x00006349 File Offset: 0x00004549
			public static bool Prefix(COMMON_BullRMIController __instance)
			{
				return false;
			}
		}

		// Token: 0x02000028 RID: 40
		[HarmonyPatch(typeof(NPCController), "OnCollisionStay")]
		internal static class NPCControllerCollisionPatch
		{
			// Token: 0x0600009F RID: 159 RVA: 0x0000634C File Offset: 0x0000454C
			public static bool Prefix(NPCController __instance, Collision collision)
			{
				ContactPoint[] array = collision.contacts;
				for (int i = 0; i < array.Length; i++)
				{
					if (NPCController.IsTouchingGround(array[i]))
					{
						__instance._isGrounded = true;
					}
				}
				return false;
			}
		}

		// Token: 0x02000029 RID: 41
		[HarmonyPatch(typeof(COMMON_PrefabSpawnerBase), "DestroyInstanceAtIndex")]
		internal static class DestroyIndexPatch
		{
			// Token: 0x060000A0 RID: 160 RVA: 0x00006389 File Offset: 0x00004589
			public static bool Prefix(COMMON_PrefabSpawnerBase __instance, int instanceIndex)
			{
				Object.Destroy(__instance._currentInstances[instanceIndex]);
				__instance._currentInstances.RemoveAt(instanceIndex);
				__instance.OnDespawnedInstance == null;
				return false;
			}
		}

		// Token: 0x0200002A RID: 42
		[HarmonyPatch(typeof(COMMON_ConveyorBelt), "Awake")]
		internal static class ConveyorBeltAwakePatch
		{
			// Token: 0x060000A1 RID: 161 RVA: 0x000063B6 File Offset: 0x000045B6
			public static bool Prefix(COMMON_ConveyorBelt __instance)
			{
				if (__instance.GameState.IsGameServer)
				{
					__instance.CheckRenderers();
				}
				return true;
			}
		}

		// Token: 0x0200002B RID: 43
		[HarmonyPatch(typeof(COMMON_ConveyorBelt), "Update")]
		internal static class ConveyorBeltUpdatePatch
		{
			// Token: 0x060000A2 RID: 162 RVA: 0x000063CC File Offset: 0x000045CC
			public static bool Prefix(COMMON_ConveyorBelt __instance)
			{
				__instance.ScrollConveyorVisuals();
				return false;
			}
		}

		// Token: 0x0200002C RID: 44
		[HarmonyPatch(typeof(COMMON_Wormhole), "OnForceTriggerStay")]
		internal static class WormholeOnTriggerStayPatch
		{
			// Token: 0x060000A3 RID: 163 RVA: 0x000063D8 File Offset: 0x000045D8
			public static bool Prefix(COMMON_Wormhole __instance, Collider ObjectCollider)
			{
				try
				{
					if (__instance._closed || ObjectCollider.isTrigger)
					{
						return false;
					}
					if (ObjectCollider.attachedRigidbody == null)
					{
						return false;
					}
					ObjectCollider.attachedRigidbody.position - __instance.transform.position;
					FallGuysCharacterController fallGuysCharacterController;
					if (__instance.GameState.IsPlayerOrRagdollCollider(ObjectCollider, ref fallGuysCharacterController) && !__instance.ShouldIgnore(fallGuysCharacterController.NetObject.NetID.GetHashCode()))
					{
						if (__instance.GameState.IsGameServer || fallGuysCharacterController.IsControlledLocally)
						{
							if (!__instance._applyOnFGCC)
							{
								__instance.HandlePlayerCharacterEnterInForceVolume(fallGuysCharacterController, true);
								return false;
							}
							if (Vector3.Dot(__instance.transform.up, Vector3.Normalize(fallGuysCharacterController.RigidBody.velocity)) > 0f)
							{
								__instance.HandlePlayerCharacterEnterInForceVolume(fallGuysCharacterController, true);
								return false;
							}
						}
					}
					else if (__instance.GameState.IsGameServer)
					{
						float magnitude = ObjectCollider.bounds.size.magnitude;
						bool flag = magnitude <= __instance._maxObjectSize && magnitude >= __instance._minObjectSize;
						if (!__instance._applyOnMPGNetObject || !flag)
						{
							__instance.HandleMPGNetObjectEnterInForceVolume(ObjectCollider, true);
						}
					}
				}
				catch (Exception ex)
				{
					Debug.LogError("BlunderLoader | Wormhole: " + ex.ToString());
				}
				return false;
			}
		}

		// Token: 0x0200002D RID: 45
		[HarmonyPatch(typeof(CheckpointZone), "CommonTriggerCollision")]
		internal static class Checkpoints
		{
			// Token: 0x060000A4 RID: 164 RVA: 0x00006550 File Offset: 0x00004750
			public static void Postfix(CheckpointZone __instance, Collider other)
			{
				if (BlunderLoaderManager.Instance.localPlay && other.gameObject.GetComponent<FallGuysCharacterController>())
				{
					BlunderLoaderManager.Instance.OnTriggerCheckpoint(__instance);
				}
			}
		}

		// Token: 0x0200002E RID: 46
		[HarmonyPatch(typeof(BlastBallManager), "SendStartSequenceEvent")]
		internal static class StartSequenceBBM
		{
			// Token: 0x060000A5 RID: 165 RVA: 0x0000657B File Offset: 0x0000477B
			public static bool Prefix(BlastBallManager __instance, COMMON_BlastBall instance, float timestamp, bool rushIntoReadyToExplode)
			{
				if (!__instance._pendingBlastBallActivations.Contains(instance))
				{
					__instance._pendingBlastBallActivations.Add(instance);
					instance.StartSequenceEvent(timestamp, rushIntoReadyToExplode);
				}
				return false;
			}
		}

		// Token: 0x0200002F RID: 47
		[HarmonyPatch(typeof(BlastBallManager), "SendOnRespawnEvent")]
		internal static class RespwnBBM
		{
			// Token: 0x060000A6 RID: 166 RVA: 0x000065A1 File Offset: 0x000047A1
			public static bool Prefix(BlastBallManager __instance, COMMON_BlastBall instance)
			{
				instance.OnRespawn();
				return false;
			}
		}

		// Token: 0x02000030 RID: 48
		[HarmonyPatch(typeof(BlastBallManager), "SendRushToNextSequenceEvent")]
		internal static class NextSequenceBBM
		{
			// Token: 0x060000A7 RID: 167 RVA: 0x000065AA File Offset: 0x000047AA
			public static bool Prefix(BlastBallManager __instance, COMMON_BlastBall instance)
			{
				instance.RushToNextSequenceEvent();
				return false;
			}
		}

		// Token: 0x02000031 RID: 49
		[HarmonyPatch(typeof(RPCFGBehaviour), "InvokeMethodRemotely")]
		internal static class RPCInvokeEverywherePatch
		{
			// Token: 0x060000A8 RID: 168 RVA: 0x000065B3 File Offset: 0x000047B3
			public static bool Prefix(RPCFGBehaviour __instance)
			{
				return false;
			}
		}

		// Token: 0x02000032 RID: 50
		[HarmonyPatch(typeof(RMIBehaviour), "InvokeMethodRemotely")]
		internal static class RMIInvokeEverywherePatch
		{
			// Token: 0x060000A9 RID: 169 RVA: 0x000065B6 File Offset: 0x000047B6
			public static bool Prefix(RMIBehaviour __instance)
			{
				return false;
			}
		}
	}
}
