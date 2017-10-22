﻿/**
* Copyright 2015 IBM Corp. All Rights Reserved.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
*      http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*
*/

//! Uncomment to enable message debugging
//#define ENABLE_MESSAGE_DEBUGGING

using IBM.Watson.DeveloperCloud.Logging;
using IBM.Watson.DeveloperCloud.Utilities;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using WebSocketSharp;


namespace IBM.Watson.DeveloperCloud.Connection
{
    /// <summary>
    /// WebSocket connector class.
    /// </summary>
    public class WSConnector
    {
        #region Public Types
        /// <summary>
        /// Callback for a connector event.
        /// </summary>
        /// <param name="connection">The WSConnector object.</param>
        public delegate void ConnectorEvent(WSConnector connection);
        /// <summary>
        /// Callback for a message received on the connector.
        /// </summary>
        /// <param name="resp">The message object.</param>
        public delegate void MessageEvent(Message resp);

        /// <summary>
        /// ConnectionState enumeration describes the current state of this connector.
        /// </summary>
        public enum ConnectionState
        {
            /// <summary>
            /// We are trying to connect.
            /// </summary>
            CONNECTING,
            /// <summary>
            /// Connector is connected to the server.
            /// </summary>
            CONNECTED,
            /// <summary>
            /// Connected has lost connection to the server.
            /// </summary>
            DISCONNECTED,
            /// <summary>
            /// Connected has been closed to the server.
            /// </summary>
            CLOSED
        }

        /// <summary>
        /// The base abstract class for a Message that can be sent/received by this class.
        /// </summary>
        public abstract class Message
        { };

        /// <summary>
        /// BinaryMessage for sending raw binary data.
        /// </summary>
        public class BinaryMessage : Message
        {
            /// <summary>
            /// Constructor for a BinaryMessage object.
            /// </summary>
            /// <param name="data">The binary data to send as a message.</param>
            public BinaryMessage(byte[] data)
            {
                Data = data;
            }

            #region Public Properties
            /// <summary>
            /// Binary payload.
            /// </summary>
            public byte[] Data { get; set; }
            #endregion
        };
        /// <summary>
        /// TextMessage is used for sending text messages (e.g. JSON, XML)
        /// </summary>
        public class TextMessage : Message
        {
            /// <summary>
            /// Constructor for a TextMessage object.
            /// </summary>
            /// <param name="text">The string of the text to send as a message.</param>
            public TextMessage(string text)
            {
                Text = text;
            }

            #region Public Properties
            /// <summary>
            /// Text payload.
            /// </summary>
            public string Text { get; set; }
            #endregion
        };
        #endregion

        #region Public Properties
        /// <summary>
        /// This delegate is invoked when the connection is closed.
        /// </summary>
        public ConnectorEvent OnClose { get; set; }
        /// <summary>
        /// This delegate is invoked when a message is received for a socket.
        /// </summary>
        public MessageEvent OnMessage { get; set; }
        /// <summary>
        /// The URL of the WebSocket.
        /// </summary>
        public string URL { get; set; }
        /// <summary>
        /// Headers to pass when making the socket.
        /// </summary>
        public Dictionary<string, string> Headers { get; set; }
        /// <summary>
        /// Credentials used to authenticate with the server.
        /// </summary>
        public Credentials Authentication { get; set; }
        /// <summary>
        /// The current state of this connector.
        /// </summary>
        public ConnectionState State { get { return _connectionState; } set { _connectionState = value; } }
        #endregion

        #region Private Data
        private ConnectionState _connectionState = ConnectionState.CLOSED;
        private Thread _sendThread = null;
        private AutoResetEvent _sendEvent = new AutoResetEvent(false);
        private Queue<Message> _sendQueue = new Queue<Message>();
        private AutoResetEvent _receiveEvent = new AutoResetEvent(false);
        private Queue<Message> _receiveQueue = new Queue<Message>();
        private int _receiverRoutine = 0;
        #endregion

        /// <summary>
        /// Helper function to convert a HTTP/HTTPS url into a WS/WSS URL.
        /// </summary>
        /// <param name="URL">The URL to fix up.</param>
        /// <returns>The fixed up URL.</returns>
        public static string FixupURL(string URL)
        {
            if (URL.StartsWith("http://"))
                URL = URL.Replace("http://", "ws://");
            else if (URL.StartsWith("https://"))
                URL = URL.Replace("https://", "wss://");

            return URL;
        }

        /// <summary>
        /// Create a WSConnector for the given service and function. 
        /// </summary>
        /// <param name="serviceID">The ID of the service.</param>
        /// <param name="function">The name of the function to connect.</param>
        /// <param name="args">Additional function arguments.</param>
        /// <returns>The WSConnector object or null or error.</returns>
        public static WSConnector CreateConnector(Credentials credentials, string url, string function, string args)
        {
            WSConnector connector = new WSConnector();
            connector.URL = FixupURL(credentials.Url) + function + args;
            connector.Authentication = credentials;

            return connector;
        }

