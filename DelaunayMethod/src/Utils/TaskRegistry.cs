using System;
using System.Collections; 
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DelaunayMethod.Utils
{
    public class TaskRegistry<T> : ICollection<Func<T, Task>>
    {
        private readonly LinkedList<Func<T, Task>> _list = new LinkedList<Func<T, Task>>();
        readonly object exGate = new object();

        /// <exception cref="T:System.Exception">A delegate callback throws an exception.</exception>
        /// <exception cref="T:System.MemberAccessException">The caller does not have access to the method represented by the delegate (for example, if the method is private).</exception>
        public Task ExecuteAllAsync(T context, CancellationToken token = default)
        {
            PreAction?.Invoke();
            lock (exGate)
                foreach (var func in _list)
                {
                    OnEvent?.Invoke("Executing => " + func.Method.Name);
                    try
                    {
                        func(context).Wait(token);
                    }
                    catch (AggregateException)
                    {
                        break;
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                }
            OnCompleted?.Invoke("Sequence completed");
            AfterAction?.Invoke();
            return Task.CompletedTask;
        }

        public IEnumerator<Func<T, Task>> GetEnumerator()
        {
            lock (exGate)
                return _list.GetEnumerator();
        }

        public void Prepend(Func<T, Task> item)
        {
            lock (exGate)
                _list.AddFirst(item);
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(Func<T, Task> item)
        {
            lock (exGate)
                _list.AddLast(item);
        }

        public void Clear()
        {
            lock (exGate)
                _list.Clear();
        }

        public bool Contains(Func<T, Task> item)
        {
            lock (exGate)
                return _list.Contains(item);
        }

        public void CopyTo(Func<T, Task>[] array, int arrayIndex)
        {
            lock (exGate)
                _list.CopyTo(array, arrayIndex);
        }

        public bool Remove(Func<T, Task> item)
        {
            throw new NotImplementedException();
        }

        public int Count
        {
            get {
                lock (exGate) return _list.Count; }
        }

        public bool IsReadOnly => false;

        public event Action<string> OnCompleted;
        public event Action<string> OnEvent;
        public event Action AfterAction;
        public event Action PreAction;
    }

    internal static class TaskExtensions
    {
        /// <exception cref="T:System.Threading.Tasks.TaskCanceledException">Condition.</exception>
        public static Task<List<T>> ToListAsync<T>(this IEnumerable<T> list, CancellationToken token = default)
        {
            IEnumerable<T> enu()
            {
                foreach (var item in list)
                {
                    if (token.IsCancellationRequested)
                        throw new TaskCanceledException();
                    yield return item;
                }
            }

            return Task.FromResult(enu().ToList());
        }
    }
}