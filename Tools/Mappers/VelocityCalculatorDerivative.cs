namespace NITHlibrary.Tools.Mappers;

/// <summary>
/// A class for calculating the velocity of an object based on its position over time.
/// It utilizes the central difference method to estimate velocity from position history.
/// </summary>
public class VelocityCalculatorDerivative
{
    private readonly Queue<float> _positionHistory = new Queue<float>();
    private const int HistorySize = 3; // We need 3 positions for central difference
    private readonly float _deltaTime; // Time interval in seconds

    /// <summary>
    /// Initializes a new instance of the <see cref="VelocityCalculatorDerivative"/> class.
    /// </summary>
    /// <param name="frequency">The frequency at which positions are sampled, in Hertz.</param>
    public VelocityCalculatorDerivative(float frequency)
    {
        _deltaTime = 1f / frequency; // Calculate delta time based on frequency
    }

    /// <summary>
    /// Calculates the current velocity based on the provided position.
    /// </summary>
    /// <param name="currentPosition">The current position of the object.</param>
    /// <returns>The calculated velocity in units per second. Returns 0 if there is not enough data.</returns>
    public float CalculateVelocity(float currentPosition)
    {
        _positionHistory.Enqueue(currentPosition);

        // Maintain a fixed size history
        if (_positionHistory.Count > HistorySize)
        {
            _positionHistory.Dequeue();
        }

        // Ensure we have enough data to calculate velocity
        if (_positionHistory.Count < 3)
        {
            return 0f; // Not enough data to calculate velocity
        }

        // Convert the queue to an array for easy access
        float[] positions = _positionHistory.ToArray();

        // Use the central difference method
        float deltaPosition = positions[2] - positions[0]; // p(t + dt) - p(t - dt)
        float velocity = deltaPosition / (2 * _deltaTime); // Divide by 2 * deltaTime

        return velocity; // Return the calculated velocity
    }
}