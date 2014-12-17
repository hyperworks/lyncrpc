using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;
using AustinHarris.JsonRpc;
using Microsoft.Lync.Model;

namespace LyncRPC
{
    public class RPCService: JsonRpcService
    {
        private LyncController _lync;

        internal RPCService (LyncController controller) : base ()
        {
            _lync = controller;
        }

        public struct EmptyRequest
        {
        }

        public struct HelloRequest
        {
            public string Name;
        };

        [JsonRpcMethod ("HELLO")]
        public string Hello (HelloRequest request)
        {
            return "Hello, " + request.Name;
        }

        [JsonRpcMethod ("PING")]
        public string Ping (EmptyRequest _)
        {
            Log.Info ("server: pinged.");
            return "PONG";
        }

        [JsonRpcMethod ("DATE")]
        public string Date (EmptyRequest _)
        {
            var now = DateTime.UtcNow;
            return XmlConvert.ToString (now, XmlDateTimeSerializationMode.RoundtripKind);
        }

        public struct SignInRequest
        {
            public string ServerUrl;
            public string SignInAddress;
            public string Username;
            public string Password;
        }

        [JsonRpcMethod ("SIGNIN")]
        public bool SignIn (SignInRequest request)
        {
            LAssert.Arg (!string.IsNullOrEmpty (request.ServerUrl), "ServerUrl missing.");
            LAssert.Arg (!string.IsNullOrEmpty (request.Username), "Username missing.");
            LAssert.Arg (!string.IsNullOrEmpty (request.Password), "Password missing.");

            if (string.IsNullOrEmpty (request.SignInAddress))
                request.SignInAddress = request.Username;
            if (!_lync.CanSignIn)
                _lync.SignOut ().Wait ();
		
            _lync.SignIn (request.ServerUrl, request.SignInAddress, request.Username, request.Password).Wait ();
            return true;
        }

        [JsonRpcMethod ("SIGNOUT")]
        public bool SignOut (EmptyRequest _)
        {
            _lync.SignOut ().Wait ();
            return true;
        }

        [JsonRpcMethod ("AVAILABILITY")]
        public string GetAvailability (EmptyRequest _)
        {
            return _lync.Contacts.GetAvailability ().Result.ToString ();
        }

        public struct SetAvailabilityRequest
        {
            public string Availability;
        }

        [JsonRpcMethod ("SET_AVAILABILITY")]
        public bool SetAvailability (SetAvailabilityRequest request)
        {
            ContactAvailability availability;
            LAssert.Arg (Enum.TryParse (request.Availability, out availability), "invalid availability value: " + request.Availability);

            _lync.Contacts.SetAvailability (availability).Wait ();
            return true;
        }

        [JsonRpcMethod ("CONTACTS")]
        public IEnumerable<ContactInfo> GetContacts (EmptyRequest _)
        {
            return _lync.Contacts.GetContactList ().Result;
        }

        public struct ContactAvailabilityRequest
        {
            public string Uri;
        }

        [JsonRpcMethod ("CONTACT_AVAILABILITY")]
        public string GetContactAvailability (ContactAvailabilityRequest req)
        {
            return _lync.Contacts.GetContactAvailability (req.Uri).Result.ToString ();
        }

        public struct BeginConversationRequest
        {
            public string RecipientUri;
        }

        [JsonRpcMethod ("BEGIN_CONVERSATION")]
        public bool BeginConversation (BeginConversationRequest req)
        {
            _lync.Conversation.BeginConversation (req.RecipientUri).Wait ();
            return true;
        }

        public struct SendMessageRequest
        {
            public string Message;
        }

        [JsonRpcMethod ("SEND_MESSAGE")]
        public bool SendMessage (SendMessageRequest req)
        {
            _lync.Conversation.SendMessage (req.Message).Wait ();
            return true;
        }

        [JsonRpcMethod ("END_CONVERSATION")]
        public bool EndConversation (EmptyRequest _)
        {
            _lync.Conversation.EndConversation ().Wait ();
            return true;
        }
    }
}