using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Lync.Model.Conversation;

namespace LyncRPC
{
	public class ConversationController
	{
		private ConversationManager _manager;
		private Conversation _conversation;

		public ConversationController (ConversationManager manager)
		{
			_manager = manager;
			_conversation = null;
		}

		public Task BeginConversation ()
		{
			LAssert.Pre (_conversation == null, "already in a conversation.");

			var waiter = new EventTask<ConversationManagerEventArgs> (
				             h => _manager.ConversationAdded += h,
				             h => _manager.ConversationAdded -= h);

			_conversation = _manager.AddConversation ();
			return waiter.Task;
		}

		public Task EndConversation ()
		{
			LAssert.Pre (_conversation != null, "not in a conversation.");

			var waiter = new EventTask<ConversationManagerEventArgs> (
				             h => _manager.ConversationRemoved += h,
				             h => _manager.ConversationRemoved -= h);

			_conversation.End ();
			_conversation = null;
			return waiter.Task;
		}
	}
}

