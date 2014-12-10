//==================================================================================================
// File      : EditorNetClient
// Brief     : Unity Editor as Client, connected to AGE Editor based on TCP
// Create    : 2014-01-15
// Modify    : 2014-01-15
// Author    : figolai <figolai@tencent.com>
// Copyright : (C) 2014 Tencent Engine Technology Center, all rights reserved.
// Website   : http://www.tencent.com
//==================================================================================================

using UnityEngine;
using System.Collections;
using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using ProtoBuf;
using AGE;
using System.Collections.Generic;

namespace AGE
{

	public class EditorNetClient 
	{

		static EditorNetClient     	mInstance;

		Socket              		mClientSocket;
		Thread              		mThread;
		string              		mHostAddr = "127.0.0.1";
		int                 		mHostPort = 9527;
		int                 		mConnectTimeout = 2000; // ms
		// accept msg from server
		public List<EditorMessage>  mMsgList;

		public bool					mLock = false;

		public bool IsConnected()
		{
			bool res = (mClientSocket != null ? mClientSocket.Connected : false);
			return res;
		}
		
		public static EditorNetClient GetInstance()
		{
			if(mInstance == null)
				mInstance = new EditorNetClient();
			return mInstance;
		}

		EditorNetClient()
		{
			mMsgList = new List<EditorMessage>();
		}

