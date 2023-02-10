using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grabbable : MonoBehaviour
{
    private Rigidbody rb;
    private Vector3 initialPosition;
    private Quaternion initialRotation;

    [SerializeField]
    private Transform spawnObejct;

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

    private void OnCollisionEnter(Collision collision)
    {
        // if kinematic is true, it won't be triggered
        if (collision.gameObject.layer == LayerMask.NameToLayer("Plane") && spawnObejct != null)
        {
            gameObject.SetActive(false);

            GrabbableSelection.Instance.ResetGrabbable();

            ContactPoint contactPoint = collision.contacts[0];
            Transform objectTansform = Instantiate(spawnObejct, contactPoint.point, Quaternion.identity);
            GameManager.Instance.spawnParticle.transform.position = objectTansform.position;
            GameManager.Instance.spawnParticle.Play();

            Vector3 yRotation = new Vector3(GameManager.Instance.playerTf.position.x, objectTansform.position.y, GameManager.Instance.playerTf.position.z);
            objectTansform.LookAt(yRotation);
        }
    }

    public Rigidbody Rigidbody => rb;
}
