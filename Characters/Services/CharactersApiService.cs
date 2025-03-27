using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Kenedia.Modules.Characters.Services
{
    public class CharactersApiService
    {
        private readonly HttpListener _listener;
        private CancellationTokenSource _cts;

        string HostUrl= "http://localhost:5001/";

        public CharactersApiService(string? url = null)
        {
            if (url != null)
            {
                this.HostUrl = url;
            }

            _listener = new HttpListener();
            _listener.Prefixes.Add(HostUrl);
        }

        public void Start()
        {
            _cts = new CancellationTokenSource();
            _listener.Start();
            Debug.WriteLine($"In-Memory HTTP Server started at {_listener.Prefixes}");

            Task.Run(() => RunServerLoop(_cts.Token));
        }

        public void Stop()
        {
            _cts.Cancel();
            _listener.Stop();
            Debug.WriteLine("Server stopped.");
        }

        private async Task RunServerLoop(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var context = await _listener.GetContextAsync();
                    Debug.WriteLine("Received request...");

                    using var reader = new StreamReader(context.Request.InputStream);
                    string requestBody = await reader.ReadToEndAsync();
                    Debug.WriteLine($"Request JSON: {requestBody}");

                    // Simulating a processing task (e.g., 3 seconds delay)
                    await Task.Delay(3000, cancellationToken);

                    // Creating a response
                    var responseObj = new { message = "Processing complete", timestamp = DateTime.UtcNow };
                    string jsonResponse = System.Text.Json.JsonSerializer.Serialize(responseObj);
                    byte[] buffer = Encoding.UTF8.GetBytes(jsonResponse);

                    // Sending response
                    context.Response.ContentType = "application/json";
                    context.Response.ContentLength64 = buffer.Length;
                    await context.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length, cancellationToken);
                    context.Response.Close();

                    Debug.WriteLine("Response sent.");
                }
                catch (OperationCanceledException)
                {
                    Debug.WriteLine("Server loop canceled.");
                    break;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error: {ex.Message}");
                }
            }
        }
    }
}
