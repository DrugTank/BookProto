using System;
using UniRx;
using UnityEngine;

public class BookManager : MonoBehaviour
{
    public static BookManager Instance { get; private set; }

    public Func<Book> OnBookChanged;

    public Book selectedBook;

    private void Awake()
    {
        Instance = this;

        Observable.EveryGameObjectUpdate()
            .TakeUntilDestroy(gameObject)
            .Where(x => Input.GetKeyDown(KeyCode.Escape))
            .Subscribe(_ => SetSelectedBook(null));
    }

    public void SetSelectedBook(Book book)
    {
        selectedBook = book;

        OnBookChanged?.Invoke();
    }
}