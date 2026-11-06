using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EpsonMassReboot
{
    public class SnmpTargetResult
    {
        public IPAddress IP { get; set; }
        public bool IsEpson { get; set; }
        public string Model { get; set; }
        public bool RebootSent { get; set; }
        public string MatchedMethod { get; set; }
        public string Error { get; set; }
    }

    public static class SnmpEngine
    {
        // Printer-MIB reset OID
        public const string ResetOid = "1.3.6.1.2.1.43.5.1.1.3.1";
        public const int PowerCycleResetValue = 4; // powerCycleReset(4)

        // Identification OIDs
        public const string SysDescrOid = "1.3.6.1.2.1.1.1.0";               // MIB-2 System Description
        public const string SysObjectIdOid = "1.3.6.1.2.1.1.2.0";            // MIB-2 System Object ID (Enterprise ID)
        public const string HrDeviceDescrOid = "1.3.6.1.2.1.25.3.2.1.3.1";   // Host Resources MIB Device Description
        public const string PrtPrinterNameOid = "1.3.6.1.2.1.43.5.1.1.16.1"; // Printer-MIB General Printer Name
        public const string EpsonEnterpriseOid = "1.3.6.1.4.1.1248.1.2.2.1.1.1.1.1"; // Seiko Epson Private MIB

        private const int SIO_UDP_CONNRESET = -1744830452;
        private static int _nextRequestId = 1000;

        public static int GetNextRequestId()
        {
            return Interlocked.Increment(ref _nextRequestId);
        }

        #region BER / ASN.1 Encoding

        public static byte[] EncodeLength(int length)
        {
            if (length < 128)
            {
                return new byte[] { (byte)length };
            }

            var bytes = new List<byte>();
            int temp = length;
            while (temp > 0)
            {
                bytes.Insert(0, (byte)(temp & 0xFF));
                temp >>= 8;
            }

            var result = new List<byte>();
            result.Add((byte)(0x80 | bytes.Count));
            result.AddRange(bytes);
            return result.ToArray();
        }

        public static byte[] EncodeInteger(long val)
        {
            var list = new List<byte>();
            long temp = val;
            do
            {
                list.Insert(0, (byte)(temp & 0xFF));
                temp >>= 8;
            } while ((temp != 0 && temp != -1) ||
                     (val > 0 && (list[0] & 0x80) != 0) ||
                     (val < 0 && (list[0] & 0x80) == 0));

            var result = new List<byte>();
            result.Add(0x02); // Tag: INTEGER
            result.AddRange(EncodeLength(list.Count));
            result.AddRange(list);
            return result.ToArray();
        }

        public static byte[] EncodeOctetString(byte[] data)
        {
            var result = new List<byte>();
            result.Add(0x04); // Tag: OCTET STRING
            result.AddRange(EncodeLength(data != null ? data.Length : 0));
            if (data != null && data.Length > 0) result.AddRange(data);
            return result.ToArray();
        }

        public static byte[] EncodeOctetString(string text)
        {
            byte[] data = Encoding.ASCII.GetBytes(text ?? string.Empty);
            return EncodeOctetString(data);
        }

        public static byte[] EncodeNull()
        {
            return new byte[] { 0x05, 0x00 };
        }

        public static byte[] EncodeOid(string oidStr)
        {
            if (string.IsNullOrEmpty(oidStr))
                return new byte[] { 0x06, 0x00 };

            string[] tokens = oidStr.TrimStart('.').Split('.');
            if (tokens.Length < 2)
                return new byte[] { 0x06, 0x00 };

            int first = int.Parse(tokens[0]);
            int second = int.Parse(tokens[1]);

            var body = new List<byte>();
            body.Add((byte)(first * 40 + second));

            for (int i = 2; i < tokens.Length; i++)
            {
                uint val = uint.Parse(tokens[i]);
                if (val < 128)
                {
                    body.Add((byte)val);
                }
                else
                {
                    var sub = new List<byte>();
                    sub.Add((byte)(val & 0x7F));
                    val >>= 7;
                    while (val > 0)
                    {
                        sub.Add((byte)((val & 0x7F) | 0x80));
                        val >>= 7;
                    }
                    sub.Reverse();
                    body.AddRange(sub);
                }
            }

            var result = new List<byte>();
            result.Add(0x06); // Tag: OBJECT IDENTIFIER
            result.AddRange(EncodeLength(body.Count));
            result.AddRange(body);
            return result.ToArray();
        }

        public static byte[] WrapTag(byte tag, byte[] content)
        {
            var result = new List<byte>();
            result.Add(tag);
            result.AddRange(EncodeLength(content != null ? content.Length : 0));
            if (content != null && content.Length > 0) result.AddRange(content);
            return result.ToArray();
        }

        public static byte[] WrapSequence(byte[] content)
        {
            return WrapTag(0x30, content);
        }

        #endregion

        #region SNMPv1 / SNMPv2c Packet Builders

        public static byte[] BuildV1OrV2Packet(SnmpVersion version, string community, byte pduType, int requestId, string oid, byte[] valEncoded)
        {
            var varBindBytes = new List<byte>();
            varBindBytes.AddRange(EncodeOid(oid));
            varBindBytes.AddRange(valEncoded ?? EncodeNull());
            byte[] varBind = WrapSequence(varBindBytes.ToArray());
            byte[] varBindList = WrapSequence(varBind);

            var pduBytes = new List<byte>();
            pduBytes.AddRange(EncodeInteger(requestId));
            pduBytes.AddRange(EncodeInteger(0)); // error-status
            pduBytes.AddRange(EncodeInteger(0)); // error-index
            pduBytes.AddRange(varBindList);
            byte[] pdu = WrapTag(pduType, pduBytes.ToArray());

            int verInt = (version == SnmpVersion.V2c) ? 1 : 0;
            var msgBytes = new List<byte>();
            msgBytes.AddRange(EncodeInteger(verInt));
            msgBytes.AddRange(EncodeOctetString(community ?? "public"));
            msgBytes.AddRange(pdu);

            return WrapSequence(msgBytes.ToArray());
        }

        #endregion

        #region SNMPv3 Packet Builders

        public static byte[] PasswordToKey(string password, string algo, byte[] engineId)
        {
            if (string.IsNullOrEmpty(password)) return new byte[0];

            byte[] passBytes = Encoding.ASCII.GetBytes(password);
            byte[] passwordBuf = new byte[1048576]; // 1MB buffer as per RFC 3414
            int passIndex = 0;
            for (int i = 0; i < passwordBuf.Length; i++)
            {
                passwordBuf[i] = passBytes[passIndex++];
                if (passIndex >= passBytes.Length) passIndex = 0;
            }

            byte[] key;
            if (string.Equals(algo, "SHA1", StringComparison.OrdinalIgnoreCase))
            {
                using (var sha = SHA1.Create())
                {
                    key = sha.ComputeHash(passwordBuf);
                }
                if (engineId != null && engineId.Length > 0)
                {
                    using (var sha = SHA1.Create())
                    {
                        var buf = new List<byte>();
                        buf.AddRange(key);
                        buf.AddRange(engineId);
                        buf.AddRange(key);
                        key = sha.ComputeHash(buf.ToArray());
                    }
                }
            }
            else
            {
                using (var md5 = MD5.Create())
                {
                    key = md5.ComputeHash(passwordBuf);
                }
                if (engineId != null && engineId.Length > 0)
                {
                    using (var md5 = MD5.Create())
                    {
                        var buf = new List<byte>();
                        buf.AddRange(key);
                        buf.AddRange(engineId);
                        buf.AddRange(key);
                        key = md5.ComputeHash(buf.ToArray());
                    }
                }
            }
            return key;
        }

        public static byte[] BuildV3Packet(SnmpConfig cfg, byte pduType, int requestId, string oid, byte[] valEncoded, byte[] engineId, int boots, int time)
        {
            // VarBind
            var varBindBytes = new List<byte>();
            varBindBytes.AddRange(EncodeOid(oid));
            varBindBytes.AddRange(valEncoded ?? EncodeNull());
            byte[] varBind = WrapSequence(varBindBytes.ToArray());
            byte[] varBindList = WrapSequence(varBind);

            // PDU
            var pduBytes = new List<byte>();
            pduBytes.AddRange(EncodeInteger(requestId));
            pduBytes.AddRange(EncodeInteger(0));
            pduBytes.AddRange(EncodeInteger(0));
            pduBytes.AddRange(varBindList);
            byte[] pdu = WrapTag(pduType, pduBytes.ToArray());

            // ScopedPDU: SEQUENCE { contextEngineID, contextName, data }
            var scopedBytes = new List<byte>();
            scopedBytes.AddRange(EncodeOctetString(engineId ?? new byte[0]));
            scopedBytes.AddRange(EncodeOctetString(cfg.V3ContextName ?? "EPSON"));
            scopedBytes.AddRange(pdu);
            byte[] scopedPdu = WrapSequence(scopedBytes.ToArray());

            bool hasAuth = !string.IsNullOrEmpty(cfg.V3AuthPassword);
            bool hasPriv = hasAuth && !string.IsNullOrEmpty(cfg.V3PrivPassword);

            byte msgFlags = 0x04; // reportable
            if (hasAuth && hasPriv) msgFlags |= 0x03;
            else if (hasAuth) msgFlags |= 0x01;

            // msgGlobalData: SEQUENCE { msgID, msgMaxSize, msgFlags, msgSecurityModel }
            var globalBytes = new List<byte>();
            globalBytes.AddRange(EncodeInteger(requestId));
            globalBytes.AddRange(EncodeInteger(65507));
            globalBytes.AddRange(EncodeOctetString(new byte[] { msgFlags }));
            globalBytes.AddRange(EncodeInteger(3)); // USM
            byte[] msgGlobalData = WrapSequence(globalBytes.ToArray());

            // USM Security Parameters
            byte[] authParams = hasAuth ? new byte[12] : new byte[0];
            byte[] privParams = new byte[0];

            var usmBytes = new List<byte>();
            usmBytes.AddRange(EncodeOctetString(engineId ?? new byte[0]));
            usmBytes.AddRange(EncodeInteger(boots));
            usmBytes.AddRange(EncodeInteger(time));
            usmBytes.AddRange(EncodeOctetString(cfg.V3UserName ?? "admin"));
            usmBytes.AddRange(EncodeOctetString(authParams));
            usmBytes.AddRange(EncodeOctetString(privParams));
            byte[] usmHeader = EncodeOctetString(WrapSequence(usmBytes.ToArray()));

            // Assemble Message
            var msgBytes = new List<byte>();
            msgBytes.AddRange(EncodeInteger(3)); // version 3
            msgBytes.AddRange(msgGlobalData);
            msgBytes.AddRange(usmHeader);
            msgBytes.AddRange(scopedPdu);
            byte[] wholeMsg = WrapSequence(msgBytes.ToArray());

            // Compute HMAC if auth is enabled
            if (hasAuth)
            {
                byte[] key = PasswordToKey(cfg.V3AuthPassword, cfg.V3AuthAlgorithm, engineId);
                byte[] hmac;
                if (string.Equals(cfg.V3AuthAlgorithm, "SHA1", StringComparison.OrdinalIgnoreCase))
                {
                    using (var h = new HMACSHA1(key)) hmac = h.ComputeHash(wholeMsg);
                }
                else
                {
                    using (var h = new HMACMD5(key)) hmac = h.ComputeHash(wholeMsg);
                }

                // Place first 12 bytes into wholeMsg
                // Locate authParams offset
                int usmOffset = 0;
                for (int i = 0; i < wholeMsg.Length - 12; i++)
                {
                    bool match = true;
                    for (int j = 0; j < 12; j++)
                    {
                        if (wholeMsg[i + j] != 0x00) { match = false; break; }
                    }
                    if (match)
                    {
                        usmOffset = i;
                        break;
                    }
                }
                if (usmOffset > 0)
                {
                    Array.Copy(hmac, 0, wholeMsg, usmOffset, 12);
                }
            }

            return wholeMsg;
        }

        #endregion

        #region String & OID Extraction

        public static string ExtractStringOrOid(byte[] responseBytes)
        {
            if (responseBytes == null || responseBytes.Length < 10) return string.Empty;
            try { return Encoding.ASCII.GetString(responseBytes); }
            catch { return string.Empty; }
        }

        public static bool IsEpsonResponse(string text, byte[] rawBytes)
        {
            if (string.IsNullOrEmpty(text) && (rawBytes == null || rawBytes.Length == 0))
                return false;

            string lower = (text ?? "").ToLowerInvariant();

            if (lower.Contains("epson") ||
                lower.Contains("seiko") ||
                lower.Contains("workforce") ||
                lower.Contains("ecotank") ||
                lower.Contains("stylus") ||
                lower.Contains("surecolor") ||
                lower.Contains("expression") ||
                lower.Contains("aculaser") ||
                lower.Contains("epsonnet") ||
                lower.Contains("esc/p"))
            {
                return true;
            }

            if (lower.Contains("1.3.6.1.4.1.1248") || lower.Contains("1.3.6.1.4.1.268"))
            {
                return true;
            }

            if (rawBytes != null && rawBytes.Length > 4)
            {
                for (int i = 0; i < rawBytes.Length - 1; i++)
                {
                    if (rawBytes[i] == 0x89 && rawBytes[i + 1] == 0x60) return true;
                    if (rawBytes[i] == 0x82 && rawBytes[i + 1] == 0x0C) return true;
                }
            }

            return false;
        }

        public static string CleanModelString(string raw)
        {
            if (string.IsNullOrEmpty(raw)) return "Epson Printer";

            int idx = raw.IndexOf("EPSON", StringComparison.OrdinalIgnoreCase);
            if (idx >= 0)
            {
                int end = raw.IndexOfAny(new char[] { '\r', '\n', ';', '\0' }, idx);
                if (end > idx)
                    return raw.Substring(idx, end - idx).Trim();
                else if (raw.Length - idx < 50)
                    return raw.Substring(idx).Trim();
                else
                    return raw.Substring(idx, 50).Trim();
            }

            return "Epson Printer";
        }

        #endregion

        #region Network Operations

        public static async Task<byte[]> SendAndReceiveUdpAsync(IPAddress ip, byte[] request, int timeoutMs = 1500)
        {
            return await Task.Run(() =>
            {
                using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
                {
                    try
                    {
                        socket.IOControl((IOControlCode)SIO_UDP_CONNRESET, new byte[] { 0, 0, 0, 0 }, null);
                    }
                    catch { }

                    socket.ReceiveTimeout = timeoutMs;
                    socket.SendTimeout = timeoutMs;

                    var ep = new IPEndPoint(ip, 161) as EndPoint;
                    byte[] buffer = new byte[2048];

                    try
                    {
                        socket.SendTo(request, ep);
                        int received = socket.ReceiveFrom(buffer, ref ep);
                        if (received > 0)
                        {
                            byte[] result = new byte[received];
                            Array.Copy(buffer, 0, result, 0, received);
                            return result;
                        }
                    }
                    catch
                    {
                        // Timeout or unreachable
                    }
                }
                return null;
            });
        }

        public static async Task<string> QuerySnmpTextAsync(IPAddress ip, SnmpVersion version, string community, string oid, int timeoutMs = 1500)
        {
            byte[] req = BuildV1OrV2Packet(version, community, 0xA0, GetNextRequestId(), oid, EncodeNull());

            byte[] resp = await SendAndReceiveUdpAsync(ip, req, timeoutMs);
            if (resp != null) return ExtractStringOrOid(resp);

            await Task.Delay(80);
            resp = await SendAndReceiveUdpAsync(ip, req, timeoutMs);
            if (resp != null) return ExtractStringOrOid(resp);

            return null;
        }

        public static async Task<bool> SendRebootCommandAsync(IPAddress ip, SnmpConfig cfg)
        {
            return await Task.Run(() =>
            {
                using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
                {
                    try
                    {
                        socket.IOControl((IOControlCode)SIO_UDP_CONNRESET, new byte[] { 0, 0, 0, 0 }, null);
                    }
                    catch { }

                    socket.SendTimeout = 1000;
                    var ep = new IPEndPoint(ip, 161);

                    try
                    {
                        if (cfg.Version == SnmpVersion.V3)
                        {
                            // Discover engine ID
                            byte[] v3Disc = BuildV3Packet(cfg, 0xA0, GetNextRequestId(), ResetOid, null, new byte[0], 0, 0);
                            byte[] discResp = SendAndReceiveUdpAsync(ip, v3Disc, 1200).Result;
                            byte[] engineId = new byte[0];
                            int boots = 0, time = 0;

                            if (discResp != null && discResp.Length > 20)
                            {
                                // Extract engine ID from report
                                for (int i = 0; i < discResp.Length - 4; i++)
                                {
                                    if (discResp[i] == 0x04 && discResp[i + 1] > 4 && discResp[i + 1] < 32)
                                    {
                                        int len = discResp[i + 1];
                                        engineId = new byte[len];
                                        Array.Copy(discResp, i + 2, engineId, 0, len);
                                        break;
                                    }
                                }
                            }

                            byte[] v3Set = BuildV3Packet(cfg, 0xA3, GetNextRequestId(), ResetOid, EncodeInteger(PowerCycleResetValue), engineId, boots, time);
                            socket.SendTo(v3Set, ep);
                            Thread.Sleep(40);
                            socket.SendTo(v3Set, ep);
                        }
                        else
                        {
                            // SNMPv1 or SNMPv2c
                            string comm = cfg.Community ?? "";
                            byte[] setReq = BuildV1OrV2Packet(cfg.Version, comm, 0xA3, GetNextRequestId(), ResetOid, EncodeInteger(PowerCycleResetValue));

                            // Send primary request twice
                            socket.SendTo(setReq, ep);
                            Thread.Sleep(40);
                            socket.SendTo(setReq, ep);

                            // If community is not 'public' and not empty, also send with 'public'
                            if (!string.IsNullOrEmpty(comm) && !string.Equals(comm, "public", StringComparison.OrdinalIgnoreCase))
                            {
                                byte[] setPublic = BuildV1OrV2Packet(cfg.Version, "public", 0xA3, GetNextRequestId(), ResetOid, EncodeInteger(PowerCycleResetValue));
                                Thread.Sleep(30);
                                socket.SendTo(setPublic, ep);
                            }

                            // If version is V1, also dispatch V2c
                            if (cfg.Version == SnmpVersion.V1)
                            {
                                byte[] setV2c = BuildV1OrV2Packet(SnmpVersion.V2c, comm, 0xA3, GetNextRequestId(), ResetOid, EncodeInteger(PowerCycleResetValue));
                                Thread.Sleep(30);
                                socket.SendTo(setV2c, ep);
                            }
                        }

                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                }
            });
        }

        public static async Task<SnmpTargetResult> ProbeAndRebootAsync(IPAddress ip, SnmpConfig cfg, bool verifyEpson)
        {
            var res = new SnmpTargetResult { IP = ip };

            var communitiesToTry = new List<string>();
            string primaryComm = cfg.Community ?? "";
            if (!string.IsNullOrEmpty(primaryComm)) communitiesToTry.Add(primaryComm);
            if (!communitiesToTry.Contains("public")) communitiesToTry.Add("public");
            if (!communitiesToTry.Contains("epson")) communitiesToTry.Add("epson");
            if (communitiesToTry.Count == 0) communitiesToTry.Add("");

            var versionsToTry = new List<SnmpVersion>();
            versionsToTry.Add(cfg.Version);
            if (cfg.Version == SnmpVersion.V1 && !versionsToTry.Contains(SnmpVersion.V2c)) versionsToTry.Add(SnmpVersion.V2c);
            if (cfg.Version == SnmpVersion.V2c && !versionsToTry.Contains(SnmpVersion.V1)) versionsToTry.Add(SnmpVersion.V1);

            bool isEpson = false;
            string detectedModel = null;
            string matchedMethod = null;

            foreach (var ver in versionsToTry)
            {
                foreach (var comm in communitiesToTry)
                {
                    string descr = await QuerySnmpTextAsync(ip, ver, comm, SysDescrOid, 1400);
                    if (!string.IsNullOrEmpty(descr) && IsEpsonResponse(descr, null))
                    {
                        isEpson = true;
                        detectedModel = CleanModelString(descr);
                        matchedMethod = "sysDescr.0";
                        break;
                    }

                    string objId = await QuerySnmpTextAsync(ip, ver, comm, SysObjectIdOid, 1200);
                    if (!string.IsNullOrEmpty(objId) && IsEpsonResponse(objId, null))
                    {
                        isEpson = true;
                        detectedModel = "Epson Device (" + objId + ")";
                        matchedMethod = "sysObjectID.0";
                        break;
                    }

                    string hrDescr = await QuerySnmpTextAsync(ip, ver, comm, HrDeviceDescrOid, 1200);
                    if (!string.IsNullOrEmpty(hrDescr) && IsEpsonResponse(hrDescr, null))
                    {
                        isEpson = true;
                        detectedModel = CleanModelString(hrDescr);
                        matchedMethod = "hrDeviceDescr.1";
                        break;
                    }

                    string prtName = await QuerySnmpTextAsync(ip, ver, comm, PrtPrinterNameOid, 1200);
                    if (!string.IsNullOrEmpty(prtName) && IsEpsonResponse(prtName, null))
                    {
                        isEpson = true;
                        detectedModel = CleanModelString(prtName);
                        matchedMethod = "prtGeneralPrinterName.1";
                        break;
                    }
                }
                if (isEpson) break;
            }

            res.IsEpson = isEpson;
            res.Model = detectedModel;
            res.MatchedMethod = matchedMethod;

            if (verifyEpson && !isEpson)
            {
                return res;
            }

            bool sent = await SendRebootCommandAsync(ip, cfg);
            res.RebootSent = sent;
            return res;
        }

        #endregion

        #region IP Range & Subnet Expansion

        public static bool TryParseSubnetMask(string maskStr, out uint mask)
        {
            mask = 0xFFFFFF00; // default 255.255.255.0
            if (string.IsNullOrWhiteSpace(maskStr))
                return true;

            string s = maskStr.Trim();
            if (s.StartsWith("/")) s = s.Substring(1).Trim();

            int prefix;
            if (int.TryParse(s, out prefix))
            {
                if (prefix >= 8 && prefix <= 30)
                {
                    mask = prefix == 0 ? 0 : 0xFFFFFFFF << (32 - prefix);
                    return true;
                }
                return false;
            }

            IPAddress maskIp;
            if (IPAddress.TryParse(s, out maskIp))
            {
                byte[] b = maskIp.GetAddressBytes();
                if (b.Length == 4)
                {
                    mask = (uint)((b[0] << 24) | (b[1] << 16) | (b[2] << 8) | b[3]);
                    return true;
                }
            }

            return false;
        }

        public static List<IPAddress> ParseIpRange(string startIpStr, string endIpStr)
        {
            var list = new List<IPAddress>();
            if (string.IsNullOrWhiteSpace(startIpStr)) return list;

            string sStart = startIpStr.Trim();
            string sEnd = (endIpStr ?? "").Trim();

            // Allow "192.168.0.1 - 192.168.0.50" in startIpStr
            if (string.IsNullOrEmpty(sEnd) && sStart.Contains("-"))
            {
                var parts = sStart.Split('-');
                if (parts.Length == 2)
                {
                    sStart = parts[0].Trim();
                    sEnd = parts[1].Trim();
                }
            }

            IPAddress startIp, endIp;
            if (!IPAddress.TryParse(sStart, out startIp) || !IPAddress.TryParse(sEnd, out endIp))
                return list;

            byte[] bStart = startIp.GetAddressBytes();
            byte[] bEnd = endIp.GetAddressBytes();
            if (bStart.Length != 4 || bEnd.Length != 4) return list;

            uint uStart = (uint)((bStart[0] << 24) | (bStart[1] << 16) | (bStart[2] << 8) | bStart[3]);
            uint uEnd = (uint)((bEnd[0] << 24) | (bEnd[1] << 16) | (bEnd[2] << 8) | bEnd[3]);

            if (uStart > uEnd)
            {
                uint tmp = uStart;
                uStart = uEnd;
                uEnd = tmp;
            }

            uint count = uEnd - uStart + 1;
            if (count > 4096)
                uEnd = uStart + 4095;

            for (uint u = uStart; u <= uEnd; u++)
            {
                byte[] bytes = new byte[]
                {
                    (byte)((u >> 24) & 0xFF),
                    (byte)((u >> 16) & 0xFF),
                    (byte)((u >> 8) & 0xFF),
                    (byte)(u & 0xFF)
                };
                list.Add(new IPAddress(bytes));
            }

            return list;
        }

        public static List<IPAddress> ParseRangeWithSubnet(string startIpStr, string endIpStr, string subnetMaskStr)
        {
            var list = new List<IPAddress>();
            if (string.IsNullOrWhiteSpace(startIpStr)) return list;

            string sStart = startIpStr.Trim();
            string sEnd = (endIpStr ?? "").Trim();
            string sMask = (subnetMaskStr ?? "").Trim();

            // Handle combined single string e.g. "192.168.0.1-192.168.0.128 subnet 255.255.255.0"
            // or "192.168.0.1 - 192.168.0.128 / 255.255.255.0" or "192.168.0.1-192.168.0.128"
            if (string.IsNullOrEmpty(sEnd))
            {
                // Check if CIDR notation e.g. "192.168.0.0/24"
                if (sStart.Contains("/") && !sStart.Contains("-"))
                {
                    return ParseSubnetCidr(sStart);
                }

                // Check for dash separator
                int dashIdx = sStart.IndexOf('-');
                if (dashIdx > 0)
                {
                    string pStart = sStart.Substring(0, dashIdx).Trim();
                    string rest = sStart.Substring(dashIdx + 1).Trim();

                    sStart = pStart;

                    // Check for subnet keyword or / in rest
                    string[] splitKeys = new string[] { "subnet", "Subnet", "SUBNET", "mask", "Mask", "MASK", "/" };
                    bool foundSubnet = false;
                    foreach (var key in splitKeys)
                    {
                        int kIdx = rest.IndexOf(key, StringComparison.OrdinalIgnoreCase);
                        if (kIdx >= 0)
                        {
                            sEnd = rest.Substring(0, kIdx).Trim();
                            if (string.IsNullOrEmpty(sMask))
                            {
                                sMask = rest.Substring(kIdx + key.Length).Trim();
                            }
                            foundSubnet = true;
                            break;
                        }
                    }

                    if (!foundSubnet)
                    {
                        // Maybe space separated e.g. "192.168.0.128 255.255.255.0"
                        string[] tokens = rest.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        if (tokens.Length >= 2)
                        {
                            sEnd = tokens[0].Trim();
                            if (string.IsNullOrEmpty(sMask)) sMask = tokens[1].Trim();
                        }
                        else
                        {
                            sEnd = rest;
                        }
                    }
                }
            }

            if (string.IsNullOrEmpty(sMask))
            {
                sMask = "255.255.255.0";
            }

            uint mask;
            if (!TryParseSubnetMask(sMask, out mask))
            {
                return list;
            }

            IPAddress startIp, endIp;
            if (!IPAddress.TryParse(sStart, out startIp) || !IPAddress.TryParse(sEnd, out endIp))
            {
                return list;
            }

            byte[] bStart = startIp.GetAddressBytes();
            byte[] bEnd = endIp.GetAddressBytes();
            if (bStart.Length != 4 || bEnd.Length != 4) return list;

            uint uStart = (uint)((bStart[0] << 24) | (bStart[1] << 16) | (bStart[2] << 8) | bStart[3]);
            uint uEnd = (uint)((bEnd[0] << 24) | (bEnd[1] << 16) | (bEnd[2] << 8) | bEnd[3]);

            if (uStart > uEnd)
            {
                uint tmp = uStart;
                uStart = uEnd;
                uEnd = tmp;
            }

            uint count = uEnd - uStart + 1;
            if (count > 8192)
                uEnd = uStart + 8191;

            for (uint u = uStart; u <= uEnd; u++)
            {
                uint net = u & mask;
                uint bcast = (u & mask) | (~mask);

                // For customers with multiple VLANs/subnets, safely skip network (.0) & broadcast (.255)
                if (u == net || u == bcast)
                    continue;

                byte[] bytes = new byte[]
                {
                    (byte)((u >> 24) & 0xFF),
                    (byte)((u >> 16) & 0xFF),
                    (byte)((u >> 8) & 0xFF),
                    (byte)(u & 0xFF)
                };
                list.Add(new IPAddress(bytes));
            }

            return list;
        }

        public static List<IPAddress> ParseSubnetCidr(string cidrStr)
        {
            var list = new List<IPAddress>();
            if (string.IsNullOrWhiteSpace(cidrStr)) return list;

            string[] parts = cidrStr.Trim().Split('/');
            if (parts.Length != 2) return list;

            IPAddress baseIp;
            int prefix;
            if (!IPAddress.TryParse(parts[0], out baseIp) || !int.TryParse(parts[1], out prefix))
                return list;

            if (prefix < 8 || prefix > 30) return list;

            byte[] b = baseIp.GetAddressBytes();
            if (b.Length != 4) return list;

            uint ip = (uint)((b[0] << 24) | (b[1] << 16) | (b[2] << 8) | b[3]);
            uint mask = prefix == 0 ? 0 : 0xFFFFFFFF << (32 - prefix);
            uint network = ip & mask;
            uint broadcast = network | ~mask;

            for (uint u = network + 1; u < broadcast; u++)
            {
                byte[] bytes = new byte[]
                {
                    (byte)((u >> 24) & 0xFF),
                    (byte)((u >> 16) & 0xFF),
                    (byte)((u >> 8) & 0xFF),
                    (byte)(u & 0xFF)
                };
                list.Add(new IPAddress(bytes));
            }

            return list;
        }

        #endregion
    }
}
