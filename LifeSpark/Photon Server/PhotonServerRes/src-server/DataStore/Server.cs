using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Photon.SocketServer;

public class Server : ApplicationBase
{
    protected override PeerBase CreatePeer(InitRequest initRequest)
    {
        return new Peer(initRequest.Protocol, initRequest.PhotonPeer);
    }

    protected override void Setup()
    {
    }

    protected override void TearDown()
    {
    }
}

