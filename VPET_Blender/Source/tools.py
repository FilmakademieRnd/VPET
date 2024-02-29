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
import sys
import subprocess  # use Python executable (for pip usage)
from pathlib import Path  # Object-oriented filesystem paths since Python 3.4

def initialize():
    global vpet, v_prop
    vpet = bpy.context.window_manager.vpet_data
    #v_prop = bpy.context.scene.vpet_properties

def checkZMQ():
    try:
        import zmq
        return True
    except Exception as e:
        print(e)
        return False
    
## Create Collections for VPET objects
def setupCollections():
    v_prop = bpy.context.scene.vpet_properties
    
    # Check if the collection exists. If not, create it and link it to the scene.
    vpetColl = bpy.data.collections.get(v_prop.vpet_collection)
    if vpetColl is None:
        vpetColl = bpy.data.collections.new(v_prop.vpet_collection)
        bpy.context.scene.collection.children.link(vpetColl)

    # Check if the "VPETsceneRoot" object exists. If not, create it and link it to the collection.
    root = bpy.context.scene.objects.get('VPETsceneRoot')
    if root is None:
        bpy.ops.object.empty_add(type='PLAIN_AXES', rotation=(0,0,0), location=(0, 0, 0), scale=(1, 1, 1))
        bpy.context.active_object.name = 'VPETsceneRoot'
        root = bpy.context.active_object
        for coll in bpy.context.scene.collection.children:
            if root.name in coll.objects:
                coll.objects.unlink(root)
        vpetColl.objects.link(root)

    else:
        # Check if the "VPETsceneRoot" object is already linked to the collection.
        if root not in vpetColl.objects:
            vpetColl.objects.link(root)

    """
    if bpy.data.collections.find(v_prop.edit_collection) < 0:
        editColl = bpy.data.collections.new(v_prop.edit_collection)
        bpy.context.scene.collection.children.link(editColl)
        #bpy.data.collections[v_prop.vpet_collection].children.link(editColl)
    """
def cleanUp(level):
    if level > 0:
        vpet.objectsToTransfer = [] #list of all objects
        vpet.nodeList = [] #list of all nodes
        vpet.geoList = [] #list of geometry data
        vpet.materialList = [] # list of materials
        vpet.textureList = [] #list of textures

    if level > 1:
        vpet.editableList = []
        vpet.headerByteData = bytearray([]) # header data as bytes
        vpet.nodesByteData = bytearray([]) # nodes data as bytes
        vpet.geoByteData = bytearray([]) # geo data as bytes
        vpet.texturesByteData = bytearray([]) # texture data as bytes
        vpet.materialsByteData = bytearray([]) # materials data as bytes
        vpet.pingByteMSG = bytearray([]) # ping msg as bytes
        ParameterUpdateMSG = bytearray([])# Parameter update msg as bytes

        vpet.rootChildCount = 0

def installZmq():
    if checkZMQ():
        return 'ZMQ is already installed'
    else:
        if bpy.app.version[0] == 2 and bpy.app.version[1] < 81:
            return 'This only works with Blender versions > 2.81'

        else:
            try:
                # will likely fail the first time, but works after `ensurepip.bootstrap()` has been called once
                import pip
            except ModuleNotFoundError as e:
                # only first attempt will reach here
                print("Pip import failed with: ", e)
                print("ERROR: Pip not activated, trying bootstrap()")
                try:
                    import ensurepip
                    ensurepip.bootstrap()
                except:  # catch *all* exceptions
                    e = sys.exc_info()[0]
                    print("ERROR: Pip not activated, trying bootstrap()")
                    print("bootstrap failed with: ", e)
            py_exec = sys.executable

        # pip update
        try:
            print("Trying pip upgrade")
            output = subprocess.check_output([py_exec, '-m', 'pip', 'install', '--upgrade', 'pip'])
            print(output)
        except subprocess.CalledProcessError as e:
            print("ERROR: Couldn't update pip. Please restart Blender and try again.")
            return (e.output)
        print("INFO: Pip working! Installing pyzmq...")

        # pyzmq pip install
        try:
            print("Trying pyzmq install")
            output = subprocess.check_output([py_exec, '-m', 'pip', 'install', '--ignore-installed', 'pyzmq'])
            print(output)
            if (str(output).find('not writeable') > -1):
                return 'admin error'
            else:
                return 'success'
        except subprocess.CalledProcessError as e:
            print("ERROR: Couldn't install pyzmq.")
            return (e.output)