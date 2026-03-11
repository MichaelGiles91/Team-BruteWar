using UnityEngine;

public interface IPickup
{
    public void getGunStats(gunStats gun);
    public void getMedkit(int ammount);

    public void GetDogTag();
}
