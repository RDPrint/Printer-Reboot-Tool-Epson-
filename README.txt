======================================================================
  PRINTER REBOOT TOOL
======================================================================
Disclaimer & Limitation of Liability

Independent Development & Non-Affiliation
This software is developed independently and is not an official product of, sponsored by, endorsed by, or in any way affiliated with Seiko Epson Corporation, Epson America, Inc., Epson Europe B.V., or any of their parent companies, subsidiaries, or affiliates. All product names, trademarks, registered trademarks, service marks, and trade names mentioned or utilized herein (including "Epson") are the property of their respective owners and are referenced solely for descriptive and compatibility purposes under nominative fair use.

THE SOFTWARE IS PROVIDED "AS IS" AND "AS AVAILABLE", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE, TITLE, AND NON-INFRINGEMENT. THE ENTIRE RISK AS TO THE QUALITY, PERFORMANCE, AND OPERATION OF THE SOFTWARE RESTS WITH THE USER.

IN NO EVENT SHALL THE AUTHOR, COPYRIGHT HOLDERS, OR CONTRIBUTORS BE LIABLE FOR ANY CLAIM, DAMAGES, OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT (INCLUDING NEGLIGENCE), OR OTHERWISE, ARISING FROM, OUT OF, OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE. 
THIS INCLUDES, WITHOUT LIMITATION, ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES, SUCH AS:
- HARDWARE DAMAGE, BRICKING, FIRMWARE CORRUPTION, MECHANICAL FAILURES, OR PRINT HEAD DAMAGE;
- LOSS OF USE, DATA, REVENUE, PROFITS, OR BUSINESS INTERRUPTION;
- VOIDING OF MANUFACTURER WARRANTIES OR SERVICE CONTRACTS; OR
- COST OF SUBSTITUTE GOODS, HARDWARE REPLACEMENTS, OR REPAIR SERVICES.


BY DOWNLOADING, INSTALLING, OR USING THIS SOFTWARE, YOU ACKNOWLEDGE THAT YOU HAVE READ THIS DISCLAIMER, UNDERSTAND IT, AND AGREE TO USE THE SOFTWARE ENTIRELY AT YOUR OWN RISK.
[ENGLISH]

About
-----
This is a small, portable tool to remotely reboot network printers 
using SNMP. There is no installer or setup required—just run PrinterRebootTool.exe.

Features
--------
- Reboot specific printers (enter up to 5 individual IP addresses).
- Standard IP Range: Scan between a Start IP and End IP (e.g. 192.168.1.50 
  to 192.168.1.80).
- Subnet / VLAN Range: Enter an IP address range along with a subnet mask 
  (e.g. 192.168.0.1 to 192.168.0.128 with subnet mask 255.255.255.0). 
  Designed for networks with multiple VLANs or custom subnets—it scans 
  across the range while automatically skipping network IDs (e.g. .0) 
  and broadcast addresses (e.g. .255) for every subnet. You can enter 
  the details into the boxes or paste the full string directly.
- Real-Time Progress: Displays the exact IP address currently being scanned 
  and an embedded status bar progress indicator.
- Safe Discovery: In range and subnet modes, the tool verifies that a device 
  is actually a supported printer before sending any reboot command.
- Language Switcher: Toggle between English and German (English / Deutsch) 
  at the top of the window.
- Classic Windows Design: Clean, utilitarian Windows administration style 
  using standard system dialog controls, tabs, group boxes, and status bar.

IMPORTANT: Printer Setup Requirement
------------------------------------
Most modern network printers (including Epson) come from the factory 
with SNMP write access disabled!

Even though "Community Name (Read Only)" is set to "public" by default, 
the "Community Name (Read/Write)" field is usually left completely blank. 
If this field is empty on the printer, it will reject reboot commands.

To fix this:
1. Open the printer's web configuration page in your browser (its IP).
2. Go to Network Settings -> SNMP (or SNMPv1/v2c Settings).
3. Look for "Community Name (Read/Write)" and enter your chosen community 
   name (e.g. public).
