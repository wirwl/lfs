using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Windows;
using System.Windows.Input;

namespace LFStudio
{
    class ProgramPipeTest
    {
        const string pipeName = "lfsstudio_pipe";
        public void ThreadStartServer()
        {
            //FileStream fs = new FileStream("c:\\1\\server.txt", FileMode.Create,FileAccess.ReadWrite);            
          //  StreamWriter sw = new StreamWriter(fs);
         //   sw.AutoFlush = true;
            
            while (true)
            {
                NamedPipeServerStream pipeStream = new NamedPipeServerStream(pipeName,PipeDirection.InOut,1);
                //sw.WriteLine("NamedPipeServerStream pipeStream = new NamedPipeServerStream(mytestpipe,PipeDirection.InOut,1);");
                while (true)
                {
                    if (G.mainWindow != null) break;
                }                
                pipeStream.WaitForConnection();                
                //sw.WriteLine("pipeStream.WaitForConnection();");
                StreamReader sr = new StreamReader(pipeStream);
                //sw.WriteLine("StreamReader sr = new StreamReader(pipeStream);");
                string temp = sr.ReadLine();
               // sw.WriteLine("string temp = sr.ReadLine();");                
               // sw.WriteLine(temp);
               // sw.WriteLine("//sw.WriteLine(temp);");
                if (temp!=null)
                if (temp.Length!=0)
                {
                 //   sw.WriteLine("if (temp != null || temp.Length!=0)");
                    G.mainWindow.Dispatcher.Invoke(
                        new Action(
                            delegate()
                            {
                                //if (G.mainWindow == null) MessageBox.Show("Хуй тебе!");
                                    G.mainWindow.OpenFile(temp); 
                                }
                                ));
                 //   sw.WriteLine("G.mainWindow.Dispatcher.Invoke(new Action(delegate() { G.mainWindow.OpenFile(temp); }));");
                }
                //pipeStream.WaitForPipeDrain();
                pipeStream.Close();
                //sw.WriteLine("pipeStream.Close();");
                pipeStream = null;
                sr.Close();// sr = null;
            //    sw.WriteLine("sr.Close();");
                //sw.Close();
            }   //while
        }

        public void ThreadStartClient(object obj)
        {
            // Ensure that we only start the client after the server has created the pipe
            ManualResetEvent SyncClientServer = (ManualResetEvent)obj;

            // Only continue after the server was created -- otherwise we just fail badly
            // SyncClientServer.WaitOne();

            using (NamedPipeClientStream pipeStream = new NamedPipeClientStream(pipeName))
            {
                // The connect function will indefinately wait for the pipe to become available
                // If that is not acceptable specify a maximum waiting time (in ms)
                pipeStream.Connect(5000);

                //Console.WriteLine("[Client] Pipe connection established");
                using (StreamWriter sw = new StreamWriter(pipeStream))
                {
                    ////sw.AutoFlush = true;
                    if (Environment.GetCommandLineArgs().Length >= 2)
                    {
                        string fname = Environment.GetCommandLineArgs()[1];
                        sw.WriteLine(fname);
                        sw.Flush();
                    }
                }
            }
        }       
    }
}