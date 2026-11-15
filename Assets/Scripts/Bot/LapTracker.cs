public class LapTracker {

    private readonly float length;
    private float lastDistance;
    private float traveled;

    public int Laps { get; private set; }

    public LapTracker(float length, float startDistance) {
        this.length = length;
        lastDistance = startDistance;
    }

    public void Advance(float distance) {
        float step = distance - lastDistance;
        if (step < -length * 0.5f) step += length;
        else if (step > length * 0.5f) step -= length;

        lastDistance = distance;
        traveled += step;

        if (traveled >= length) {
            traveled -= length;
            Laps++;
        }
    }
}
