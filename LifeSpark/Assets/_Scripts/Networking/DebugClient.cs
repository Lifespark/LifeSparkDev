using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ExitGames.Client.Photon;
using ExitGames.Client.Photon.Lite;
using System.Collections;

using UnityEngine;

    public class DebugClient : IPhotonPeerListener
    {
        PhotonPeer peer;
        int PlayerNum;

        public DebugClient(int PlayerNum)
        {
            peer = new PhotonPeer(this, ConnectionProtocol.Udp);
            this.PlayerNum = PlayerNum;
        }

        public void Run()
        {
            //DebugLevel should usally be ERROR or Warning - ALL lets you "see" more details of what the sdk is doing.
            //Output is passed to you in the DebugReturn callback
            peer.DebugOut = DebugLevel.ALL;
            if (peer.Connect("localhost:5055", "Lite"))
            {
                Debug.Log("Player" + PlayerNum + " successfully connected."); //allows you to "see" the game loop is working, check your output-tab when running from within VS
                peer.Service();
                System.Threading.Thread.Sleep(50);
            }
            else
                Debug.Log("Unknown hostname!");
            //peer.Disconnect(); //<- uncomment this line to see a faster disconnect/leave on the other clients.
        }

        #region IPhotonPeerListener Members
        public void DebugReturn(DebugLevel level, string message)
        {
            // level of detail depends on the setting of peer.DebugOut
            Debug.Log("\nDebugReturn:" + message); //check your output-tab when running from within VS
        }

        public void OnOperationResponse(OperationResponse operationResponse)
        {
            if (operationResponse.ReturnCode == 0)
                Debug.Log("\n---OnOperationResponse: OK - " + (OpCodeEnum)operationResponse.OperationCode + "(" + operationResponse.OperationCode + ")");
            else
            {
                Debug.Log("\n---OnOperationResponse: NOK - " + (OpCodeEnum)operationResponse.OperationCode + "(" + operationResponse.OperationCode + ")\n ->ReturnCode=" + operationResponse.ReturnCode
                  + " DebugMessage=" + operationResponse.DebugMessage);
                return;
            }

            switch (operationResponse.OperationCode)
            {
                case LiteOpCode.Join:
                    int myActorNr = (int)operationResponse.Parameters[LiteOpKey.ActorNr];
                    Debug.Log(" ->My PlayerNr (or ActorNr) is:" + myActorNr);

                    Debug.Log("Calling OpRaiseEvent ...");
                    Dictionary<byte, object> opParams = new Dictionary<byte, object>();
                    opParams[LiteOpKey.Code] = (byte)101;
                    //opParams[LiteOpKey.Data] = "Hello World!"; //<- returns an error, server expects a hashtable

                    ExitGames.Client.Photon.Hashtable evData = new ExitGames.Client.Photon.Hashtable();
                    evData[(byte)1] = "Hello World!";
                    opParams[LiteOpKey.Data] = evData;
                    peer.OpCustom((byte)LiteOpCode.RaiseEvent, opParams, true);
                    break;
            }
        }

        public void OnStatusChanged(StatusCode statusCode)
        {
            Console.WriteLine("\n---OnStatusChanged:" + statusCode);
            switch (statusCode)
            {
                case StatusCode.Connect:
                    Console.WriteLine("Calling OpJoin ...");
                    Dictionary<byte, object> opParams = new Dictionary<byte, object>();
                    opParams[LiteOpKey.GameId] = "MyRoomName";
                    peer.OpCustom((byte)LiteOpCode.Join, opParams, true);
                    break;
                default:
                    break;
            }
        }

        public void OnEvent(EventData eventData)
        {
            Console.WriteLine("\n---OnEvent: " + (EvCodeEnum)eventData.Code + "(" + eventData.Code + ")");

            switch (eventData.Code)
            {
                case LiteEventCode.Join:
                    int actorNrJoined = (int)eventData.Parameters[LiteEventKey.ActorNr];
                    Console.WriteLine(" ->Player" + actorNrJoined + " joined!");

                    int[] actorList = (int[])eventData.Parameters[LiteEventKey.ActorList];
                    Console.Write(" ->Total num players in room:" + actorList.Length + ", Actornr List: ");
                    foreach (int actorNr in actorList)
                    {
                        Console.Write(actorNr + ",");
                    }
                    Console.WriteLine("");
                    break;

                case 101:
                    int sourceActorNr = (int)eventData.Parameters[LiteEventKey.ActorNr];
                    ExitGames.Client.Photon.Hashtable evData = (ExitGames.Client.Photon.Hashtable)eventData.Parameters[LiteEventKey.Data];
                    Console.WriteLine(" ->Player" + sourceActorNr + " say's: " + evData[(byte)1]);
                    break;
            }
        }

        #endregion
    }

    enum OpCodeEnum : byte
    {
        Join = 255,
        Leave = 254,
        RaiseEvent = 253,
        SetProperties = 252,
        GetProperties = 251
    }

    enum EvCodeEnum : byte
    {
        Join = 255,
        Leave = 254,
        PropertiesChanged = 253
    }