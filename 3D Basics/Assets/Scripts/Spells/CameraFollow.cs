using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    private Vector3 offset;
    [SerializeField] private float rotationSpeed;


    void Start()
    {
        offset = target.position - transform.position;
    }

    void Update()
    {
        if (!SpellManager.Instance.pauseGame)
        {
            transform.position = target.position - offset;
            float mouseX = Input.GetAxis("Mouse X");
            if (mouseX != 0)
            {
                transform.RotateAround(target.transform.position, Vector3.up, mouseX * rotationSpeed * Time.deltaTime);
            }
        }
    }
}
