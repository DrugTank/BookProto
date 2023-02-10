using System;
using UniRx;
using UnityEngine;

public class Book : MonoBehaviour
{
    [Header("Text with 'Press Button F' ")]
    [SerializeField]
    private GameObject interactTxt;

    [Header("Drag and Drop positions you want to lerp")]
    [SerializeField]
    private Transform lastLerpPosition;
    private Transform[] viewPosition;

    [Header("If you added <Page> script in children, don't need to manually set this")]
    public Page[] pages;

    [Header("Speed")]
    [SerializeField]
    private float positioningSpeed;
    [SerializeField]
    private float pageTurningSpeed;

    [Header("Material you wanna change all")]
    [SerializeField]
    private Material material;

    [Header("Starts with -1")]
    [SerializeField]
    private int currentPage;

    private int totalPages;

    private float[] pageAngle;
    private float[] pageAngleMin;
    private float[] pageAngleMax;

    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private GameObject firstLerpPosition;

    private bool isPositioned1 = false;
    private bool isPositioned2 = false;
    private bool isReturning = false;
    private bool canInteract = false;
    private bool canTurn = false;

    private readonly string lerpTransformName = "First Lerp Position";

    public Action OnPageChanged;

    private void Awake()
    {
        pages = GetComponentsInChildren<Page>();
        currentPage = -1;
    }

    private void Start()
    {
        SetInteract(false);

        MakeFirstLerpPosition();

        totalPages = pages.Length;
        pageAngle = new float[totalPages];
        pageAngleMin = new float[totalPages];
        pageAngleMax = new float[totalPages];

        initialPosition = transform.position;
        initialRotation = transform.rotation;

        for (int i = 0; i < totalPages; i++)
        {
            pageAngleMin[i] = pages[i].transform.localEulerAngles.y;
            pageAngleMax[i] = pages[i].transform.localEulerAngles.y + 170;
        }

        ChangeMaterial();

        Observable.EveryGameObjectUpdate()
            .TakeUntilDestroy(gameObject)
            .Subscribe(_ => ControllingBook());

        Observable.EveryGameObjectUpdate()
            .TakeUntilDestroy(gameObject)
            .Subscribe(_ => PositioningBook());

        Observable.EveryGameObjectUpdate()
            .TakeUntilDestroy(gameObject)
            .Subscribe(_ => ReturningBook());

        BookManager.Instance.OnBookChanged += ResetBook;
        OnPageChanged += () => SetInteract(false);
    }

    private void ControllingBook()
    {
        if (BookManager.Instance.selectedBook == this)
        {
            if (Input.GetKeyDown(KeyCode.Q)) TurnPage(1);
            if (Input.GetKeyDown(KeyCode.E)) TurnPage(-1);

            InteractionWithPage();

            for (int i = 0; i < totalPages; i++)
            {
                if (currentPage >= i)
                {
                    if (pageAngle[i] < pageAngleMax[i])
                        pageAngle[i] += Time.deltaTime * pageTurningSpeed;
                }

                else
                {
                    if (pageAngle[i] > pageAngleMin[i])
                        pageAngle[i] -= Time.deltaTime * pageTurningSpeed;
                }

                pageAngle[i] = Mathf.Clamp(pageAngle[i], pageAngleMin[i], pageAngleMax[i]);

                pages[i].fullyOpened.Value = pageAngle[i] >= pageAngleMax[i];
                pages[i].fullyClosed.Value = pageAngle[i] <= pageAngleMin[i];

                pages[i].transform.localEulerAngles = new Vector3(0, pageAngle[i], 0);
            }
        }

        else
        {
            for (int i = 0; i < totalPages; i++)
            {
                if (pageAngle[i] > pageAngleMin[i])
                    pageAngle[i] -= Time.deltaTime * pageTurningSpeed;

                pageAngle[i] = Mathf.Clamp(pageAngle[i], pageAngleMin[i], pageAngleMax[i]);
                pages[i].transform.localEulerAngles = new Vector3(0, pageAngle[i], 0);
            }
        }
    }

