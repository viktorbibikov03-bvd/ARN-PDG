using System.Collections.Generic;

namespace VrdoPdg.Model.Services
{
    /// <summary>
    /// Состояние ввода режима в пределах одного часа: исчерпанные СКРМ,
    /// направления выполненных изменений и число шагов генераторов.
    /// </summary>
    internal sealed class HourState
    {
        /// <summary>
        /// СКРМ, исключенные из перебора для КП: режим не сошелся или изменение не подействовало.
        /// </summary>
        private readonly HashSet<(int ControlPointId, int DeviceId)> _exhausted =
            new HashSet<(int ControlPointId, int DeviceId)>();

        /// <summary>
        /// Направления изменений СКРМ, выполненных в этом часе.
        /// </summary>
        private readonly Dictionary<int, RegulationDirection> _directions = new Dictionary<int, RegulationDirection>();

        /// <summary>
        /// Число шагов изменения заданного напряжения генераторов в этом часе.
        /// </summary>
        private readonly Dictionary<int, int> _generatorSteps = new Dictionary<int, int>();

        /// <summary>
        /// Создает состояние часа.
        /// </summary>
        /// <param name="hour">Час суток (1–24).</param>
        public HourState(int hour)
        {
            Hour = hour;
        }

        /// <summary>
        /// Час суток (1–24).
        /// </summary>
        public int Hour { get; }

        /// <summary>
        /// Исключает СКРМ из перебора для КП в этом часе.
        /// </summary>
        /// <param name="controlPointId">Код КП.</param>
        /// <param name="deviceId">Код СКРМ.</param>
        public void MarkExhausted(int controlPointId, int deviceId)
        {
            _exhausted.Add((controlPointId, deviceId));
        }

        /// <summary>
        /// Проверяет, исключено ли СКРМ из перебора для КП.
        /// </summary>
        /// <param name="controlPointId">Код КП.</param>
        /// <param name="deviceId">Код СКРМ.</param>
        /// <returns><c>true</c>, если СКРМ исключено.</returns>
        public bool IsExhausted(int controlPointId, int deviceId)
        {
            return _exhausted.Contains((controlPointId, deviceId));
        }

        /// <summary>
        /// Запоминает направление изменения СКРМ.
        /// </summary>
        /// <param name="deviceId">Код СКРМ.</param>
        /// <param name="direction">Направление изменения.</param>
        public void RegisterChange(int deviceId, RegulationDirection direction)
        {
            _directions[deviceId] = direction;
        }

        /// <summary>
        /// Проверяет, изменялось ли СКРМ в этом часе в противоположном направлении.
        /// Такое СКРМ не используется, чтобы ввод режима в одном КП не отменял
        /// действия, выполненные для другого КП.
        /// </summary>
        /// <param name="deviceId">Код СКРМ.</param>
        /// <param name="direction">Требуемое направление изменения.</param>
        /// <returns><c>true</c>, если СКРМ изменялось в противоположном направлении.</returns>
        public bool IsLocked(int deviceId, RegulationDirection direction)
        {
            RegulationDirection previous;
            return _directions.TryGetValue(deviceId, out previous) && previous != direction;
        }

        /// <summary>
        /// Увеличивает число шагов изменения заданного напряжения генератора
        /// </summary>
        /// <param name="deviceId">Код СКРМ генератора.</param>
        public void AddGeneratorStep(int deviceId)
        {
            _generatorSteps[deviceId] = GetGeneratorSteps(deviceId) + 1;
        }

        /// <summary>
        /// Возвращает число шагов изменения заданного напряжения генератора
        /// </summary>
        /// <param name="deviceId">Код СКРМ генератора.</param>
        /// <returns>Число шагов.</returns>
        public int GetGeneratorSteps(int deviceId)
        {
            int steps;
            return _generatorSteps.TryGetValue(deviceId, out steps)
                ? steps
                : 0;
        }
    }
}
