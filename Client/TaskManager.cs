using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Client
{
    public class TaskManager<T>
    {
        //TASK is BinaryAutomataIterator.GetAllWithLongSynchronizedWord(minimalLength, size, index);
        private Thread[] threads = new Thread[0];
        public TaskManager(int threadCount, Func<int, bool> shouldCall, Action<T> consumeTask, Func<Task> callForAdditional, ConcurrentQueue<T> resources, Semaphore resourceSemaphore)
        {
            var loadResource = new object();
            var relativeResources = 0;

            threads = Enumerable.Range(0, threadCount).Select(i => new Thread(threadTaskAsync)).ToArray();


            async void threadTaskAsync()
            {
                while (true)
                {
                    var call = false;

                    if (shouldCall(resources.Count))
                        lock (loadResource)
                        {
                            if (shouldCall(resources.Count) && resources.Count > relativeResources)
                            {
                                call = true;
                                relativeResources = resources.Count;
                            }
                        }

                    if (call)
                        await callForAdditional();

                    try
                    {
                        resourceSemaphore.WaitOne();
                    }
                    catch(Exception e)
                    {
                        break;
                    }

                    T task;

                    lock (loadResource)
                    {
                        if (!resources.TryDequeue(out task))
                            throw new Exception("Could not dequeue.");

                        relativeResources -= 1;
                    }

                    consumeTask(task);
                }
            }
        }

        public void Launch()
        {
            foreach (var thread in threads)
                thread.Start();
        }

        public void Abort()
        {
            foreach (var thread in threads)
                thread.Abort();
        }
    }
}