    public void TurnPage(int direction)
    {
        if (!canTurn) return;

        switch (direction)
        {
            case 1:
                if (currentPage < totalPages - 1) currentPage++;
                pages[currentPage].TriggerAnimation(1);
                break;

            case -1:
                if (currentPage > -1) currentPage--;
                pages[currentPage + 1].TriggerAnimation(-1);
                break;
        }

        OnPageChanged?.Invoke();
    }

    private void PositioningBook()
    {
        if (BookManager.Instance.selectedBook != this) return;

        if (Vector3.Distance(transform.position, viewPosition[0].position) > 0.05f && !isPositioned1)
        {
            transform.position = Vector3.Lerp(transform.position, viewPosition[0].position, positioningSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Lerp(transform.rotation, viewPosition[0].rotation, positioningSpeed * Time.deltaTime);
        }

        else if (!isPositioned2)
        {
            isPositioned1 = true;

            if (Vector3.Distance(transform.position, viewPosition[1].position) > 0.01f && !isPositioned2)
            {
                transform.position = Vector3.Lerp(transform.position, viewPosition[1].position, positioningSpeed * Time.deltaTime);
                transform.rotation = Quaternion.Lerp(transform.rotation, viewPosition[1].rotation, positioningSpeed * Time.deltaTime);
            }

            else
            {
                isPositioned2 = true;
                canInteract = true;
                canTurn = true;

                transform.position = viewPosition[1].position;
                transform.rotation = viewPosition[1].rotation;
            }
        }
    }

    private void ReturningBook()
    {
        if (BookManager.Instance.selectedBook == this || transform.position == initialPosition) return;

        if (Vector3.Distance(transform.position, viewPosition[0].position) > 0.05f && !isReturning)
        {
            transform.position = Vector3.Lerp(transform.position, viewPosition[0].position, positioningSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Lerp(transform.rotation, viewPosition[0].rotation, positioningSpeed * Time.deltaTime);
        }

        else if (Vector3.Distance(transform.position, initialPosition) > 0.01f)
        {
            isReturning = true;
            transform.position = Vector3.Lerp(transform.position, initialPosition, positioningSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Lerp(transform.rotation, initialRotation, positioningSpeed * Time.deltaTime);
        }

        else
        {
            isReturning = false;
            transform.position = initialPosition;
            transform.rotation = initialRotation;
        }
    }

    private void ResetBook()
    {
        if (BookManager.Instance.selectedBook != this)
        {
            currentPage = -1;

            isPositioned1 = false;
            isPositioned2 = false;
            canTurn = false;

            SetInteract(false);

            for (int i = 0; i < totalPages; i++)
            {
                pages[i].TriggerAnimation(-1);
                pages[i].fullyOpened.Value = false;
                pages[i].fullyClosed.Value = true;
                pages[i].CancelInteraction();
            }
        }
    }

    private void InteractionWithPage()
    {
        if (BookManager.Instance.selectedBook != this) return;

        if (Input.GetKeyDown(KeyCode.F) && TryInteractWIthPage() && canInteract)
        {
            pages[currentPage].Interaction();
        }
    }

    public void SetInteract(bool Set = true)
    {
        if (TryInteractWIthPage())
        {
            interactTxt.SetActive(Set);
            canInteract = Set;
        }

        else
        {
            interactTxt.SetActive(false);
            canInteract = false;
        }
    }

    private bool TryInteractWIthPage()
    {
        if (currentPage < totalPages - 1 && currentPage > -1) return true;
        else return false;
    }

    private void ChangeMaterial()
    {
        if (material != null)
        {
            for (int i = 0; i < totalPages; i++)
            {
                pages[i].ChangeMaterial(material);
            }
        }
    }

    private void MakeFirstLerpPosition()
    {
        viewPosition = new Transform[2];
        firstLerpPosition = new GameObject();
        firstLerpPosition.name = lerpTransformName;
        firstLerpPosition.transform.parent = lastLerpPosition;
        firstLerpPosition.transform.position = transform.position + transform.right;
        firstLerpPosition.transform.rotation = transform.rotation;
        viewPosition[0] = firstLerpPosition.transform;
        viewPosition[1] = lastLerpPosition;
    }
}
