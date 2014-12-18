using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Lync.Model;
using Microsoft.Lync.Model.Conversation;
using System.Collections.Concurrent;

namespace LyncRPC
{
    class ConversationController: Controller
    {
        readonly BlockingCollection<IDictionary<InstantMessageContentType, string>> _messages;

        Conversation _conversation = null;
        InstantMessageModality _modality = null;

        public ConversationController (LyncClient client) : base (client)
        {
            var queue = new ConcurrentQueue<IDictionary<InstantMessageContentType, string>> ();
            _messages = new BlockingCollection<IDictionary<InstantMessageContentType, string>> (queue, 10);
        }

        public override void Dispose ()
        {
            _messages.Dispose ();
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

        public async Task<string> ReceiveMessage ()
        {
            LAssert.Pre (_modality != null, "not in a conversation or it has not complete begun.");
            LAssert.Pre (_conversation.Participants.Count > 0, "no participants in conversation.");

            Log.Verbose ("lync: waiting for message...");
            var content = await Task.Factory.StartNew (() => _messages.Take ());
            if (content.ContainsKey (InstantMessageContentType.PlainText)) {
                return content [InstantMessageContentType.PlainText];
            } else if (content.ContainsKey (InstantMessageContentType.RichText)) {
                return content [InstantMessageContentType.RichText];
            }

            LAssert.Lync (false, "received message has unsupported content type.");
            return null;
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

            await attachIM ((InstantMessageModality)_conversation.Modalities [ModalityTypes.InstantMessage]);
            Log.Info ("lync: conversation began.");
        }

        public async Task AcceptConversation ()
        {
            LAssert.Pre (_modality == null, "already in a conversation or not completely ended.");

            var handler = new Func<object, ConversationManagerEventArgs, bool> ((sender, e) => {
                var modality = e.Conversation.Modalities [ModalityTypes.InstantMessage];
                if (modality.State == ModalityState.Notified) {
                    modality.Accept ();
                    return true;
                }

                return false;
            });

            var addition = new EventWaiter<ConversationManagerEventArgs> (
                               h => ConversationManager.ConversationAdded += h,
                               h => ConversationManager.ConversationAdded -= h,
                               handler);
            await addition.Task; // wait for invitation to comes in.

            _conversation = addition.Task.Result.Conversation;
            var participant = _conversation.Participants
                .FirstOrDefault (p => p.Contact.Uri != _conversation.SelfParticipant.Contact.Uri);
            LAssert.Lync (participant != null, "conversation has no participant.");

            await attachIM ((InstantMessageModality)participant.Modalities [ModalityTypes.InstantMessage]);
            Log.Info ("lync: conversation invite accepted.");
        }

        async Task attachIM (InstantMessageModality modality)
        {
            // Document seems to state so but it is not always connected:
            // LAssert.Lync (modality.State == ModalityState.Connected, "IM modality not connected.");

            if (_modality != null) {
                _modality.InstantMessageReceived -= modality_InstantMessageReceived;
                _modality = null;
            }

            if (modality != null) {
                _modality = modality;
                _modality.InstantMessageReceived += modality_InstantMessageReceived;

                if (_modality.State != ModalityState.Connected) {
                    var waiter = new EventWaiter<ModalityStateChangedEventArgs> (
                                     h => modality.ModalityStateChanged += h,
                                     h => modality.ModalityStateChanged -= h,
                                     (sender, e) => {
                            Log.Verbose ("lync: modality state changed to: " + e.NewState.ToString ());
                            return e.NewState == ModalityState.Connected;
                        });

                    Log.Verbose ("lync: waiting for IM to connect...");
                    await waiter.Task;
                }
            }
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

            await attachIM (null);
            Log.Info ("lync: conversation ended.");
        }


        void modality_InstantMessageReceived (object sender, MessageSentEventArgs e)
        {
            Log.Info ("lync: message received.");
            _messages.Add (e.Contents);
        }
    }
}

