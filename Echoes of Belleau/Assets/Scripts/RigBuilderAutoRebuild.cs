using System.Collections;
using UnityEngine;
using UnityEngine.Animations.Rigging;

[DefaultExecutionOrder(10000)]
public class RigBuilderAutoRebuild : MonoBehaviour
{
    [SerializeField] RigBuilder rigBuilder;
    [SerializeField] Animator animator;

    void Reset()
    {
        rigBuilder = GetComponent<RigBuilder>();
        animator = GetComponent<Animator>();
    }

    void OnEnable()
    {
        if (!rigBuilder) rigBuilder = GetComponent<RigBuilder>();
        if (!animator) animator = GetComponent<Animator>();

        if (animator) animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;

        StartCoroutine(RebuildAfterStart());
    }

    IEnumerator RebuildAfterStart()
    {
        yield return null;
        yield return new WaitForEndOfFrame();
        yield return null;

        if (!rigBuilder) yield break;

        rigBuilder.Clear();
        rigBuilder.Build();
    }
}