		public void Init()
		{
			// tcp client
			mClientSocket = new Socket( AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			IPAddress ipAddr = IPAddress.Parse(mHostAddr);
			IPEndPoint ipEndPoint = new IPEndPoint( ipAddr, mHostPort);
			IAsyncResult res = mClientSocket.BeginConnect( ipEndPoint, new AsyncCallback(ConnectCallback), mClientSocket);
			bool conSucc = res.AsyncWaitHandle.WaitOne( mConnectTimeout, true);
			if( !conSucc )
			{
				Closed();
				AgeLogger.Log (" Connect Time out, please reconnect!");
			}
			else
			{		
				//AgeLogger.Log(" Connected to:" + mHostAddr + ":" + mHostPort + " connect state = " + mClientSocket.Connected);
				if( mClientSocket.Connected && mThread == null )
				{
					mThread = new Thread( new ThreadStart( RecvFromServer));
					mThread.IsBackground = true;
					mThread.Start();
					AgeLogger.Log( " create and start recv thread..." );
				}
			}
		}

		void ConnectCallback(IAsyncResult asyncConnect)
		{
			//AgeLogger.Log("Connected to:" + mHostAddr + ":" + mHostPort);
		}
		
		public void Closed()
		{
			AgeLogger.Log( " shutdown socket" );
			if( mClientSocket != null && mClientSocket.Connected )
			{
				mClientSocket.Shutdown(SocketShutdown.Both);
				mClientSocket.Close();
				mClientSocket = null;
			}
			if( mThread != null)
			{
				mThread.Interrupt();
				mThread.Abort();
				mThread = null;
			}
			mLock = false;
		}

		void RecvFromServer()
		{
			AgeLogger.Log( " start network msg recv loop..." );
			while(true)
			{
				if( mClientSocket == null )
					break;

				if( !mClientSocket.Connected )
				{
					AgeLogger.Log(" OnRecvLoop - lost connected, break!");
					Closed();
					break;
				}

				if( mLock )
					continue;
				mLock = true;

				try
				{
					byte[] byteData = new byte[1024];
					//AgeLogger.Log("Recving data from server!");
					
					int i = mClientSocket.Receive(byteData);
					if( i <= 0 )
					{
						AgeLogger.Log( " invalid data len, close socket..." );
						this.Closed();				
						break;
					}
					// package head : 4 byte for msg data length
					if( i > 4 )
					{
						ParseByteData( byteData, 0 );					
					}
					else
					{
						AgeLogger.Log(" length of byte data package must be > 4");
					}

				}
				catch( Exception e)
				{
					AgeLogger.Log(" Socket Error:" + e);
					Closed();
					break;
				}

				mLock = false;

				//AgeLogger.Log( "loop net client ..." + (k++) );
				Thread.Sleep(100);
			}
		}


		void ParseByteData( byte[] data, int index)
		{
			int startPos = index;
			while( true )
			{
				byte[] headerData = new byte[4];

				// parse header
				Array.Copy( data, startPos, headerData, 0, 4);
				int msgLength = BitConverter.ToInt32(headerData, 0);
				int msgPos = startPos + 4;
				if( msgLength > 0)
				{
					// have msg data
					byte[] msgData = new byte[msgLength];
					//AgeLogger.Log("msgPOS:" + msgPos +",msg Length:" + msgLength);
					Array.Copy(data, msgPos, msgData, 0, msgLength);
					EditorMessage msg = convertByteToMsg(msgData);
					//add to msg list ( note: no locked here)
					if( msg != null)
					{
						mMsgList.Add(msg);
						//AgeLogger.Log("msg!");
					}
					// mext msg data pack
					startPos = msgPos + msgLength;
					//break;

				}
				else
				{
					// no data package
					break;
				}
			}
		}


		byte[] serialMsgToByte(EditorMessage msg)
		{	
			using (MemoryStream ms = new MemoryStream())
			{
				Serializer.Serialize<EditorMessage>(ms, msg);
				byte[] data = new byte[ms.Length];
				ms.Position= 0;
				ms.Read(data, 0, data.Length);
				return data;
			}
		}
		
		EditorMessage convertByteToMsg(byte[] msgByte)
		{			
			EditorMessage msg = null;
			using(MemoryStream ms = new MemoryStream())
			{
				ms.Write(msgByte,0,msgByte.Length);
				ms.Position = 0;
				msg = Serializer.Deserialize<EditorMessage>(ms);
			}
			return msg;
			
		}

		//send msg to server by byte data package
		public void SendMessageToServer(EditorMessage msg)
		{
			if( mClientSocket == null )
				return;

			if( !mClientSocket.Connected)
			{
				mClientSocket.Close();
				AgeLogger.Log (" Disconnected, can't send msg to server!");
				return;
			}
			
			try
			{
				// assemble data package: msg.length + msg
				byte[] msgData = serialMsgToByte(msg);
				byte[] headerData = IntToBytes(msgData.Length);
				
				//debug check
				if( headerData.Length != 4)
				{
					AgeLogger.Log(" Attention: the length of msg header is not 4!!");
				}
				//AgeLogger.Log ("msg Length: " + msgData.Length + "; header length:" + headerData.Length + "; total:");
				byte[] dataPack = new byte[ msgData.Length + headerData.Length];
				Array.Copy( headerData,0, dataPack, 0, headerData.Length);
				Array.Copy( msgData, 0, dataPack, headerData.Length, msgData.Length);
				
				int packLength = dataPack.Length;
				//AgeLogger.Log ("msg Length: " + msgData.Length + "; header length:" + headerData.Length + "; total:" + packLength);
				IAsyncResult asyncSend = mClientSocket.BeginSend(dataPack, 0, packLength, SocketFlags.None,
				                                                 new AsyncCallback(SendMsgCallback), mClientSocket);
				bool sendSucc = asyncSend.AsyncWaitHandle.WaitOne( mConnectTimeout, true);
				
				if( !sendSucc )
				{
					mClientSocket.Close();
					AgeLogger.Log(" Failed to send msg: Time out! Please check the connection1");
				}
			}
			catch( Exception e)
			{
				AgeLogger.Log( " Message Sending error:" + e);
			}
			
		}
		
		void SendMsgCallback(IAsyncResult asyncSend){
			//AgeLogger.Log(" sending msg to server ...");
		}

		//error
		int BytesToInt(byte[] bytes, int offset)
		{
			return 0;
			/*if(bytes.Length != 4)
			{
				return 0;
			}
			int num = 0;
			for (int i = offset; i < offset + 4; i++)
			{
				num <<= 8;
				num |= (bytes[i] & 0xff);
			}
			return num;
				/*
			num  = bytes[0 + offset] & 0x000000ff;  
			num |= ((bytes[1 + offset] << 8) & 0x0000ff00);  
			num |= ((bytes[2 + offset] << 16) & 0x00ff0000);  
			num |= ((bytes[3 + offset] << 24) & 0xff000000);
			return num;*/
		}
		
		byte[] IntToBytes(int i)
		{
			byte[] bytes = new byte[4];
			bytes[0] = (byte) (0x000000ff & i);
			bytes[1] = (byte) ((0x0000ff00 & i) >>8);
			bytes[2] = (byte) ((0x00ff0000 & i) >>16);
			bytes[3] = (byte) ((0xff000000 & i) >>24);
			return bytes;
		}
		
	}

}
