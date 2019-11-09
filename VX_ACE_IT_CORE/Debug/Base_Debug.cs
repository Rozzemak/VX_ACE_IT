using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace VX_ACE_IT_CORE.Debug
{
    public class BaseDebug
    {
        private readonly Thread _debugThread;
        private ConsoleSettings _consoleSettings = new ConsoleSettings(Encoding.UTF8);

        protected delegate void Worker(MessageTypeEnum messageTypeOnly = MessageTypeEnum.DefaultWriteAll);
        protected Worker Worker1;

        private readonly ConcurrentQueue<Message<object>> _messages = new ConcurrentQueue<Message<object>>();
        private readonly List<Task> _tasks = new List<Task>();

        private bool _isSafeToDelete;

        public EventHandler OnMessageAdded;

        public BaseDebug(bool threadStart = true)
        {
            _debugThread = new Thread( async () =>
            {
                while (true)
                {
                    Thread.Sleep(10);
                    for (var i = 0; i < _tasks.Count; i++)
                    {
                        switch (_tasks[i]?.Status)
                        {
                            case TaskStatus.Created:
                                _tasks[i]?.Start();
                                break;
                            case TaskStatus.Faulted:
                            {
                                var ex = _tasks[i].Exception;
                                var faulted = _tasks[i].IsFaulted;
                                if (faulted && ex != null)
                                {
                                    var task = _tasks[i];
                                    _tasks.RemoveAt(i);
                                    try
                                    {
                                        await ResultHandlerAsync(task).ConfigureAwait(false);
                                    }
                                    catch (AggregateException e)
                                    {
                                        HandleUnadledExceptions(e);
                                    }
                                };
                                break;
                            }
                        }

                        if (i < _tasks.Count && _tasks[i]?.Status == TaskStatus.RanToCompletion)
                        {
                            _tasks.RemoveAt(i);
                        }
                    }
                    Thread.Sleep(10);
                    if (Worker1 != null && _tasks.Count > 0)
                    {
                        Work();
                    }
                }
            });
            if (threadStart)
                _debugThread.Start();
            //  Worker1 += Work_2;

        }

        public void AddMessage<T>(Message<object> msg)
        {
            var task = new Task(() =>
            {
                _messages.Enqueue(msg);
                OnMessageAdded?.Invoke(msg, EventArgs.Empty);
                PrintAllPendingMessages();
            });
            _tasks.Add(task);
        }

        public void AddMessages<T>(List<Message<object>> msgs)
        {
            var task = new Task(() =>
            {
                foreach (var msg in msgs)
                {
                    _messages.Enqueue(msg);
                    OnMessageAdded?.Invoke(msg, EventArgs.Empty);
                }
                PrintAllPendingMessages();
            });
            _tasks.Add(task);
        }

        public async Task<bool> AddMessage_Async<T>(Message<object> msg)
        {
            var task = new Task(() =>
            {
                _messages.Enqueue(msg);
                OnMessageAdded?.Invoke(msg, EventArgs.Empty);
                Dispatcher.FromThread(_debugThread)?.Invoke(() => PrintAllPendingMsg());
                while (_messages.Count > 0)
                {
                    Thread.Sleep(5);
                }
            });
            task.Start();
            _tasks.Add(task);
            await ResultHandlerAsync(task).ConfigureAwait(false);
            return task.Status == TaskStatus.RanToCompletion;
        }

        private void PrintAllPendingMsg(MessageTypeEnum messageTypeOnly = MessageTypeEnum.DefaultWriteAll)
        {
            lock (Console.Out)
            {

                if (messageTypeOnly == MessageTypeEnum.DefaultWriteAll)
                {

                    for (var i = 0; i < _messages.Count; i++)
                    {
                        var msg = _messages.ElementAt(i);
                        if (_isSafeToDelete || msg is null || msg.shown) continue;
                        // Console.ResetColor();
                        switch (msg.MessageType)
                        {
                            case MessageTypeEnum.Standard:
                                break;
                            case MessageTypeEnum.Warning:
                                Console.BackgroundColor = ConsoleColor.Red;
                                break;
                            case MessageTypeEnum.Error:
                                Console.ForegroundColor = ConsoleColor.Red;
                                break;
                            case MessageTypeEnum.Exception:
                                Console.BackgroundColor = ConsoleColor.Red;
                                Console.ForegroundColor = ConsoleColor.Black;
                                break;
                            case MessageTypeEnum.Indifferent:
                                Console.BackgroundColor = ConsoleColor.Yellow;
                                Console.ForegroundColor = ConsoleColor.Black;
                                break;
                            case MessageTypeEnum.Event:
                                Console.BackgroundColor = ConsoleColor.DarkRed;
                                Console.ForegroundColor = ConsoleColor.Green;
                                break;
                            case MessageTypeEnum.Rest:
                                break;
                            case MessageTypeEnum.HttpClient:
                                Console.ForegroundColor = ConsoleColor.Green;
                                break;
                            case MessageTypeEnum.DefaultWriteAll:
                                break;
                        }
                        Console.WriteLine("[" + Enum.GetName(typeof(MessageTypeEnum), msg.MessageType) +
                                          "]" + msg.MessageContent);
                        Console.ResetColor();
                        msg.shown = true;
                    }
                }
                else
                {
                    for (var i = 0; i < _messages.Count; i++)
                    {
                        var msg = _messages.ElementAt(i);
                        if (msg.MessageType == messageTypeOnly)
                            Console.WriteLine("[" + Enum.GetName(typeof(MessageTypeEnum), msg.MessageType) + "]" + msg.MessageContent);
                        msg.shown = true;
                    }
                }
                _isSafeToDelete = true;
                while (_isSafeToDelete)
                {
                    _messages.Clear();
                    _isSafeToDelete = false;
                }            

            }
        }

        public void PrintAllPendingMessages(MessageTypeEnum messageTypeOnly = MessageTypeEnum.DefaultWriteAll)
        {
            // Will not allow for handler to be subscribed to Worker delegate. 
            // Used for exception throwing puproses.
            // if (!Worker1.GetInvocationList().Any(x => x.Method.Name.Equals(nameof(PrintAllPendingMsg))))
            lock (_messages)
            {
                Worker1 = PrintAllPendingMsg;
            }
        }

        protected void Work()
        {
            lock (_messages)
            {
                Worker1();
            }
        }

        protected async Task ResultHandlerAsync(Task task)
        {
            try
            {
                await task.ConfigureAwait(false);
            }
            catch (AggregateException ae)
            {
                foreach (var e in ae.InnerExceptions)
                {
                    // Handle the custom exception.
                    if (e is DebugException)
                    {
                        AddMessage<object>(new Message<object>((e.Message), MessageTypeEnum.Exception));
                    }
                    else
                    {
                        HandleUnadledExceptions(ae);
                    }
                }
            }
        }

        /// <summary>
        /// DebugException is meant for debug message delivery to cli. If that fails, exception is raised. 
        /// However, There could be exceptions, which does not relate to message processing, but .Net itself,
        /// as calling empty delegate etc...  
        /// </summary>
        /// <param name="ae"></param>
        private void HandleUnadledExceptions(AggregateException ae)
        {
            var msg = "DebugClass Exception: \n";
            msg += ae.Message + "\n";
            if (ae.InnerException != null)
                msg += "Worker delegate propably not init. try => Worker1 += PrintAllPendingMsg " + "\n";
            msg = ae.InnerExceptions.Aggregate(msg, (current, innerException) => current + (innerException.Message + "\n"));
            msg += "Quick fix: Run this program with '--nodebug' param, => NOT IMPL YET :(" + "\n";
            Console.WriteLine(msg);
            throw new Exception(msg);
        }
    }
}
