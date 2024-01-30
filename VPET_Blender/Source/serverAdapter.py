"""
-----------------------------------------------------------------------------
This source file is part of VPET - Virtual Production Editing Tools
http://vpet.research.animationsinstitut.de/
http://github.com/FilmakademieRnd/VPET

Copyright (c) 2021 Filmakademie Baden-Wuerttemberg, Animationsinstitut R&D Lab

This project has been initiated in the scope of the EU funded project
Dreamspace under grant agreement no 610005 in the years 2014, 2015 and 2016.
http://dreamspaceproject.eu/
Post Dreamspace the project has been further developed on behalf of the
research and development activities of Animationsinstitut.

The VPET component Blender Scene Distribution is intended for research and development
purposes only. Commercial use of any kind is not permitted.

There is no support by Filmakademie. Since the Blender Scene Distribution is available
for free, Filmakademie shall only be liable for intent and gross negligence;
warranty is limited to malice. Scene DistributiorUSD may under no circumstances
be used for racist, sexual or any illegal purposes. In all non-commercial
productions, scientific publications, prototypical non-commercial software tools,
etc. using the Blender Scene Distribution Filmakademie has to be named as follows:
“VPET-Virtual Production Editing Tool by Filmakademie Baden-Württemberg,
Animationsinstitut (http://research.animationsinstitut.de)“.

In case a company or individual would like to use the Blender Scene Distribution in
a commercial surrounding or for commercial purposes, software based on these
components or any part thereof, the company/individual will have to contact
Filmakademie (research<at>filmakademie.de).
-----------------------------------------------------------------------------
"""

import bpy
import struct
import mathutils
import math

## Setup ZMQ thread
def set_up_thread():
    try:
        import zmq
    except Exception as e:
        print('Could not import ZMQ\n' + str(e))
    global vpet, v_prop
    vpet = bpy.context.window_manager.vpet_data
    v_prop = bpy.context.scene.vpet_properties
    # Prepare ZMQ
    vpet.ctx = zmq.Context()
    
    # Prepare Subscriber
    vpet.socket_s = vpet.ctx.socket(zmq.SUB)
    vpet.socket_s.connect(f'tcp://{v_prop.server_ip}:{v_prop.sync_port}')
    vpet.socket_s.setsockopt_string(zmq.SUBSCRIBE, "")
    vpet.socket_s.setsockopt(zmq.RCVTIMEO,1)
    

    
    bpy.app.timers.register(listener)
    
    # Prepare Distributor
    vpet.socket_d = vpet.ctx.socket(zmq.REP)
    vpet.socket_d.bind(f'tcp://{v_prop.server_ip}:{v_prop.dist_port}')

    # Prepare poller
    vpet.poller = zmq.Poller()
    vpet.poller.register(vpet.socket_d, zmq.POLLIN)    

    bpy.app.timers.register(read_thread)

     # Prepare command socket
    vpet.socket_c = vpet.ctx.socket(zmq.PUB)
    vpet.socket_c.connect(f'tcp://{v_prop.server_ip}:{v_prop.Command_Module_port}')
    #vpet.socket_c.setsockopt_string(zmq.SUBSCRIBE, "")
    #vpet.socket_c.setsockopt(zmq.SNDTIMEO,1)
    createPingMessage()

    bpy.app.timers.register(ping)
    

## Read requests and send packages
def read_thread():
    global vpet, v_prop
    vpet = bpy.context.window_manager.vpet_data
    v_prop = bpy.context.scene.vpet_properties
    if vpet.socket_d:
        # Get sockets with messages (0: don't wait for msgs)
        sockets = dict(vpet.poller.poll(0))
        # Check if this socket has a message
        if vpet.socket_d in sockets:
            # Receive message
            msg = vpet.socket_d.recv_string()
            print(msg) # debug
            # Classify message
            if msg == "header":
                print("Header request! Sending...")
                vpet.socket_d.send(vpet.headerByteData)
            elif msg == "nodes":
                print("Nodes request! Sending...")
                vpet.socket_d.send(vpet.nodesByteData)
            elif msg == "objects":
                print("Object request! Sending...")
                vpet.socket_d.send(vpet.geoByteData)
            elif msg == "textures":
                print("Texture request! Sending...")
                if(vpet.textureList != None):
                    vpet.socket_d.send(vpet.texturesByteData)
            elif msg == "materials":
                print("Materials request! Sending...")
                if(vpet.materialsByteData != None):
                    vpet.socket_d.send(vpet.materialsByteData)
            else: # sent empty
                vpet.socket_d.send_string("")
    return 0.1 # repeat every .1 second

