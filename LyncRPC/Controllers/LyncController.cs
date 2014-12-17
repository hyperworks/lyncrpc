using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Lync.Model;

namespace LyncRPC
{
    internal class LyncController: Controller
    {
        public readonly ContactsController Contacts;
        public readonly ConversationController Conversation;

        public bool ShouldInitialize { get { return Client.State == ClientState.Uninitialized; } }


        public LyncController () : base ()
        {
            Log.Info ("lync: current state: " + Client.State.ToString ());
            Client.CredentialRequested += client_CredentialRequested;
            Client.StateChanged += client_StateChanged;
            Client.SignInDelayed += client_SignInDelayed;

            Contacts = new ContactsController (Client);
            Conversation = new ConversationController (Client);
        }

        public override void Dispose ()
        {
            Client.CredentialRequested -= client_CredentialRequested;
            Client.StateChanged -= client_StateChanged;
            Client.SignInDelayed -= client_SignInDelayed;

            Contacts.Dispose ();
            Conversation.Dispose ();
            base.Dispose ();
        }


        public Task Initialize ()
        {
            LAssert.Pre (ShouldInitialize, "already initializing or initialized.");
            Log.Info ("lync: initializing...");
            return Task.Factory.FromAsync (Client.BeginInitialize, Client.EndInitialize, null)
				.ContinueWith (HandleTaskException)
				.ContinueWith (task => Log.Info ("lync: initialized."));
        }

        public async Task SignIn (string serverUrl, string signInAddress, string username, string password)
        {
            LAssert.Pre (CanSignIn, "not signed out.");
            Client.SignInConfiguration.Mode = LyncClientConfigurationMode.Manual;
            Client.SignInConfiguration.InternalServerUrl = serverUrl;
            Client.SignInConfiguration.ExternalServerUrl = serverUrl;
            Client.SignInConfiguration.SignInAutoRetry = false;
            Client.SignInConfiguration.IsPasswordSaved = false;

            var credsHandler = new EventHandler<CredentialRequestedEventArgs> ((sender, e) => {
                if (e.Type != CredentialRequestedType.LyncAutodiscover)
                    return;

                Log.Info ("lync: submitting credentials...");
                e.Submit (username, password, false);
            });

            Log.Info ("lync: signing in...");
            Client.CredentialRequested -= client_CredentialRequested;
            Client.CredentialRequested += credsHandler;
            await Task.Factory.FromAsync (Client.BeginSignIn, Client.EndSignIn, signInAddress, username, password, null);
            Client.CredentialRequested -= credsHandler;
            Client.CredentialRequested += client_CredentialRequested;

            LAssert.Post (IsSignedIn, "sign-in failure, last state: " + Client.State.ToString ());
            Log.Info ("lync: signed in " + username);
        }

        public Task SignOut ()
        {
            LAssert.Pre (!CanSignIn, "not signed in.");
            return Task.Factory.FromAsync (Client.BeginSignOut, Client.EndSignOut, null)
				.ContinueWith (HandleTaskException);
        }


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
    }
}

