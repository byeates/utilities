using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// For use with the state machine
/// </summary>
public class State
{
	public string name { get; private set; }
	public List<StateMachine.OnEnterState> onEnterCallbacks { get; private set; }
	public List<StateMachine.OnExitState> onExitCallbacks { get; private set; }

	public State(string name, StateMachine.OnEnterState onEnter = null, StateMachine.OnExitState onExit = null)
	{
		this.name = name;
		
		onEnterCallbacks = new List<StateMachine.OnEnterState>();
		onExitCallbacks = new List<StateMachine.OnExitState>();

		AttachEnterCallback(onEnter);
		AttachExitCallback(onExit);
	}

	public void Destroy()
	{
		onEnterCallbacks?.Clear();
		onExitCallbacks?.Clear();
	}

	public void AttachEnterCallback(StateMachine.OnEnterState onEnter)
	{
		if (onEnter != null && !onEnterCallbacks.Contains(onEnter))
		{
			onEnterCallbacks.Add(onEnter);
		}
	}
	
	public void RemoveEnterCallback(StateMachine.OnEnterState onEnter)
	{
		if (onEnter != null && !onEnterCallbacks.Contains(onEnter))
		{
			onEnterCallbacks.Remove(onEnter);
		}
	}

	public void AttachExitCallback(StateMachine.OnExitState onExit)
	{
		if (onExit != null && !onExitCallbacks.Contains(onExit))
		{
			onExitCallbacks.Add(onExit);
		}
	}
	
	public void RemoveExitCallback(StateMachine.OnExitState onExit)
	{
		if (onExit != null && !onExitCallbacks.Contains(onExit))
		{
			onExitCallbacks.Remove(onExit);
		}
	}

	/// <summary>
	/// Returns the state name
	/// </summary>
	/// <returns></returns>
	public override string ToString()
	{
		return name;
	}
}