        #region Public Functions
        /// <summary>
        /// This function sends the given message object.
        /// </summary>
        /// <param name="msg">This is either a BinaryMessage or TextMessage object.</param>
        /// <param name="queue">If true, then this function will not signal or start the sending thread.</param>
        public void Send(Message msg, bool queue = false)
        {
#if ENABLE_MESSAGE_DEBUGGING
            Log.Debug( "WSConnector", "Sending {0} message: {1}",
                msg is TextMessage ? "TextMessage" : "BinaryMessage", 
                msg is TextMessage ? ((TextMessage)msg).Text : ((BinaryMessage)msg).Data.Length.ToString() + " bytes" );
#endif
            lock (_sendQueue)
            {
                _sendQueue.Enqueue(msg);
                if (!queue)
                    _sendEvent.Set();
            }

            if (!queue && _sendThread == null)
            {
                _connectionState = ConnectionState.CONNECTING;

                // start an actual thread for working with the WebSocket, otherwise
                // we'll get errors from deep inside the library code.
                _sendThread = new Thread(SendMessages);
                _sendThread.Start();
            }

            // Run our receiver as a co-routine so it can invoke functions 
            // on the main thread.
            if (_receiverRoutine == 0)
                _receiverRoutine = Runnable.Run(ProcessReceiveQueue());
        }

        /// <summary>
        /// This closes this connector, it will block until the send thread exits.
        /// </summary>
        public void Close()
        {
            // setting the state to closed will make the SendThread automatically exit.
            _connectionState = ConnectionState.CLOSED;
        }
        #endregion

        #region Private Functions
        private IEnumerator ProcessReceiveQueue()
        {
            while (_connectionState == ConnectionState.CONNECTED
                || _connectionState == ConnectionState.CONNECTING)
            {
                yield return null;

                // check for a signal with a timeout of 0, this it just a quicker way to know if we have messages
                // without having to lock the _receiveQueue object.
                if (_receiveEvent.WaitOne(0))
                {
                    lock (_receiveQueue)
                    {
                        while (_receiveQueue.Count > 0)
                        {
                            Message msg = _receiveQueue.Dequeue();
#if ENABLE_MESSAGE_DEBUGGING
                            Log.Debug( "WSConnector", "Received {0} message: {1}",
                                msg is TextMessage ? "TextMessage" : "BinaryMessage", 
                                msg is TextMessage ? ((TextMessage)msg).Text : ((BinaryMessage)msg).Data.Length.ToString() + " bytes" );
#endif
                            if (OnMessage != null)
                                OnMessage(msg);
                        }
                    }
                }
            }
            if (OnClose != null)
                OnClose(this);
        }
        #endregion

        #region Threaded Functions
        // NOTE: ALl functions in this region are operating in a background thread, do NOT call any Unity functions!
        private void SendMessages()
        {
            try
            {
                WebSocket ws = null;

                ws = new WebSocket(URL);
                //if (Headers != null)
                //    ws.Headers = Headers;
                if (Authentication != null)
                    ws.SetCredentials(Authentication.Username, Authentication.Password, true);
                ws.OnOpen += OnWSOpen;
                ws.OnClose += OnWSClose;
                ws.OnError += OnWSError;
                ws.OnMessage += OnWSMessage;
                ws.Connect();

                while (_connectionState == ConnectionState.CONNECTED)
                {
                    _sendEvent.WaitOne(500);

                    Message msg = null;
                    lock (_sendQueue)
                    {
                        if (_sendQueue.Count > 0)
                            msg = _sendQueue.Dequeue();
                    }

                    if (msg == null)
                        continue;

                    if (msg is TextMessage)
                        ws.Send(((TextMessage)msg).Text);
                    else if (msg is BinaryMessage)
                        ws.Send(((BinaryMessage)msg).Data);
                }

                ws.Close();
            }
            catch (System.Exception e)
            {
                _connectionState = ConnectionState.DISCONNECTED;
                Log.Error("WSConnector", "Caught WebSocket exception: {0}", e.ToString());
            }
        }

        private void OnWSOpen(object sender, System.EventArgs e)
        {
            _connectionState = ConnectionState.CONNECTED;
        }

        private void OnWSClose(object sender, CloseEventArgs e)
        {
            _connectionState = e.WasClean ? ConnectionState.CLOSED : ConnectionState.DISCONNECTED;
        }

        private void OnWSMessage(object sender, MessageEventArgs e)
        {
            Message msg = null;
            if (e.Opcode == Opcode.Text)
                msg = new TextMessage(e.Data);
            else if (e.Opcode == Opcode.Binary)
                msg = new BinaryMessage(e.RawData);

            lock (_receiveQueue)
                _receiveQueue.Enqueue(msg);
            _receiveEvent.Set();
        }

        private void OnWSError(object sender, ErrorEventArgs e)
        {
            _connectionState = ConnectionState.DISCONNECTED;
        }
        #endregion
    }
}
