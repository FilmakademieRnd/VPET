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

from .bl_op import AddPath, AddPointAfter, AddPointBefore, UpdateCurveViz, ToggleAutoUpdate, ControlPointSelect, EditControlPointHandle, FKIKToggle

## Interface
# 
class VPET_Panel:
    bl_category = "VPET Addon"
    bl_space_type = "VIEW_3D"
    bl_region_type = "UI"

class VPET_PT_Panel(VPET_Panel, bpy.types.Panel):
    bl_idname = "VPET_PT_PANEL"
    bl_label = "VPET"
    
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

        row = layout.row()
        row.operator('object.rpc', text = "RPC CHANGE LATER")

class VPET_PT_Anim_Path_Panel(VPET_Panel, bpy.types.Panel):
    bl_idname = "VPET_PT_ANIM_PATH_PANEL"
    bl_label = "Animation Path"

    def draw(self, context):
        layout = self.layout

        if bpy.context.mode == 'EDIT_CURVE':
            #if the user is edidting the points of the bezier spline, disable Control Point features and display message
            row = layout.row()
            row.alert = True
            row.label(text="Feature not available in Edit Curve Mode")
        else:
            row = layout.row()
            row.operator(AddPath.bl_idname, text=AddPath.bl_label)
            row.operator(UpdateCurveViz.bl_idname, text=UpdateCurveViz.bl_label)
            row = layout.row()
            row.operator(AddPointAfter.bl_idname, text=AddPointAfter.bl_label)
            row.operator(AddPointBefore.bl_idname, text=AddPointBefore.bl_label)
            row = layout.row()
            row.operator(FKIKToggle.bl_idname, text=FKIKToggle.bl_label)
            if AddPath.default_name in bpy.data.objects:
                row = layout.row()
                row.operator(ToggleAutoUpdate.bl_idname, text=ToggleAutoUpdate.bl_label)
                #row.operator(ToggleAutoEval.bl_idname, text=ToggleAutoEval.bl_label)

class VPET_PT_Control_Points_Panel(VPET_Panel, bpy.types.Panel):
    bl_idname = "VPET_PT_control_points_panel"
    bl_label = "Control Points"

    # By setting VPET_PT_Anim_Path_Panel as parent of Control_Points_Panel, this panel will be nested into its parent in the UI 
    bl_parent_id = VPET_PT_Anim_Path_Panel.bl_idname

    def draw(self, context):
        layout = self.layout

        # If the proportional editing is ENABLED, show warning message and disable control points property editing
        if bpy.context.mode == 'EDIT_CURVE':
            #if the user is edidting the points of the bezier spline, disable Control Point features and display message
            row = layout.row()
            row.label(text="Feature not available in Edit Curve Mode")
        elif bpy.context.tool_settings.use_proportional_edit_objects:
            # If the proportional editing is ENABLED, show warning message and disable control points property editing
            row = layout.row()
            row.label(text="To use the Control Point Property Panel and the Path Auto Update")
            row = layout.row()
            row.label(text="Disable Proportional Editing")
        elif not bpy.data.objects[AddPath.default_name]["Auto Update"]:
            # If Auto Update editing is DISABLED, disable control points property editing
            row = layout.row()
            row.label(text="To use the Control Point Property Panel")
            row = layout.row()
            row.label(text="Enable Auto Update")
        elif AddPath.default_name in bpy.data.objects:
            # Getting Control Points Properties
            cp_props = bpy.context.scene.control_point_settings
            anim_path = bpy.data.objects[AddPath.default_name]
            grid = layout.grid_flow(row_major=True, columns=6, even_rows=True, even_columns=True, align=True)

            title1 = grid.box(); title1.alert = True; title1.label(text="NAME")
            title2 = grid.box(); title2.alert = True; title2.label(text="POSITION")
            title3 = grid.box(); title3.alert = True; title3.label(text="FRAME")
            title4 = grid.box(); title4.alert = True; title4.label(text="IN")
            title5 = grid.box(); title5.alert = True; title5.label(text="OUT")
            title6 = grid.box(); title6.alert = True; title6.label(text="STYLE")
                
            # Setting the owner of the data, if it exists
            cp_list_size = len(anim_path["Control Points"])
            for i in range(cp_list_size):
                cp = anim_path["Control Points"][i]
                row = layout.row()

                name_select = grid.box(); name_select.alignment = 'CENTER' # alignment does nothing. Buggy Blender.
                name_select.operator(ControlPointSelect.bl_idname, text=cp.name).cp_name = cp.name
                
                # Highlight the selected Control Point by marking the panel entry with a dot
                if (not context.active_object == None) and (context.active_object.name == cp.name):
                    grid.prop(cp_props, property="position", text="", slider=False)
                    grid.prop(cp_props, property="frame", text="", slider=False)
                    grid.prop(cp_props, property="ease_in", text="", slider=True)
                    grid.prop(cp_props, property="ease_out", text="", slider=True)
                    grid.prop_menu_enum(data=cp_props, property="style", text=cp["Style"])
                else:
                    postn = grid.box(); postn.alignment = 'CENTER'; postn.label(text=str(i));           # alignment does nothing. Buggy Blender.
                    
                    frame = grid.box()
                    # If a frame value is not valid (smaller than the previous or bigger than the following,
                    # mark it as an alert
                    if (  i > 0             and cp["Frame"] < anim_path["Control Points"][i-1]["Frame"])\
                    or (i+1 < cp_list_size  and cp["Frame"] > anim_path["Control Points"][i+1]["Frame"]):
                        frame.alert = True
                    else:
                        frame.alert = False
                    frame.alignment = 'CENTER'; frame.label(text=str(cp["Frame"]));                         # alignment does nothing. Buggy Blender.
                    
                    e__in = grid.box(); e__in.alignment = 'CENTER'; e__in.label(text=str(cp["Ease In"]));   # alignment does nothing. Buggy Blender.
                    e_out = grid.box(); e_out.alignment = 'CENTER'; e_out.label(text=str(cp["Ease Out"]));  # alignment does nothing. Buggy Blender.
                    style = grid.box(); style.alignment = 'CENTER'; style.label(text=cp["Style"]);          # alignment does nothing. Buggy Blender.
            
            row = layout.row()
            row.operator(EditControlPointHandle.bl_idname, text=EditControlPointHandle.bl_label)
                

class VPET_PT_Anim_Path_Menu(bpy.types.Menu):
    bl_label = "Animation Path"	   
    bl_idname = "OBJECT_MT_custom_spline_menu"

    def draw(self, context):
        if bpy.context.mode == 'OBJECT':
            self.layout.operator(AddPath.bl_idname,
                                 text="Animation Path",
                                 icon='OUTLINER_DATA_CURVE')
            self.layout.operator(AddPointAfter.bl_idname,
                                 text="Path Control Point After Selected",
                                 icon='RESTRICT_SELECT_OFF') # alternative option EMPTY_SINGLE_ARROW
            self.layout.operator(AddPointBefore.bl_idname,
                                 text="Path Control Point Before Selected",
                                 icon='RESTRICT_SELECT_OFF') # alternative option EMPTY_SINGLE_ARROW