4. Save the settings.

Settings & SNMP Versions
------------------------
Click the "SNMP Settings..." button to configure SNMP:
- SNMPv1: Standard mode, compatible with most printers.
- SNMPv2c: Recommended if your printers have SNMPv1 disabled in firmware.
- SNMPv3: For secured environments. Supports user authentication 
  (MD5 / SHA-1) and privacy encryption (DES / AES-128) matching the 
  options in the printer's web configuration.
- Community String (Read/Write): Defaults to blank. Enter the community 
  name that has been configured on your printer.



======================================================================

[DEUTSCH]

Über dieses Tool
----------------
Ein kleines, portables Werkzeug zum ferngesteuerten Neustarten von 
Netzwerkdruckern über SNMP. Keine Installation erforderlich – 
einfach PrinterRebootTool.exe starten.

Funktionen
----------
- Einzelne Drucker neu starten (bis zu 5 IP-Adressen eingeben).
- Standard IP-Bereich: Scannt zwischen Start-IP und End-IP 
  (z.B. 192.168.1.50 bis 192.168.1.80).
- Subnetz / VLAN-Bereich: Ermöglicht die Eingabe eines IP-Bereichs 
  zusammen mit einer Subnetzmaske (z.B. 192.168.0.1 bis 192.168.0.128 
  mit Subnetzmaske 255.255.255.0). Ideal für Netzwerke mit mehreren VLANs 
  oder Subnetzen – die Netzwerk-IDs (z.B. .0) und Broadcast-Adressen 
  (z.B. .255) werden für jedes Subnetz automatisch übersprungen. Die Werte 
  können in die Einzelfelder eingetragen oder als Gesamtstring eingefügt werden.
- Live-Fortschrittsanzeige: Zeigt in Echtzeit die aktuell gescannte 
  IP-Adresse und den Fortschrittsbalken in der Statusleiste an.
- Sichere Erkennung: Im Bereichs- und Subnetzmodus prüft das Tool 
  automatisch, ob es sich wirklich um einen unterstützten Drucker handelt, 
  bevor der Neustart-Befehl gesendet wird.
- Sprachumschaltung: Jederzeit zwischen Englisch und Deutsch (English / Deutsch) 
  oben im Fenster umschaltbar.
- Klassisches Windows-Design: Übersichtlicher, zweckmäßiger Windows-Admin-Stil 
  mit Standard-Steuerelementen, Reitern, Gruppenrahmen und Statusleiste.

WICHTIG: Vorbereitung am Drucker
--------------------------------
Bei vielen modernen Netzwerkdruckern (wie z.B. von Epson) ist der Schreibzugriff per SNMP 
ab Werk deaktiviert!

Während der Nur-Lese-Name ("Read Only") meist standardmäßig auf "public" 
steht, ist das Feld "Community Name (Read/Write)" standardmäßig komplett 
leer. Ist dieses Feld leer, verweigert der Drucker den Neustart.

So richtet man es ein:
1. Die Web-Konfiguration des Druckers im Browser aufrufen (über die IP).
2. Zu Netzwerkeinstellungen -> SNMP (bzw. SNMPv1/v2c-Einstellungen) gehen.
3. Den Eintrag "Community Name (Read/Write)" suchen und einen Namen 
   vergeben (z.B. public).
4. Einstellungen speichern.

Einstellungen & SNMP-Versionen
-------------------------------
Über die Schaltfläche "SNMP-Einstellungen..." lassen sich die Optionen anpassen:
- SNMPv1: Standardmodus, funktioniert mit den meisten Geräten.
- SNMPv2c: Empfohlen, falls SNMPv1 in der Firmware deaktiviert wurde.
- SNMPv3: Für abgesicherte Netzwerke. Unterstützt Authentifizierung 
  (MD5 / SHA-1) und Verschlüsselung (DES / AES-128) passend zur 
  Web-Konfiguration des Druckers.
- Community-String (Read/Write): Standardmäßig leer. Tragen Sie hier den 
  Namen ein, der am Drucker hinterlegt wurde.


