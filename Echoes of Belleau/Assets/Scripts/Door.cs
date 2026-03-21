
using UnityEngine;

public class Door : MonoBehaviour
{
    public static Door Instance;

    [SerializeField] GameObject model;
    public GameObject UI;

    bool playerInTrigger;
    public bool isLocked;

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if (Input.GetButtonDown("Interact") && playerInTrigger && !isLocked)
        {
            model.SetActive(false);
            UI.SetActive(false);
        }

        if (Input.GetButtonDown("Interact") && playerInTrigger && isLocked)
        {
            gameManager.instance.ShowLockedText();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInTrigger = true;
            UI.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            model.SetActive(true);
            playerInTrigger = false;
            UI.SetActive(false);
        }
    }


}
