using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Policy;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Lync.Model;
using Microsoft.Lync.Model.Conversation;
using System.Runtime.InteropServices;

namespace LyncRPC
{
    internal class ConversationController: Controller
    {
        private Conversation _conversation = null;
        private InstantMessageModality _modality = null;

        public ConversationController (LyncClient client) : base (client)
        {
        }

        public override void Dispose ()
        {
            _conversation = null;
            _modality = null;
            base.Dispose ();
        }


        public async Task SendMessage (string message)
        {
            LAssert.Pre (_modality != null, "not in a conversation or it has not completely begun.");
            LAssert.Pre (_conversation.Participants.Count > 0, "no participants in conversation.");

            var content = new Dictionary<InstantMessageContentType, string> {
                { InstantMessageContentType.PlainText, message }
            };

            await Task.Factory.FromAsync (_modality.BeginSetComposing, _modality.EndSetComposing, true, null);
            await Task.Factory.FromAsync (_modality.BeginSendMessage, _modality.EndSendMessage, content, null);
            await Task.Factory.FromAsync (_modality.BeginSetComposing, _modality.EndSetComposing, false, null);

            Log.Info ("lync: message sent.");
        }

        public async Task BeginConversation (string recipientUri)
        {
            LAssert.Pre (_modality == null, "already in a conversation or not completely ended.");

            var conversation = ConversationManager.Conversations
				.Where (c => c.Participants.Count == 2)
				.FirstOrDefault (c => c.Participants.Any (p => p.Contact.Uri == recipientUri));
            if (conversation != null) {
                _conversation = conversation;
                _modality = (InstantMessageModality)conversation.SelfParticipant.Modalities [ModalityTypes.InstantMessage];
                Log.Info ("lync: attached to existing conversation.");
                return;
            }

            var addition = new EventWaiter<ConversationManagerEventArgs> (
                               h => ConversationManager.ConversationAdded += h,
                               h => ConversationManager.ConversationAdded -= h);

            _conversation = ConversationManager.AddConversation ();
            Log.Verbose ("lync: waiting for conversation...");
            await addition.Task;

            _conversation = addition.Task.Result.Conversation;
            var contact = ContactManager.GetContactByUri (recipientUri);
            var participation = new EventWaiter<ParticipantCollectionChangedEventArgs> (
                                    h => _conversation.ParticipantAdded += h,
                                    h => _conversation.ParticipantRemoved -= h);

            _conversation.AddParticipant (contact);
            Log.Verbose ("lync: waiting for participant addition...");
            await participation.Task;

            var self = _conversation.SelfParticipant;
            var modality = (InstantMessageModality)self.Modalities [ModalityTypes.InstantMessage];
            // Document seems to state so but it is not always connected:
            // LAssert.Lync (modality.State == ModalityState.Connected, "IM modality not connected.");

            _modality = modality;
            Log.Info ("lync: conversation began.");
        }

        public async Task EndConversation ()
        {
            LAssert.Pre (_conversation != null && _modality != null, "not in a conversation or it has already ended.");

            var waiter = new EventWaiter<ConversationManagerEventArgs> (
                             h => ConversationManager.ConversationRemoved += h,
                             h => ConversationManager.ConversationRemoved -= h);

            _conversation.End ();
            _conversation = null;
            await waiter.Task;

            _modality = null;
            Log.Info ("lync: conversation ended.");
        }
    }
}

