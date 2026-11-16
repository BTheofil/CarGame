using UnityEngine;

public class BotSpawner : MonoBehaviour {

    [SerializeField] private BotInputProvider botPrefab;
    [SerializeField] private SplineTrackPath[] routes;
    [SerializeField] private int botCount = 10;
    [SerializeField] private float rowSpacing = 7f;
    [SerializeField] private float laneWidth = 3f;
    [SerializeField] private float laneSpread = 1.5f;
    [SerializeField] private float spawnHeight = 0.5f;
    [SerializeField] private Vector2 skillRange = new Vector2(0.86f, 1f);

    private SplineTrackPath Grid => routes[0];

    private void Start() {
        float startLine = Grid.DistanceOf(transform.position);

        for (int i = 0; i < botCount; i++) {
            float lane = i % 2 == 0 ? -laneWidth : laneWidth;
            Spawn(startLine - rowSpacing * (i / 2 + 1), lane, i);
        }
    }

    private void Spawn(float distance, float lane, int index) {
        Vector3 position = Grid.PointBeside(distance, lane) + Vector3.up * spawnHeight;
        BotInputProvider bot = Instantiate(botPrefab, position, Quaternion.LookRotation(Grid.DirectionAt(distance)));

        bot.name = $"Bot {index + 1}";
        bot.Configure(routes[index % routes.Length], Random.Range(skillRange.x, skillRange.y), Random.Range(-laneSpread, laneSpread));
    }
}
