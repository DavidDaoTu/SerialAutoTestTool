using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Threading;
using System.IO;

namespace Serial_AutoTestTool
{
    class Program
    {
        static bool _continue;
        static SerialPort _serialPort;
        static string testFilePath = "mqtt_test_case";
        static string resp_msg;
        static string stars_text = "\r\n***********************************************************\r\n";
        static string intro_text = "This is automation testing tool for the secure MQTT project\r\n";
        static string author_version = "Author\t\t: Viet-Tu Dao\r\n" +
                                       "SW version\t: v0.2\r\n" +
                                       "Date Release\t: 24-11-2021\r\n";
        static string help_text = " - /start: Begin to execute the test case script\r\n" +
                                  " - /help: Display this help\r\n" + 
                                  " - /quit: Exit the program\r\n";

        static void Main(string[] args)
        {
            //string name;
            string message;
            text_init();

            StringComparer stringComparer = StringComparer.OrdinalIgnoreCase;
            Thread readThread = new Thread(Read);

            // Create a new SerialPort object with default settings.
            _serialPort = new SerialPort();

            // Allow the user to set the appropriate properties.
            _serialPort.PortName = SetPortName(_serialPort.PortName);
            _serialPort.BaudRate = SetPortBaudRate(_serialPort.BaudRate);
            _serialPort.Parity = SetPortParity(_serialPort.Parity);
            _serialPort.DataBits = SetPortDataBits(_serialPort.DataBits);
            _serialPort.StopBits = SetPortStopBits(_serialPort.StopBits);
            _serialPort.Handshake = SetPortHandshake(_serialPort.Handshake);

            // Set the read/write timeouts
            _serialPort.ReadTimeout = 500;
            _serialPort.WriteTimeout = 500;

            _serialPort.Open();
            _continue = true;
            readThread.Start();

            //Console.Write("Name: ");
            //name = Console.ReadLine();

            //Console.WriteLine("Type QUIT to exit");

            while (_continue)
            {
                message = Console.ReadLine();

                if (stringComparer.Equals("/quit", message))
                {
                    goto Quit;
                }
                else if (stringComparer.Equals("/start", message)) 
                {
                    if (File.Exists(testFilePath) == false)
                    {
                        Console.WriteLine($"Can't find the path: {testFilePath}");
                        goto Quit;
                    }
                    // Open the stream and read it back.
                    //using (StreamReader file = new StreamReader(testFilePath))
                    string[] lines = File.ReadAllLines(testFilePath);
                    {
                        //byte[] b = new byte[1024];
                        //UTF8Encoding temp = new UTF8Encoding(true);

                        //while (fs.Read(b, 0, b.Length) > 0)

                        //string line;
                        char[] separtors = new char[] { ' ' };

                        //while ((line = file.ReadLine()) != null) // Read line by line of the test file
                        foreach (string line in lines)
                        {
                            //Console.WriteLine(temp.GetString(b));
                            //string inputString = temp.GetString(b);
                            if (String.IsNullOrEmpty(line)) continue;
                            if (line[0] == '/' && line.Length < 50)  // my command line
                            {
                                string cmd_args_str = line.Substring(1, line.Length - 1); // command & arguments string
                                string[] cmd_args = cmd_args_str.Split(separtors, StringSplitOptions.RemoveEmptyEntries);
                                string cmd = cmd_args[0];
                                int argc = cmd_args.Length;
                                //Console.WriteLine(line);
                                if (stringComparer.Equals(cmd, "wait"))
                                {
                                    
                                    if (argc != 2)
                                    {
                                        Console.WriteLine($"Invalid number of arguments {argc}");
                                        //file.Close();
                                        goto Quit;
                                    }
                                    
                                    int sleep_time_sec;
                                    if (Int32.TryParse(cmd_args[1], out sleep_time_sec) == false)
                                    {
                                        Console.WriteLine($"Failed to parse {cmd_args[1]}");
                                        //file.Close();
                                        goto Quit;
                                    }
                                    if (sleep_time_sec > 0)
                                    {
                                        Thread.Sleep(sleep_time_sec * 1000); // milisec -> sec
                                    }
                                    else
                                    {
                                        Console.WriteLine($"Invalid time{sleep_time_sec}");
                                        //file.Close();
                                        goto Quit;
                                    }

                                }
                                else if (stringComparer.Equals(cmd, "expected"))
                                {
                                    if (argc != 2)
                                    {
                                        Console.WriteLine($"Invalid number of arguments {argc}");
                                        //file.Close();
                                        goto Quit;
                                    }
                                    string _resp_msg = resp_msg.Substring(0, resp_msg.Length - 2);
                                    if (String.Equals(_resp_msg, cmd_args[1], StringComparison.OrdinalIgnoreCase))
                                    {
                                        Console.WriteLine("<-------------Passed--------------->");
                                    }
                                    else
                                    {

                                        //Console.WriteLine($"Expected {cmd_args[1]}");
                                        Console.WriteLine("=====================================================> Failed!!!");
                                    }
                                    
                                }
                                
                            }
                            else if (line[0] == '#')
                            {
                                continue; // skips the comment
                            } 
                            else
                            {
                                _serialPort.WriteLine(line);
                            }

                            
                        }
                        //file.Close();
                    }
                    
                }
                else if (stringComparer.Equals("/help", message))
                {
                    Console.WriteLine(help_text);
                }
                else
                {
                    //_serialPort.WriteLine(
                    //    String.Format("<{0}>: {1}", name, message));
                    _serialPort.WriteLine(
                        String.Format("{0}", message));
                }
            }
            Quit:
                _continue = false;
                readThread.Join();                
                _serialPort.Close();
                Console.WriteLine("Press enter to exit");
                Console.ReadKey();
                return;
        }

