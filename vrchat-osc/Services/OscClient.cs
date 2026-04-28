using System.Net;
using System.Net.Sockets;
using System.Text;

namespace vrchat_osc.Services;

public class OscClient(string ip = "127.0.0.1", int port = 9000)
{
    private readonly UdpClient _udp = new();
    private readonly IPEndPoint _endPoint = new(IPAddress.Parse(ip), port);

    public void Send(string address, params object[] args)
    {
        var packet = BuildMessage(address, args);
        _udp.Send(packet, packet.Length, _endPoint);
    }

    private static byte[] BuildMessage(string address, object[] args)
    {
        var data = new List<byte>();

        // address
        AddPaddedString(data, address);

        // type tags
        var typeTags = args.Aggregate(",", (current, arg) => current + arg switch
        {
            string => "s",
            int => "i",
            float => "f",
            bool b => b ? "T" : "F",
            _ => throw new Exception($"Unsupported type: {arg.GetType()}")
        });

        AddPaddedString(data, typeTags);

        // arguments
        foreach (var arg in args)
        {
            switch (arg)
            {
                case string s:
                    AddPaddedString(data, s);
                    break;
                case int i:
                    AddInt(data, i);
                    break;
                case float f:
                    AddFloat(data, f);
                    break;
                case bool:
                    continue;
            }
        }

        return data.ToArray();
    }

    private static void AddPaddedString(List<byte> data, string str)
    {
        var bytes = Encoding.UTF8.GetBytes(str);
        data.AddRange(bytes);
        data.Add(0);

        while (data.Count % 4 != 0)
            data.Add(0);
    }

    private static void AddInt(List<byte> data, int value)
    {
        var bytes = BitConverter.GetBytes(value);
        if (BitConverter.IsLittleEndian)
            Array.Reverse(bytes);

        data.AddRange(bytes);
    }

    private static void AddFloat(List<byte> data, float value)
    {
        var bytes = BitConverter.GetBytes(value);
        if (BitConverter.IsLittleEndian)
            Array.Reverse(bytes);

        data.AddRange(bytes);
    }
}