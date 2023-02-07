using System;
using UniRx;
using UnityEngine;

public class Book : MonoBehaviour
{
    [SerializeField]
    private GameObject interactTxt;

    [SerializeField]
    private Transform[] viewPosition;

    public Page[] pages;

    [SerializeField]
    private float positioningSpeed;

    [SerializeField]
    private float pageTurningSpeed;

    [SerializeField]
    private int currentPage;

    [SerializeField]
    private int totalPages;

    private float[] pageAngle;
    private float[] pageAngleMin;
    private float[] pageAngleMax;

    private Vector3 initialPosition;
    private Quaternion initialRotation;

    private bool isPositioned1 = false;
    private bool isPositioned2 = false;
    private bool canInteract = false;
    private bool canTurn = false;

    public Action OnPageChanged;

    private void Awake()
    {
        if (viewPosition == null) viewPosition[1] = transform.parent; // this logic could be different

        pages = GetComponentsInChildren<Page>();
    }

    private void Start()
    {
        SetInteract(false);

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

        Observable.EveryGameObjectUpdate()
            .TakeUntilDestroy(gameObject)
            .Subscribe(_ => ControllingBook());

        Observable.EveryGameObjectUpdate()
            .TakeUntilDestroy(gameObject)
            .Subscribe(_ => PositioningBook());

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
                break;

            case -1:
                if (currentPage > -1) currentPage--;
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

    private void ResetBook()
    {
        if (BookManager.Instance.selectedBook != this)
        {
            currentPage = -1;

            isPositioned1 = false;
            isPositioned2 = false;
            canTurn = false;

            SetInteract(false);

            transform.position = initialPosition;
            transform.rotation = initialRotation;

            for (int i = 0; i < totalPages; i++)
            {
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
}
