using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource),typeof(Rigidbody))]
public class CartridgeSfx : MonoBehaviour
{
    AudioSource aud;
    Rigidbody rigid;
    public AudioClip clip;
    // Start is called before the first frame update
    void Start()
    {
        aud = GetComponent<AudioSource>();
        rigid = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    private void OnCollisionEnter(Collision collision)
    {
        aud.pitch = Random.Range(0.95f, 1.05f);
        aud.PlayOneShot(clip);
    }
}