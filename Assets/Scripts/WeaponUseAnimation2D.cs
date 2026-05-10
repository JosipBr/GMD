using System.Collections;
using UnityEngine;

public class WeaponUseAnimation2D : MonoBehaviour
{
    [Header("Melee Attack Pose")]
    [SerializeField] private Vector3 attackPointLocalOffset = Vector3.zero;
    [SerializeField] private Vector3 attackLocalRotation = new Vector3(0f, 0f, 55f);

    [Header("Relative Use Fallback")]
    [SerializeField] private Vector3 relativeUsePositionOffset = new Vector3(-0.08f, 0.05f, 0f);
    [SerializeField] private Vector3 relativeUseRotationOffset = new Vector3(0f, 0f, 8f);

    [Header("Timing")]
    [SerializeField] private float moveToAttackDuration = 0.06f;
    [SerializeField] private float returnDuration = 0.10f;

    private Vector3 restingLocalPosition;
    private Quaternion restingLocalRotation;
    private Vector3 restingLocalScale;

    private Coroutine animationCoroutine;

    private void Awake()
    {
        StoreRestingPose();
    }

    private void OnEnable()
    {
        StoreRestingPose();
    }

    public void PlayUseAnimation()
    {
        PlayUseAnimation(null);
    }

    public void PlayUseAnimation(Transform attackPoint)
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }

        animationCoroutine = StartCoroutine(UseAnimationRoutine(attackPoint));
    }

    private IEnumerator UseAnimationRoutine(Transform attackPoint)
    {
        StoreRestingPose();

        Vector3 attackPosition;
        Quaternion attackRotation;

        if (attackPoint != null && transform.parent != null)
        {
            attackPosition =
                transform.parent.InverseTransformPoint(attackPoint.position) +
                attackPointLocalOffset;

            attackRotation = Quaternion.Euler(attackLocalRotation);
        }
        else
        {
            attackPosition = restingLocalPosition + relativeUsePositionOffset;
            attackRotation = restingLocalRotation * Quaternion.Euler(relativeUseRotationOffset);
        }

        float elapsed = 0f;

        while (elapsed < moveToAttackDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / moveToAttackDuration;

            transform.localPosition = Vector3.Lerp(restingLocalPosition, attackPosition, t);
            transform.localRotation = Quaternion.Lerp(restingLocalRotation, attackRotation, t);
            transform.localScale = restingLocalScale;

            yield return null;
        }

        transform.localPosition = attackPosition;
        transform.localRotation = attackRotation;
        transform.localScale = restingLocalScale;

        elapsed = 0f;

        while (elapsed < returnDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / returnDuration;

            transform.localPosition = Vector3.Lerp(attackPosition, restingLocalPosition, t);
            transform.localRotation = Quaternion.Lerp(attackRotation, restingLocalRotation, t);
            transform.localScale = restingLocalScale;

            yield return null;
        }

        transform.localPosition = restingLocalPosition;
        transform.localRotation = restingLocalRotation;
        transform.localScale = restingLocalScale;

        animationCoroutine = null;
    }

    private void StoreRestingPose()
    {
        restingLocalPosition = transform.localPosition;
        restingLocalRotation = transform.localRotation;
        restingLocalScale = transform.localScale;
    }
}