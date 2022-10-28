using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LensFlareBehaviour : MonoBehaviour
{
    public Light light;
    public GameObject flash;
    // Start is called before the first frame update
    void Start()
    {
        flash.SetActive(true);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (light == null || flash == null) return;

        flash.SetActive(light.enabled);

        Vector3 look = (light.transform.position + light.transform.forward * 10);
        flash.transform.LookAt(look);
    }
}
