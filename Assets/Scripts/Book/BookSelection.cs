using UniRx;
using UnityEngine;

public class BookSelection : MonoBehaviour
{
    [SerializeField]
    private LayerMask bookLayer;

    private void Start()
    {
        Observable.EveryUpdate()
        .TakeUntilDestroy(gameObject)
            .Subscribe(_ => Selection());
    }

    private void Selection()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out RaycastHit hit, 20f, bookLayer))
            {
                if (hit.transform.TryGetComponent(out Book book))
                {
                    BookManager.Instance.SetSelectedBook(book);
                }
            }
        }
    }
}