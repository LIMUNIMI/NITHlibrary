using NITHlibrary.Nith.Internals;
using NITHlibrary.Nith.Module;
using NITHlibrary.Tools.Filters.ValueFilters;
using NITHlibrary.Tools.Mappers;
using System.Globalization;

namespace NITHlibrary.Nith.Preprocessors
{
    /// <summary>
    /// Un preprocessore che calcola i dati di velocità della testa dai dati di posizione.
    /// Se i dati di posizione della testa sono presenti ma quelli di velocità non lo sono, 
    /// questo preprocessore li calcola e li aggiunge ai dati del sensore.
    /// </summary>
    public class NithPreprocessor_HeadVelocityCalculator : INithPreprocessor
    {
        // Calcolatori di velocità con timestamp
        private readonly VelocityCalculatorTimestamped _yawVelocityCalculator;
        private readonly VelocityCalculatorTimestamped _pitchVelocityCalculator;
        private readonly VelocityCalculatorTimestamped _rollVelocityCalculator;

        // Filtri per stabilizzare i valori di velocità
        private readonly DoubleFilterMAexpDecaying _yawVelFilter;
        private readonly DoubleFilterMAexpDecaying _pitchVelFilter;
        private readonly DoubleFilterMAexpDecaying _rollVelFilter;

        // Ultime posizioni rilevate
        private double _lastYawPos = 0;
        private double _lastPitchPos = 0;
        private double _lastRollPos = 0;

        // Costanti per il calcolo della velocità
        private readonly float _velocitySensitivity;
        private const string VelocityNumberFormat = "0.00000";

        /// <summary>
        /// Inizializza una nuova istanza della classe <see cref="NithPreprocessor_HeadVelocityCalculator"/>.
        /// </summary>
        /// <param name="filterAlpha">Il fattore alpha per il filtro esponenziale a media mobile. 
        /// Valori più alti rendono la risposta più rapida ma meno stabile. Default: 0.3.</param>
        /// <param name="velocitySensitivity">Il fattore di sensibilità per le velocità. Default: 0.2.</param>
        /// <param name="maxSamples">Il numero massimo di campioni da mantenere nella storia per il calcolo della velocità. Default: 3.</param>
        /// <param name="maxSampleAgeMs">Il tempo massimo in millisecondi oltre il quale i campioni vengono considerati obsoleti. Default: 500.</param>
        public NithPreprocessor_HeadVelocityCalculator(
            float filterAlpha = 0.3f,
            float velocitySensitivity = 0.2f,
            int maxSamples = 3,
            int maxSampleAgeMs = 500)
        {
            _velocitySensitivity = velocitySensitivity;

            // Inizializzazione dei filtri con il valore alpha configurabile
            _yawVelFilter = new DoubleFilterMAexpDecaying(filterAlpha);
            _pitchVelFilter = new DoubleFilterMAexpDecaying(filterAlpha);
            _rollVelFilter = new DoubleFilterMAexpDecaying(filterAlpha);

            // Inizializzazione dei calcolatori di velocità basati sui timestamp
            _yawVelocityCalculator = new VelocityCalculatorTimestamped(maxSamples, maxSampleAgeMs);
            _pitchVelocityCalculator = new VelocityCalculatorTimestamped(maxSamples, maxSampleAgeMs);
            _rollVelocityCalculator = new VelocityCalculatorTimestamped(maxSamples, maxSampleAgeMs);
        }

