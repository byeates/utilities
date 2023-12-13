using System;
using System.Collections;
using System.Collections.Generic;
using Code.Common.Dispatchers;
using Code.Common.IO;
using TMPro;
using UnityEngine;

namespace Code.Common
{
	/// <summary>
	/// A delay based timer class, it executes methods after the delay has been elapsed
	/// </summary>
	public class SimpleTimer
	{
		//====================
		// PUBLIC
		//====================
		/// <summary>
		/// Delay before timer is completed
		/// </summary>
		public float delay;

		/// <summary>
		/// Repeat the timer when finished
		/// </summary>
		public bool repeat;

		/// <summary>
		/// Automatically destroys/cleans up the timer when it finishes
		/// </summary>
		public bool autoDestroy;

		//====================
		// PRIVATE
		//====================
		/// <summary>
		/// Global list of timers
		/// </summary>
		private static List<SimpleTimer> timers = new List<SimpleTimer>();

		/// <summary>
		/// Time since timer start was called
		/// </summary>
		private float elapsedTime;

		/// <summary>
		/// State of the timer
		/// </summary>
		private StateMachine stateMachine;

		/// <summary>
		/// Action for the when the timer is complete
		/// </summary>
		private Action completeHandler;

		/// <summary>
		/// Action for the timer update occurs
		/// </summary>
		private Action<float> updateHandler;

		/// <summary>
		/// Construct a new timer instance
		/// </summary>
		/// <param name="delay">execution delay</param>
		/// <param name="repeat">set to true to reset/rerun the timer after it completes</param>
		/// <param name="callback">on complete callback</param>
		/// <param name="update">update per frame callback</param>
		/// <param name="autoDestroy">automatically cleans up listeners when completed, and calls Destroy (does not work with repeat true), and you'll need to recreate the timer to use it again</param>
		public SimpleTimer
		(
			float delay,
			bool repeat = false,
			Action callback = null,
			Action<float> update = null,
			bool autoDestroy = false
		)
		{
			this.delay = delay;
			this.repeat = repeat;
			this.autoDestroy = autoDestroy;
			stateMachine = new StateMachine("simple_timer");
			stateMachine.AddState(GameState.States.RUNNING);
			stateMachine.AddState(GameState.States.STOPPED);
			stateMachine.AddState(GameState.States.READY);
			stateMachine.UpdateState(GameState.States.READY);

			AddCompleteHandler(callback);
			AddUpdateHandler(update);
		}

		/// <summary>
		/// Removes timer from global list
		/// </summary>
		public void Destroy()
		{
			RemoveCallbacks();
			Remove(this);
		}

		/// <summary>
		/// Removes all timers
		/// </summary>
		public static void DestroyAll()
		{
			foreach (var timer in timers)
			{
				timer.Stop();
				timer.RemoveCallbacks();
			}
			
			timers.Clear();
		}

		/// <summary>
		/// Removes timer from list
		/// </summary>
		/// <param name="timer"></param>
		private static void Remove(SimpleTimer timer)
		{
			timer.Stop();
			timers.Remove(timer);
		}

		/// <summary>
		/// Starts the timer. If the timer was stopped, it will resume where it left off when start is called
		/// </summary>
		public void Start()
		{
			if (stateMachine.isState(GameState.States.RUNNING))
			{
				Debug.LogWarning("SimpleTimer.Start() called while timer is already running");
				return;
			}
			
			if (stateMachine.isState(GameState.States.READY))
			{
				if (!timers.Contains(this))
				{
					timers.Add(this);
				}
			}
			
			HeartbeatDispatcher.AddHandler(Update);
			stateMachine.UpdateState(GameState.States.RUNNING);
		}

		private void Update()
		{
			if (!stateMachine.isState(GameState.States.RUNNING))
			{
				return;
			}
			
			elapsedTime += Time.deltaTime;

			if (elapsedTime >= delay)
			{
				OnComplete();
			}
			else
			{
				updateHandler?.Invoke(elapsedTime / delay);
			}
		}

		private void OnComplete()
		{
			completeHandler?.Invoke();

			if (repeat)
			{
				Reset();
				Start();
			}
			else
			{
				if (autoDestroy)
				{
					Destroy();
				}
				else
				{
					Remove(this);
				}
			}
		}

		/// <summary>
		/// Stops/pauses the timer
		/// </summary>
		public void Stop()
		{
			HeartbeatDispatcher.RemoveHandler(Update);
			stateMachine.UpdateState(GameState.States.STOPPED);
		}

		/// <summary>
		/// Resets the timer. Start() is needed to continue running
		/// </summary>
		public void Reset()
		{
			elapsedTime = 0;
			HeartbeatDispatcher.RemoveHandler(Update);
			stateMachine.UpdateState(GameState.States.READY);
		}
		
		/// <summary>
		/// Restarts the timer at 0 seconds elapsed
		/// </summary>
		public void Restart()
		{
			elapsedTime = 0f;
		}

		/*================================================================================
		CALLBACK HANDLING    
		=================================================================================*/
		/// <summary>
		/// Remove all callbacks
		/// </summary>
		public void RemoveCallbacks()
		{
			completeHandler = null;
			updateHandler = null;
		}

		public void AddCompleteHandler(Action handler)
		{
			if (handler != null)
			{
				completeHandler -= handler;
				completeHandler += handler;
			}
		}

		public void RemoveCompleteHandler(Action handler)
		{
			completeHandler -= handler;
		}

		public void AddUpdateHandler(Action<float> handler)
		{
			if (handler != null)
			{
				updateHandler -= handler;
				updateHandler += handler;
			}
		}

		public void RemoveUpdateHandler(Action<float> handler)
		{
			updateHandler -= handler;
		}

		/*================================================================================
		ANCILLARY    
		=================================================================================*/
		/// <summary>
		/// Returns formatted time remaining, using <see cref="TimeSpan"/> and the input format
		/// parameter
		/// </summary>
		/// <param name="format">Typical formatting, e.g. @"dd:hh:mm"</param>
		/// <param name="excludeZeros">when set to true removes leading zeros</param>
		/// <returns></returns>
		public string GetTimeFormatted(string format, bool excludeZeros = false)
		{
			float timeLeft = delay - elapsedTime;
			TimeSpan span = TimeSpan.FromSeconds(timeLeft);

			if (excludeZeros)
			{
				return span.ToString(format).TrimStart(' ','d','h','m','s','0',':');
			}
			
			return span.ToString(format);
		}

		/// <summary>
		/// Returns true if the timer is going
		/// </summary>
		public bool isRunning
		{
			get { return stateMachine.isState(GameState.States.RUNNING); }
		}
		
		/// <summary>
		/// Amount of time left
		/// </summary>
		public float TimeRemaining => isRunning ? delay - elapsedTime : 0f;
	}
}
