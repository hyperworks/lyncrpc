using System;
using System.Threading.Tasks;
using Microsoft.Lync.Model;
using Microsoft.Lync.Model.Conversation;
using LyncRPC;

namespace LyncRPC
{
    internal class Controller: IDisposable
    {
        protected readonly LyncClient Client;

        protected ContactManager ContactManager { get { return Client.ContactManager; } }

        protected ConversationManager ConversationManager { get { return Client.ConversationManager; } }

        public bool CanSignIn { get { return Client.State == ClientState.SignedOut; } }

        public bool IsSignedIn { get { return Client.State == ClientState.SignedIn; } }


        public Controller ()
        {
            Client = LyncClient.GetClient (false);
        }

        public Controller (LyncClient client)
        {
            Client = client;
        }

        public virtual void Dispose ()
        {
            // TODO:
        }


        protected void HandleTaskException (Task task)
        {
            if (task.Exception != null) {
                Log.Error (task.Exception.ToString ());
                throw task.Exception;
            }
        }

        protected T HandleTaskException<T> (Task<T> task)
        {
            var result = task.Result; // cause task to run immediately.
            HandleTaskException ((Task)task);
            return result;
        }

    }
}

