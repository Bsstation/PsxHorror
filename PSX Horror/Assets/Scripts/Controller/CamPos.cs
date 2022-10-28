using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using Cinemachine;

public class CamPos : MonoBehaviour
{
    public static CamPos instance;

    //public Camera cam;
    public Transform player;
    CinemachineClearShot clearShot;

    private void Awake()
    {
        clearShot = GetComponent<CinemachineClearShot>();
        Init();
    }

    void Init()
    {
        instance = this;

        if(!clearShot) clearShot = GetComponent<CinemachineClearShot>();

        if (!player) player = GameObject.FindGameObjectWithTag("Player").transform;

        if(clearShot && player) clearShot.m_LookAt = player;
    }


    public void SetInstance()
    {
        Init();
    }

    public void ImpulseNoise()
    {
        /*
        foreach (Transform cam in transform)
        {
            if (cam.gameObject.activeInHierarchy)
            {
                cam.GetComponent<CinemachineImpulseSource>().GenerateImpulse();
            }
        }*/
        GetComponent<CinemachineImpulseSource>().GenerateImpulse();
    }

    public int GetActiveCam()
    {
        for(int i =0; i < clearShot.ChildCameras.Length; i++)
        {

        }

        return 0;
    }
}
