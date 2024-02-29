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

from .serverAdapter import set_up_thread
from .serverAdapter import close_socket_d
from .serverAdapter import close_socket_s
from .serverAdapter import close_socket_c
from .serverAdapter import close_socket_u
from .tools import cleanUp, installZmq, checkZMQ
from .tools import setupCollections
from .sceneDistribution import gatherSceneData
from .GenerateSkeletonObj import process_armature


## operator classes
#
class SetupScene(bpy.types.Operator):
    bl_idname = "object.setup_vpet"
    bl_label = "VPET Scene Setup"
    bl_description = 'Create Collections for static and editable objects'

    def execute(self, context):
        print('setup scene')
        setupCollections()
        return {'FINISHED'}


class DoDistribute(bpy.types.Operator):
    bl_idname = "object.zmq_distribute"
    bl_label = "VPET Do Distribute"
    bl_description = 'Distribute the scene to VPET clients'

    def execute(self, context):
        print("do distribute")
        if checkZMQ():
            reset()
            objCount = gatherSceneData()
            bpy.ops.wm.real_time_updater('INVOKE_DEFAULT')
            bpy.ops.object.single_select('INVOKE_DEFAULT')
            cleanUp(level=1)
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
    bl_description = 'Stop the distribution and free the sockets. Important!'

    def execute(self, context):
        print('stop distribute')
        print('stop subscription')
        reset()
        return {'FINISHED'}

class InstallZMQ(bpy.types.Operator):
    bl_idname = "object.zmq_install"
    bl_label = "Install ZMQ"
    bl_description = 'Install Zero MQ. You need admin rights for this to work!'

    def execute(self, context):
        print('Installing ZMQ')
        zmq_result = installZmq()
        if zmq_result == 'admin error':
            self.report({'ERROR'}, f'You need to be Admin to install ZMQ')
            return {'FINISHED'}
        if zmq_result == 'success':
            self.report({'INFO'}, f'Successfully Installed ZMQ')
            return {'FINISHED'}
        else:
            self.report({'ERROR'}, str(zmq_result))
            return {'FINISHED'}

class SetupCharacter(bpy.types.Operator):
    bl_idname = "object.setup_character"
    bl_label = "VPET Character Setup"
    bl_description = 'generate obj for each Character bone'

    def execute(self, context):
        print('Setup Character')
        editobj = bpy.data.collections.get("VPET_Collection")
        for obj in editobj.objects:
            if obj.type == 'ARMATURE':
                process_armature(obj)
        return {'FINISHED'}
    
class MakeEditable(bpy.types.Operator):
    bl_idname = "object.make_obj_editable"
    bl_label = "Make selected Editable"
    bl_description = 'generate a new custom property called Editable for all selected obj'

    def execute(self, context):
        print('Make obj Editable')
        selected_objects = bpy.context.selected_objects
        for obj in selected_objects:
            # Add custom property "Editable" with type bool and default value True
            obj["VPET-Editable"] = True
        return{'FINISHED'}
    


    

class ParentToRoot(bpy.types.Operator):
    bl_idname = "object.parent_to_root"
    bl_label = "Parent obj to root obj"
    bl_description = 'Parent all the selectet object to the TRACER root obj'

    def execute(self, context):
        print('Parent obj')

        selected_objects = bpy.context.selected_objects
        parent_object_name = "VPETsceneRoot"
        parent_object = bpy.data.objects.get(parent_object_name)
        if parent_object:
            # Iterate through selected objects
            for obj in selected_objects:
                # Check if the object is not the parent object itself
                if obj != parent_object:
                    # Set the parent of the selected object to the parent object
                    obj.parent = parent_object
                    obj.matrix_parent_inverse = parent_object.matrix_world.inverted()
        else:
            print(f"Object '{parent_object_name}' not found.")



def reset():
    close_socket_d()
    close_socket_s()
    close_socket_c()
    close_socket_u()
    cleanUp(level=2)