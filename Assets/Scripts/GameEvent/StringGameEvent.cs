using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "String Game Event", menuName = "GameEvent/String Game Event")]
public class StringGameEvent : ScriptableObject
{
	private List<StringGameEventListener> listeners =
		new List<StringGameEventListener>();

	public virtual void Raise(string value)
	{
		for (int i = listeners.Count - 1; i >= 0; i--)
			listeners[i].OnEventRaised(value);
	}

	public void RegisterListener(StringGameEventListener listener)
	{ listeners.Add(listener); }

	public void UnregisterListener(StringGameEventListener listener)
	{ listeners.Remove(listener); }
}