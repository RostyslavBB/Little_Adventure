using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gate : MonoBehaviour
{
    public GameObject gateVisual;
    public float OpenDuration = 2f;
    public float OpenTatgetY = -1.5f;

    private Collider _gatecolider;

    private void Awake()
    {
        _gatecolider = GetComponent<Collider>();
    }

    IEnumerator OpenGateAnimation()
    {
        float currentOpenDuration = 0;
        Vector3 startPos = gateVisual.transform.position;
        Vector3 targetPos = startPos + Vector3.up * OpenTatgetY;

        while(currentOpenDuration < OpenDuration) 
        {
            currentOpenDuration += Time.deltaTime;
            gateVisual.transform.position = Vector3.Lerp(startPos, targetPos, currentOpenDuration / OpenDuration);
            yield return null;
        }
        _gatecolider.enabled = false;
    }
    public void Open()
    {
        StartCoroutine(OpenGateAnimation());
    }
}
