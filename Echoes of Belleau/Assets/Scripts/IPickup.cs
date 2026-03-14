using UnityEngine;

public interface IPickup
{
    public void getGunStats(gunStats gun);
    public void getGrenade(int amount);
    public void getMedkit(int ammount);
}
