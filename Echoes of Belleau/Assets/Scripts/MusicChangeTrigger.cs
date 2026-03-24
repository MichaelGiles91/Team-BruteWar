using UnityEngine;

public class MusicChangeTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        MusicManager.instance.PlayBaseMusic();
    }
}
