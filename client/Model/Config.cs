using System;
using System.Collections;
using System.Collections.Generic;
using Windows.Data.Json;

namespace MPVPN
{
    public class Config
    {
        public struct SecurityAssociationParameters
        {
            public string EncryptionAlgorithm;
            public string integrityAlgorithm;
            public int diffieHellmanGroup;
            public int lifetimeMinutes;
        }
        SecurityAssociationParameters childSecurityAssociationParameters;
        SecurityAssociationParameters ikeSecurityAssociationParameters;

        public struct Server
        {
            public string country;
            public string serverAddress;
            public string remoteIdentifier;
            public string eap_name;
            public string eap_secret;
        }
        public List<Server> servers;

        public string AuthenticationMethod;
        public string deadPeerDetectionRate;
        public bool disableMOBIKE;
        public bool disableRedirect;
        public bool enableRevocationCheck;
        public bool enablePFS;

        public Config(string jsonString)
        {
            JsonObject jsonObject = JsonObject.Parse(jsonString);
            AuthenticationMethod = jsonObject.GetNamedString("AuthenticationMethod", "");
            deadPeerDetectionRate = jsonObject.GetNamedString("deadPeerDetectionRate", "");;
            disableMOBIKE = jsonObject.GetNamedBoolean("disableMOBIKE");
            disableRedirect = jsonObject.GetNamedBoolean("disableRedirect");
            enableRevocationCheck = jsonObject.GetNamedBoolean("enableRevocationCheck");
            enablePFS = jsonObject.GetNamedBoolean("enablePFS");

            childSecurityAssociationParameters = ParseSecurityAssociationParameters(jsonObject.GetNamedObject("ChildSecurityAssociationParameters"));
            ikeSecurityAssociationParameters = ParseSecurityAssociationParameters(jsonObject.GetNamedObject("ikeSecurityAssociationParameters"));

            var serversjson = jsonObject.GetNamedArray("servers");

            servers = new List<Server>();

            foreach (JsonValue serverJson in serversjson)
            {
                Server server = ParseServer(serverJson.GetObject());
                servers.Add(server);
            }
        }

        private SecurityAssociationParameters ParseSecurityAssociationParameters(JsonObject json)
        {
            SecurityAssociationParameters parameters;

            parameters.EncryptionAlgorithm = json.GetNamedString("EncryptionAlgorithm");
            parameters.integrityAlgorithm = json.GetNamedString("integrityAlgorithm");
            parameters.diffieHellmanGroup = (int)json.GetNamedNumber("diffieHellmanGroup");
            parameters.lifetimeMinutes = (int)json.GetNamedNumber("lifetimeMinutes");

            return parameters;
        }

        private Server ParseServer(JsonObject json)
        {
            Server server;
            server.country = json.GetNamedString("country");
            server.serverAddress = json.GetNamedString("serverAddress");
            server.remoteIdentifier = json.GetNamedString("remoteIdentifier");
            server.eap_name = json.GetNamedString("eap-name");
            server.eap_secret = json.GetNamedString("eap-secret");

            return server;
        }
    }
}