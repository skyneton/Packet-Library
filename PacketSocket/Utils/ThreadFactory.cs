using System.Collections.Concurrent;
using System.Threading;

namespace PacketSocket.Utils
{
    public class ThreadFactory
    {
        private readonly ConcurrentQueue<Thread> _threads = new ();

        public Thread LaunchThread(Thread thread)
        {
            thread.Start();
            _threads.Enqueue(thread);

            return thread;
        }

        public void KillAll()
        {
            while (!_threads.IsEmpty)
            {
                if (!_threads.TryDequeue(out var thread)) return;
                if(thread.IsAlive) thread.Interrupt();
            }
        }
    }
}