namespace NITHlibrary.Tools.Mappers;

/// <summary>
/// Represents an adaptive Kalman filter for calculating velocity based on position measurements.
/// This class utilizes filtering techniques to estimate the velocity over time while adapting to measurement noise.
/// </summary>
public class VelocityCalculatorAdaptiveKalman
{
    private float _dt; // Time step
    private float[,] _F; // State transition matrix
    private float[,] _H; // Observation matrix
    private float[,] _Q; // Process noise covariance
    private float[,] _R; // Measurement noise covariance
    private float[,] _P; // Estimate error covariance
    private float[,] _x; // State vector
    private Queue<float> _residuals; // Queue for residuals to adaptively estimate R
    private int _windowSize; // Size of the moving window for residuals

    /// <summary>
    /// Initializes a new instance of the <see cref="VelocityCalculatorAdaptiveKalman"/> class.
    /// </summary>
    /// <param name="dt">The time step between measurements.</param>
    /// <param name="initialProcessNoise">The initial process noise covariance.</param>
    /// <param name="initialMeasurementNoise">The initial measurement noise covariance.</param>
    /// <param name="windowSize">The size of the moving window for residuals (default is 10).</param>
    public VelocityCalculatorAdaptiveKalman(float dt, float initialProcessNoise, float initialMeasurementNoise, int windowSize = 10)
    {
        _dt = dt;

        // State transition matrix
        _F = new float[,]
        {
            { 1, _dt },
            { 0, 1 }
        };

        // Observation matrix
        _H = new float[,]
        {
            { 1, 0 }
        };

        // Initial process noise covariance
        _Q = new float[,]
        {
            { initialProcessNoise, 0 },
            { 0, initialProcessNoise }
        };

        // Initial measurement noise covariance
        _R = new float[,]
        {
            { initialMeasurementNoise }
        };

        // Initial estimate error covariance
        _P = new float[,]
        {
            { 1, 0 },
            { 0, 1 }
        };

        // Initial state (position and velocity)
        _x = new float[,]
        {
            { 0 },
            { 0 }
        };

        _residuals = new Queue<float>();
        _windowSize = windowSize;
    }

    /// <summary>
    /// Updates the velocity estimate with a new measured position.
    /// </summary>
    /// <param name="measuredPosition">The measured position to use for updating the estimate.</param>
    /// <returns>The estimated velocity derived from the updated state.</returns>
    public float Update(float measuredPosition)
    {
        // Prediction step
        _x = MatrixMultiply(_F, _x);
        _P = MatrixAdd(MatrixMultiply(MatrixMultiply(_F, _P), Transpose(_F)), _Q);

        // Measurement update step
        float[,] y = MatrixSubtract(new float[,] { { measuredPosition } }, MatrixMultiply(_H, _x));
        float[,] S = MatrixAdd(MatrixMultiply(MatrixMultiply(_H, _P), Transpose(_H)), _R);
        float[,] K = MatrixMultiply(MatrixMultiply(_P, Transpose(_H)), Invert(S));

        // Update state estimate
        _x = MatrixAdd(_x, MatrixMultiply(K, y));

        // Update estimate error covariance
        float[,] I = new float[,] { { 1, 0 }, { 0, 1 } };
        _P = MatrixMultiply(MatrixSubtract(I, MatrixMultiply(K, _H)), _P);

        // Update measurement noise covariance R adaptively
        UpdateMeasurementNoise(y[0, 0]);

        // Return the estimated velocity
        return _x[1, 0];
    }

    /// <summary>
    /// Updates the measurement noise covariance R based on the most recent residual.
    /// </summary>
    /// <param name="residual">The current residual used to adaptively adjust the measurement noise.</param>
    private void UpdateMeasurementNoise(float residual)
    {
        _residuals.Enqueue(residual);
        if (_residuals.Count > _windowSize)
        {
            _residuals.Dequeue();
        }

        float sumOfSquares = 0;
        foreach (var res in _residuals)
        {
            sumOfSquares += res * res;
        }

        float newMeasurementNoise = sumOfSquares / _residuals.Count;
        _R[0, 0] = newMeasurementNoise;
    }

    // Matrix operations (multiply, add, subtract, transpose, invert)

    /// <summary>
    /// Multiplies two matrices and returns the result.
    /// </summary>
    /// <param name="A">The first matrix.</param>
    /// <param name="B">The second matrix.</param>
    /// <returns>The product of the two matrices.</returns>
    private float[,] MatrixMultiply(float[,] A, float[,] B)
    {
        int aRows = A.GetLength(0);
        int aCols = A.GetLength(1);
        int bCols = B.GetLength(1);
        float[,] result = new float[aRows, bCols];

        for (int i = 0; i < aRows; i++)
        {
            for (int j = 0; j < bCols; j++)
            {
                for (int k = 0; k < aCols; k++)
                {
                    result[i, j] += A[i, k] * B[k, j];
                }
            }
        }
        return result;
    }

    /// <summary>
    /// Adds two matrices and returns the result.
    /// </summary>
    /// <param name="A">The first matrix.</param>
    /// <param name="B">The second matrix.</param>
    /// <returns>The sum of the two matrices.</returns>
    private float[,] MatrixAdd(float[,] A, float[,] B)
    {
        int rows = A.GetLength(0);
        int cols = A.GetLength(1);
        float[,] result = new float[rows, cols];

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                result[i, j] = A[i, j] + B[i, j];
            }
        }
        return result;
    }

    /// <summary>
    /// Subtracts the second matrix from the first and returns the result.
    /// </summary>
    /// <param name="A">The first matrix.</param>
    /// <param name="B">The second matrix.</param>
    /// <returns>The result of the subtraction.</returns>
    private float[,] MatrixSubtract(float[,] A, float[,] B)
    {
        int rows = A.GetLength(0);
        int cols = A.GetLength(1);
        float[,] result = new float[rows, cols];

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                result[i, j] = A[i, j] - B[i, j];
            }
        }
        return result;
    }

    /// <summary>
    /// Transposes the given matrix and returns the result.
    /// </summary>
    /// <param name="A">The matrix to transpose.</param>
    /// <returns>The transposed matrix.</returns>
    private float[,] Transpose(float[,] A)
    {
        int rows = A.GetLength(0);
        int cols = A.GetLength(1);
        float[,] result = new float[cols, rows];

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                result[j, i] = A[i, j];
            }
        }
        return result;
    }

    /// <summary>
    /// Inverts the given matrix and returns the result.
    /// </summary>
    /// <param name="A">The matrix to invert.</param>
    /// <returns>The inverted matrix.</returns>
    private float[,] Invert(float[,] A)
    {
        int n = A.GetLength(0);
        float[,] result = new float[n, n];
        float[,] identity = new float[n, n];

        for (int i = 0; i < n; i++)
        {
            identity[i, i] = 1;
        }

        for (int i = 0; i < n; i++)
        {
            float diag = A[i, i];
            for (int j = 0; j < n; j++)
            {
                A[i, j] /= diag;
                identity[i, j] /= diag;
            }

            for (int k = 0; k < n; k++)
            {
                if (k != i)
                {
                    float factor = A[k, i];
                    for (int j = 0; j < n; j++)
                    {
                        A[k, j] -= factor * A[i, j];
                        identity[k, j] -= factor * identity[i, j];
                    }
                }
            }
        }

        Array.Copy(identity, result, identity.Length);
        return result;
    }
}