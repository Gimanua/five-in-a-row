using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Game : MonoBehaviour
{
    private const int MARKS_IN_A_ROW_FOR_WIN = 5;

    [SerializeField] private Texture ringTexture;
    [SerializeField] private Texture crossTexture;
    [SerializeField] private GameObject ringPrefab;
    [SerializeField] private GameObject crossPrefab;
    [SerializeField] private GameObject crossCollection;
    [SerializeField] private GameObject ringCollection;
    [SerializeField] private RawImage currentMarkImage;
    [SerializeField] private float markFadeAlpha;

    private readonly Dictionary<Vector2Int, Mark> placedMarks = new Dictionary<Vector2Int, Mark>();
    private readonly Dictionary<Vector2Int, SpriteRenderer> spriteRenderers = new Dictionary<Vector2Int, SpriteRenderer>();
    private Mark CurrentMark { get; set; } = Mark.Cross;

    private void OnMouseDown()
    {
        Vector3 mousePositionInWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 snapPosition = new Vector3(Mathf.Round(mousePositionInWorld.x), Mathf.Round(mousePositionInWorld.y), 0);
        Vector2Int logicalPosition = new Vector2Int((int)snapPosition.x, (int)snapPosition.y);
        if (placedMarks.ContainsKey(logicalPosition))
            return;
        
        PlaceCurrentMarkAt(logicalPosition);
        if (CheckForWin(logicalPosition, out IEnumerable<Vector2Int> winMarkPositions))
        {
            EndGame(winMarkPositions);
        }
        else
        {
            SwitchCurrentMark();
        }
    }

    private enum Mark
    {
        Cross,
        Ring
    }

    private void EndGame(IEnumerable<Vector2Int> winMarkPositions)
    {
        var allMarkSpriteRenderersExceptWinning = spriteRenderers
                .Where(kvp => winMarkPositions.Contains(kvp.Key) == false)
                .Select(kvp => kvp.Value);
        foreach (var spriteRenderer in allMarkSpriteRenderersExceptWinning)
        {
            spriteRenderer.color = new Color(1, 1, 1, markFadeAlpha);
        }

        currentMarkImage.enabled = false;
        GetComponent<BoxCollider2D>().enabled = false;
    }

    private void PlaceCurrentMarkAt(Vector2Int position)
    {
        GameObject prefab = GetCurrentMarkPrefab();
        GameObject currentMarkCollection = GetCurrentMarkCollection();
        GameObject instance = Instantiate(prefab, new Vector3(position.x, position.y, 0), Quaternion.identity, currentMarkCollection.transform);
        placedMarks.Add(position, CurrentMark);
        spriteRenderers.Add(position, instance.GetComponent<SpriteRenderer>());
    }

    private void SwitchCurrentMark()
    {
        if (CurrentMark == Mark.Cross)
        {
            CurrentMark = Mark.Ring;
            currentMarkImage.texture = ringTexture;
        }
        else
        {
            CurrentMark = Mark.Cross;
            currentMarkImage.texture = crossTexture;
        }
    }

    private bool CheckForWin(Vector2Int position, out IEnumerable<Vector2Int> winMarkPositions)
    {
        Vector2Int[] directions = new Vector2Int[]
        {
            Vector2Int.left,
            Vector2Int.right,
            Vector2Int.up,
            Vector2Int.down,
            new Vector2Int(-1, -1),
            new Vector2Int(-1, 1),
            new Vector2Int(1, -1),
            new Vector2Int(1, 1)
        };
        foreach (Vector2Int direction in directions)
        {
            if (CheckForWinDirection(position, direction))
            {
                winMarkPositions = Enumerable
                    .Range(0, MARKS_IN_A_ROW_FOR_WIN)
                    .Select(i => position + direction * i);
                return true;
            }
            // The start and end does not need to be checked, they are both already checked with the previous statement
            int intermediateChecks = MARKS_IN_A_ROW_FOR_WIN - 2;
            Vector2Int reverseDirection = direction * -1;
            for (int i = 1; i <= intermediateChecks; i++)
            {
                Vector2Int intermediatePosition = position + direction * i;
                if (CheckForWinDirection(intermediatePosition, reverseDirection))
                {
                    winMarkPositions = Enumerable
                        .Range(0, MARKS_IN_A_ROW_FOR_WIN)
                        .Select(i => intermediatePosition + reverseDirection * i).ToArray();
                    return true;
                }
            }
        }

        winMarkPositions = null;
        return false;
    }

    private bool CheckForWinDirection(Vector2Int position, Vector2Int direction)
    {
        for (int i = 0; i < MARKS_IN_A_ROW_FOR_WIN; i++)
        {
            Vector2Int checkPosition = position + direction * i;
            if (placedMarks.TryGetValue(checkPosition, out Mark mark) == false || mark != CurrentMark)
                return false;
        }
        return true;
    }

    private GameObject GetCurrentMarkPrefab()
    {
        if (CurrentMark == Mark.Cross)
            return crossPrefab;
        else
            return ringPrefab;
    }
    private GameObject GetCurrentMarkCollection()
    {
        if (CurrentMark == Mark.Cross)
        {
            return crossCollection;
        }
        else
        {
            return ringCollection;
        }
    }
}
