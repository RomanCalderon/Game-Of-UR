using System.Collections;
using UnityEngine;

public class TileObject : MonoBehaviour
{
    public enum TileType
    {
        Normal,
        Rosette,
        Start
    }
    public enum TerritoryType
    {
        Neutral,
        Player1,
        Player2
    }

    [SerializeField]
    private TileType tileType = TileType.Normal;
    [SerializeField]
    private TerritoryType territoryType = TerritoryType.Neutral;
    private Transform geometry;
    Vector3 originalPos;
    Vector3 targetPos;
    /// <summary>
    /// Coroutine for making this Tile glow for a short period
    /// before ending the glow
    /// </summary>
    private Coroutine glowCoroutine;

    private void Start()
    {
        geometry = transform.FindChild("Graphic");
        originalPos = geometry.position;
        targetPos = geometry.position + Vector3.up * 0.15f;
    }

    public TileType GetTileType()
    {
        return tileType;
    }

    public TerritoryType GetTerritoryType()
    {
        return territoryType;
    }

    public void Reveal()
    {
        if (glowCoroutine != null)
        {
            StopCoroutine(glowCoroutine);
            glowCoroutine = StartCoroutine(GlowEffect());
        }
        else
            glowCoroutine = StartCoroutine(GlowEffect());
    }

    private IEnumerator GlowEffect()
    {
        float timer = 0.1f;
        float cooler = timer;

        while (cooler > 0f)
        {
            geometry.position = Vector3.Lerp(geometry.position, targetPos, Time.deltaTime * 4f);
            cooler -= Time.deltaTime;
            yield return null;
        }

        while (geometry.position != originalPos)
        {
            geometry.position = Vector3.Lerp(geometry.position, originalPos, Time.deltaTime * 10f);
            yield return null;
        }
    }
}
