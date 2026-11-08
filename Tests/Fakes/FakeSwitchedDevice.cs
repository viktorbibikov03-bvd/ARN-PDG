using System.Collections.Generic;

namespace VrdoPdg.Tests.Fakes
{
    /// <summary>
    /// Коммутируемое СКРМ тестовой модели режима: включение изменяет напряжение на заданную величину.
    /// </summary>
    internal sealed class FakeSwitchedDevice
    {
        /// <summary>
        /// Узлы, напряжение в которых зависит от СКРМ.
        /// </summary>
        private readonly HashSet<int> _nodes;

        /// <summary>
        /// Создает коммутируемое СКРМ.
        /// </summary>
        /// <param name="isOn">Исходное состояние.</param>
        /// <param name="switchOnEffect">Изменение напряжения при включении, кВ.</param>
        /// <param name="nodes">Узлы, напряжение в которых зависит от СКРМ.</param>
        public FakeSwitchedDevice(bool isOn, double switchOnEffect, params int[] nodes)
        {
            IsOn = isOn;
            InitialIsOn = isOn;
            SwitchOnEffect = switchOnEffect;
            _nodes = new HashSet<int>(nodes);
        }

        /// <summary>
        /// Признак состояния «Включено».
        /// </summary>
        public bool IsOn { get; set; }

        /// <summary>
        /// Исходное состояние.
        /// </summary>
        public bool InitialIsOn { get; }

        /// <summary>
        /// Изменение напряжения при включении, кВ.
        /// </summary>
        public double SwitchOnEffect { get; }

        /// <summary>
        /// Номинальная мощность, Мвар.
        /// </summary>
        public double NominalPower { get; set; } = 180;

        /// <summary>
        /// Возвращает вклад СКРМ в напряжение узла.
        /// </summary>
        /// <param name="nodeNumber">Номер узла.</param>
        /// <returns>Изменение напряжения, кВ.</returns>
        public double GetEffect(int nodeNumber)
        {
            if (!_nodes.Contains(nodeNumber) || IsOn == InitialIsOn)
            {
                return 0;
            }

            return IsOn
                ? SwitchOnEffect
                : -SwitchOnEffect;
        }
    }
}
