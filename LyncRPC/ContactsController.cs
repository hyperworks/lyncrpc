using System;
using System.Linq;
using Microsoft.Lync.Model;
using System.Threading.Tasks;
using Microsoft.Lync.Model.Group;
using System.Net.Sockets;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Lync.Controls.Internal.ViewModel;

namespace LyncRPC
{
    public struct ContactInfo
    {
        public string Uri;
        public string Name;
        public IEnumerable<string> Groups;
    }

    public class ContactsController: IDisposable
    {
        private ContactManager _contacts;

        public ContactsController (ContactManager contacts)
        {
            _contacts = contacts;
        }

        public void Dispose ()
        {
            _contacts = null;
        }

        public Task<IEnumerable<ContactInfo>> GetContactList ()
        {
            return Task.FromResult (_contacts.Groups.SelectMany (g => g.Select (c => new ContactInfo {
                Uri = c.Uri,
                Name = c.GetContactInformation (ContactInformationType.DisplayName) as string,
                Groups = c.CustomGroups.Select (cg => cg.Name).Concat (new string[]{ g.Name })
            })));
        }
    }
}

