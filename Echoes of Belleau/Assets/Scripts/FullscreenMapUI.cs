using UnityEngine;

public class FullscreenMapUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] RectTransform mapRect;
    [SerializeField] RectTransform playerMarker;
    [SerializeField] RectTransform objectiveMarker;
    [SerializeField] RectTransform objectiveZoneUI;

    [Header("World Bounds (X/Z)")]
    [SerializeField] Transform mapMin;
    [SerializeField] Transform mapMax;

    Transform player;
    Transform objectivePin;
    Collider activeZone;

    public void SetObjectivePin(Transform t) => objectivePin = t;
    public void SetActiveZone(Collider zone) => activeZone = zone;

    void Awake()
    {
        if (gameManager.instance != null && gameManager.instance.player != null)
            player = gameManager.instance.player.transform;
    }

    void LateUpdate()
    {
        if (!player || !mapMin || !mapMax) return;

        PlaceMarker(playerMarker, player.position);
        playerMarker.localEulerAngles = new Vector3(0, 0, -player.eulerAngles.y);

        if (objectiveMarker != null)
        {
            objectiveMarker.gameObject.SetActive(objectivePin != null);
            if (objectivePin != null)
                PlaceMarker(objectiveMarker, objectivePin.position);
        }

        if (objectiveZoneUI != null)
        {
            objectiveZoneUI.gameObject.SetActive(activeZone != null);
            if (activeZone != null)
                PlaceZone(objectiveZoneUI, activeZone);
        }
    }

    void PlaceMarker(RectTransform marker, Vector3 worldPos)
    {
        Vector2 n = WorldToNormalized(worldPos);
        Vector2 size = mapRect.rect.size;

        marker.anchoredPosition = new Vector2((n.x - 0.5f) * size.x, (n.y - 0.5f) * size.y);
    }

    void PlaceZone(RectTransform zoneUI, Collider zoneCollider)
    {
        PlaceMarker(zoneUI, zoneCollider.bounds.center);

        Vector3 b = zoneCollider.bounds.size;

        float mapWorldW = Mathf.Abs(mapMax.position.x - mapMin.position.x);
        float mapWorldH = Mathf.Abs(mapMax.position.z - mapMin.position.z);

        Vector2 mapPx = mapRect.rect.size;
        zoneUI.sizeDelta = new Vector2((b.x / mapWorldW) * mapPx.x, (b.z / mapWorldH) * mapPx.y);

        zoneUI.localEulerAngles = Vector3.zero;
    }

    Vector2 WorldToNormalized(Vector3 worldPos)
    {
        float x = Mathf.InverseLerp(mapMin.position.x, mapMax.position.x, worldPos.x);
        float y = Mathf.InverseLerp(mapMin.position.z, mapMax.position.z, worldPos.z);
        return new Vector2(x, y);
    }
}