        /// <summary>
        /// Applica le trasformazioni ai dati. Questo metodo verrà tipicamente chiamato dal <see cref="NithModule"/>.
        /// </summary>
        /// <param name="sensorData">Dati provenienti dal sensore NITH.</param>
        /// <returns>I dati trasformati.</returns>
        public NithSensorData TransformData(NithSensorData sensorData)
        {
            // Calcola le velocità se sono disponibili i dati di posizione della testa ma non già i dati di velocità
            if ((sensorData.ContainsParameter(NithParameters.head_pos_yaw) ||
                sensorData.ContainsParameter(NithParameters.head_pos_pitch) ||
                sensorData.ContainsParameter(NithParameters.head_pos_roll)) &&
                (!sensorData.ContainsParameter(NithParameters.head_vel_yaw) ||
                !sensorData.ContainsParameter(NithParameters.head_vel_pitch) ||
                !sensorData.ContainsParameter(NithParameters.head_vel_roll)))
            {
                // Usa lo stesso timestamp per tutti i calcoli di questa chiamata
                DateTime currentTime = DateTime.Now;

                // Calcola le velocità per ciascun asse se è disponibile il dato di posizione
                if (sensorData.ContainsParameter(NithParameters.head_pos_yaw) &&
                    !sensorData.ContainsParameter(NithParameters.head_vel_yaw))
                {
                    double yawPos = sensorData.GetParameterValue(NithParameters.head_pos_yaw).Value.ValueAsDouble;

                    // Calcola velocità usando il timestamp attuale
                    float yawVelocity = _yawVelocityCalculator.Push((float)yawPos, currentTime);

                    // Filtra la velocità per ridurre il rumore
                    _yawVelFilter.Push(yawVelocity * _velocitySensitivity);
                    double filteredYawVel = _yawVelFilter.Pull();

                    // Aggiungi il valore di velocità calcolato
                    sensorData.Values.Add(new()
                    {
                        DataType = NithDataTypes.OnlyValue,
                        Parameter = NithParameters.head_vel_yaw,
                        Value = filteredYawVel.ToString(VelocityNumberFormat, CultureInfo.InvariantCulture),
                    });

                    // Salva la posizione attuale per il prossimo calcolo
                    _lastYawPos = yawPos;
                }

                if (sensorData.ContainsParameter(NithParameters.head_pos_pitch) &&
                    !sensorData.ContainsParameter(NithParameters.head_vel_pitch))
                {
                    double pitchPos = sensorData.GetParameterValue(NithParameters.head_pos_pitch).Value.ValueAsDouble;

                    // Calcola velocità usando il timestamp attuale
                    float pitchVelocity = _pitchVelocityCalculator.Push((float)pitchPos, currentTime);

                    // Filtra la velocità per ridurre il rumore
                    _pitchVelFilter.Push(pitchVelocity * _velocitySensitivity);
                    double filteredPitchVel = _pitchVelFilter.Pull();

                    // Aggiungi il valore di velocità calcolato
                    sensorData.Values.Add(new()
                    {
                        DataType = NithDataTypes.OnlyValue,
                        Parameter = NithParameters.head_vel_pitch,
                        Value = filteredPitchVel.ToString(VelocityNumberFormat, CultureInfo.InvariantCulture),
                    });

                    // Salva la posizione attuale per il prossimo calcolo
                    _lastPitchPos = pitchPos;
                }

                if (sensorData.ContainsParameter(NithParameters.head_pos_roll) &&
                    !sensorData.ContainsParameter(NithParameters.head_vel_roll))
                {
                    double rollPos = sensorData.GetParameterValue(NithParameters.head_pos_roll).Value.ValueAsDouble;

                    // Calcola velocità usando il timestamp attuale
                    float rollVelocity = _rollVelocityCalculator.Push((float)rollPos, currentTime);

                    // Filtra la velocità per ridurre il rumore
                    _rollVelFilter.Push(rollVelocity * _velocitySensitivity);
                    double filteredRollVel = _rollVelFilter.Pull();

                    // Aggiungi il valore di velocità calcolato
                    sensorData.Values.Add(new()
                    {
                        DataType = NithDataTypes.OnlyValue,
                        Parameter = NithParameters.head_vel_roll,
                        Value = filteredRollVel.ToString(VelocityNumberFormat, CultureInfo.InvariantCulture),
                    });

                    // Salva la posizione attuale per il prossimo calcolo
                    _lastRollPos = rollPos;
                }
            }

            return sensorData;
        }
    }
}