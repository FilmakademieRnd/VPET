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
from bpy.types import SceneObjects

## Class to keep editable parameters
class VpetProperties(bpy.types.PropertyGroup):
    server_ip: bpy.props.StringProperty(name='Server IP', default = '127.0.0.1', description='IP adress of the machine you are running Blender on. \'127.0.0.1\' for tests only on this machine.')
    dist_port: bpy.props.StringProperty(default = '5565')
    sync_port: bpy.props.StringProperty(default = '5556')

    vpet_collection: bpy.props.StringProperty(name = 'Static Collection', default = 'VPET_static', maxlen=30)
    edit_collection: bpy.props.StringProperty(name = 'Editable Collection', default = 'VPET_editable', maxlen=30)

## Class to keep data
#
class VpetData():

    sceneObject = {}
    sceneLight = {}
    sceneCamera = {}
    sceneMesh = {}

    geoPackage = {}
    materialPackage = {}
    texturePackage = {}

    objectsToTransfer = []
    nodeList = []
    geoList = []
    materialList = []
    textureList = []
    editableList = []

    rootChildCount = 0
    
    socket_d = None
    socket_s = None
    poller = None
    ctx = None

    nodesByteData = bytearray([])
    geoByteData = bytearray([])
    texturesByteData = bytearray([])
    headerByteData = bytearray([])

    nodeTypes = ['GROUP', 'GEO', 'LIGHT', 'CAMERA', 'SKINNEDMESH']
    lightTypes = ['SPOT', 'SUN', 'POINT', 'AREA']
    parameterTypes = ['POS', 'ROT', 'SCALE', 'LOCK', 'HIDDENLOCK', 'KINEMATIC', \
                    'FOV', 'ASPECT', 'FOCUSDIST', 'FOCUSSIZE', 'APERTURE', \
                    'COLOR', 'INTENSITY', 'EXPOSURE', 'RANGE', 'ANGLE', \
                    'BONEANIM', \
                    'VERTEXANIM', \
                    'PING', 'RESENDUPDATE', \
                    'CHARACTERTARGET']

    debugCounter = 0