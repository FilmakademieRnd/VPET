import bpy

from .serverAdapter import set_up_thread
from .serverAdapter import close_socket_d
from .serverAdapter import close_socket_s
from .tools import cleanUp, installZmq, checkZMQ
from .tools import setupCollections
from .sceneDistribution import gatherSceneData

## operator classes
#
class SetupScene(bpy.types.Operator):
    bl_idname = "object.setup_vpet"
    bl_label = "VPET Scene Setup"

    def execute(self, context):
        print('setup scene')
        setupCollections()
        return {'FINISHED'}


class DoDistribute(bpy.types.Operator):
    bl_idname = "object.zmq_distribute"
    bl_label = "VPET Do Distribute"

    def execute(self, context):
        print("do distribute")
        if checkZMQ():
            reset()
            objCount = gatherSceneData()
            if objCount > 0:
                set_up_thread()
                self.report({'INFO'}, f'Sending {str(objCount)} Objects to VPET')
            else:
                self.report({'ERROR'}, 'VPET collections not found or empty')
        else:
            self.report({'ERROR'}, 'Please Install Zero MQ before continuing')
        
        return {'FINISHED'}

class StopDistribute(bpy.types.Operator):
    bl_idname = "object.zmq_stopdistribute"
    bl_label = "VPET Stop Distribute"

    def execute(self, context):
        print('stop distribute')
        print('stop subscription')
        reset()
        return {'FINISHED'}

class InstallZMQ(bpy.types.Operator):
    bl_idname = "object.zmq_install"
    bl_label = "Install ZMQ"

    def execute(self, context):
        print('installing ZMQ')
        zmq_result = installZmq()
        if zmq_result == True:
            return {'FINISHED'}
        else:
            self.report({'ERROR'}, str(zmq_result))
            return {'FAILED'}

def reset():
    close_socket_d()
    close_socket_s()
    cleanUp()