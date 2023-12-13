using System.Collections;
using System.Collections.Generic;
using Code.Common.IO;
using Code.Common.Tutor;
using UnityEngine;

public class GameState
{
	//====================
	// PUBLIC
	//====================
	public enum States
	{
		LOADING,
		MAIN_MENU,
		INIT,
		PURCHASING,
		RUNNING,
		STOPPED,
		READY,
		PENDING,
		BUSY,
		DONE,
		PLAYING,
		DIALOG,
		WAITING,
		BLOCKING
	}

	//====================
	// PROTECTED
	//====================
	protected bool blocked;
	protected object blocker;
	protected static StateMachine globalStateMachine;

	// can only ever be 1 "game state"
	private static GameState instance;

	public GameState()
	{
		globalStateMachine ??= new StateMachine();
		instance = this;
	}

	private static void SafeInit()
	{
		globalStateMachine ??= new StateMachine();
	}

	public void Destroy()
	{
		globalStateMachine = new StateMachine();
		instance = null;
	}

	/// <summary>
	/// Put a block or hold on the game state until some system resolves the block
	/// </summary>
	/// <param name="blocker">blocking class/object</param>
	public static void SetBlocked(object blocker)
	{
		if (instance.blocked)
		{
			Debug.Log("State is already blocked: " + instance.blocker.GetType().FullName + " is blocking");
			return;
		}
		instance.blocker = blocker;
		instance.blocked = true;
	}

	/// <summary>
	/// Sets the blocked state back to false, so long as the blocking class/object matches the current blocker
	/// </summary>
	/// <param name="blocker"></param>
	public static void RemoveBlock(object blocker)
	{
		if (instance.blocker == blocker || instance.blocker == null)
		{
			instance.blocked = false;
			instance.blocker = null;
		}
	}
	
	public static void UpdateGameState(States state)
	{
		globalStateMachine.UpdateState(state);
	}

	public static void AddEnterCallback(States name, StateMachine.OnEnterState callback)
	{
		SafeInit();
		globalStateMachine.AddEnterCallback(name, callback);
	}
    
	public static void RemoveEnterCallback(States name, StateMachine.OnEnterState callback)
	{
		SafeInit();
		globalStateMachine.RemoveEnterCallback(name, callback);
	}
    
	public static void AddExitCallback(States name, StateMachine.OnExitState callback)
	{
		SafeInit();
		globalStateMachine.AddExitCallback(name, callback);
	}
    
	public static void RemoveExitCallback(States name, StateMachine.OnEnterState callback)
	{
		SafeInit();
		globalStateMachine.RemoveExitCallback(name, callback);
	}
	
	public static bool isState(string state)
	{
		return globalStateMachine.isState(state);
	}

	public static bool isState(States state)
	{
		return globalStateMachine.isState(state);
	}
	
	/*================================================================================
	GAME STATE RETRIEVAL	
	=================================================================================*/
	/// <summary>
	/// Returns true if game state has a block set
	/// </summary>
	public virtual bool HasBlock => blocked;

	/// <summary>
	/// Returns <see cref="HasBlock"/> if instance exists (which game state should always exist)
	/// </summary>
	public static bool IsBlocked => instance != null && instance.blocked;
	
	/// <summary>
	/// Returns true if we have a dialog we are waiting on
	/// </summary>
	public static bool HasDialog => DialogLoader.ScheduledDialogCount > 0;
	
	/// <summary>
	/// Returns the current state name
	/// </summary>
	public static State CurrentState => instance != null && globalStateMachine != null ? globalStateMachine.currentState : null;
}