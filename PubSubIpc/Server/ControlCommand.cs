using System;
using System.Text;
using PubSubIpc.Shared;

namespace PubSubIpc.Server
{
    public struct ControlCommand
    {
        public ControlBytes Control {get; set;}
        public string Data {get; set;}

        public static ControlCommand FromByteArray(byte[] bytes)
        {
            if (bytes[0] != (byte)ControlBytes.Escape) throw new Exception("Conversion to ControlCommand failed");

            byte control = bytes[1];
            string data = Encoding.ASCII.GetString(bytes, 2, bytes.Length - 2);
            var command = new ControlCommand{Control = (ControlBytes)control, Data = data};
            return command;
        }
    }
}