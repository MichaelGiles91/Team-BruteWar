using UnityEngine;

[CreateAssetMenu]

public class gunStats : ScriptableObject
{
    public GameObject gunModel;
    public AnimatorOverrideController overrideController;
    public GameObject bulletPrefab;

    [Range(1,10)]public int shootDamage;
    [Range(15, 1000)] public int shootDist;
    [Range(0.1f, 2f)] public float shootRate;

    public int magSize = 30;
    public int pickupSize = 10;
    public int ammoCur;
    [Range(5, 50)] public int ammoMax;
    public int ammoMaxOrig;

    public ParticleSystem hitEffect;
    public ParticleSystem muzzleFlash;
    public AudioClip[] shootSound;
    [Range(0, 1)] public float shootSoundVol;

}
