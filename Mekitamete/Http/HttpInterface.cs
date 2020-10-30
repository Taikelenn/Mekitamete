using Mekitamete.Http.Responses;
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
            HttpRequestArgs args = new HttpRequestArgs(ctx);
            ctx.Response.ContentType = "application/json";

            // if the HTTP server is closing, ignore all incoming requests
            if (isTerminating)
            {
                args.SetResponse(503, new HttpErrorResponse("The server is shutting down"));
            }
            else if (args.Url == null)
            {
                args.SetResponse(403, new HttpErrorResponse("Invalid API key"));
            }
            else
            {
                try
                {
                    MainApplication.HandleHTTPRequest(args);
                }
                catch (Exception ex)
                {
                    args.SetResponse(500, new HttpErrorResponse($"An exception has occurred during request processing:\n{ex}"));
                }
            }

            if (args.Response != null)
            {
                byte[] resp = args.Response.ToJsonResponse();
                ctx.Response.ContentLength64 = resp.LongLength;
                ctx.Response.OutputStream.Write(resp);
            }
            else
            {
                Logger.Log($"Request {args.Url} did not produce any response", Logger.MessageLevel.Warning);
            }

            ctx.Response.Close();
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
