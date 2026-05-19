using System.Net.Sockets;
using System.Net;

namespace Flaskepost_2026
{
    public class Program
    {
        /// <summary>
        /// Adressen på den computer, som vi vil sende vores tal til
        /// </summary>
        static string PeerAddress = "127.0.0.1";
        //static string PeerAddress = "192.168.1.64";
        //static int PeerPort = 6969;
        static int PeerPort = 8080;

        public static void Main(string[] args)
        {
            while (true)
            {
                Console.Clear();

                Console.WriteLine("1. Modtag tal og send videre");
                Console.WriteLine("2. Send tal til Peer");
                Console.WriteLine("3. Sæt Peer adresse og Peer Port");
                Console.WriteLine("4. Luk programmet");
                Console.Write("Dit valg: ");
                string choice = Console.ReadLine();
                switch (choice)
                {
                    case "1":
                        Console.Clear();
                        int number = ReceiveNumber();
                        number = number + 1;
                        ForwardNumber(number);
                        break;
                    case "2":
                        ForwardNumber(1);
                        break;
                    case "3":
                        SetPeer();
                        break;
                    case "4":
                    default:
                        return;
                }
            }

        }

        public static int ReceiveNumber()
        {
            //Vi sætter en TcpListener op til at lytte på hvilken som helst addresse på maskinen, på port 6969
            TcpListener listener = new TcpListener(IPAddress.Parse("0.0.0.0"), PeerPort);
            Console.WriteLine($"Lytter på port {PeerPort}");

            //Sæt TcpListeneren i gang med at lytte
            listener.Start();

            //Stop programmet indtil nogen forbinder til serveren
            Console.WriteLine("Afventer forbindelse");
            TcpClient client = listener.AcceptTcpClient();

            //Modtag en netværksstrøm af bytes
            NetworkStream stream = client.GetStream();

            //Vi gemmer data vi modtager i en buffer og vi modtager op til 1024 ad gangen.
            byte[] buffer = new byte[1024];

            //Læs op til 1024 bytes fra strømmen
            stream.Read(buffer, 0, buffer.Length);

            //Konvertér de 4 første bytes til et tal
            int number = BitConverter.ToInt32(buffer, 0);

            //Vi har modtaget number.
            Console.WriteLine($"Modtog tallet {number}");

            return number;
        }

        public static void ForwardNumber(int number)
        {
            TcpClient client = new TcpClient();
            client.Connect(PeerAddress, PeerPort);

            if (client.Connected)
            {
                Console.WriteLine($"Forbundet til {PeerAddress}:{PeerPort}");
                //Hent den strøm vi bruger til at sende bytes i
                NetworkStream stream = client.GetStream();

                //Konvertér tallet til bytes
                byte[] bytes = BitConverter.GetBytes(number);

                //Send bytes i stream
                stream.Write(bytes);
                Console.WriteLine($"Sendte tallet {number} til Peer");
                Console.ReadKey();

                //Luk forbindelse
                client.Close();
            }
            else
            {
                Console.WriteLine($"Kunne ikke forbinde til {PeerAddress}");
            }
        }

        public static void SetPeer()
        {
            IPAddress address;
            string input = "";

            Console.Write("Indtast peer adresse: ");
            input = Console.ReadLine();
            while (!IPAddress.TryParse(input, out address))
            {
                Console.WriteLine("Ugyldig IP addresse. Prøv igen");
            }

            PeerAddress = input;
        }
    }
}
