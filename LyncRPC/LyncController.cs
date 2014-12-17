using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Lync.Model;

namespace LyncRPC
{
    public class LyncController: IDisposable
    {
        private LyncClient _client;
        public readonly ConversationController Conversations;
        public readonly ContactsController Contacts;

        public bool ShouldInitialize { get { return _client.State == ClientState.Uninitialized; } }

        public bool CanSignIn { get { return _client.State == ClientState.SignedOut; } }

        public bool IsSignedIn { get { return _client.State == ClientState.SignedIn; } }

        public LyncController ()
        {
            _client = LyncClient.GetClient (false);
            _client.CredentialRequested += client_CredentialRequested;
            _client.StateChanged += client_StateChanged;
            _client.SignInDelayed += client_SignInDelayed;
            Log.Info ("lync: current state: " + _client.State.ToString ());

            Conversations = new ConversationController (_client.ConversationManager);
            Contacts = new ContactsController (_client.ContactManager);
        }

        public void Dispose ()
        {
            _client.StateChanged -= client_StateChanged;
            _client = null;

            Conversations.Dispose ();
            Contacts.Dispose ();
        }


        #region "Initialization and SignIn/Out"

        public Task Initialize ()
        {
            LAssert.Pre (ShouldInitialize, "already initializing or initialized.");
            Log.Info ("lync: initializing...");
            return Task.Factory.FromAsync (_client.BeginInitialize, _client.EndInitialize, null)
				.ContinueWith (handleException)
				.ContinueWith (task => Log.Info ("lync: initialized."));
        }

        public Task SignIn (string serverUrl, string signInAddress, string username, string password)
        {
            LAssert.Pre (CanSignIn, "not signed out.");
            _client.SignInConfiguration.Mode = LyncClientConfigurationMode.Manual;
            _client.SignInConfiguration.InternalServerUrl = serverUrl;
            _client.SignInConfiguration.ExternalServerUrl = serverUrl;
            _client.SignInConfiguration.SignInAutoRetry = false;
            _client.SignInConfiguration.IsPasswordSaved = false;

            var credsHandler = new EventHandler<CredentialRequestedEventArgs> ((sender, e) => {
                if (e.Type != CredentialRequestedType.LyncAutodiscover)
                    return;

                Log.Info ("lync: submitting credentials...");
                e.Submit (username, password, false);
            });

            var finish = new Action<Task> (task => {
                _client.CredentialRequested -= credsHandler;
                _client.CredentialRequested += client_CredentialRequested;
                if (IsSignedIn) {
                    Log.Info ("lync: signed in " + username);
                } else {
                    throw new Exception ("sign-in failure, last state: " + _client.State.ToString ());
                }
            });

            Log.Info ("lync: signing in...");
            _client.CredentialRequested -= client_CredentialRequested;
            _client.CredentialRequested += credsHandler;
            return Task.Factory.FromAsync (_client.BeginSignIn, _client.EndSignIn, signInAddress, username, password, null)
				.ContinueWith (finish)
				.ContinueWith (handleException);
        }

        public Task SignOut ()
        {
            LAssert.Pre (!CanSignIn, "not signed in.");
            return Task.Factory.FromAsync (_client.BeginSignOut, _client.EndSignOut, null)
				.ContinueWith (handleException);
        }

        #endregion


        #region "Presence Information"

        public Task<ContactAvailability> GetAvailability ()
        {
            LAssert.Pre (IsSignedIn, "not signed in.");
            return new Task<ContactAvailability> (() => (ContactAvailability)_client.Self.Contact.GetContactInformation (ContactInformationType.Availability))
				.ContinueWith<ContactAvailability> (handleException);
        }

        public Task SetAvailability (ContactAvailability availability)
        {
            LAssert.Pre (IsSignedIn, "not signed in.");
            var dict = new Dictionary<PublishableContactInformationType, object> {
                { PublishableContactInformationType.Availability, availability }
            };
				
            return Task.Factory.FromAsync (_client.Self.BeginPublishContactInformation, _client.Self.EndPublishContactInformation, dict, null)
				.ContinueWith (handleException)
				.ContinueWith (task => Log.Info ("published availability: " + availability.ToString ()));
        }

        #endregion


        #region "Client Events"

        private void client_StateChanged (object sender, ClientStateChangedEventArgs e)
        {
            Log.Info ("lync: state changed to: " + e.NewState.ToString ());
        }

        private void client_CredentialRequested (object sender, CredentialRequestedEventArgs e)
        {
            e.Cancel (true);
        }

        private void client_SignInDelayed (object sender, SignInDelayedEventArgs e)
        {
            Log.Info ("lync: sign in delayed, code: " + e.StatusCode.ToString ());
        }

        #endregion


        private void handleException (Task task)
        {
            if (task.Exception != null) {
                Log.Error (task.Exception.ToString ());
                throw task.Exception;
            }
        }

        private T handleException<T> (Task<T> task)
        {
            var result = task.Result; // cause task to run immediately.
            handleException ((Task)task);
            return result;
        }
    }
}