        public static void text_init()
        {
            Console.WriteLine(stars_text + intro_text + author_version + "Help:\r\n" + help_text + stars_text);
        }

        public static void Read()
        {
            while (_continue)
            {
                try
                {
                    //string message = _serialPort.ReadLine();
                    //Console.WriteLine(message);
                    resp_msg = _serialPort.ReadLine();
                    Console.WriteLine(resp_msg);
                }
                catch (TimeoutException) { }
            }
        }

        // Display Port values and prompt user to enter a port.
        public static string SetPortName(string defaultPortName)
        {
            string portName;

            Console.WriteLine("Available Ports:");
            foreach (string s in SerialPort.GetPortNames())
            {
                Console.WriteLine("   {0}", s);
            }

            Console.Write("Enter COM port value (Default: {0}): ", defaultPortName);
            portName = Console.ReadLine();

            if (portName == "" || !(portName.ToLower()).StartsWith("com"))
            {
                portName = defaultPortName;
            }
            return portName;
        }
        // Display BaudRate values and prompt user to enter a value.
        public static int SetPortBaudRate(int defaultPortBaudRate)
        {
            string baudRate;

            Console.Write("Baud Rate(default:{0}): ", defaultPortBaudRate);
            baudRate = Console.ReadLine();

            if (baudRate == "")
            {
                baudRate = defaultPortBaudRate.ToString();
            }

            return int.Parse(baudRate);
        }

        // Display PortParity values and prompt user to enter a value.
        public static Parity SetPortParity(Parity defaultPortParity)
        {
            string parity;

            Console.WriteLine("Available Parity options:");
            foreach (string s in Enum.GetNames(typeof(Parity)))
            {
                Console.WriteLine("   {0}", s);
            }

            Console.Write("Enter Parity value (Default: {0}):", defaultPortParity.ToString(), true);
            parity = Console.ReadLine();

            if (parity == "")
            {
                parity = defaultPortParity.ToString();
            }

            return (Parity)Enum.Parse(typeof(Parity), parity, true);
        }
        // Display DataBits values and prompt user to enter a value.
        public static int SetPortDataBits(int defaultPortDataBits)
        {
            string dataBits;

            Console.Write("Enter DataBits value (Default: {0}): ", defaultPortDataBits);
            dataBits = Console.ReadLine();

            if (dataBits == "")
            {
                dataBits = defaultPortDataBits.ToString();
            }

            return int.Parse(dataBits.ToUpperInvariant());
        }

        // Display StopBits values and prompt user to enter a value.
        public static StopBits SetPortStopBits(StopBits defaultPortStopBits)
        {
            string stopBits;

            Console.WriteLine("Available StopBits options:");
            foreach (string s in Enum.GetNames(typeof(StopBits)))
            {
                Console.WriteLine("   {0}", s);
            }

            Console.Write("Enter StopBits value (None is not supported and \n" +
             "raises an ArgumentOutOfRangeException. \n (Default: {0}):", defaultPortStopBits.ToString());
            stopBits = Console.ReadLine();

            if (stopBits == "")
            {
                stopBits = defaultPortStopBits.ToString();
            }

            return (StopBits)Enum.Parse(typeof(StopBits), stopBits, true);
        }
        public static Handshake SetPortHandshake(Handshake defaultPortHandshake)
        {
            string handshake;

            Console.WriteLine("Available Handshake options:");
            foreach (string s in Enum.GetNames(typeof(Handshake)))
            {
                Console.WriteLine("   {0}", s);
            }

            Console.Write("Enter Handshake value (Default: {0}):", defaultPortHandshake.ToString());
            handshake = Console.ReadLine();

            if (handshake == "")
            {
                handshake = defaultPortHandshake.ToString();
            }

            return (Handshake)Enum.Parse(typeof(Handshake), handshake, true);
        }
    }
}
