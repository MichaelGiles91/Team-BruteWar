using UnityEngine;

[CreateAssetMenu]

public class gunStats : ScriptableObject
{
    public GameObject gunModel;
    public AnimatorOverrideController overrideController;
    public GameObject bulletPrefab;
    public Sprite weaponIcon;

    public int shootDamage;
    public int shootDist;
    public float shootRate;

    public int magSize = 30;
    public int pickupSize = 10;
    public int ammoCur;
    public int ammoMax;
    public int ammoMaxOrig;

    public ParticleSystem hitEffect;
    public ParticleSystem muzzleFlash;
    public AudioClip[] shootSound;
    [Range(0, 1)] public float shootSoundVol;
    public AudioClip[] reloadSound;
    [Range(0, 1)] public float reloadSoundVol;
    public float reloadTime;

    public bool singleUseWeapon = false;
    public bool uniquePickup = false;
    public gunStats sourceAsset;
}
