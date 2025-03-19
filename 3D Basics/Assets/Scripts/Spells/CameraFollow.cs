using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public KeyCode panLeft;
    public KeyCode panRight;
    public bool manualControl;

    public Transform target;
    private Vector3 offset;
    public float rotationSpeed;


    void Start()
    {
        offset = target.position - transform.position;
    }

    void Update()
    {
        if (!SpellManager.Instance.pauseGame)
        {
            transform.position = target.position - offset;
            
            if (manualControl)
            {
                int mouseX = 0;
                if (Input.GetKey(panLeft))
                    mouseX--;
                if (Input.GetKey(panRight))
                    mouseX++;
                if (mouseX != 0)
                {
                    transform.RotateAround(target.transform.position, Vector3.up, mouseX * rotationSpeed * Time.deltaTime);
                }
            }

            else
            {
                float currY = transform.rotation.eulerAngles.y;
                float targetY = target.transform.rotation.eulerAngles.y;
                float yRot = Mathf.LerpAngle(currY, targetY, Time.deltaTime * rotationSpeed);
                transform.RotateAround(target.transform.position, Vector3.up, yRot - currY);
            }
        }
    }
}
