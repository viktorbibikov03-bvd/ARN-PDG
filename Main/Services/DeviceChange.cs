using System;

using VrdoPdg.Model.Entities;

namespace VrdoPdg.Model.Services
{
    /// <summary>
    /// Изменение состояния или загрузки СКРМ, выбранное алгоритмом ввода режима.
    /// </summary>
    internal sealed class DeviceChange
    {
        /// <summary>
        /// Действие, применяющее изменение к МТ.
        /// </summary>
        private readonly Action _apply;

        /// <summary>
        /// Создает изменение СКРМ.
        /// </summary>
        /// <param name="device">СКРМ.</param>
        /// <param name="direction">Направление ввода режима.</param>
        /// <param name="description">Описание изменения для протокола.</param>
        /// <param name="apply">Действие, применяющее изменение к МТ.</param>
        /// <param name="reactivePowerBefore">Загрузка генератора до изменения, Мвар.</param>
        public DeviceChange(
            Device device,
            RegulationDirection direction,
            string description,
            Action apply,
            double? reactivePowerBefore)
        {
            Device = device;
            Direction = direction;
            Description = description;
            _apply = apply;
            ReactivePowerBefore = reactivePowerBefore;
        }

        /// <summary>
        /// СКРМ.
        /// </summary>
        public Device Device { get; }

        /// <summary>
        /// Направление ввода режима.
        /// </summary>
        public RegulationDirection Direction { get; }

        /// <summary>
        /// Описание изменения для протокола.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Загрузка генератора по реактивной мощности до изменения, Мвар;
        /// для остальных типов СКРМ не задана.
        /// </summary>
        public double? ReactivePowerBefore { get; }

        /// <summary>
        /// Применяет изменение к МТ.
        /// </summary>
        public void Apply()
        {
            _apply();
        }
    }
}
