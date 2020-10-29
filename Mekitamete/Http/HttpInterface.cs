using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Mekitamete.Http
{
    public class HttpInterface 
    {
        private HttpListener listener;
        private List<Task> runningRequests;

        private bool isTerminating;

        public HttpInterface(ushort listenPort)
        {
            runningRequests = new List<Task>();

            listener = new HttpListener();
            listener.IgnoreWriteExceptions = true;
            listener.Prefixes.Add($"http://*:{listenPort}/");
            
        }

        private void HandleRequest(HttpListenerContext ctx)
        {
            if (isTerminating)
            {
                ctx.Response.Close();
            }

            Logger.Log($"HTTP request: {ctx.Request.RawUrl}");

            byte[] response;
            try
            {
                response = MainApplication.HandleHTTPRequest(ctx);
            }
            catch (Exception ex)
            {

            }
            finally
            {
                ctx.Response.Close();
            }
        }

        public void Stop()
        {
            Logger.Log("Received a request to stop the HTTP listener");
            isTerminating = true;

            if (!Task.WaitAll(runningRequests.ToArray(), 6000))
            {
                Logger.Log("Warning: some requests were not completed", Logger.MessageLevel.Warning);
            }

            runningRequests.Clear();
            listener.Stop();
        }

        public async void Listen()
        {
            listener.Start();
            
            await Task.Run(async () =>
            {
                while (listener.IsListening)
                {
                    Logger.Log("awaiting request");

                    try
                    {
                        if (isTerminating)
                        {
                            return;
                        }

                        var ctx = await listener.GetContextAsync();
                        var task = Task.Run(delegate { HandleRequest(ctx); });

                        runningRequests.Add(task);
                        _ = task.ContinueWith(delegate { runningRequests.Remove(task); });
                    }
                    catch (HttpListenerException)
                    {
                        return;
                    }
                }
            });
        }
    }
}
