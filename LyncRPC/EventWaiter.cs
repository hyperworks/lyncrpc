using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace LyncRPC
{
    public class EventWaiter<T> where T: EventArgs
    {
        readonly TaskCompletionSource<T> _completion;
        readonly Action<EventHandler<T>> _remover;
        readonly Func<object, T, bool> _predicate;

        public Task<T> Task { get { return _completion.Task; } }

        public EventWaiter (Action<EventHandler<T>> adder, Action<EventHandler<T>> remover) : this (adder, remover, (obj, e) => true)
        {
        }

        public EventWaiter (Action<EventHandler<T>> adder,
                            Action<EventHandler<T>> remover,
                            Func<object, T, bool> predicate) : base ()
        {
            _remover = remover;
            _predicate = predicate;
            _completion = new TaskCompletionSource<T> ();
            adder (trigger);
        }

        void trigger (object sender, T e)
        {
            if (_predicate != null && !_predicate (sender, e))
                return;

            _completion.SetResult (e);
            _remover (trigger);
        }
    }
}

