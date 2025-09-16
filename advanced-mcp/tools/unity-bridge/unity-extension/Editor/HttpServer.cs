using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using UnityEngine;

namespace UnityBridge
{
    /// <summary>
    /// Простой HTTP сервер для Unity Bridge
    /// Только HTTP логика - без бизнес-логики
    /// </summary>
    public class HttpServer
    {
        private readonly int port;
        private readonly Func<string, Dictionary<string, object>, Dictionary<string, object>> requestHandler;
        private HttpListener listener;
        private Thread listenerThread;
        private bool isRunning;
        
        public HttpServer(int port, Func<string, Dictionary<string, object>, Dictionary<string, object>> requestHandler)
        {
            this.port = port;
            this.requestHandler = requestHandler;
        }
        
        public bool Start()
        {
            try
            {
                listener = new HttpListener();
                listener.Prefixes.Add($"http://localhost:{port}/");
                listener.Start();
                
                isRunning = true;
                listenerThread = new Thread(ListenForRequests) 
                { 
                    IsBackground = true,
                    Name = "UnityBridge-HttpListener"
                };
                listenerThread.Start();
                
                Debug.Log($"HTTP Server started on port {port}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to start HTTP server: {ex.Message}");
                return false;
            }
        }
        
        public void Stop()
        {
            if (!isRunning) return;
            
            try
            {
                isRunning = false;
                listener?.Stop();
                listener?.Close();
                
                if (listenerThread?.IsAlive == true)
                {
                    // Используем более безопасный способ остановки потока
                    // Вместо Thread.Abort() используем CancellationToken
                    listenerThread.Join(2000); // Увеличиваем время ожидания
                    
                    // Если поток все еще жив, просто логируем предупреждение
                    if (listenerThread.IsAlive)
                    {
                        Debug.LogWarning("HTTP listener thread did not stop gracefully, but server is stopped");
                    }
                }
                
                Debug.Log("HTTP Server stopped");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error stopping HTTP server: {ex.Message}");
            }
        }
        
        private void ListenForRequests()
        {
            while (isRunning && listener != null)
            {
                try
                {
                    var context = listener.GetContext();
                    ThreadPool.QueueUserWorkItem(_ => ProcessRequest(context));
                }
                catch (HttpListenerException ex)
                {
                    // Сервер останавливается - это нормально
                    if (!isRunning) 
                    {
                        Debug.Log("HTTP listener stopped gracefully");
                        return;
                    }
                    Debug.LogWarning($"HTTP listener exception: {ex.Message}");
                }
                catch (ThreadAbortException)
                {
                    // Поток был прерван - это нормально при остановке
                    Debug.Log("HTTP listener thread was aborted");
                    return;
                }
                catch (Exception ex)
                {
                    if (isRunning)
                        Debug.LogError($"HTTP listener error: {ex.Message}");
                }
            }
        }
        
        private void ProcessRequest(HttpListenerContext context)
        {
            var request = context.Request;
            var response = context.Response;
            
            try
            {
                // CORS headers
                response.AddHeader("Access-Control-Allow-Origin", "*");
                response.AddHeader("Access-Control-Allow-Methods", "POST, OPTIONS");
                response.AddHeader("Access-Control-Allow-Headers", "Content-Type");
                
                // Handle OPTIONS preflight
                if (request.HttpMethod == "OPTIONS")
                {
                    response.StatusCode = 204;
                    response.Close();
                    return;
                }
                
                // Parse request
                var endpoint = request.Url.AbsolutePath;
                var requestData = ParseRequestBody(request);
                
                // Process request
                var responseData = requestHandler(endpoint, requestData);
                
                // Send response
                SendJsonResponse(response, responseData, 200);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Request processing error: {ex.Message}");
                var errorResponse = ResponseBuilder.BuildErrorResponse($"Request processing failed: {ex.Message}");
                SendJsonResponse(response, errorResponse, 500);
            }
        }
        
        private Dictionary<string, object> ParseRequestBody(HttpListenerRequest request)
        {
            if (request.ContentLength64 == 0)
                return new Dictionary<string, object>();
                
            try
            {
                // 🚀 Принудительно используем UTF-8 для корректной обработки кириллицы
                using (var reader = new StreamReader(request.InputStream, Encoding.UTF8))
                {
                    var body = reader.ReadToEnd();
                    return string.IsNullOrWhiteSpace(body) 
                        ? new Dictionary<string, object>() 
                        : JsonUtils.FromJson(body);
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to parse request body: {ex.Message}");
                return new Dictionary<string, object>();
            }
        }
        
        private void SendJsonResponse(HttpListenerResponse response, Dictionary<string, object> data, int statusCode)
        {
            try
            {
                var json = JsonUtils.ToJson(data);
                var buffer = Encoding.UTF8.GetBytes(json);
                
                // 🚀 Поддержка UTF-8 кодировки для кириллицы
                response.ContentType = "application/json; charset=utf-8";
                response.ContentEncoding = Encoding.UTF8;
                response.ContentLength64 = buffer.Length;
                response.StatusCode = statusCode;
                
                response.OutputStream.Write(buffer, 0, buffer.Length);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to send response: {ex.Message}");
            }
            finally
            {
                try
                {
                    response.Close();
                }
                catch
                {
                    // Ignore close errors
                }
            }
        }
    }
} 