using System;

public class VelocityCalculatorKalman
{
    private float _dt; // Time step
    private float[,] _F; // State transition matrix
    private float[,] _H; // Observation matrix
    private float[,] _Q; // Process noise covariance
    private float[,] _R; // Measurement noise covariance
    private float[,] _P; // Estimate error covariance
    private float[,] _x; // State vector

    public VelocityCalculatorKalman(float dt, float processNoise, float measurementNoise)
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

        // Process noise covariance
        _Q = new float[,]
        {
            { processNoise, 0 },
            { 0, processNoise }
        };

        // Measurement noise covariance
        _R = new float[,]
        {
            { measurementNoise }
        };

        // Estimate error covariance
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
    }

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

        // Return the estimated velocity
        return _x[1, 0];
    }

    // Matrix operations (multiply, add, subtract, transpose, invert)
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
