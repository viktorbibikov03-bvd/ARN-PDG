using System.Collections.Generic;

namespace VrdoPdg.Model.Services
{
    /// <summary>
    /// Состояние ввода режима в пределах одного часа: этап ввода режима, исчерпанные СКРМ,
    /// отложенные изменения, направления выполненных изменений и число шагов генераторов
    /// </summary>
    internal sealed class HourState
    {
        /// <summary>
        /// СКРМ, исключенные из перебора для КП: режим не сошелся или изменение не подействовало.
        /// </summary>
        private readonly HashSet<(int ControlPointId, int DeviceId)> _exhausted =
            new HashSet<(int ControlPointId, int DeviceId)>();

        /// <summary>
        /// СКРМ, изменение которых на первом этапе вывело за границы графика другой КП:
        /// изменение отложено до второго этапа
        /// </summary>
        private readonly HashSet<(int ControlPointId, int DeviceId)> _deferred =
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
        /// Признак второго этапа ввода режима: изменение СКРМ допускается, даже если другой КП выходит
        /// за границы графика, при условии, что наибольшее отклонение напряжения среди КП уменьшается
        /// </summary>
        public bool IsSecondStage { get; set; }

        /// <summary>
        /// Признак отложенных на второй этап изменений СКРМ
        /// </summary>
        public bool HasDeferredChanges => _deferred.Count > 0;

        /// <summary>
        /// Откладывает изменение СКРМ для КП до второго этапа ввода режима
        /// </summary>
        /// <param name="controlPointId">Код КП</param>
        /// <param name="deviceId">Код СКРМ</param>
        public void MarkDeferred(int controlPointId, int deviceId)
        {
            _deferred.Add((controlPointId, deviceId));
        }

        /// <summary>
        /// Проверяет, можно ли использовать СКРМ для ввода режима в КП: СКРМ не исключено из перебора,
        /// а отложенное изменение доступно только на втором этапе
        /// </summary>
        /// <param name="controlPointId">Код КП</param>
        /// <param name="deviceId">Код СКРМ</param>
        /// <returns><c>true</c>, если СКРМ можно использовать</returns>
        public bool IsAvailable(int controlPointId, int deviceId)
        {
            return !_exhausted.Contains((controlPointId, deviceId))
                && (IsSecondStage || !_deferred.Contains((controlPointId, deviceId)));
        }

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
