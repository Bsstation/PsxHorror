using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TriggerEvents : MonoBehaviour
{
    public UnityEvent start;

    public void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerController>())
        {
            GetComponent<Collider>().enabled = false;
            Play();
        }
    }

    public void Play()
    {
        start.Invoke();
    }
}
