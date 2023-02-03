using UniRx;
using Unity.VisualScripting;
using UnityEngine;

public class Book : MonoBehaviour
{
    [SerializeField]
    private Transform[] pages;

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

    private void Start()
    {
        totalPages = pages.Length;
        pageAngle = new float[totalPages];
        pageAngleMin = new float[totalPages];
        pageAngleMax = new float[totalPages];

        initialPosition = transform.position;
        initialRotation = transform.rotation;

        for (int i = 0; i < totalPages; i++)
        {
            pageAngleMin[i] = pages[i].localEulerAngles.y;
            pageAngleMax[i] = pages[i].localEulerAngles.y + 170;
        }

        Observable.EveryGameObjectUpdate()
            .TakeUntilDestroy(gameObject)
            .Subscribe(_ => ControllingBook());

        Observable.EveryGameObjectUpdate()
            .TakeUntilDestroy(gameObject)
            .Subscribe(_ => PositioningBook());

        BookManager.Instance.OnBookChanged += ResetBook;
    }

    private void ControllingBook()
    {
        if (BookManager.Instance.selectedBook == this)
        {
            if (Input.GetKeyDown(KeyCode.Q)) TurnPage(1);
            if (Input.GetKeyDown(KeyCode.E)) TurnPage(-1);

            for (int i = 0; i < totalPages; i++)
            {
                if (currentPage >= i) pageAngle[i] += Time.deltaTime * pageTurningSpeed;
                else pageAngle[i] -= Time.deltaTime * pageTurningSpeed;

                pageAngle[i] = Mathf.Clamp(pageAngle[i], pageAngleMin[i], pageAngleMax[i]);
                pages[i].localEulerAngles = new Vector3(0, pageAngle[i], 0);
            }
        }

        else
        {
            for (int i = 0; i < totalPages; i++)
            {
                pageAngle[i] -= Time.deltaTime * pageTurningSpeed;

                pageAngle[i] = Mathf.Clamp(pageAngle[i], pageAngleMin[i], pageAngleMax[i]);
                pages[i].localEulerAngles = new Vector3(0, pageAngle[i], 0);
            }
        }
    }

    public void TurnPage(int direction)
    {
        switch (direction)
        {
            case 1:
                if (currentPage < totalPages - 1) currentPage++;
                break;

            case -1:
                if (currentPage > -1) currentPage--;
                break;
        }
    }

    private void PositioningBook()
    {
        if (BookManager.Instance.selectedBook != this) return;

        if (Vector3.Distance(transform.position, GameManager.Instance.bookAimingTransform.position) > 1.5f)
        {
            transform.position = Vector3.Lerp(transform.position, GameManager.Instance.bookAimingTransform.position + (Vector3.up * 0.5f), Time.deltaTime);
        }

        transform.LookAt(GameManager.Instance.playerTransform.position + (Vector3.up * 0.5f));
    }

    private Book ResetBook()
    {
        if (BookManager.Instance.selectedBook != this)
        {
            currentPage = -1;

            transform.position = initialPosition;
            transform.rotation = initialRotation;
        }

        return this;
    }
}
