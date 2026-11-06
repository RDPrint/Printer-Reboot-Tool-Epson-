using System;
using System.Collections.Generic;

namespace EpsonMassReboot
{
    public enum AppLanguage
    {
        English,
        German
    }

    public static class Localization
    {
        private static AppLanguage _currentLanguage = AppLanguage.English;
        public static AppLanguage CurrentLanguage
        {
            get { return _currentLanguage; }
            set { _currentLanguage = value; }
        }

        private static readonly Dictionary<string, string> En = new Dictionary<string, string>
        {
            { "AppTitle", "Printer reboot tool" },
            { "AppSubtitle", "Network Printer Management Utility" },
            { "BtnSettings", "⚙ SNMP Settings..." },
            { "LangLabel", "Language:" },
            { "ThemeLight", "Light Mode" },
            { "ThemeDark", "Dark Mode" },
            
            // Tabs & Groups
            { "TabSpecific", "Specific IP Addresses" },
            { "TabRange", "IP Range / Subnet" },
            { "GrpTargets", "Target Printers" },
            { "GrpScanMode", "Scan Mode" },
            { "GrpRangeParams", "Range Parameters" },
            { "IpLabel", "IP Address {0}:" },
            
            // Range & Subnet
            { "ModeRange", "Standard IP Range (Start - End)" },
            { "ModeSubnet", "Subnet / VLAN Range (with Mask)" },
            { "StartIp", "Start IP:" },
            { "EndIp", "End IP:" },
            { "SubnetMask", "Subnet Mask:" },
            { "SubnetHint", "Format: 192.168.0.1 - 192.168.0.128 with mask 255.255.255.0 (skips .0 and .255 across VLANs)" },
            { "RangeNote", "Note: In range/subnet mode, devices are automatically verified as Epson printers before reboot commands are sent." },
            
            // Settings Modal
            { "SettingsTitle", "SNMP Configuration" },
            { "GrpSnmpVersion", "SNMP Protocol Version" },
            { "GrpCommunity", "Community Name (Read/Write)" },
            { "CommunityString", "Community String (Read/Write) - MUST BE SET ON DEVICE:" },
            { "CommunityDeviceNotice", "Modern Epson printers leave Read/Write blank by default. You must configure this in the printer Web Config." },
            { "GrpV3", "SNMPv3 Security Settings" },
            { "V3User", "User Name:" },
            { "GrpV3Auth", "Authentication Settings" },
            { "V3Algo", "Algorithm:" },
            { "V3Password", "Password:" },
            { "GrpV3Priv", "Encryption Settings" },
            { "V3ContextName", "Context Name:" },
            { "BtnOk", "OK" },
            { "BtnCancel", "Cancel" },

            // Main Actions & Status
            { "BtnReboot", "Reboot Printers" },
            { "BtnRunning", "Running..." },
            { "StatusReady", "Ready" },
            { "StatusScanning", "Scanning {0}... ({1}/{2})" },
            { "StatusRebootSentTo", "Reboot command sent to {0}..." },
            { "StatusNoIps", "Please enter at least one valid IP address or range." },
            { "StatusInvalidRange", "Invalid IP range or subnet mask." },
            { "StatusCompleted", "Completed: Reboot command sent to {0} printer(s)." },
            { "StatusCompletedDetails", "Completed: Reboot sent to {0} printer(s) [{1}]." },
            { "StatusNoneFound", "Completed: No responding Epson printers found." }
        };

        private static readonly Dictionary<string, string> De = new Dictionary<string, string>
        {
            { "AppTitle", "Printer reboot tool" },
            { "AppSubtitle", "Netzwerkdrucker-Verwaltungsprogramm" },
            { "BtnSettings", "⚙ SNMP-Einstellungen..." },
            { "LangLabel", "Sprache:" },
            { "ThemeLight", "Heller Modus" },
            { "ThemeDark", "Dunkler Modus" },
            
            // Tabs & Groups
            { "TabSpecific", "Einzelne IP-Adressen" },
            { "TabRange", "IP-Bereich / Subnetz" },
            { "GrpTargets", "Ziel-Drucker" },
            { "GrpScanMode", "Scan-Modus" },
            { "GrpRangeParams", "Bereichs-Parameter" },
            { "IpLabel", "IP-Adresse {0}:" },
            
            // Range & Subnet
            { "ModeRange", "Standard IP-Bereich (Start - Ende)" },
            { "ModeSubnet", "Subnetz / VLAN-Bereich (mit Maske)" },
            { "StartIp", "Start-IP:" },
            { "EndIp", "End-IP:" },
            { "SubnetMask", "Subnetzmaske:" },
            { "SubnetHint", "Format: 192.168.0.1 - 192.168.0.128 mit Maske 255.255.255.0 (überspringt .0 und .255 über VLANs)" },
            { "RangeNote", "Hinweis: Im Bereichs-/Subnetzmodus werden Geräte vor dem Senden von Neustart-Befehlen automatisch als Epson-Drucker verifiziert." },
            
            // Settings Modal
            { "SettingsTitle", "SNMP-Konfiguration" },
            { "GrpSnmpVersion", "SNMP-Protokollversion" },
            { "GrpCommunity", "Community-Name (Read/Write)" },
            { "CommunityString", "Community-Zeichenfolge (Read/Write) - MUSS AM GERÄT GESETZT SEIN:" },
            { "CommunityDeviceNotice", "Bei modernen Epson-Druckern ist Read/Write standardmäßig leer. Dies muss im Drucker-Web-Config gesetzt werden." },
            { "GrpV3", "SNMPv3-Sicherheitseinstellungen" },
            { "V3User", "Benutzername:" },
            { "GrpV3Auth", "Authentifizierungseinstellungen" },
            { "V3Algo", "Algorithmus:" },
            { "V3Password", "Passwort:" },
            { "GrpV3Priv", "Verschlüsselungseinstellungen" },
            { "V3ContextName", "Kontextname:" },
            { "BtnOk", "OK" },
            { "BtnCancel", "Abbrechen" },

            // Main Actions & Status
            { "BtnReboot", "Drucker neu starten" },
            { "BtnRunning", "Wird ausgeführt..." },
            { "StatusReady", "Bereit" },
            { "StatusScanning", "Scanne {0}... ({1}/{2})" },
            { "StatusRebootSentTo", "Neustart-Befehl an {0} gesendet..." },
            { "StatusNoIps", "Bitte geben Sie mindestens eine gültige IP-Adresse oder einen Bereich ein." },
            { "StatusInvalidRange", "Ungültiger IP-Bereich oder ungültige Subnetzmaske." },
            { "StatusCompleted", "Abgeschlossen: Neustart-Befehl an {0} Drucker gesendet." },
            { "StatusCompletedDetails", "Abgeschlossen: Neustart an {0} Drucker gesendet [{1}]." },
            { "StatusNoneFound", "Abgeschlossen: Keine antwortenden Epson-Drucker gefunden." }
        };

        public static string Get(string key, params object[] args)
        {
            var dict = (CurrentLanguage == AppLanguage.German) ? De : En;
            string val;
            if (!dict.TryGetValue(key, out val))
            {
                if (!En.TryGetValue(key, out val))
                    val = key;
            }

            if (args != null && args.Length > 0)
            {
                try { return string.Format(val, args); }
                catch { return val; }
            }
            return val;
        }
    }
}
