using System;
using System.Threading;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using System.Diagnostics;
using DataSetGenerator;

namespace SW9_Project {
    public class Connection {

        Socket socket;

        private static List<Connection> allConnections;
        private static bool alive = true;
        public static Gyroscope gyro = new Gyroscope();

        public static List<Connection> AllConnections {
            get {
                if (allConnections == null) {
                    allConnections = new List<Connection>();
                }
                return allConnections;
            }
        }

        public Connection(Socket socket) {
            this.socket = socket;

            Task.Factory.StartNew(() => {
                ManageMobileConnection();
            });

        }

        static byte[] GetBytes(string str) {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }

        private static void StartService()
        {
            UdpClient dispatcher = new UdpClient(49255);
            Task.Factory.StartNew(() =>
            {
                byte[] response;
                //response = GetBytes( "DISCOVER_IS903SERVER_RESPONSE");
                response = Encoding.ASCII.GetBytes("DISCOVER_IS903SERVER_RESPONSE");
                while (alive)
                {
                    var remoteEP = new IPEndPoint(IPAddress.Any, 49255);
                    var data = dispatcher.Receive(ref remoteEP); // listen on port 49255
                    Console.Write("receive data from " + remoteEP.ToString());
                    dispatcher.Send(response, response.Length, remoteEP); //reply back
                }
            });
        }

        public static void StartService(int port = 8000) {
            UdpClient dispatcher = new UdpClient(49255);
            Task.Factory.StartNew(() =>
            {
                byte[] response  = Encoding.ASCII.GetBytes("DISCOVER_IS903SERVER_RESPONSE");
                while (alive)
                {
                    var remoteEP = new IPEndPoint(IPAddress.Any, 49255);
                    var data = dispatcher.Receive(ref remoteEP); // listen on port 49255
                    string returnData = Encoding.ASCII.GetString(data);
                    if (returnData.StartsWith("gyrodata"))
                    {
                        gyro.Update(returnData.Split(':')[2], returnData.Split(':')[4], returnData.Split(':')[6], returnData.Split(':')[8]);
                    }
                    dispatcher.Send(response, response.Length, remoteEP); //reply back
                }
            });

            TcpListener listener = new TcpListener(IPAddress.Any, port);
            listener.Start();
            Console.WriteLine("Now listening on port " + port);
            Task.Factory.StartNew(() => {
                while (alive) {
                    Socket socket = listener.AcceptSocket();
                    AllConnections.Add(new Connection(socket));
                }
                listener.Stop();
            });
        }

        StreamWriter sw;
        StreamReader sr;
        private void ManageMobileConnection() {
            try
            {
                Console.WriteLine("User connected! Address: " + socket.RemoteEndPoint);
                CanvasWindow.SetConnection(this);
                Connected = true;

                using (NetworkStream stream = new NetworkStream(socket))
                using (sr = new StreamReader(stream))
                using (sw = new StreamWriter(stream))
                {
                    sw.AutoFlush = true;
                    string start = GestureParser.GetDirectionContext() == GestureDirection.Pull ? "startpull" : "startpush";
                    sw.WriteLine(start);
                    SetGesture(GestureParser.GetTypeContext());
                    while (true)
                    {
                        String line = sr.ReadLine();
                        if (line.Contains("nextshape:"))
                        {
                            nextShape = line.Split(':')[1];
                        }
                        else if (line.Contains("resetgyro"))
                        {
                            gyro.ResetGyroscope();
                        }
                        //else if (line.StartsWith("gyrodata"))
                        //{
                        //    gyro.Update(line.Split(':')[2], line.Split(':')[4], line.Split(':')[6], line.Split(':')[8]);
                        //}
                        else {
                            dynamic jO = JsonConvert.DeserializeObject(line);
                            if (jO.GetType().GetProperty("Type") != null)
                            {
                                GestureParser.AddMobileGesture(new MobileGesture(jO));
                            }
                        }
                    }
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                socket.Close();
                Connected = false;
            }
        }

        public bool Connected { get; set; }

        public void StartTest(GestureDirection direction) {
            String test = direction == GestureDirection.Pull ? "startpull" : "startpush";
            sw.WriteLine(test);
            sw.Flush();
        }

        public void SwitchShapes() {
            sw.WriteLine("switch");
        }

        public void SendPinch() {
            sw.WriteLine("pinch:pull");
        }

        String nextShape = "";
        public void SetNextShape(string shape) {
            sw.WriteLine("nextshape:" + shape);
        }

        public void SetGesture(GestureType type) {
            string gesture = "";
            switch (type) {
                case GestureType.Pinch: gesture = "pinch"; break;
                case GestureType.Swipe: gesture = "swipe"; break;
                case GestureType.Throw: gesture = "throw"; break;
                case GestureType.Tilt: gesture = "tilt"; break;
            }
            sw.WriteLine("gesture:" + gesture);
        }
    }
}
