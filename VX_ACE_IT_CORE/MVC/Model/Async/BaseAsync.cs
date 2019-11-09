using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        protected List<Task<Task<List<object>>>> Works = new List<Task<Task<List<object>>>>();
        protected int Precision;

        /// <summary>
        /// This is basic asynchronous structure impl. with task result usage. 
        /// </summary>
        /// <param name="debug"></param>
        /// <param name="gameProcess"></param>
        /// <param name="precision">Is used for thread sleep. Lover -> faster but needs more resources.</param>
        protected BaseAsync(BaseDebug debug, GameProcess.GameProcess gameProcess, int precision = 33)
        {
            this.Precision = precision;
            this.Debug = debug;
            this.GameProcess = gameProcess;
            DoWork();
        }

        protected void DoWork()
        {
            _workerThread = new Thread( async () =>
            {
                while (true)
                {
                    Thread.Sleep(Precision);
                    for (int i = 0; i < Works.Count; i++)
                    {
                        if (Works[i].Status == TaskStatus.Created)
                            Works[i].Start();
                        else if (Works[i].Status == TaskStatus.Faulted)
                        {
                            var ex = Works[i].Exception;
                            var faulted = Works[i].IsFaulted;
                            if (faulted && ex != null)
                            {
                                if (Works[i] as Task<List<T>> != null)
                                {
                                    await Debug.AddMessage_Assync<object>(new Message<object>("[" + this.GetType().Name + "] request faulted." + " |TaskID[" + Works[i].Id + "]"
                                                                                              + " |TaskResult[" + (Works[i] as Task<List<T>>).Exception.InnerException.Message + "]", MessageTypeEnum.Error)).ConfigureAwait(false);
                                }
                                else
                                {
                                    await Debug.AddMessage_Assync<object>(new Message<object>("[" + this.GetType().Name + "]  request faulted." + " |TaskID[" + Works[i].Id + "]"
                                                                                              + " |TaskResult[" + Works[i].Exception.Message + "]", MessageTypeEnum.Error)).ConfigureAwait(false);
                                    await Debug.AddMessage_Assync<object>(new Message<object>(ex.Data, MessageTypeEnum.Exception)).ConfigureAwait(false);
                                }
                                Works.RemoveAt(i);
                            };
                        }
                        if (i < Works.Count && Works[i].Status == TaskStatus.RanToCompletion)
                        {
                            Works.RemoveAt(i);
                        }
                    }
                }
            });
            _workerThread.Start();
        }

        protected void AddWork(Task<Task<List<object>>> tasks)
        {
            this.Works.Add(tasks);
        }

        protected async Task<T> ResultHandler(Task<T> task)
        {
            try
            {
                return await task.ConfigureAwait(false);
            }
            catch (AggregateException ae)
            {
                foreach (var e in ae.InnerExceptions)
                {
                    // Handle the custom exception.
                    if (e is AsyncException)
                    {
                        await Debug.AddMessage_Assync<object>(new Message<object>((e.Message), MessageTypeEnum.Exception)).ConfigureAwait(false);
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return default(T);
        }

        protected async Task<List<T>> ResultHandler(Task<List<T>> task)
        {
            try
            {
                return await task.ConfigureAwait(false);
            }
            catch (AggregateException ae)
            {
                foreach (var e in ae.InnerExceptions)
                {
                    // Handle the custom exception.
                    if (e is AsyncException)
                    {
                        await Debug.AddMessage_Assync<object>(new Message<object>((e.Message), MessageTypeEnum.Exception)).ConfigureAwait(false);
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

