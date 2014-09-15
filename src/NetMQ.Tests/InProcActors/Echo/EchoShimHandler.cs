﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using NetMQ.InProcActors;
using NetMQ.Sockets;
using NetMQ.zmq;
using NetMQ.Actors;

namespace NetMQ.Tests.InProcActors.Echo
{
    /// <summary>
    /// This hander class is specific implementation that you would need
    /// to implement per actor. This essentially contains your commands/protocol
    /// and should deal with any command workload, as well as sending back to the
    /// other end of the PairSocket which calling code would receive by using the
    /// Actor classes various RecieveXXX() methods
    /// 
    /// This is a VERY simple protocol but it just demonstrates what you would need
    /// to do to implement your own Shim handler
    /// 
    /// The only things you MUST do is to follow this example for handling
    /// a fews things
    /// 
    /// 1. Bad commands should always send the following message
    ///    "Error: invalid message to actor"
    /// 2. When we recieve a command from the actor telling us to exit the pipeline we should immediately
    ///    break out of the while loop, and dispose of the shim socket
    /// 3. When an Exception occurs you should send that down the wire to Actors calling code
    /// </summary>
    public class EchoShimHandler : IShimHandler<string>
    {

        public void Initialise(string state)
        {
            if (string.IsNullOrEmpty(state) || state != "Hello World")
                throw new InvalidOperationException(
                    "Args were not correct, expected 'Hello World'");
        }


        public void RunPipeline(PairSocket shim)
        {

            while (true)
            {
                try
                {
                    //Message for this actor/shim handler is expected to be 
                    //Frame[0] : Command
                    //Frame[1] : Payload
                    //
                    //Result back to actor is a simple echoing of the Payload, where
                    //the payload is prefixed with "ECHO BACK "
                    NetMQMessage msg = shim.ReceiveMessage();

                    string command = msg[0].ConvertToString();

                    if (command == ActorKnownMessages.END_PIPE)
                        break;

                    if (command == "ECHO")
                    {
                        shim.Send(string.Format("ECHO BACK : {0}",
                            msg[1].ConvertToString()));
                    }
                    else
                    {
                        shim.Send("Error: invalid message to actor");
                    }
                }
                //You WILL need to decide what Exceptions should be caught here, this is for 
                //demonstration purposes only, any unhandled falut will bubble up to callers code
                catch (Exception e)
                {
                    shim.Send(string.Format("Error: Exception occurred {0}", e.Message));
                }
            }

            //broken out of work loop, so should dispose shim socket now
            shim.Dispose();
        }
         

    }
}
