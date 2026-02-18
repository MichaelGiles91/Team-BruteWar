using UnityEngine;

    public class CheckPoint : MonoBehaviour
    {
        [SerializeField] Renderer checkpointRenderer;
        [SerializeField] Color activatedColor = Color.green;
   
      bool isActivated;
   
      void OnTriggerEnter(Collider other)
      {
        Debug.Log("Trigger hit by:" + other.name + " tag:" + other.tag);

          if (isActivated)
              return;
  
          if (other.CompareTag("Player"))
          {
              isActivated = true;
               gameManager.instance.SetCheckpoint(transform);
  
             if (checkpointRenderer != null)
                  checkpointRenderer.material.color = activatedColor;
          }
    }
}