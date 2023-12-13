using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

/// <summary>
/// SSM
/// </summary>
public class StateMachine
{
    //====================
    // PRIVATE
    //====================
    private List<State> states = new List<State>();
    
    //====================
    // PUBLIC
    //====================
    public State currentState { get; private set; }
    public List<string> previousStates { get; private set; }
    public string name;
    public delegate void OnEnterState();
    public delegate void OnExitState();

    public StateMachine(string name = "state machine")
    {
        this.name = name;
        previousStates = new List<string>();
    }

    /// <summary>
    /// Cleans up all states
    /// </summary>
    public void Destroy()
    {
	    foreach (var state in states)
	    {
		    state?.Destroy();
	    }
    }
    
    /// <summary>
    /// Add a state with callback handling
    /// </summary>
    /// <param name="stateName">name of the state</param>
    /// <param name="enterCallback">function to call when the statemachine switches to this state</param>
    /// <param name="exitCallback">function to call when the statemachine exist this state</param>
    public void AddState(string stateName, OnEnterState enterCallback = null, OnExitState exitCallback = null)
    {
        State state = FindOrCreate(stateName);
        state.AttachEnterCallback(enterCallback);
        state.AttachExitCallback(exitCallback);

        if (isState(stateName) && enterCallback != null)
        {
            enterCallback();
        }
    }
    
    /// <summary>
    /// Add a state with callback handling
    /// </summary>
    /// <param name="stateName">GameState enum state</param>
    /// <param name="enterCallback">function to call when the statemachine switches to this state</param>
    /// <param name="exitCallback">function to call when the statemachine exist this state</param>
    public void AddState(GameState.States stateName, OnEnterState enterCallback = null, OnExitState exitCallback = null)
    {
        AddState(stateName.ToString(), enterCallback, exitCallback);
    }

    /// <summary>
    /// Adds a method to call when entering the state
    /// </summary>
    /// <param name="stateName"></param>
    /// <param name="enterCallback"></param>
    public void AddEnterCallback(string stateName, OnEnterState enterCallback)
    {
        State state = FindOrCreate(stateName);
        state.AttachEnterCallback(enterCallback);
        
        if (isState(stateName) && enterCallback != null)
        {
            enterCallback();
        }
    }
    
    /// <summary>
    /// Adds a method to call when entering the state
    /// </summary>
    /// <param name="stateName"></param>
    /// <param name="enterCallback"></param>
    public void AddEnterCallback(GameState.States stateName, OnEnterState enterCallback)
    {
        AddEnterCallback(stateName.ToString(), enterCallback);
    }
    
    /// <summary>
    /// Adds a method to call when entering the state
    /// </summary>
    /// <param name="stateName"></param>
    /// <param name="enterCallback"></param>
    public void RemoveEnterCallback(GameState.States stateName, OnEnterState enterCallback)
    {
	    RemoveEnterCallback(stateName.ToString(), enterCallback);
    }
    
    /// <summary>
    /// Adds a method to call when entering the state
    /// </summary>
    /// <param name="stateName"></param>
    /// <param name="enterCallback"></param>
    public void RemoveEnterCallback(string stateName, OnEnterState enterCallback)
    {
	    State state = FindState(stateName);
	    state?.RemoveEnterCallback(enterCallback);
    }
    
    /// <summary>
    /// Adds a method to call when exiting the state
    /// </summary>
    /// <param name="stateName"></param>
    /// <param name="enterCallback"></param>
    public void AddExitCallback(GameState.States stateName, OnExitState exitCallback)
    {
        AddExitCallback(stateName.ToString(), exitCallback);
    }
    
    /// <summary>
    /// Adds a method to call when exiting the state
    /// </summary>
    /// <param name="stateName"></param>
    /// <param name="enterCallback"></param>
    public void AddExitCallback(string stateName, OnExitState exitCallback)
    {
        State state = FindOrCreate(stateName);
        state.AttachExitCallback(exitCallback);
    }
    
