using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.Lync.Controls.Internal.ViewModel;
using Microsoft.Lync.Model;
using Microsoft.Lync.Model.Group;

namespace LyncRPC
{
    public struct ContactInfo
    {
        public string Uri;
        public string Name;
        public IEnumerable<string> Groups;
    }

    internal class ContactsController: Controller
    {
        public ContactsController (LyncClient client) : base (client)
        {
        }

        public Task<ContactAvailability> GetAvailability ()
        {
            LAssert.Pre (IsSignedIn, "not signed in.");
            var result = (ContactAvailability)Client.Self.Contact.GetContactInformation (ContactInformationType.Availability);
            return Task.FromResult (result);
        }

        public Task SetAvailability (ContactAvailability availability)
        {
            LAssert.Pre (IsSignedIn, "not signed in.");
            var dict = new Dictionary<PublishableContactInformationType, object> {
                { PublishableContactInformationType.Availability, availability }
            };

            return Task.Factory.FromAsync (Client.Self.BeginPublishContactInformation, Client.Self.EndPublishContactInformation, dict, null)
                .ContinueWith (HandleTaskException)
                .ContinueWith (task => Log.Info ("published availability: " + availability.ToString ()));
        }

        public Task<IEnumerable<ContactInfo>> GetContactList ()
        {
            return Task.FromResult (ContactManager.Groups.SelectMany (g => g.Select (c => new ContactInfo {
                Uri = c.Uri,
                Name = c.GetContactInformation (ContactInformationType.DisplayName) as string,
                Groups = c.CustomGroups.Select (cg => cg.Name).Concat (new string[]{ g.Name })
            })));
        }

        public Task<ContactAvailability> GetContactAvailability (string uri)
        {
            var contact = ContactManager.GetContactByUri (uri);
            var result = (ContactAvailability)contact.GetContactInformation (ContactInformationType.Availability);
            return Task.FromResult (result);
        }
    }
}

