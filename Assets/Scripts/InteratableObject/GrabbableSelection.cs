using UniRx;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.UIElements;

public class GrabbableSelection : MonoBehaviour
{
    public static GrabbableSelection Instance { get; private set; }

    [SerializeField]
    private LayerMask grabbableLayer;

    [SerializeField]
    private float power;

    [SerializeField]
    private float positioningSpeed;

    [SerializeField]
    private float rotationSpeed;

    [SerializeField]
    private Transform grabbingPosition;

    private Grabbable grabbableObject;

    private bool isGrabbing;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        Observable.EveryUpdate()
            .TakeUntilDestroy(gameObject)
            .Where(x => Input.GetMouseButtonDown(0))
            .Subscribe(_ => Selection());

        Observable.EveryUpdate()
            .TakeUntilDestroy(gameObject)
            .Subscribe(_ => ControllingGrabbable());

        Observable.EveryGameObjectUpdate()
            .TakeUntilDestroy(gameObject)
            .Subscribe(_ => Throw());

        BookManager.Instance.OnBookChanged += ResetGrabbable;
    }

    private void Selection()
    {
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out RaycastHit hit, float.MaxValue))
        {
            if (hit.transform.TryGetComponent(out Grabbable grabbable))
            {
                grabbableObject = grabbable;

                isGrabbing = true;
            }
        }
    }

    private void ControllingGrabbable()
    {
        if (grabbableObject != null && isGrabbing)
        {
            grabbableObject.transform.position = Vector3.Lerp(grabbableObject.transform.position, grabbingPosition.transform.position, positioningSpeed * Time.deltaTime);
            grabbableObject.SetKinematic(true);

            if (Input.GetKey(KeyCode.R))
            {
                grabbableObject.transform.localEulerAngles += new Vector3(0, rotationSpeed * Time.deltaTime, 0);
            }

            else if (Input.GetKey(KeyCode.T))
            {
                grabbableObject.transform.localEulerAngles += new Vector3(0, 0, rotationSpeed * Time.deltaTime);
            }
        }
    }

    private void Throw()
    {
        if (grabbableObject != null && Input.GetKeyDown(KeyCode.V))
        {
            isGrabbing = false;

            grabbableObject.SetKinematic(false);

            Rigidbody rb = grabbableObject.Rigidbody;

            Vector3 forceVec = grabbingPosition.forward * power;
            rb.AddForce(forceVec, ForceMode.Impulse);

            grabbableObject = null;
        }
    }

    public void ResetGrabbable()
    {
        grabbableObject = null;
        isGrabbing = false;
    }
}
