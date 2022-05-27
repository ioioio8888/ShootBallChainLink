using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine;

public class GameObjectGameEventListener : MonoBehaviour
{
    public GameObjectGameEvent Event;
    public UnityEvent<GameObject> Response;

    private void OnEnable()
    { Event.RegisterListener(this); }

    private void OnDisable()
    { Event.UnregisterListener(this); }

    public void OnEventRaised(GameObject value)
    { Response.Invoke(value); }
}