using UnityEngine;
using UnityEngine.UI;

public class ObjMarker : MonoBehaviour
{
    [Header("Marker Visual")]
    public Sprite icon;

    [Header("Objective Order (0 = first, 1 = second, etc.)")]
    public int objectiveOrder;

    [HideInInspector] public Image image;
    [HideInInspector] public bool isActive;

    public Vector2 position => new Vector2(transform.position.x, transform.position.z);

    void OnEnable()
    {

        if (gameManager.instance != null)
            gameManager.instance.RegisterObjectiveMarker(this);
    }

    void OnDisable()
    {

        if (gameManager.instance != null)
            gameManager.instance.UnregisterObjectiveMarker(this);
    }

    public void SetActive(bool value)
    {
        isActive = value;

        if (image != null)
            image.gameObject.SetActive(value);
    }
}
