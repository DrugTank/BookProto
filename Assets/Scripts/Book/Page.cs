using UnityEngine;
using UnityEngine.Video;
using UniRx;

public enum InteractionType
{
    Null,
    PoppingUp,
    Animation,
    Video,
    Particle,
    Sound,
}

public class Page : MonoBehaviour
{
    private Book book;

    public InteractionType interactionType;

    public ReactiveProperty<bool> fullyOpened = new ReactiveProperty<bool>(false);
    public ReactiveProperty<bool> fullyClosed = new ReactiveProperty<bool>(false);

    // Components
    public VideoPlayer videoPlayer;
    public GameObject interactableObject;
    public ParticleSystem particle;
    public AudioSource audioSource;

    private void Awake()
    {
        book = GetComponentInParent<Book>();
    }

    private void Start()
    {
        fullyOpened
            .TakeUntilDestroy(gameObject)
            .Where(x => fullyOpened.Value)
            .Subscribe(_ => book.SetInteractText());

        fullyClosed
            .TakeUntilDestroy(gameObject)
            .Where(x => fullyClosed.Value)
            .Subscribe(_ => book.SetInteractText());


        book.OnPageChanged += CancelInteraction;
    }

    public void Interaction()
    {
        if (!fullyOpened.Value) return;

        switch (interactionType)
        {
            case InteractionType.PoppingUp:
                interactableObject.SetActive(true);
                break;

            case InteractionType.Animation:
                break;

            case InteractionType.Video:
                videoPlayer.Play();
                break;

            case InteractionType.Particle:
                particle.Play();
                break;

            case InteractionType.Sound:
                audioSource.Play();
                break;
        }
    }

    public void CancelInteraction()
    {
        switch (interactionType)
        {
            case InteractionType.PoppingUp:
                interactableObject.SetActive(false);
                break;

            case InteractionType.Video:
                videoPlayer.Stop();
                break;

            case InteractionType.Sound:
                audioSource.Stop();
                break;
        }
    }
}
