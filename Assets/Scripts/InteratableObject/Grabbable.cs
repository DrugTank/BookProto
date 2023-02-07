using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grabbable : MonoBehaviour
{
    private Rigidbody rb;
    private Vector3 initialPosition;
    private Quaternion initialRotation;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        initialPosition = transform.localPosition;
        initialRotation = transform.localRotation;
    }

    public void SetKinematic(bool set)
    {
        rb.isKinematic = set;
    }

    private void OnDisable()
    {
        GrabbableSelection.Instance.ResetGrabbable();
        rb.isKinematic = true;
        transform.localPosition = initialPosition;
        transform.localRotation = initialRotation;
    }

    public Rigidbody Rigidbody => rb;
}
