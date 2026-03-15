using UnityEngine;

public class TempOutroTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        gameManager.instance.LoadSceneWithFade("Outro Scene (placeholder)");
    }
}
