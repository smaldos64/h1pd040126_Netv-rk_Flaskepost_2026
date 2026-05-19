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
                Console.WriteLine("3. Sæt Peer adresse");
                Console.WriteLine("4. Luk programmet");
                Console.Write("Dit valg: ");
                string choice = Console.ReadLine();
                switch (choice)
                {
                    case "1":
                        Console.Clear();
                        RunFlaskepostLoop();
                        //int number = ReceiveNumber();
                        //number = number + 1;
                        //ForwardNumber(number);
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

        //public static void SetPeer()
        //{
        //    IPAddress address;
        //    Console.Write("Indtast peer adresse (f.eks. 127.0.0.1): ");
        //    string input = Console.ReadLine();
        //    while (!IPAddress.TryParse(input, out address))
        //    {
        //        Console.WriteLine("Ugyldig IP addresse. Prøv igen");
        //        input = Console.ReadLine();
        //    }
        //    PeerAddress = input;

        //    Console.Write("Indtast peer port (f.eks. 8081): ");
        //    string portInput = Console.ReadLine();
        //    int port;
        //    while (!int.TryParse(portInput, out port))
        //    {
        //        Console.WriteLine("Ugyldig port. Prøv igen");
        //        portInput = Console.ReadLine();
        //    }
        //    PeerPort = port;
        //}

        public static void RunFlaskepostLoop()
        {
            // Start listeneren én gang uden for løkken
            TcpListener listener = new TcpListener(IPAddress.Parse("0.0.0.0"), PeerPort);
            listener.Start();

            Console.WriteLine($"Lytter på port {PeerPort}...");
            Console.WriteLine("Tryk på ESC for at afslutte og vende tilbage til menuen.\n");

            // Denne løkke kører uendeligt, indtil der trykkes ESC
            while (true)
            {
                // 1. Tjek om brugeren har trykket på en tast
                if (Console.KeyAvailable)
                {
                    ConsoleKeyInfo key = Console.ReadKey(true); // true skjuler tasten i konsollen
                    if (key.Key == ConsoleKey.Escape)
                    {
                        Console.WriteLine("\nAfbryder lytning...");
                        break; // Hopper ud af while-løkken
                    }
                }

                // 2. Tjek om der er en indkommende netværksforbindelse
                if (listener.Pending())
                {
                    using (TcpClient client = listener.AcceptTcpClient())
                    using (NetworkStream stream = client.GetStream())
                    {
                        byte[] buffer = new byte[1024];
                        stream.Read(buffer, 0, buffer.Length);

                        int number = BitConverter.ToInt32(buffer, 0);

                        // --- PÆDAGOGISK PAUSE STARTER HER ---
                        Console.WriteLine($"\n[{DateTime.Now.ToLongTimeString()}] 📬 MODTOG tallet: {number}");
                        Console.WriteLine("Tænker i 5 sekunder før flaskeposten sendes videre...");

                        // Vi tæller ned på skærmen, så eleverne kan følge med live
                        for (int i = 5; i > 0; i--)
                        {
                            Console.Write(i + "... ");
                            Thread.Sleep(1000); // Vent 1 sekund af gangen
                        }
                        Console.WriteLine(); // Ny linje

                        // Tæl op og send videre
                        int nextNumber = number + 1;
                        Console.WriteLine($"🚀 SENDER tallet {nextNumber} videre til {PeerAddress}:{PeerPort}!");
                        ForwardNumber(nextNumber);
                        // --- PÆDAGOGISK PAUSE SLUTTER HER ---
                    }

                    Console.WriteLine($"\nKlar igen. Lytter på port {PeerPort} (Tryk ESC for at lukke)...");
                }

                // 3. En ultrakort pause (100 ms) for at computerens processor ikke kører på 100%
                Thread.Sleep(100);
            }

            // Stop listeneren når vi forlader løkken via ESC
            listener.Stop();
            Console.WriteLine("Tryk på en vilkårlig tast for at fortsætte...");
            Console.ReadKey();
        }
    }
}
