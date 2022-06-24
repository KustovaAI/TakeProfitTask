using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TakeProfitTask
{
	class Program
	{
		static String host = "88.212.241.115";
		static int[] array_digits = new int[2018];		
		static int port = 2013;
		static int timeout = 30000;

		static void Main(string[] args)
		{
			int n = 2018;
			Parallel.For(1, n + 1, SendRequest);

			int med = GetMedian();
			Console.WriteLine("Mediana: " + med);
			Console.ReadLine();
		}

		public static int GetMedian()
		{
			Array.Sort(array_digits);
			int size = array_digits.Length;
			int med;
			if (size % 2 != 0)
				med = array_digits[size / 2];
			else
				med = (array_digits[size / 2 - 1] + array_digits[size / 2]) / 2;
			return med;
		}

		public static void SendRequest(int n)
		{
			try
			{
				IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse(host), port);   // Getting address
				Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);   // Creating socket
				socket.Connect(ipPoint);    // Connection to server	
				if (socket.Connected)
				{
					byte[] in_data = new byte[256]; // Input stream	
					int bytes = 0; // Length of input stream
					string digit = "";
					bool line_feed = false;
					var sw = new Stopwatch();
					sw.Start();
					byte[] out_data = Encoding.UTF8.GetBytes(n.ToString() + "\n");    // Output stream
					socket.Send(out_data);
					while (sw.ElapsedMilliseconds < timeout)
					{
						StringBuilder builder = new StringBuilder();
						do
						{
							bytes = socket.Receive(in_data);
							builder.Append(Encoding.GetEncoding("koi8r").GetString(in_data, 0, bytes));
						}
						while (socket.Available > 0);
						if (builder.ToString() != "")
						{
							ParseResponses(builder.ToString(), ref digit, ref line_feed);
							if (line_feed == true)
							{
								array_digits[n - 1] = int.Parse(digit + " ");
								Console.WriteLine(digit);
								sw.Stop();
								break;
							}
						}
					}
					sw.Stop();
					socket.Close();
					if (line_feed == false)
						SendRequest(n);
				} else
				{
					socket.Close();
				}

			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
		}

		public static void ParseResponses(String str, ref string digit, ref bool line_feed)
		{
			for (int i = 0; i < str.Length; i++)
			{
				if (Char.IsDigit(str[i]))
					digit += str[i];
				if (str[i] == '\n')
					line_feed = true;
			}
		}

	}

}
