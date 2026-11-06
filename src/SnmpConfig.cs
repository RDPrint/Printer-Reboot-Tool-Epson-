using System;

namespace EpsonMassReboot
{
    public enum SnmpVersion
    {
        V1,
        V2c,
        V3
    }

    public class SnmpConfig
    {
        private static SnmpConfig _current;
        public static SnmpConfig Current
        {
            get
            {
                if (_current == null) _current = new SnmpConfig();
                return _current;
            }
        }

        public SnmpVersion Version { get; set; }
        public string Community { get; set; }

        // SNMPv3 options matching Epson Web Config
        public string V3UserName { get; set; }
        public string V3AuthAlgorithm { get; set; } // "MD5" or "SHA1"
        public string V3AuthPassword { get; set; }
        public string V3PrivAlgorithm { get; set; } // "DES" or "AES128"
        public string V3PrivPassword { get; set; }
        public string V3ContextName { get; set; }

        public SnmpConfig()
        {
            Version = SnmpVersion.V1;
            Community = "";
            V3UserName = "admin";
            V3AuthAlgorithm = "MD5";
            V3AuthPassword = "";
            V3PrivAlgorithm = "DES";
            V3PrivPassword = "";
            V3ContextName = "EPSON";
        }
    }
}
