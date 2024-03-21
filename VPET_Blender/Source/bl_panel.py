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

## Interface
#  
class VPET_PT_Panel(bpy.types.Panel):
    bl_idname = "VPET_PT_PANEL"
    bl_label = "VPET"
    bl_category = "VPET Addon"
    bl_space_type = "VIEW_3D"
    bl_region_type = "UI"
    
    def draw(self, context):
        layout = self.layout
        #scene = context.scene
        
        row = layout.row()
        row.operator('object.zmq_install', text = 'Pip Install ZMQ')
        row.operator('object.setup_vpet', text='Setup Scene for VPET')

        row = layout.row()
        row.operator('object.setup_character', text='Setup Character for VPET')
        row.operator('object.make_obj_editable', text='Make selected Editable')
        row.operator('object.parent_to_root', text='Parent TO Root')

        
        row = layout.row()
        row.operator('object.zmq_distribute', text = "Do Distribute")
        row.operator('object.zmq_stopdistribute', text = "Stop Distribute")

        row = layout.row()
        row.prop(bpy.context.scene.vpet_properties, 'vpet_collection')
        row = layout.row()
        #row.prop(bpy.context.scene.vpet_properties, 'edit_collection')
        #row = layout.row()
        row.prop(bpy.context.scene.vpet_properties, 'server_ip')

        row = layout.row()
        row.prop(bpy.context.scene.vpet_properties, 'mixamo_humanoid', text="Mixamo Humanoid?")

        