using System;
using System.Collections;
using System.Collections.Generic;
using Code.Common;
using Common;
using UnityEngine;

namespace Code.Common.Dispatchers
{
	/// <summary>
	/// Handles actions to be called on update
	/// </summary>
	public class HeartbeatDispatcher : MonoBehaviour
	{
		private static Action onUpdate;
		private static Action onLateUpdate;

		public static HeartbeatDispatcher instance;

		void Awake()
		{
			if (instance != null)
			{
				Destroy(gameObject);
			}
			
			instance = this;
		}

		public static void AddHandler(Action f)
		{
			onUpdate -= f;
			onUpdate += f;
		}

		public static void AddLateHandler(Action f)
		{
			onLateUpdate -= f;
			onLateUpdate += f;
		}

		public static void RemoveHandler(Action f)
		{
			onUpdate -= f;
		}

		public static void RemoveLateHandler(Action f)
		{
			onLateUpdate -= f;
		}

		private void Update()
		{
			onUpdate?.Invoke();
		}

		private void LateUpdate()
		{
			onLateUpdate?.Invoke();
		}
	}
}
