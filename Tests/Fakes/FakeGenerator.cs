using System;
using System.Collections.Generic;

namespace VrdoPdg.Tests.Fakes
{
    /// <summary>
    /// Генератор тестовой модели режима: загрузка по реактивной мощности линейно зависит
    /// от заданного напряжения и ограничена пределами; вклад в напряжение пропорционален
    /// изменению загрузки.
    /// </summary>
    internal sealed class FakeGenerator
    {
        /// <summary>
        /// Узлы, напряжение в которых зависит от генератора.
        /// </summary>
        private readonly HashSet<int> _nodes;

        /// <summary>
        /// Создает генератор.
        /// </summary>
        /// <param name="nodeNumber">Узел подключения.</param>
        /// <param name="setVoltage">Исходное заданное напряжение, кВ.</param>
        /// <param name="reactivePower">Исходная загрузка, Мвар.</param>
        /// <param name="minReactivePower">Qmin, Мвар.</param>
        /// <param name="maxReactivePower">Qmax, Мвар.</param>
        /// <param name="reactivePowerPerKv">Изменение загрузки на 1 кВ заданного напряжения, Мвар.</param>
        /// <param name="voltagePerMvar">Изменение напряжения на 1 Мвар, кВ.</param>
        /// <param name="nodes">Узлы, напряжение в которых зависит от генератора.</param>
        public FakeGenerator(
            int nodeNumber,
            double setVoltage,
            double reactivePower,
            double minReactivePower,
            double maxReactivePower,
            double reactivePowerPerKv,
            double voltagePerMvar,
            params int[] nodes)
        {
            NodeNumber = nodeNumber;
            SetVoltage = setVoltage;
            InitialSetVoltage = setVoltage;
            InitialReactivePower = reactivePower;
            MinReactivePower = minReactivePower;
            MaxReactivePower = maxReactivePower;
            ReactivePowerPerKv = reactivePowerPerKv;
            VoltagePerMvar = voltagePerMvar;
            _nodes = new HashSet<int>(nodes);
        }

        /// <summary>
        /// Признак состояния «Включено».
        /// </summary>
        public bool IsOn { get; set; } = true;

        /// <summary>
        /// Узел подключения.
        /// </summary>
        public int NodeNumber { get; }

        /// <summary>
        /// Активная мощность, МВт.
        /// </summary>
        public double ActivePower { get; set; } = 100;

        /// <summary>
        /// Заданное напряжение, кВ.
        /// </summary>
        public double SetVoltage { get; set; }

        /// <summary>
        /// Исходное заданное напряжение, кВ.
        /// </summary>
        public double InitialSetVoltage { get; }

        /// <summary>
        /// Исходная загрузка, Мвар.
        /// </summary>
        public double InitialReactivePower { get; }

        /// <summary>
        /// Qmin, Мвар.
        /// </summary>
        public double MinReactivePower { get; }

        /// <summary>
        /// Qmax, Мвар.
        /// </summary>
        public double MaxReactivePower { get; }

        /// <summary>
        /// Изменение загрузки на 1 кВ заданного напряжения, Мвар.
        /// </summary>
        public double ReactivePowerPerKv { get; }

        /// <summary>
        /// Изменение напряжения на 1 Мвар, кВ.
        /// </summary>
        public double VoltagePerMvar { get; }

        /// <summary>
        /// Текущая загрузка, Мвар.
        /// </summary>
        public double ReactivePower
        {
            get
            {
                double value = InitialReactivePower + ReactivePowerPerKv * (SetVoltage - InitialSetVoltage);
                return Math.Min(Math.Max(value, MinReactivePower), MaxReactivePower);
            }
        }

        /// <summary>
        /// Возвращает вклад генератора в напряжение узла.
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