## process scene updates
def listener():
    global vpet, v_prop
    vpet = bpy.context.window_manager.vpet_data
    v_prop = bpy.context.scene.vpet_properties
    msg = None
    
    try:
        msg = vpet.socket_s.recv()
    except Exception as e:
        msg = None

    

    if (msg != None and len(msg)> 3 ):
        print("the MSG LEN IS: " + str(len(msg)))
        
        clientID = msg[0]
        time = msg[1]
        type = vpet.messageType[msg[2]]
        print(str(type))
        
        
        start = 3

        while(start < len(msg)):
            
            if(type == "LOCK"):
                objID = msg[start+1]
                if 0 < objID <= len(vpet.SceneObjects):
                    lockstate = msg[start+3]
                    vpet.SceneObjects[objID - 1].LockUnlock(lockstate)
                    print(str(vpet.SceneObjects[objID - 1]._lock))

                start = len(msg)
            
            elif(type == "PARAMETERUPDATE"):
                sceneID = msg[start]
                objID = msg[start+1]
                paramID = msg[start+3]
                length = msg[start+6]
                parameterData = msg[start+7]
                for i, n in enumerate(vpet.SceneObjects):
                    print(str(n) + " ... " + str(i) + "  ... and msg id is:" + str(objID))
                    print("Param ID:" + str(paramID))

                if 0 < objID <= len(vpet.SceneObjects) and 0 <= paramID < len(vpet.SceneObjects[objID - 1]._parameterList):
                    pos = vpet.SceneObjects[objID - 1]._parameterList[paramID]
                    pos.decodeMsg(msg, start+7)
                
                start += length

            elif(type == "UNDOREDOADD"):
                start = len(msg)
        
    return 0.01 # repeat every .1 second
    

#def unpack(type, array, offset):
    #return struct.unpack_from(type, array, offset)[0]
                
## Stopping the thread and closing the sockets

def createPingMessage():
    vpet.pingByteMSG.extend(struct.pack('B', 254))
    vpet.pingByteMSG.extend(struct.pack('B', 60))
    vpet.pingByteMSG.extend(struct.pack('B', 3))
    print(vpet.pingByteMSG)
    

def ping():

    global vpet, v_prop
    vpet = bpy.context.window_manager.vpet_data
    v_prop = bpy.context.scene.vpet_properties
    if (vpet.socket_c):
        vpet.socket_c.send(vpet.pingByteMSG)
    return 1


def close_socket_d():
    global vpet, v_prop
    vpet = bpy.context.window_manager.vpet_data
    v_prop = bpy.context.scene.vpet_properties
    if bpy.app.timers.is_registered(read_thread):
        print("Stopping thread")
        bpy.app.timers.unregister(read_thread)
    if vpet.socket_d:
        vpet.socket_d.close()
        
def close_socket_s():
    global vpet, v_prop
    vpet = bpy.context.window_manager.vpet_data
    v_prop = bpy.context.scene.vpet_properties
    if bpy.app.timers.is_registered(listener):
        print("Stopping subscription")
        bpy.app.timers.unregister(listener)
    if vpet.socket_s:
        vpet.socket_s.close()

def close_socket_c():
    global vpet, v_prop
    vpet = bpy.context.window_manager.vpet_data
    v_prop = bpy.context.scene.vpet_properties
    if bpy.app.timers.is_registered(createPingMessage):
        print("Stopping createPingMessage")
        bpy.app.timers.unregister(createPingMessage)
    if vpet.socket_c:
        vpet.socket_c.close()

