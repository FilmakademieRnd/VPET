bl_info = {
    "name" : "VPET Blender",
    "author" : "Tonio Freitag",
    "description" : "",
    "blender" : (2, 92, 2),
    "version" : (0, 5, 0),
    "location" : "VIEW3D",
    "warning" : "",
    "category" : "Animationsinstitut"
}

from typing import Set
import bpy
from .bl_op import DoDistribute
from .bl_op import StopDistribute
from .bl_op import SetupScene
from .bl_op import InstallZMQ
from .bl_panel import VPET_PT_Panel
from .tools import initialize
from .settings import VpetData
from .settings import VpetProperties

# imported classes to register
classes = (DoDistribute, StopDistribute, SetupScene, VPET_PT_Panel, VpetProperties, InstallZMQ) 

## Register classes and VpetSettings
#
def register():
    bpy.types.WindowManager.vpet_data = VpetData()
    from bpy.utils import register_class
    for cls in classes:
        try:
            register_class(cls)
            print(f"Registering {cls.__name__}")
        except Exception as e:
            print(f"{cls.__name__} "+ str(e))
    
    bpy.types.Scene.vpet_properties = bpy.props.PointerProperty(type=VpetProperties)
    initialize()
    print("Registered VPET Addon")

## Unregister for removal of Addon
#
def unregister():
    del bpy.types.WindowManager.vpet_data
    
    from bpy.utils import unregister_class
    for cls in classes:
        try:
            unregister_class(cls)
        except Exception as e:
            print(f"{cls.__name__} "+ str(e))
    print("Unregistered VPET Addon")