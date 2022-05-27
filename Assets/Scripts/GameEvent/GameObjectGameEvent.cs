using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameObject Game Event", menuName = "GameEvent/GameObject Game Event")]
public class GameObjectGameEvent : ScriptableObject
{
	private List<GameObjectGameEventListener> listeners =
		new List<GameObjectGameEventListener>();

	public virtual void Raise(GameObject value)
	{
		for (int i = listeners.Count - 1; i >= 0; i--)
			listeners[i].OnEventRaised(value);
	}

	public void RegisterListener(GameObjectGameEventListener listener)
	{ listeners.Add(listener); }

	public void UnregisterListener(GameObjectGameEventListener listener)
	{ listeners.Remove(listener); }
}