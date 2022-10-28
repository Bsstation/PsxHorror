using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserSight : MonoBehaviour
{
    LineRenderer line;
    // Start is called before the first frame update
    void Start()
    {
        line = GetComponent<LineRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, 7, FXController.instance.layers.laserSight))
            line.SetPosition(1, new Vector3(0, 0, hit.distance));
        else
            line.SetPosition(1, new Vector3(0, 0, 7));
    }
}
