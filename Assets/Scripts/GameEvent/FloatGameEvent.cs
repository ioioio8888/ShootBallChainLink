using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Float Game Event", menuName = "GameEvent/Float Game Event")]
public class FloatGameEvent : ScriptableObject
{
	private List<FloatGameEventListener> listeners =
		new List<FloatGameEventListener>();

	public virtual void Raise(float value)
	{
		for (int i = listeners.Count - 1; i >= 0; i--)
			listeners[i].OnEventRaised(value);
	}

	public void RegisterListener(FloatGameEventListener listener)
	{ listeners.Add(listener); }

	public void UnregisterListener(FloatGameEventListener listener)
	{ listeners.Remove(listener); }
}