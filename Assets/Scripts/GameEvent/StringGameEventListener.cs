using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine;

public class StringGameEventListener : MonoBehaviour
{
    public StringGameEvent Event;
    public UnityEvent<string> Response;

    private void OnEnable()
    { Event.RegisterListener(this); }

    private void OnDisable()
    { Event.UnregisterListener(this); }

    public void OnEventRaised(string value)
    { Response.Invoke(value); }
}