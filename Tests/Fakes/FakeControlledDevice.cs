using System;
using System.Collections.Generic;

namespace VrdoPdg.Tests.Fakes
{
    /// <summary>
    /// Управляемое СКРМ тестовой модели режима: загрузка равна исходной, ограниченной
    /// пределами реактивной мощности, а вклад в напряжение пропорционален изменению загрузки.
    /// </summary>
    internal sealed class FakeControlledDevice
    {
        /// <summary>
        /// Узлы, напряжение в которых зависит от СКРМ.
        /// </summary>
        private readonly HashSet<int> _nodes;

        /// <summary>
        /// Создает управляемое СКРМ.
        /// </summary>
        /// <param name="reactivePower">Исходная загрузка, Мвар.</param>
        /// <param name="minReactivePower">Qmin, Мвар.</param>
        /// <param name="maxReactivePower">Qmax, Мвар.</param>
        /// <param name="voltagePerMvar">Изменение напряжения на 1 Мвар, кВ.</param>
        /// <param name="nodes">Узлы, напряжение в которых зависит от СКРМ.</param>
        public FakeControlledDevice(
            double reactivePower,
            double minReactivePower,
            double maxReactivePower,
            double voltagePerMvar,
            params int[] nodes)
        {
            InitialReactivePower = reactivePower;
            MinReactivePower = minReactivePower;
            MaxReactivePower = maxReactivePower;
            VoltagePerMvar = voltagePerMvar;
            _nodes = new HashSet<int>(nodes);
        }

        /// <summary>
        /// Признак состояния «Включено».
        /// </summary>
        public bool IsOn { get; set; } = true;

        /// <summary>
        /// Исходная загрузка, Мвар.
        /// </summary>
        public double InitialReactivePower { get; }

        /// <summary>
        /// Qmin, Мвар.
        /// </summary>
        public double MinReactivePower { get; set; }

        /// <summary>
        /// Qmax, Мвар.
        /// </summary>
        public double MaxReactivePower { get; set; }

        /// <summary>
        /// Заданное напряжение, кВ.
        /// </summary>
        public double SetVoltage { get; set; } = 230;

        /// <summary>
        /// Изменение напряжения на 1 Мвар, кВ.
        /// </summary>
        public double VoltagePerMvar { get; }

        /// <summary>
        /// Текущая загрузка, Мвар.
        /// </summary>
        public double ReactivePower => Math.Min(Math.Max(InitialReactivePower, MinReactivePower), MaxReactivePower);

        /// <summary>
        /// Возвращает вклад СКРМ в напряжение узла.
        /// </summary>
        /// <param name="nodeNumber">Номер узла.</param>
        /// <returns>Изменение напряжения, кВ.</returns>
        public double GetEffect(int nodeNumber)
        {
            return IsOn && _nodes.Contains(nodeNumber)
                ? VoltagePerMvar * (ReactivePower - InitialReactivePower)
                : 0;
        }
    }
}
