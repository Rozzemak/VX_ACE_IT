using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using VX_ACE_IT_CORE.Debug;
using VX_ACE_IT_CORE.MVC.Model.Exceptions;

namespace VX_ACE_IT_CORE.MVC.Model.Async
{
    public abstract class BaseAsync<T>
    {
        protected BaseDebug Debug;
        protected GameProcess.GameProcess GameProcess;
        protected List<T> ServiceCollection = new List<T>();
        private Thread _workerThread;
        protected readonly ConcurrentBag<Task<List<object>>> Works = new ConcurrentBag<Task<List<object>>>();
        protected int Precision;

        /// <summary>
        /// This is basic asynchronous structure impl. with task result usage. 
        /// </summary>
        /// <param name="debug"></param>
        /// <param name="gameProcess"></param>
        /// <param name="precision">Is used for thread sleep. Lover -> faster but needs more resources.</param>
        protected BaseAsync(BaseDebug debug, GameProcess.GameProcess gameProcess, int precision = 33)
        {
            Precision = precision;
            Debug = debug;
            GameProcess = gameProcess;
            DoWork();
        }

        protected void DoWork()
        {
            _workerThread = new Thread(async () =>
            {
                while (true)
                {
                    Thread.Sleep(Precision);
                    for (var i = 0; i < Works.Count; i++)
                    {
                        var work = Works.ElementAt(i);
                        if (work?.Status == TaskStatus.Created)
                            work.Start();
                        else if (work?.Status == TaskStatus.Faulted)
                        {
                            var ex = work.Exception;
                            var faulted = work.IsFaulted;
                            if (faulted && ex != null)
                            {
                                await Debug.AddMessage_Async<object>(new Message<object>("[" + GetType().Name + "]  request faulted." + " |TaskID[" + work.Id + "]"
                                                                                         + " |TaskResult[" + work?.Exception?.Message + "]", MessageTypeEnum.Error)).ConfigureAwait(false);
                                await Debug.AddMessage_Async<object>(new Message<object>(ex.Data, MessageTypeEnum.Exception)).ConfigureAwait(false);
                                Works.TryTake(out work);
                            };
                        }
                        if (i < Works.Count && work?.Status == TaskStatus.RanToCompletion)
                        {
                            Works.TryTake(out work);
                        }
                    }
                }
            });
            _workerThread.Start();
        }

        protected void AddWork(Task<List<object>> tasks)
        {
            Works.Add(tasks);
        }

        protected T ResultHandler(Task<T> task)
        {
            try
            {
                task.Wait(-1);
                return task.Result;
            }
            catch (AggregateException ae)
            {
                foreach (var e in ae.InnerExceptions)
                {
                    // Handle the custom exception.
                    if (e is AsyncException)
                    {
                        Debug.AddMessage_Async<object>(new Message<object>((e.Message), MessageTypeEnum.Exception)).ConfigureAwait(false);
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return default(T);
        }

        protected List<T> ResultHandler(Task<List<T>> task)
        {
            try
            {
                // ReSharper disable once AsyncConverter.AsyncWait
                task.Wait(-1);
                // ReSharper disable once AsyncConverter.AsyncWait
                return task.Result;
            }
            catch (AggregateException ae)
            {
                foreach (var e in ae.InnerExceptions)
                {
                    // Handle the custom exception.
                    if (e is AsyncException)
                    {
                        Debug.AddMessage_Async<object>(new Message<object>((e.Message), MessageTypeEnum.Exception)).ConfigureAwait(false);
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return new List<T>() { default(T) };
        }
    }

}

