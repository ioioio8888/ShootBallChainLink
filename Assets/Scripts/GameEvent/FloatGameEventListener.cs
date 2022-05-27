using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine;

public class FloatGameEventListener : MonoBehaviour
{
    public FloatGameEvent Event;
    public UnityEvent<float> Response;

    private void OnEnable()
    { Event.RegisterListener(this); }

    private void OnDisable()
    { Event.UnregisterListener(this); }

    public void OnEventRaised(float value)
    { Response.Invoke(value); }
}