using System;
using System.Threading.Tasks;
using System.Xml;
using AustinHarris.JsonRpc;
using Microsoft.Lync.Model;
using System.Collections;
using System.Collections.Generic;

namespace LyncRPC
{
    public class RPCService: JsonRpcService
    {
        private LyncController _lync;

        public RPCService (LyncController controller) : base ()
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
        public SuccessResult SignIn (SignInRequest request)
        {
            LAssert.Arg (!string.IsNullOrEmpty (request.ServerUrl), "ServerUrl missing.");
            LAssert.Arg (!string.IsNullOrEmpty (request.Username), "Username missing.");
            LAssert.Arg (!string.IsNullOrEmpty (request.Password), "Password missing.");

            if (string.IsNullOrEmpty (request.SignInAddress))
                request.SignInAddress = request.Username;
            if (!_lync.CanSignIn)
                _lync.SignOut ().Wait ();
		
            _lync.SignIn (request.ServerUrl, request.SignInAddress, request.Username, request.Password).Wait ();
            return new SuccessResult ();
        }

        [JsonRpcMethod ("SIGNOUT")]
        public SuccessResult SignOut (EmptyRequest _)
        {
            _lync.SignOut ().Wait ();
            return new SuccessResult ();
        }

        [JsonRpcMethod ("AVAILABILITY")]
        public string GetAvailability (EmptyRequest _)
        {
            return _lync.GetAvailability ().Result.ToString ();
        }

        public struct SetAvailabilityRequest
        {
            public string Availability;
        }

        [JsonRpcMethod ("SET_AVAILABILITY")]
        public SuccessResult SetAvailability (SetAvailabilityRequest request)
        {
            ContactAvailability availability;
            LAssert.Arg (Enum.TryParse (request.Availability, out availability), "invalid availability value: " + request.Availability);

            _lync.SetAvailability (availability).Wait ();
            return new SuccessResult ();
        }

        [JsonRpcMethod ("CONTACTS")]
        public IEnumerable<ContactInfo> GetContacts (EmptyRequest _)
        {
            return _lync.Contacts.GetContactList ().Result;
        }
    }
}