    /// <summary>
    /// Adds a method to call when entering the state
    /// </summary>
    /// <param name="stateName"></param>
    /// <param name="enterCallback"></param>
    public void RemoveExitCallback(GameState.States stateName, OnEnterState enterCallback)
    {
	    RemoveEnterCallback(stateName.ToString(), enterCallback);
    }
    
    /// <summary>
    /// Adds a method to call when entering the state
    /// </summary>
    /// <param name="stateName"></param>
    /// <param name="enterCallback"></param>
    public void RemoveExitCallback(string stateName, OnEnterState enterCallback)
    {
	    State state = FindState(stateName);
	    state?.RemoveEnterCallback(enterCallback);
    }

    /// <summary>
    /// Remove a state
    /// </summary>
    /// <param name="stateName"></param>
    public void RemoveState(string stateName)
    {
        for (int i = 0; i < states.Count; ++i)
        {
            if (states[i].name == stateName)
            {
               states.RemoveAt(i);
               break;
            }
        }
    }

    /// <summary>
    /// Update to a state
    /// </summary>
    /// <param name="stateName"></param>
    /// <param name="storePreviousState">holds onto the last state, in case we want to return to it after reverting</param>
    public void UpdateState(string stateName, bool storePreviousState = true)
    {
	    if (currentState != null)
	    {
		    // state machine is already in the current state, don't do anything
		    if (currentState.name == stateName)
		    {
			    return;
		    }
		    
		    CallOnExitState(currentState.name);

		    if (previousStates.Contains(currentState.name))
		    {
			    int index = previousStates.IndexOf(currentState.name);
			    previousStates.RemoveAt(index);
		    }

		    if (storePreviousState)
		    {
			    previousStates.Add(currentState.name);
		    }
	    }
	    
        currentState = FindOrCreate(stateName);
        CallOnEnterState(stateName);
    }

    /// <summary>
    /// Forward iteration of the state based on the order the states were added
    /// </summary>
    public void MoveNextState()
    {
	    int index = states.IndexOf(currentState);
	    index = index + 1 >= states.Count ? 0 : index + 1;

	    State state = states[index];
	    UpdateState(state.name);
    }

    public void CallOnEnterState(string stateName)
    {
        if (!string.IsNullOrEmpty(stateName))
        {
            for (int i = 0; i < states.Count; ++i)
            {
                if (states[i].name == stateName)
                {
                    for (int k = 0; k < states[i].onEnterCallbacks.Count; k++)
                    {
                        states[i].onEnterCallbacks[k]();
                    }
                }
            }
        }
    }
    
    public void CallOnExitState(string stateName)
    {
        if (!string.IsNullOrEmpty(stateName))
        {
            for (int i = 0; i < states.Count; ++i)
            {
                if (states[i].name == stateName)
                {
                    for (int k = 0; k < states[i].onExitCallbacks.Count; k++)
                    {
                        states[i].onExitCallbacks[k]();
                    }
                }
            }
        }
    }
    
    public void UpdateState(GameState.States stateName)
    {
        UpdateState(stateName.ToString());
    }

    /*================================================================================
    ANCILLARY    
    =================================================================================*/
    public bool isState(string stateName)
    {
        if (currentState != null)
        {
            return currentState.name == stateName;
        }
        return false;
    }
    
    public bool isState(GameState.States stateName)
    {
        if (currentState != null)
        {
            return currentState.name == stateName.ToString();
        }
        return false;
    }

    public bool HasState(string stateName)
    {
        for (int i = 0; i < states.Count; ++i)
        {
            if (states[i].name == stateName)
            {
                return true;
            }
        }

        return false;
    }
    
    public State FindState(string stateName)
    {
        for (int i = 0; i < states.Count; ++i)
        {
            if (states[i].name == stateName)
            {
                return states[i];
            }
        }

        return null;
    }
    
    private State FindOrCreate(string stateName)
    {
        if (HasState(stateName))
        {
            return FindState(stateName);
        }
        
        State state = new State(stateName);
        states.Add(state);
        return state;
    }
}
