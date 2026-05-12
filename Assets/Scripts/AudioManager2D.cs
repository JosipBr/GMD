using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioManager2D : MonoBehaviour
{
    public static AudioManager2D Instance { get; private set; }

    [Header("Movement")]
    [SerializeField] private AudioClip jumpClip;
    [SerializeField] private AudioClip dashClip;
    [SerializeField] private AudioClip landingClip;

    [Header("Combat")]
    [SerializeField] private AudioClip gunShotClip;
    [SerializeField] private AudioClip knifeSwingClip;
    [SerializeField] private AudioClip punchImpactClip;
    [SerializeField] private AudioClip deathClip;

    [Header("Interaction")]
    [SerializeField] private AudioClip pickupClip;

    [Header("Round")]
    [SerializeField] private AudioClip readyFightClip;
    [SerializeField] private AudioClip roundWinClip;

    [Header("Menu")]
    [SerializeField] private AudioClip menuMoveClip;
    [SerializeField] private AudioClip menuSelectClip;
    [SerializeField] private AudioClip menuBackClip;
    [SerializeField] private AudioClip pauseClip;
    [SerializeField] private AudioClip gameStartClip;
    [SerializeField] private AudioClip pressStartClip;

    [Header("Settings")]
    [Range(0f, 1f)]
    [SerializeField] private float masterVolume = 0.55f;

    [SerializeField] private float pitchVariation = 0.05f;

    private AudioSource audioSource;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        audioSource = GetComponent<AudioSource>();
    }

    public void PlayJump()
    {
        PlayClip(jumpClip, 0.45f);
    }

    public void PlayDash()
    {
        PlayClip(dashClip, 0.5f);
    }

    public void PlayLanding()
    {
        PlayClip(landingClip, 0.35f);
    }

    public void PlayGunShot()
    {
        PlayClip(gunShotClip, 0.55f);
    }

    public void PlayKnifeSwing()
    {
        PlayClip(knifeSwingClip, 0.45f);
    }

    public void PlayPunchImpact()
    {
        PlayClip(punchImpactClip, 0.55f);
    }

    public void PlayDeath()
    {
        PlayClip(deathClip, 0.6f);
    }

    public void PlayPickup()
    {
        PlayClip(pickupClip, 0.45f);
    }

    public void PlayReadyFight()
    {
        PlayClip(readyFightClip, 0.7f, usePitchVariation: false);
    }

    public void PlayRoundWin()
    {
        PlayClip(roundWinClip, 0.65f, usePitchVariation: false);
    }

    public void PlayMenuMove()
    {
        PlayClip(menuMoveClip, 0.35f, usePitchVariation: false);
    }

    public void PlayMenuSelect()
    {
        PlayClip(menuSelectClip, 0.45f, usePitchVariation: false);
    }

    public void PlayMenuBack()
    {
        PlayClip(menuBackClip, 0.45f, usePitchVariation: false);
    }

    public void PlayPause()
    {
        PlayClip(pauseClip, 0.5f, usePitchVariation: false);
    }

    public void PlayGameStart()
    {
        PlayClip(gameStartClip, 0.65f, usePitchVariation: false);
    }

    public void PlayPressStart()
    {
        PlayClip(pressStartClip, 0.55f, usePitchVariation: false);
    }

    private void PlayClip(AudioClip clip, float volume, bool usePitchVariation = true)
    {
        if (clip == null || audioSource == null)
        {
            return;
        }

        audioSource.pitch = usePitchVariation
            ? Random.Range(1f - pitchVariation, 1f + pitchVariation)
            : 1f;

        audioSource.PlayOneShot(clip, volume * masterVolume);
    }
}