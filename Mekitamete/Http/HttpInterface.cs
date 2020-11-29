using Mekitamete.Http.Endpoints;
using Mekitamete.Http.Responders;
using Mekitamete.Http.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
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

        private void HandleRequestInternal(HttpRequestArgs args)
        {
            // TODO: verify the performance of this pile of reflection

            // any of the endpoints will do here; it's done this way in order to avoid hardcoding the namespace
            string endpointsNamespace = typeof(StatusEndpoint).Namespace;

            // get all endpoint classes
            var allEndpoints = Assembly.GetExecutingAssembly().GetTypes().Where(x => x.Namespace == endpointsNamespace);

            // get all endpoints that contain the HttpEndpointAttribute
            var attributedEndpoints = allEndpoints.Select(x => new Tuple<Type, Attribute>(x, x.GetCustomAttribute(typeof(HttpEndpointAttribute)))).Where(x => x.Item2 != null);

            // get the endpoint that should serve the request
            var endpointTuple = attributedEndpoints.FirstOrDefault(x => ((HttpEndpointAttribute)x.Item2).ShouldServeRequest(args.Url));
            if (endpointTuple == null)
            {
                // TODO: send some response
                args.SetResponse(404, new HttpErrorResponse("Endpoint not found"));
                return;
            }

            var endpointAttribute = (HttpEndpointAttribute)endpointTuple.Item2;
            var endpoint = endpointTuple.Item1;

            // locate the correct method for requested HTTP method
            var attributedMethods = endpoint.GetMethods(BindingFlags.Public | BindingFlags.Static).Select(x => new Tuple<MethodInfo, Attribute>(x, x.GetCustomAttribute(typeof(HttpMethodAttribute)))).Where(x => x.Item2 != null);
            var methodTuple = attributedMethods.FirstOrDefault(x => ((HttpMethodAttribute)x.Item2).Method == args.Context.Request.HttpMethod);
            if (methodTuple == null)
            {
                // TODO: send some response
                args.SetResponse(405, new HttpErrorResponse("HTTP method not supported for this endpoint"));
                return;
            }

            // determine if the endpoint contains an asterisk
            string urlArguments = null;
            if (endpointAttribute.UrlContainsArguments)
            {
                urlArguments = endpointAttribute.GetUrlArguments(args.Url);
            }

            // call the handler
            methodTuple.Item1.Invoke(null, new object[] { args });
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
                    HandleRequestInternal(args);
                }
                catch (Exception ex)
                {
                    args.SetResponse(500, new HttpErrorResponse($"An exception has occurred during request processing:\n{ex}"));
                }
            }

            if (args.Response == null)
            {
                Logger.Log("Http", $"Request {args.Url} did not produce any response", Logger.MessageLevel.Warning);
                args.Response = new HttpErrorResponse("Empty response");
            }

            byte[] resp = args.Response.ToJsonResponse();
            ctx.Response.ContentLength64 = resp.LongLength;
            ctx.Response.OutputStream.Write(resp);

            ctx.Response.Close();
        }

        public void Stop()
        {
            Logger.Log("Http", "Received a request to stop the HTTP listener");
            isTerminating = true;

            if (!Task.WaitAll(runningRequests.ToArray(), 6000))
            {
                Logger.Log("Http", "Warning: some requests were not completed", Logger.MessageLevel.Warning);
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
