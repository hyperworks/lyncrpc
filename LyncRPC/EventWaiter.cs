using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace LyncRPC
{
    public class EventWaiter<T>: TaskCompletionSource<T> where T: EventArgs
    {
        private Action<EventHandler<T>> _remover = null;

        public EventWaiter (Action<EventHandler<T>> adder, Action<EventHandler<T>> remover) : base ()
        {
            _remover = remover;
            adder (trigger);
        }

        private void trigger (object sender, T e)
        {
            SetResult (e);
            _remover (trigger);
        }
    }
}

