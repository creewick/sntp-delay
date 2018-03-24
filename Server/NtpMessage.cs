using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;

namespace Server
{
    public class NtpMessage
    {
        public enum LeapType
        {
            NoWarning,
            OneMoreSecond,
            OneLessSecond,
            Alert
        }

        public enum ModeType
        {
            Reserved,
            SymmetricActive,
            SymmetricPassive,
            Client,
            Server,
            Broadcast,
            ControlMessage,
            Private
        }

        public LeapType LeapIndicator { get; private set; }
        public int VersionNumber { get; }
        public ModeType Mode { get; private set; }
        public byte Stratum { get; private set; }
        public byte Poll;
        public byte Precision { get; private set; }
        public byte[] RootDelay;
        public byte[] RootDispersion;
        public byte[] ReferenceId;
        public byte[] ReferenceTimestamp;
        public byte[] OriginateTimestamp;
        public byte[] ReceiveTimestamp;
        public byte[] TransmitTimestamp;

        public NtpMessage(byte[] request)
        {
            LeapIndicator = (LeapType) ((request[0] & 0b1100_0000) >> 6);
            VersionNumber = (request[0] & 0b0011_1000) >> 3;
            Mode = (ModeType) (request[0] & 0b0000_0111);
            Stratum = request[1];
            Poll = request[2];
            Precision = request[3];
            RootDelay = request.Skip(4).Take(4).ToArray();
            RootDispersion = request.Skip(8).Take(4).ToArray();
            ReferenceId = request.Skip(12).Take(4).ToArray();
            ReferenceTimestamp = request.Skip(16).Take(8).ToArray();
            OriginateTimestamp = request.Skip(24).Take(8).ToArray();
            ReceiveTimestamp = request.Skip(32).Take(8).ToArray();
            TransmitTimestamp = request.Skip(40).Take(8).ToArray();
        }

        public NtpMessage GetAnswer(double lie)
        {
            LeapIndicator = 0;
            Mode = ModeType.Server; 
            Stratum = 1;
            Precision = unchecked((byte)-12);
            RootDelay = new byte[4];
            RootDispersion = new byte[4];
            ReferenceId = new byte[4];

            var timestamp = Lie(lie);
            
            ReferenceTimestamp = timestamp;
            TransmitTimestamp.CopyTo(OriginateTimestamp, 0);
            ReceiveTimestamp = timestamp;
            TransmitTimestamp = timestamp;
            return this;
        }

        private static byte[] Lie(double lie)
        {
            var utcTime = DateTime.UtcNow.AddSeconds(lie) - new DateTime(1900, 1, 1);
            
            var seconds = (UInt32) utcTime.TotalSeconds;
            var byteSeconds = BitConverter.GetBytes(seconds)
                .Reverse().ToArray();
            
            var fraction = utcTime.TotalSeconds % 1 * Math.Pow(2, 32);
            var byteFraction = BitConverter.GetBytes((UInt32) fraction)
                .Reverse().ToArray();

            var result = new byte[8];
            Array.Copy(byteSeconds, 0, result, 0, 4);
            Array.Copy(byteFraction, 0, result, 4, 4);
            return result;
        }

        public byte[] ToBytes()
        {
            var result = new List<byte>();
            var firstByte = ((byte) LeapIndicator << 6) | ((byte) VersionNumber << 3) | (byte) Mode;
            result.Add((byte)firstByte);
            result.Add(Stratum);
            result.Add(Poll);
            result.Add(Precision);
            
            result.AddRange(RootDelay);
            result.AddRange(RootDispersion);
            result.AddRange(ReferenceId);
            
            result.AddRange(ReferenceTimestamp);
            result.AddRange(OriginateTimestamp);
            result.AddRange(ReceiveTimestamp);
            result.AddRange(TransmitTimestamp);
            return result.ToArray();
        }
    }
}