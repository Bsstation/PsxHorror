using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RandomImage : MonoBehaviour
{
    public Sprite[] sprites;
    Image image;
    int currentImage;

    float currentTimeToChange;
    public float timeToChange;

    // Start is called before the first frame update
    void Start()
    {
        image = GetComponent<Image>();
        int n = Random.Range(0, sprites.Length);
        currentImage = n;
        image.sprite = sprites[currentImage];
    }

    // Update is called once per frame
    void Update()
    {
        currentTimeToChange += Time.unscaledDeltaTime;
        if(currentTimeToChange > timeToChange)
        {
            currentTimeToChange = 0;
            int n = Random.Range(0, sprites.Length);

            while(n == currentImage)
                n = Random.Range(0, sprites.Length);

            currentImage = n;
            image.sprite = sprites[currentImage];
        }
    }
}
