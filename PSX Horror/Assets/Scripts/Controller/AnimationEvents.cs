using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationEvents : MonoBehaviour
{
    float currentLeft, lastLeft;
    float currentRight, lastRight;

    Animator anim;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    private void Update()
    {
        currentLeft = anim.GetFloat("FootstepL");
        if (currentLeft > 0 && lastLeft < 0)
            CreateFootstep();
        lastLeft = anim.GetFloat("FootstepL");

        currentRight = anim.GetFloat("FootstepR");
        if (currentRight < 0 && lastRight > 0)
            CreateFootstep();
        lastRight = anim.GetFloat("FootstepR");
    }
    
    public void CreateFootstep()
    {
        RaycastHit hit;
        Vector3 origin = transform.position + transform.up;
        Vector3 direction = Vector3.down;

        if (Physics.Raycast(origin, direction, out hit, 1.5f, FXController.instance.layers.floor))
        {
            TakeAudioFootstep(hit.collider.tag);
        }
        else
        {
            var sfx = Resources.Load("FX/Footstep") as GameObject;
            GameObject tempSFX = Instantiate(sfx);
            tempSFX.transform.position = transform.position;
            float random = Random.Range(0.8f, 1.1f);
            tempSFX.GetComponent<AudioSource>().pitch = random;

            Destroy(tempSFX, 3);
        }
    }

    public void TakeAudioFootstep(string tag)
    {
        if (Resources.Load("FX/Footstep " + tag))
        {
            var sfx = Resources.Load("FX/Footstep " + tag) as GameObject;
            GameObject tempSFX = Instantiate(sfx);
            tempSFX.transform.position = transform.position;
            float random = Random.Range(0.8f, 1.1f);
            tempSFX.GetComponent<AudioSource>().pitch = random;

            Destroy(tempSFX, 1);
        }
        else
        {
            var sfx = Resources.Load("FX/Footstep") as GameObject;
            GameObject tempSFX = Instantiate(sfx);
            tempSFX.transform.position = transform.position;
            float random = Random.Range(0.8f, 1.1f);
            tempSFX.GetComponent<AudioSource>().pitch = random;

            Destroy(tempSFX, 1);
        }
    }
}
