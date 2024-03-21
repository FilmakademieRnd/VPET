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
        
def select_hierarchy(obj):
    def select_children(obj):
        obj.select_set(True)
        for child in obj.children:
            select_children(child)

    # Deselect all objects first
    bpy.ops.object.select_all(action='DESELECT')

    # If obj is a single object
    if isinstance(obj, bpy.types.Object):
        bpy.context.view_layer.objects.active = obj
        select_children(obj)
    # If obj is a list of objects
    elif isinstance(obj, list):
        bpy.context.view_layer.objects.active = obj[0]  # Set the first object as the active object
        for o in obj:
            select_children(o)
    else:
        print("Invalid object type provided.")


def get_current_collections(obj):
    current_collections = []
    for coll in obj.users_collection:
        current_collections.append(coll.name)
    return current_collections
    

def parent_to_root():
    selected_objects =  bpy.context.selected_objects
    parent_object_name = "VPETsceneRoot"
    parent_object = bpy.data.objects.get(parent_object_name)
    if parent_object is None:
        setupCollections()
        parent_object = bpy.data.objects.get(parent_object_name)


    for obj in selected_objects:
        # Check if the object is not the parent object itself
        if obj != parent_object:
            # Set the parent of the selected object to the parent object
            obj.parent = parent_object
            obj.matrix_parent_inverse = parent_object.matrix_world.inverted()
            select_hierarchy(selected_objects)
            switch_collection()


def switch_collection():
    collection_name = "VPET_Collection"  # Specify the collection name
    collection = bpy.data.collections.get(collection_name)
    if collection is None:
        setupCollections
                    
    for obj in bpy.context.selected_objects:
        for coll in obj.users_collection:
            coll.objects.unlink(obj)

        # Link the object to the new collection
        collection.objects.link(obj)