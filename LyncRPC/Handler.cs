using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using AustinHarris.JsonRpc;

namespace LyncRPC
{
	public class Handler: IDisposable
	{
		private ManualResetEventSlim _stopSignal = null;

		private Socket _socket = null;
		private Stream _stream = null;
		private TextReader _reader = null;
		private TextWriter _writer = null;

		public Handler (Socket socket, ManualResetEventSlim stopSignal)
		{
			_stopSignal = stopSignal;
			_socket = socket;
			_stream = new BufferedStream (new NetworkStream (socket));
			_reader = new StreamReader (_stream);
			_writer = new StreamWriter (_stream);
		}

		void IDisposable.Dispose ()
		{
			_stopSignal = null;

			if (_reader != null) {
				_reader.Close ();
				_reader.Dispose ();
				_reader = null;
			}

			if (_writer != null) {
				_writer.Close ();
				_writer.Dispose ();
				_writer = null;
			}

			if (_stream != null) {
				_stream.Close ();
				_stream.Dispose ();
				_stream = null;
			}

			if (_socket != null) {
				_socket.Close ();
				_socket.Dispose ();
				_socket = null;
			}
		}

		public void Run ()
		{
			string line;
			while (!_stopSignal.IsSet && !string.IsNullOrEmpty (line = _reader.ReadLine ())) {
				ProcessLine (line);
			}
		}

		private void ProcessLine (string line)
		{
			Log.Verbose ("<<< " + line);

			var state = new JsonRpcStateAsync (HandleRpcResult, null);
			state.JsonRpc = line;
			JsonRpcProcessor.Process (state);
		}

		private void HandleRpcResult (IAsyncResult ar)
		{
			var state = (JsonRpcStateAsync)ar;
			var result = state.Result;
			Log.Verbose (">>> " + result);

			_writer.Write (result);
			_writer.Flush ();
		}
	}
}

