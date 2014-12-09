using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using AustinHarris.JsonRpc;

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
		private ManualResetEventSlim _stopSignal = new ManualResetEventSlim (false);
		private ManualResetEventSlim _stoppedSignal = new ManualResetEventSlim (false);

		public Server (): this(DefaultEndPoint)
		{
		}

		public Server (IPEndPoint endpoint)
		{
			_endpoint = endpoint;
		}

		public void Start ()
		{
			if (_pumpThread != null) {
				Stop ();
			}

			_stopSignal.Reset ();
			_stoppedSignal.Reset ();

			_pumpThread = new Thread (ClientPump);
			_pumpThread.IsBackground = false;
			_pumpThread.Start ();
		}

		public void Stop ()
		{
			if (_pumpThread == null)
				return;

			_stopSignal.Set ();
			_stoppedSignal.Wait ();
			_pumpThread = null;
		}

		private void ClientPump ()
		{
			var listener = new TcpListener (_endpoint);
			listener.Start ();

			_clientSlots = new SemaphoreSlim (MaxClients, MaxClients);
			while (!_stopSignal.IsSet) {
				var socket = listener.AcceptSocket ();
				socket.NoDelay = true;

				new Thread(() => { HandleClient(socket); }).Start ();
			}

			for (var i = 0; i < MaxClients; i++)
				_clientSlots.Wait ();
			_stoppedSignal.Set ();
		}

		private void HandleClient (Socket socket)
		{
			_clientSlots.Wait ();
			try {
				new Handler(socket, _stopSignal).Run ();

			} finally {
				_clientSlots.Release ();
			}
		}
	}
}