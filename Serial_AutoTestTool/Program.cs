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
        static string testFilePath = "./mqtt_test_case";
        static string resp_msg;
        static string stars_text = "\r\n***********************************************************\r\n";
        static string intro_text = "This is automation testing tool for the secure MQTT project\r\n";
        static string author_version = "Author\t\t: Viet-Tu Dao\r\n" +
                                       "SW Version\t: v0.4\r\n" +
                                       "Date Release\t: 30-11-2021\r\n";
        static string help_text = " - /start: Begin to execute the test case script\r\n" +
                                  " - /help: Display this help\r\n" + 
                                  " - /quit: Exit the program\r\n" +
                                  " - /file: Path to the test case script file (Notice: The path MUST not include white-space)\r\n";

        static void Main(string[] args)
        {
            string console_inputs;
            char[] separtors = new char[] { ' ', '\t' };

            string[] console_input_cmd_args; // = cmd_args_str.Split(separtors, StringSplitOptions.RemoveEmptyEntries);
            string console_input_cmd; // = cmd_args[0];
            int console_input_argc; // = cmd_args.Length;

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
                /* Processing console inputs */
                console_inputs = Console.ReadLine();
                if (String.IsNullOrEmpty(console_inputs) || console_inputs[0] != '/')
                {
                    _serialPort.WriteLine(
                        String.Format("{0}", console_inputs));
                    continue;
                }

                console_input_cmd_args = console_inputs.Split(separtors, StringSplitOptions.RemoveEmptyEntries);
                console_input_cmd = console_input_cmd_args[0]; // input command
                console_input_argc = console_input_cmd_args.Length; // number of input arguments

                if (stringComparer.Equals("/quit", console_input_cmd))
                {
                    goto Quit;
                }
                else if (stringComparer.Equals("/start", console_input_cmd)) 
                {
                    /* Processing the script */
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


                        //while ((line = file.ReadLine()) != null) // Read line by line of the test file

                        Console.WriteLine($"Executing the test script file: '{testFilePath}'");

                        foreach (string line in lines)
                        {
                            //Console.WriteLine(temp.GetString(b));
                            //string inputString = temp.GetString(b);
                            if (String.IsNullOrEmpty(line)) continue;
                            
                            if (is_my_command(line))
                            {
                                //string cmd_args_str = line.Substring(1, line.Length - 1); // command & arguments string
                                string[] cmd_args = line.Split(separtors, StringSplitOptions.RemoveEmptyEntries);
                                string cmd = cmd_args[0];
                                int argc = cmd_args.Length;

                                //Console.WriteLine(line);
                                if (stringComparer.Equals(cmd, "/wait"))
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
                                else if (stringComparer.Equals(cmd, "/expected"))
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
                                        Console.WriteLine($"[-------------Passed---------------]");
                                    }
                                    else
                                    {
                                        //Console.WriteLine($"Expected {cmd_args[1]}");
                                        Console.WriteLine($"[===================Failed==============]");
                                    }
                                    
                                }                                
                            }
                            //else if (line[0] == '#')
                            else if (line.TrimStart(' ')[0] == '#')
                            {
                                continue; // skips the comment
                            } 
                            else
                            {
                                _serialPort.WriteLine(line);
                            }
                            //_serialPort.WriteLine(line);
                            Thread.Sleep(300); // wait 0.3 second

                        }
                        //file.Close();
                    }
                    
                }
                else if (stringComparer.Equals("/help", console_input_cmd))
                {
                    Console.WriteLine(help_text);
                }
                else if (stringComparer.Equals("/file", console_input_cmd))
                {
                    if (console_input_argc != 2)
                    {
                        Console.WriteLine($"Default file path '{testFilePath}' will be used!");                        
                    }
                    else
                    {
                        testFilePath = console_input_cmd_args[1];
                        Console.WriteLine($"Test case script file path '{testFilePath}' will be used!");
                    }

                    if (File.Exists(testFilePath) == false)
                    {
                        Console.WriteLine($"Can't find the path: '{testFilePath}'");
                        goto Quit;
                    }
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
        public static bool is_my_command(string in_str)
        {
            bool ret = false;
            char[] delims = {' ', '\t'};

            string[] args = in_str.Split(delims, StringSplitOptions.RemoveEmptyEntries);
            if (args[0][0] == '/' && (args.Length > 1 || args[0].Length < 32)) // comand length shouldbe shorter than 32 byte-size
            {
                return true;
            }
            return ret;
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
