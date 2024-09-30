using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    public float horizontalAxis;
    public float verticalAxis;
    public bool mouseButtonDown;
    public bool spaceKeyDown;
    void Update()
    {
        if (!mouseButtonDown && Time.timeScale != 0)
            mouseButtonDown = Input.GetMouseButtonDown(0);
        if (!spaceKeyDown && Time.timeScale !=0)
            spaceKeyDown = Input.GetKeyDown(KeyCode.Space);

        horizontalAxis = Input.GetAxisRaw("Horizontal");
        verticalAxis = Input.GetAxisRaw("Vertical");
    }

    private void OnDisable()
    {
        ClearCache();
    }
    public void ClearCache()
    {
        mouseButtonDown = false;
        spaceKeyDown = false;
        horizontalAxis = 0;
        verticalAxis = 0;
    }
}
