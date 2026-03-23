using UnityEngine;

[CreateAssetMenu]

public class gunStats : ScriptableObject
{
    public GameObject gunModel;
    public AnimatorOverrideController overrideController;
    public GameObject bulletPrefab;
    public Sprite weaponIcon;

    [Header("--- Shooting ---")]
    public int shootDamage;
    public int shootDist;
    public float shootRate;

    [Header("--- Ammo ---")]
    public int magSize = 30;
    public int pickupSize = 10;
    public int ammoCur;
    public int ammoMax;
    public int ammoMaxOrig;

    [Header("--- Effects / Sounds ---")]
    public ParticleSystem hitEffect;
    public ParticleSystem muzzleFlash;
    public AudioClip[] shootSound;
    [Range(0, 1)] public float shootSoundVol;
    public AudioClip[] reloadSound;
    [Range(0, 1)] public float reloadSoundVol;
    public float reloadTime;

    [Header("--- Recoil / Spread ---")]
    public float sprayAmount = 0.02f;
    public float verticalRecoil = 1.2f;
    public float horizontalRecoil = 0.35f;
    public float recoilResetSpeed = 8f;
    public float crosshairKick = 18f;

    [Header("--- Other ---")]
    public bool singleUseWeapon = false;
    public bool uniquePickup = false;
    public gunStats sourceAsset;


}
