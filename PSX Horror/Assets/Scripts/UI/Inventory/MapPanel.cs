using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapPanel : MonoBehaviour
{
    MapController mc;

    public Camera cam;

    public RawImage mapImage;
    public Text notHaveText, mapTitle;
    Transform playerPos;
    Vector3 playerInitScale;

    public bool zoom;

    public float dragSpeed = -4;
    public float zoomSmooth = 1;
    public float movSmooth = 3;
    private bool isPanning, inDrag;

    Vector3 followPos;

    public GameObject normalGuide, mappingGuide;
    public GameObject[] objs;

    public bool dragging;

    // Start is called before the first frame update
    public void Init()
    {
        mc = MapController.instance;
        cam = GameObject.FindGameObjectWithTag("MapCam").GetComponent<Camera>();
        mc.Start();

        Update();
    }

    public void GetPlayerPos(GameObject gameObject)
    {
        playerInitScale = gameObject.transform.localScale;
        playerPos = gameObject.transform;
    }

    // Update is called once per frame
    public void Update()
    {
        foreach(GameObject obj in objs)
        {
            if (mc.haveMap)
            {
                if (!obj.activeInHierarchy)
                    obj.SetActive(true);
            }
            else
            {
                if (obj.activeInHierarchy)
                    obj.SetActive(false);
            }
        }

        if (mc && mc.haveMap)
        {
            mapImage.enabled = true;
            mapTitle.text = mc.areaName[Settings.instance.currentLanguage];

            if(playerPos)
                playerPos.transform.localScale = 3 * playerInitScale * (cam.orthographicSize / 15 * 0.075f);

            notHaveText.enabled = false;

            Control();

            if (zoom)
            {
                mapTitle.enabled = false;
                float newSize = Mathf.Lerp(cam.orthographicSize, mc.panoramicZoom / 3, zoomSmooth * Time.unscaledDeltaTime);
                cam.orthographicSize = newSize;

                cam.transform.position = Vector3.Lerp(cam.transform.position, followPos, movSmooth * Time.unscaledDeltaTime);

                normalGuide.SetActive(false);
                mappingGuide.SetActive(true);
            }
            else
            {
                mapTitle.enabled = true;
                dragging = false;
                float newSize = Mathf.Lerp(cam.orthographicSize, mc.panoramicZoom, zoomSmooth * Time.unscaledDeltaTime);
                cam.orthographicSize = newSize;

                Vector3 newPos = new Vector3(mc.map.transform.position.x, 10, mc.map.transform.position.z);
                cam.transform.position = Vector3.Lerp(cam.transform.position, newPos, movSmooth * Time.unscaledDeltaTime);

                normalGuide.SetActive(true);
                mappingGuide.SetActive(false);
            }
        }
        else
        {
            mapTitle.enabled = false;
            dragging = false;
            mapImage.enabled = false;
            notHaveText.enabled = true;

            if (Input.GetButtonDown("Cancel") || (InputManager.instance && (Input.GetKeyDown(InputManager.instance.kKeys.sprint) ||
                InputManager.instance.GetJoyButtonDown("B"))))
            {
                InventoryUI.instance.PlayCancelAudio();
                GameManager.instance.ChangeStateToGame();
            }
        }
    }

    void Control()
    {
        if (gameObject.activeInHierarchy)
        {
            if ((Input.GetKeyDown(InputManager.instance.kKeys.action) || Input.GetButtonDown("Submit") || (Input.GetKeyDown(KeyCode.Mouse0) && !Settings.instance.cursorOn) ||
                InputManager.instance.GetJoyButtonDown("A")))
            {
                GameManager.instance.ChangeSelected(null);
                zoom = !zoom;
                if (zoom)
                    InventoryUI.instance.PlayAcceptAudio();
                else
                    InventoryUI.instance.PlayCancelAudio();

                if (zoom && mc.haveMap)
                {
                    Transform player = GameManager.instance.player.transform;

                    Vector3 camCurrentPosZoom = Vector3.zero;
                    camCurrentPosZoom.x = Mathf.Clamp(player.position.x, -mc.clamp.x, mc.clamp.x);
                    camCurrentPosZoom.z = Mathf.Clamp(player.position.z, -mc.clamp.y, mc.clamp.y);

                    followPos = camCurrentPosZoom;
                }
            }

            if (Input.GetButtonDown("Cancel") || (InputManager.instance && (Input.GetKeyDown(InputManager.instance.kKeys.sprint) ||
                InputManager.instance.GetJoyButtonDown("B"))))
            {
                if (!zoom)
                {
                    InventoryUI.instance.PlayCancelAudio();
                    DisableCam();
                    GameManager.instance.ChangeStateToGame();
                }
                else
                {
                    InventoryUI.instance.PlayCancelAudio();
                    zoom = false;
                }
            }

            if (zoom)
            {
                Vector2 velocity = (InputManager.instance.mode == InputMode.keyboard) ? InputManager.instance.UiMovementWithMouse() :
                    InputManager.instance.JoystickMove();

                Movement(-velocity);
            }
        }
    }

    public void EnableCam()
    {
        if(mc.haveMap)
            cam.enabled = true;
    }

    public void DisableCam()
    {
        if(cam.enabled)
            cam.enabled = false;
    }

    public void Movement(Vector2 velocity)
    {
        Vector3 move = new Vector3(dragSpeed * (mc.panoramicZoom / 15) * -velocity.x, 0, dragSpeed * (mc.panoramicZoom / 15) * -velocity.y);

        Vector3 newPos = followPos + move;

        newPos.x = Mathf.Clamp(newPos.x, -mc.clamp.x + mc.map.transform.position.x, mc.clamp.x + mc.map.transform.position.x);
        newPos.z = Mathf.Clamp(newPos.z, -mc.clamp.y + mc.map.transform.position.z, mc.clamp.y + mc.map.transform.position.z);
        newPos.y = 10;

        followPos = newPos;
    }

    public void Zoom()
    {
        if (mc && mc.haveMap && !dragging)
        {
            zoom = !zoom;
            if (zoom)
            {
                InventoryUI.instance.PlayAcceptAudio();

                Transform player = GameManager.instance.player.transform;

                Vector3 camCurrentPosZoom = Vector3.zero;
                camCurrentPosZoom.x = Mathf.Clamp(player.position.x, -mc.clamp.x, mc.clamp.x);
                camCurrentPosZoom.z = Mathf.Clamp(player.position.z, -mc.clamp.y, mc.clamp.y);

                followPos = camCurrentPosZoom;
            }
            else
                InventoryUI.instance.PlayCancelAudio();
        }
    }

    public void Dragging()
    {
        if (zoom)
        {
            Vector2 velocity = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));

            Vector3 move = new Vector3(dragSpeed * (mc.panoramicZoom / 15) * -velocity.x, 0, dragSpeed * (mc.panoramicZoom / 15) * -velocity.y);

            Vector3 newPos = followPos + move;

            newPos.x = Mathf.Clamp(newPos.x, -mc.clamp.x + mc.map.transform.position.x, mc.clamp.x + mc.map.transform.position.x);
            newPos.z = Mathf.Clamp(newPos.z, -mc.clamp.y + mc.map.transform.position.z, mc.clamp.y + mc.map.transform.position.z);
            newPos.y = 10;

            followPos = newPos;
        }
    }

    public void InitDrag()
    {
        if(zoom)
            dragging = true;
    }

    public void EndDrag()
    {
        if(zoom)
            dragging = false;
    }
}
