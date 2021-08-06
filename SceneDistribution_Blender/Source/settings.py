import bpy
from bpy.types import SceneObjects

## Class to keep editable parameters
class VpetProperties(bpy.types.PropertyGroup):
    server_ip: bpy.props.StringProperty(default = '127.0.0.1')
    dist_port: bpy.props.StringProperty(default = '5565')
    sync_port: bpy.props.StringProperty(default = '5556')

    vpet_collection: bpy.props.StringProperty(name = 'VPET Collection', default = 'VPET_static', maxlen=30)
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