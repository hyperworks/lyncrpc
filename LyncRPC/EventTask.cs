using System;
using System.Threading;
using System.Threading.Tasks;

namespace LyncRPC
{
	public class EventTask<T>: IDisposable where T: EventArgs
	{
		private ManualResetEventSlim _eventTriggered = new ManualResetEventSlim (false);
		private T _eventArgs = null;

		private Task<T> _task;

		public Task<T> Task { get { return _task; } }

		public EventTask (Action<EventHandler<T>> adder, Action<EventHandler<T>> remover)
		{
			adder (trigger);
			_task = new Task<T> (() => {
				_eventTriggered.Wait ();
				remover (trigger);
				return _eventArgs;
			});
		}

		public void Dispose ()
		{
			_eventTriggered.Dispose ();
			_eventTriggered = null;
			_eventArgs = null;
		}


		private void trigger (object sender, T e)
		{
			_eventArgs = e;
			_eventTriggered.Set ();
		}
	}
}

