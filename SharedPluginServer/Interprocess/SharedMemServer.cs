using System;
using SharedMemory;
using System.Text;
using System.IO;
using System.Threading;
using System.Runtime.Serialization.Formatters.Binary;
using MessageLibrary;
using System.Windows.Forms;

namespace SharedPluginServer
{


    public class SharedCommServer
    {

        private static readonly log4net.ILog log =
log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private SharedArray<byte> _inputBuf;

        private SharedArray<byte> _outputBuf;

        private bool _isInputOpen;

        public string FilenameInput;

        private bool _isOutputOpen;

        public string FilenameOutput;

        //messaging

        EventPacket _current = null;

        public delegate void ReceivedMessage(EventPacket msg);

        public bool isWorking = true;

        public static event ReceivedMessage OnReceivedMessage;


        public void Init(string filenameInput,string filenameOutput,bool create)
        {
            if(create)
            _inputBuf = new SharedArray<byte>(filenameInput, 4096); //one size fits all
            _isInputOpen = true;
            FilenameInput = filenameInput;

            if (create)
                _outputBuf = new SharedArray<byte>(filenameOutput, 4096); //one size fits all
            _isOutputOpen = true;
            FilenameOutput = filenameOutput;
            isWorking = true;
        }

        public void Write(byte[] bytes)
        {
            if (_isOutputOpen)
            {
                if (bytes.Length<4096)
                {
                    _outputBuf.Write(bytes);
                }
                
            }
        }

        public void SendData(byte[] msg)
        {
            byte[] tosend = new byte[4096000];
            byte[] intBytes = BitConverter.GetBytes(msg.Length);
            tosend[0] = intBytes[0];
            tosend[1] = intBytes[1];
            tosend[2] = intBytes[2];
            tosend[3] = intBytes[3];

            Buffer.BlockCopy(msg, 0, tosend, 4, msg.Length);
            Write(tosend);

        }

        public void WriteString(string toWrite)
        {
            byte[] bstr = Encoding.UTF8.GetBytes(toWrite);
            Write(bstr);
        }

        public byte[] Read()
        {
            if(_isInputOpen)
            {
                byte[] data = new byte[4096000];

                _inputBuf.CopyTo(data);
                return data;
            }

            return null;
        }

        public void CheckMessage()
        {
            try
            {
               
                byte[] read = Read();
                // byte[] psize = new byte[2];
                // psize[0] = read[0];
                // psize[1] = read[1];
                int length = BitConverter.ToInt32(read, 0);

                byte[] msg = new byte[4096000 - 4];

                Buffer.BlockCopy(read, 4, msg, 0, 4096000 - 4);

                //Decode
                MemoryStream mstr = new MemoryStream(msg);
                BinaryFormatter bf = new BinaryFormatter();
                EventPacket ep = bf.Deserialize(mstr) as EventPacket;

                if (ep != _current)
                    if (ep != null)
                    {
                        MessageBox.Show("Reading");
                        OnReceivedMessage?.Invoke(ep);
                    }
            }
            catch (Exception ex)
            {
                log.Error("Exception in receive:" + ex.Message);
            }
        }


        void MainCycle()
        {
            while(isWorking)
            {
                CheckMessage();
            }
            
        }

        

        public void Dispose()
        {
            _isInputOpen = false;
            _inputBuf.Close();
            _isOutputOpen = false;
            _outputBuf.Close();
        }
    }


    public class SharedMemServer:IDisposable
    {
        private SharedArray<byte> _sharedBuf;

        private bool _isOpen;

        public string Filename;

    //    private static readonly log4net.ILog log =
   //log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


       

        public void Init(int size,string filename)
        {
            _sharedBuf=new SharedArray<byte>(filename,size);
            _isOpen = true;
            Filename = filename;

        }

        public void Resize(int newSize)
        {


            if (_sharedBuf.Length != newSize)
            {
                _sharedBuf.Close();
                _sharedBuf = new SharedArray<byte>(Filename, newSize);
            }
        }

        public void WriteBytes(byte[] bytes)
        {
            if (_isOpen)
            {
                if (bytes.Length > _sharedBuf.Length)
                {
                    Resize(bytes.Length);
                }
                _sharedBuf.Write(bytes);
            }
        }

        public void Dispose()
        {
            _isOpen = false;
            _sharedBuf.Close();
        }
    

      

    }

 }
