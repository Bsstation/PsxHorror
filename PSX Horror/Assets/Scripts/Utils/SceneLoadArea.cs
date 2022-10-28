using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneLoadArea : MonoBehaviour
{
    public GameObject area;

    public void Start()
    {
        GetComponent<Collider>().enabled = true;
    }

    private void OnEnable()
    {
        GetComponent<Collider>().enabled = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerController>())
        {
            GameObject[] scenes = GameObject.FindGameObjectsWithTag("Scene");

            foreach (GameObject s in scenes)
            {
                if (s == area)
                    s.SetActive(true);
                else
                    s.SetActive(false);
            }
            area.SetActive(true);
            GetComponent<Collider>().enabled = false;
        }
    }
}
