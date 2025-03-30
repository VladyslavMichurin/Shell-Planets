using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateAround : MonoBehaviour
{
    public Transform pivot;
    [Range(0.001f, 100.0f)]
    public float rotationSpeed = 10;
    private void FixedUpdate()
    {
        transform.RotateAround(pivot.position, Vector3.up, Time.deltaTime * rotationSpeed);
    }

}
