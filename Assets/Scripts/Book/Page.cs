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
    Effect,
    Grabbable
}

public enum SpecifyAnimatorLocation
{ 
    None,
    Self,
    Child
}


public class Page : MonoBehaviour
{
    private Book book;
    private Animator animator;

    [SerializeField]
    private bool hasAnimation;
    [SerializeField]
    private SpecifyAnimatorLocation specifyAnimatorLocation;    

    [Header("Page's material change")]
    public bool automaticallyChangeMaterial;
    private SkinnedMeshRenderer skinnedMeshRenderer;

    [Header("Select type you want")]
    public InteractionType interactionType;

    [HideInInspector]
    public ReactiveProperty<bool> fullyOpened = new ReactiveProperty<bool>(false);
    [HideInInspector]
    public ReactiveProperty<bool> fullyClosed = new ReactiveProperty<bool>(false);

    // Components
    [Header("Drag and Drop proper component, Match with type")]
    public VideoPlayer videoPlayer;
    public GameObject interactableObject;
    public ParticleSystem particle;
    public AudioSource audioSource;
    public Grabbable grabbableObject;

    private readonly int RL = Animator.StringToHash("RL");
    private readonly int LR = Animator.StringToHash("LR");

    private void Awake()
    {
        book = GetComponentInParent<Book>();

        if (hasAnimation)
        {
            switch (specifyAnimatorLocation)
            {
                case SpecifyAnimatorLocation.Self:
                    animator = GetComponent<Animator>();
                    break;

                case SpecifyAnimatorLocation.Child:
                    animator = GetComponentInChildren<Animator>();
                    break;
            }
        }

        skinnedMeshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
    }

    private void Start()
    {
        fullyOpened
            .TakeUntilDestroy(gameObject)
            .Where(x => fullyOpened.Value)
            .Subscribe(_ => book.SetInteract());

        fullyClosed
            .TakeUntilDestroy(gameObject)
            .Where(x => fullyClosed.Value)
            .Subscribe(_ => book.SetInteract());

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

            case InteractionType.Grabbable:
                grabbableObject.gameObject.SetActive(false);
                grabbableObject.gameObject.SetActive(true);
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

            case InteractionType.Grabbable:
                grabbableObject.gameObject.SetActive(false);
                break;
        }
    }

    public void TriggerAnimation(int direction)
    {
        if (!hasAnimation) return;

        switch (direction)
        {
            case 1:
                animator.SetTrigger(RL);
                break;

            case -1:
                animator.SetTrigger(LR);
                break;
        }
    }

    public void ChangeMaterial(Material material)
    {
        if(automaticallyChangeMaterial) skinnedMeshRenderer.material = material;
    }
}
