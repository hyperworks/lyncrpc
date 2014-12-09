using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace LyncRPC
{
	public class Server
	{
		public static readonly IPEndPoint DefaultEndPoint = new IPEndPoint(new IPAddress(new byte[] { 0, 0, 0, 0 }), DefaultPort);
		public const int DefaultPort = 7331;
		public const int MaxClients = 1000;

		private IPEndPoint _endpoint = null;
		private SemaphoreSlim _clientSlots = null;

		private Thread _pumpThread = null;
		private ManualResetEventSlim _stopSignal = new ManualResetEventSlim ();
		private ManualResetEventSlim _stoppedSignal = new ManualResetEventSlim ();

		public Server (): this(DefaultEndPoint)
		{
		}

		public Server (IPEndPoint endpoint)
		{
			_endpoint = endpoint;
			_stopSignal.Reset ();
			_stoppedSignal.Reset ();
		}

		public void Start ()
		{
			Console.WriteLine ("starting...");
			if (_pumpThread != null) {
				Stop ();
			}

			_stopSignal.Reset ();
			_stoppedSignal.Reset ();

			_pumpThread = new Thread (clientPump);
			_pumpThread.IsBackground = false;
			_pumpThread.Start ();
		}

		public void Stop ()
		{
			Console.WriteLine ("stopping...");
			if (_pumpThread == null)
				return;

			_stopSignal.Set ();
			_stoppedSignal.Wait ();
			_pumpThread = null;
			Console.WriteLine ("stopped.");
		}

		private void clientPump ()
		{
			var listener = new TcpListener (_endpoint);
			listener.Start ();

			_clientSlots = new SemaphoreSlim (MaxClients, MaxClients);
			while (!_stopSignal.IsSet) {
				var socket = listener.AcceptSocket ();
				socket.NoDelay = true;

				var handler = new Thread (new ThreadStart (() => echo (socket)));
				handler.Start ();
			}

			for (var i = 0; i < MaxClients; i++)
				_clientSlots.Wait ();
			_stoppedSignal.Set ();
		}

		private void echo (Socket socket)
		{
			_clientSlots.Wait ();
			try {
				var stream = new NetworkStream (socket);
				var reader = new BufferedStream (stream);
				var buffer = new byte[4096];
				var bytesRead = 0;

				while (!_stopSignal.IsSet) {
					bytesRead = reader.Read (buffer, 0, buffer.Length);
					if (bytesRead == 0) {
						break;
					}

					stream.Write (buffer, 0, bytesRead);
				}

			} finally {
				_clientSlots.Release ();
			}
		}
	}
}

