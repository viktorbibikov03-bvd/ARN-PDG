using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace VrdoPdg.View.Threading
{
    /// <summary>
    /// Рабочий поток с однопоточным подразделением (STA) для обращений к ASTRALib.
    /// Загрузка данных и расчет выполняются в этом потоке асинхронно, не блокируя
    /// главное окно; объект ASTRALib создается и используется только в нем.
    /// </summary>
    internal sealed class StaWorker : IDisposable
    {
        /// <summary>
        /// Очередь операций рабочего потока.
        /// </summary>
        private readonly BlockingCollection<Action> _queue = new BlockingCollection<Action>();

        /// <summary>
        /// Рабочий поток.
        /// </summary>
        private readonly Thread _thread;

        /// <summary>
        /// Создает и запускает рабочий поток.
        /// </summary>
        public StaWorker()
        {
            _thread = new Thread(ProcessQueue)
            {
                IsBackground = true,
                Name = "ASTRALib"
            };
            _thread.SetApartmentState(ApartmentState.STA);
            _thread.Start();
        }

        /// <summary>
        /// Выполняет функцию в рабочем потоке.
        /// </summary>
        /// <typeparam name="T">Тип результата.</typeparam>
        /// <param name="function">Функция.</param>
        /// <returns>Задача с результатом функции.</returns>
        public Task<T> Run<T>(Func<T> function)
        {
            TaskCompletionSource<T> completion = new TaskCompletionSource<T>(
                TaskCreationOptions.RunContinuationsAsynchronously);
            _queue.Add(() =>
            {
                try
                {
                    completion.SetResult(function());
                }
                catch (OperationCanceledException exception)
                {
                    completion.SetCanceled(exception.CancellationToken);
                }
                catch (Exception exception)
                {
                    completion.SetException(exception);
                }
            });
            return completion.Task;
        }

        /// <summary>
        /// Выполняет действие в рабочем потоке.
        /// </summary>
        /// <param name="action">Действие.</param>
        /// <returns>Задача выполнения действия.</returns>
        public Task Run(Action action)
        {
            return Run(() =>
            {
                action();
                return true;
            });
        }

        /// <summary>
        /// Завершает рабочий поток после выполнения поставленных операций.
        /// </summary>
        public void Dispose()
        {
            _queue.CompleteAdding();
            _thread.Join(TimeSpan.FromSeconds(10));
            _queue.Dispose();
        }

        /// <summary>
        /// Выполняет операции очереди до завершения добавления.
        /// </summary>
        private void ProcessQueue()
        {
            foreach (Action action in _queue.GetConsumingEnumerable())
            {
                action();
            }
        }
    }
}
