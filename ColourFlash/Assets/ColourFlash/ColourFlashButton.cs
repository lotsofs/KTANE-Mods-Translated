using UnityEngine;

public class ColourFlashButton : MonoBehaviour
{
    public Animator ButtonDepressAnimator;
    public KMSelectable KMSelectable;

    public KMAudio KMAudio
    {
        get
        {
            return GetComponent<KMAudio>();
        }
    }

    void Start()
    {
        KMSelectable.OnInteract += DoDepressAnimation;
    }

    private bool DoDepressAnimation()
    {
        ButtonDepressAnimator.SetTrigger("PressTrigger");
        KMAudio.PlaySoundAtTransform("ButtonClick", transform);
        return false;
    }
}
