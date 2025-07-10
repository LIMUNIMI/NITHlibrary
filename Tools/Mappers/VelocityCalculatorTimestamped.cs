using System;
using System.Collections.Generic;

namespace NITHlibrary.Tools.Mappers;

/// <summary>
/// Un calcolatore di velocità che utilizza i timestamp per determinare automaticamente
/// la frequenza di campionamento e calcolare la velocità in base ai tempi effettivi.
/// </summary>
public class VelocityCalculatorTimestamped
{
    private class PositionSample
    {
        public float Position { get; set; }
        public DateTime Timestamp { get; set; }
    }

    private readonly Queue<PositionSample> _samples = new();
    private readonly int _maxSamples;
    private readonly TimeSpan _maxSampleAge;

    /// <summary>
    /// Inizializza una nuova istanza della classe <see cref="VelocityCalculatorTimestamped"/>.
    /// </summary>
    /// <param name="maxSamples">Il numero massimo di campioni da mantenere nella storia (minimo 2).</param>
    /// <param name="maxSampleAgeMs">Il tempo massimo in millisecondi oltre il quale i campioni vengono considerati obsoleti.</param>
    public VelocityCalculatorTimestamped(int maxSamples = 3, int maxSampleAgeMs = 500)
    {
        _maxSamples = Math.Max(2, maxSamples);
        _maxSampleAge = TimeSpan.FromMilliseconds(maxSampleAgeMs);
    }

    /// <summary>
    /// Aggiunge un nuovo valore di posizione e calcola la velocità attuale.
    /// </summary>
    /// <param name="currentPosition">La posizione attuale.</param>
    /// <param name="timestamp">Il timestamp della misurazione (usa DateTime.Now come default).</param>
    /// <returns>La velocità calcolata in unità al secondo. Ritorna 0 se non ci sono dati sufficienti.</returns>
    public float Push(float currentPosition, DateTime? timestamp = null)
    {
        DateTime currentTime = timestamp ?? DateTime.Now;

        // Aggiunge il nuovo campione
        _samples.Enqueue(new PositionSample
        {
            Position = currentPosition,
            Timestamp = currentTime
        });

        // Mantiene la dimensione massima della coda
        while (_samples.Count > _maxSamples)
        {
            _samples.Dequeue();
        }

        // Rimuove i campioni obsoleti
        while (_samples.Count > 0 &&
              (currentTime - _samples.Peek().Timestamp) > _maxSampleAge)
        {
            _samples.Dequeue();
        }

        return CalculateVelocity();
    }

    /// <summary>
    /// Calcola la velocità in base ai campioni disponibili.
    /// </summary>
    /// <returns>La velocità calcolata. Ritorna 0 se non ci sono dati sufficienti.</returns>
    private float CalculateVelocity()
    {
        if (_samples.Count < 2)
        {
            return 0f;
        }

        // Utilizziamo il metodo della differenza centrale se possibile
        if (_samples.Count >= 3)
        {
            var samplesArray = _samples.ToArray();

            // Prende il primo, il medio e l'ultimo campione per la differenza centrale
            var first = samplesArray[0];
            var last = samplesArray[samplesArray.Length - 1];

            // Calcola la velocità usando la differenza centrale
            float deltaPosition = last.Position - first.Position;
            double deltaTimeSeconds = (last.Timestamp - first.Timestamp).TotalSeconds;

            if (deltaTimeSeconds > 0)
            {
                return (float)(deltaPosition / deltaTimeSeconds);
            }
        }
        else
        {
            // Con solo due punti, usa la differenza in avanti
            var samplesArray = _samples.ToArray();
            float deltaPosition = samplesArray[1].Position - samplesArray[0].Position;
            double deltaTimeSeconds = (samplesArray[1].Timestamp - samplesArray[0].Timestamp).TotalSeconds;

            if (deltaTimeSeconds > 0)
            {
                return (float)(deltaPosition / deltaTimeSeconds);
            }
        }

        return 0f;
    }

    /// <summary>
    /// Ottiene la velocità attuale senza aggiungere nuovi dati.
    /// </summary>
    /// <returns>La velocità calcolata in base ai dati esistenti.</returns>
    public float GetCurrentVelocity()
    {
        return CalculateVelocity();
    }

    /// <summary>
    /// Reimposta la storia dei campioni.
    /// </summary>
    public void Reset()
    {
        _samples.Clear();
    }
}