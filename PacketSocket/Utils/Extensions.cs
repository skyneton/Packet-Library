using System.Collections.Concurrent;
using System.Collections.Generic;

namespace PacketSocket.Utils
{
    public static class Extensions
    {
        public static void Remove<T>(this ConcurrentBag<T> data, T target)
        {
            var removeQueue = new Queue<T>();
            while(!data.IsEmpty)
            {
                if (data.TryTake(out var item) && item.Equals(target))
                    break;
                
                removeQueue.Enqueue(item);
            }
            
            foreach (var item in removeQueue)
            {
                data.Add(item);
            }
        }
    }
}