//==================================================================================================
// File      : EditorNetServer
// Brief     : For test EditorNetServer: use Unity as server ,AGE as client
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

namespace AGE{

public class EditorNetServer {
	
	Socket     mServerSocket;
	Socket     mClientSocket;
	IPEndPoint mClientIP;
	Thread     mThread;

	TcpListener mTcpServer;
	TcpClient   mTcpClient;

	int        mSocketPort;

	public bool isThreadMsg;
	public List<string> msgList = new List<string>();


	//test delegate
/*	public delegate void OnRevMsg( string msg);
	public OnRevMsg onRecMsgDelegate;
*/	
	public void Init(){
		mSocketPort = 9527;
		isThreadMsg = false;


/*
		string hostName = System.Net.Dns.GetHostName();
		System.Net.IPHostEntry ipEntry = System.Net.Dns.GetHostEntry(hostName);
		// ip list
		System.Net.IPAddress[] ipAddrList = ipEntry.AddressList;
		IPEndPoint ipEnd = new IPEndPoint( ipAddrList[0], mSocketPort);
		//creat tcp sever
		mServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		mServerSocket.Bind (ipEnd);
		mServerSocket.Listen(10);
*/

		string hostName = "127.0.0.1"; //local host
		IPAddress localHost = IPAddress.Parse(hostName);
		mTcpServer = new TcpListener( localHost, mSocketPort);
		mTcpServer.Start();

		mThread = new Thread( new ThreadStart(ListeningClient));
		mThread.Start();
	}

	public void Destory(){
		if( mTcpClient != null){
		    mTcpClient.Close();
			AgeLogger.Log ("Closed TcpClient!");
		}

		if( mThread != null){
			mThread.Interrupt();
			mThread.Abort();
			AgeLogger.Log ("Closed Thread!");
		}

		if( mTcpServer != null){
		    mTcpServer.Stop();
			AgeLogger.Log ("Closed server!");
		}

	}

	void ConnetClient(){
/*		if( mClientSocket != null)
		{
			mClientSocket.Close();
		}
		// wait for available connet
		mClientSocket = mServerSocket.Accept();
*/
		if(mTcpClient != null){
			mTcpClient.Close();
		}

		mTcpClient = mTcpServer.AcceptTcpClient();
		AgeLogger.Log (" connected!");
	}

	void ListeningClient(){

		ConnetClient();

		int    recvLength = 0;
		byte[] recvData = new byte[1024];

		while(true){
			if(mTcpClient == null){
				ConnetClient();
				continue;
			}
			NetworkStream ns = mTcpClient.GetStream();

			recvLength = ns.Read(recvData, 0, recvData.Length);

			while( recvLength != 0 ){


				string testdata = System.Text.Encoding.ASCII.GetString(recvData, 0, recvLength);

				AgeLogger.Log(testdata);
				isThreadMsg = true;
				msgList.Add (testdata);

				break;
				// convert data byte to mesggse
				//EditorMessage msg = convertByteToMsg(recvData);

				//process msg string
				//ProcessMessage(msg);

			}

			//shut down connection
			//mTcpClient.Close();
		}


	}

	void SendMessage( EditorMessage msg){

		byte[] data = serialMsgToByte(msg);
		if(data.Length == 0){
			return;
		}
		// send data byte


	}

	void ProcessMessage( EditorMessage msg){
		AgeLogger.Log(" process msg type:" + (int)(msg.type));
	}

	private byte[] serialMsgToByte(EditorMessage msg){	
		using (MemoryStream ms = new MemoryStream())
		{
			Serializer.Serialize<EditorMessage>(ms, msg);
			byte[] data = new byte[ms.Length];
			ms.Position= 0;
			ms.Read(data, 0, data.Length);
			return data;
		}
	}

	private EditorMessage convertByteToMsg(byte[] msgByte){

		EditorMessage msg = null;
		using(MemoryStream ms = new MemoryStream()){
			ms.Write(msgByte,0,msgByte.Length);
			ms.Position = 0;
			msg = Serializer.Deserialize<EditorMessage>(ms);

			AgeLogger.Log(" Convert received msg!");
		}
		return msg;

	}

}

}