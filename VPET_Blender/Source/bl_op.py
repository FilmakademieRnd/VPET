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

from typing import Annotated, Set
import bpy
import re
from mathutils import Vector

from bpy.types import Context
from .serverAdapter import set_up_thread, close_socket_d, close_socket_s, close_socket_c, close_socket_u
from .tools import cleanUp, installZmq, checkZMQ, setupCollections, parent_to_root, add_path, add_point, move_point, update_curve, path_points_check
from .sceneDistribution import gatherSceneData, resendCurve
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
        for obj in bpy.context.selected_objects:
            obj.select_set(False)
        print("do distribute")
        if checkZMQ():
            reset()
            objCount = gatherSceneData() # TODO: is it possible to move the scene initialization (gatherSceneData()) outside of the DoDistribute function? A good place could be in the SetupScene function
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
        self.report({'INFO'}, f'STOP SENDING Objects to VPET')
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
        selected_objects = bpy.context.selected_objects
        for obj in selected_objects:
            if obj.type == 'ARMATURE':
                process_armature(obj)
                print 
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
        parent_to_root()
        return {'FINISHED'}
   
### Operator to add a new Animation Path
#   The execution is triggered by a button in the VPET Panel or by an entry in the Add Menu
class AddPath(bpy.types.Operator):
    bl_idname = "object.add_path"
    bl_label = "Add Control Path"
    bl_description = 'Create an Object to act as a Control Path for a locomotion animation. The character will be animated by AnimHost to follow such path'
    bl_options = {'REGISTER', 'UNDO'}

    default_name = "AnimPath"

    def execute(self, context):
        self.total_frames = 180
        #TODO: eventually bind the path to a character in the scene
        # if(context.active_object == "ARMATURE"):
        #     print('Add Path START')
        #     add_path(context.active_object)
        #     #resendCurve()
        # else:
        #     print("Select a Character Object to execute this functionality")
        print('Add Path START')
        add_path(context.active_object, self.default_name)      # Call the function resposible of creating the animation path
        bpy.ops.path.interaction_listener('INVOKE_DEFAULT')     # Invoke Modal Operaton for automatically update the Animation Path in (almost) real-time
        return {'FINISHED'}

class FKIKToggle(bpy.types.Operator):
    bl_idname = "scene.fk_ik_toggle"
    bl_label = "Switch to Inverse Kinematics"
    bl_description = 'Switch between Forward and Inverse Kinematic for animating the character over its Control Path'

    def execute(self, context):
        # If the toggling should happen only when the chartacter is selected, add also the following condition -> and context.active_object.type == 'ARMATURE' (to be tested)
        if context.active_object and context.active_object.type == 'ARMATURE' and context.active_object.data["IK_FK_Switch"] >= 0:
            context.active_object.data["IK_FK_Switch"] = abs(round(context.active_object.data["IK_FK_Switch"]) - 1)

            # Forcing update visualisation of Property Panel
            for area in bpy.context.screen.areas:
                if area.type == 'PROPERTIES':
                    area.tag_redraw()

            if context.active_object.data["IK_FK_Switch"] > 0:
                FKIKToggle.bl_label = "Switch to Forward Kinematics"
            else:
                FKIKToggle.bl_label = "Switch to Inverse Kinematics"

        return {'FINISHED'}

### Operator to add a new Animation Control Point
#   The execution is triggered by a button in the VPET Panel or by an entry in the Add Menu
class AddPointAfter(bpy.types.Operator):
    bl_idname = "object.add_control_point_after"
    bl_label = "Add Point After"
    bl_description = 'Add a new Control Point to the (selected) Animation Path after the one currently selected'
    bl_options = {'REGISTER', 'UNDO'}

    def execute(self, context):
        #TODO: eventually make it contextual to the selection of a specific path
        # if(context.active_object == "ARMATURE"):
        #     print('Add Path START')
        #     add_path(context.active_object)
        #     #resendCurve()
        # else:
        #     print("Select a Character Object to execute this functionality")
        print('Add Point START')
        anim_path = bpy.data.objects[AddPath.default_name]
        new_point_index = anim_path["Control Points"].index(context.active_object)  if (context.active_object in anim_path["Control Points"] \
                                                                                        and anim_path["Control Points"].index(context.active_object) < len(anim_path["Control Points"])-1) \
                    else  -1

        add_point(anim_path, pos=new_point_index, after=True)
        return {'FINISHED'}
    
### Operator to add a new Animation Control Point
#   The execution is triggered by a button in the VPET Panel or by an entry in the Add Menu
class AddPointBefore(bpy.types.Operator):
    bl_idname = "object.add_control_point_before"
    bl_label = "New Point Before"
    bl_description = 'Add a new Control Point to the (selected) Animation Path before the one currently selected'
    bl_options = {'REGISTER', 'UNDO'}

    def execute(self, context):
        #TODO: eventually make it contextual to the selection of a specific path
        # if(context.active_object == "ARMATURE"):
        #     print('Add Path START')
        #     add_path(context.active_object)
        #     #resendCurve()
        # else:
        #     print("Select a Character Object to execute this functionality")
        print('Add Point START')
        anim_path = bpy.data.objects[AddPath.default_name]
        new_point_index = anim_path["Control Points"].index(context.active_object) if (context.active_object in anim_path["Control Points"] \
                                                                                   and anim_path["Control Points"].index(context.active_object) < len(anim_path["Control Points"])) \
                    else  0

        add_point(anim_path, pos=new_point_index, after=False)
        return {'FINISHED'}
    
### Operator to manage the Properties of the Animation Control Points
class ControlPointProps(bpy.types.PropertyGroup):
    bl_idname = "path.control_point_props"
    bl_label = "Confirm Changes"
    #bl_options = {'REGISTER', 'UNDO'}

    def get_items():
        items = [("Walking", "Walking", "Walking Locomotion Style"),
                 ("Running", "Running", "Running Locomotion Style"),
                 ("Jumping", "Jumping", "Jumping Locomotion Style")]
        return items
    
    def update_property_ui(scene):
        if not bpy.context.active_object == None:
            active_obj = bpy.context.active_object
        else:
            return
        
        if AddPath.default_name in bpy.data.objects:
            anim_path = bpy.data.objects[AddPath.default_name]
        else:
            return

        # If a Path has been created in the scene AND the selected object is a Control Point AND the Auto Update (with the other advanced features) is enabled
        if (not anim_path == None) and (re.search(r'Pointer', active_obj.name)) and (anim_path["Auto Update"]):
            scene.control_point_settings.position = active_obj.parent["Control Points"].index(active_obj)
            scene.control_point_settings.frame = active_obj["Frame"]
            scene.control_point_settings.ease_in = active_obj["Ease In"]
            scene.control_point_settings.ease_out = active_obj["Ease Out"]
            scene.control_point_settings.style = active_obj["Style"]
            active_obj.select_set(True)
        else:
            return

    def update_position(self, context):
        if bpy.context.tool_settings.use_proportional_edit_objects:
            return
        context.active_object["Position"] = self.position
        move_point(context.active_object, self.position)
        print("Update! " + str(self.position))

    def update_frame(self, context):
        # Set the property of the active control point to the new UI value
        # TODO: update also following points keeping a constant delta to the previous ones ???
        context.active_object["Frame"] = self.frame
        print("Update! " + str(self.frame))

    def update_in(self, context):
        # Set the property of the active control point to the new UI value
        context.active_object["Ease In"] = self.ease_in
        print("Update! " + str(self.ease_in))

    def update_out(self, context):
        # Set the property of the active control point to the new UI value
        context.active_object["Ease Out"] = self.ease_out
        print("Update! " + str(self.ease_out))

    def update_style(self, context):
        context.active_object["Style"] = self.style
        print("Update! " + self.style)

    position: bpy.props.IntProperty(name="Position", min=0, update=update_position)
    frame: bpy.props.IntProperty(name="Frame", min=0, max=6000, update=update_frame)
    ease_in: bpy.props.IntProperty(name="Ease In", min=0, max=100, update=update_in)
    ease_out: bpy.props.IntProperty(name="Ease Out", min=0, max=100, update=update_out)
    style: bpy.props.EnumProperty(items=get_items(), name="Style", description="Choose a Locomotion Style", default="Running", update=update_style)

### Operator to add a new Animation Path
#   The execution is triggered by a button in the VPET Panel or by an entry in the Add Menu
class UpdateCurveViz(bpy.types.Operator):
    bl_idname = "object.update_curve"
    bl_label = "Update Curve"
    bl_description = 'Update the Control Path given the new configuration of the Control Points'

    def execute(self, context):
        print('Evaluate Curve START')
        if AddPath.default_name in bpy.data.objects:
            anim_path = bpy.data.objects[AddPath.default_name]
        else:
            return
        # Check for deleted control points and evtl. do some cleanup before updating the curve 
        for child in anim_path.children:
            if not bpy.context.scene in child.users_scene:
                print(child.name + " IS NOT in the scene")
                bpy.data.objects.remove(child, do_unlink=True)
        update_curve(anim_path)
        for area in bpy.context.screen.areas:
            if area.type == 'PROPERTIES':
                area.tag_redraw()
        
        return {'FINISHED'}
    
    def on_delete_update_handler(scene):
        if AddPath.default_name in bpy.data.objects:
            anim_path = bpy.data.objects[AddPath.default_name]
        else:
            return

        # Check for deleted control points and evtl. do some cleanup before updating the curve  
        for i, child in enumerate(anim_path.children):
            if not bpy.context.scene in child.users_scene:
                print(child.name + " IS NOT in the scene")
                bpy.data.objects.remove(child, do_unlink=True)
                update_curve(anim_path)
                if i < len(anim_path["Control Points"]) - 1:
                    # If the removed element was not the last point in the list
                    # Select the element that is now in that position
                    anim_path["Control Points"][i].select_set(True)
                else:
                    # Select the new last element
                    anim_path["Control Points"][-1].select_set(True)
        
        for area in bpy.context.screen.areas:
            if area.type == 'PROPERTIES':
                area.tag_redraw()

### Operator toggling the automatic updating of the animation path
#   Inverts value of the Auto Update bool property for the AnimPath object. Triggered by a button in the VPET Add On Panel
class ToggleAutoUpdate(bpy.types.Operator):
    bl_idname = "object.toggle_auto_eval"
    bl_label = "Enable Path Auto Update"
    bl_description = 'Enable/Disable the automatic re-calculation of the path'

    def execute(self, context):
        # If the toggling should happen only when the path is selected, add also the following condition -> and bpy.data.objects[AddPath.default_name].select_get()
        if (AddPath.default_name in bpy.data.objects):
            anim_path = bpy.data.objects[AddPath.default_name]
            anim_path["Auto Update"] = not anim_path["Auto Update"]

            # Forcing update visualisation of Property Panel
            for area in bpy.context.screen.areas:
                if area.type == 'PROPERTIES':
                    area.tag_redraw()

            if (not anim_path["Auto Update"]) or bpy.context.tool_settings.use_proportional_edit_objects:
                ToggleAutoUpdate.bl_label = "Enable Path Auto Update"
            else:
                ToggleAutoUpdate.bl_label = "Disable Path Auto Update"

        return {'FINISHED'}

### Operator for selecting a Control Point.
#   The cp_name property is used to pass the name of the Control Point to be selected on to the Operator, at the click of the corresponding button in the VPET Control Point UI Panel
class ControlPointSelect(bpy.types.Operator):
    bl_idname = "object.control_point_select"
    bl_label = ""
    bl_description = 'Select the signalled Control Point'

    cp_name: bpy.props.StringProperty()

    def execute(self, context):
        if bpy.data.objects["AnimPath"]["Auto Update"]:
            for obj in bpy.data.objects:
                obj.select_set(False)

            if not self.cp_name in context.view_layer.objects:
                bpy.data.objects.remove(bpy.data.objects[self.cp_name], do_unlink=True)
                path_points_check(bpy.data.objects["AnimPath"])
            else:
                bpy.data.objects[self.cp_name].select_set(True)
                bpy.context.view_layer.objects.active = bpy.data.objects[self.cp_name]
                bpy.ops.object.mode_set(mode='OBJECT', toggle=False)     # Force Object Mode

        return {'FINISHED'}

### Operator that allows the user to enter Edit Mode directly from the click of a button in the VPET Control Points Panel
#   The user is going to edit the curve with the traditional Blender UX, the selected bezier point is the one corresponding to the currently selected Animation Path Control Point Object
#   If no point is selected, the button doesn't do anything other then checking that everything is up to date and eventually updating the appearance of the curve and
class EditControlPointHandle(bpy.types.Operator):
    bl_idname = "curve.edit_control_point_handle"
    bl_label = "Edit Selected Control Point Handles"
    bl_description = 'Edit the handles of the currently selected Control Point'

    def execute(self, context):
        anim_path = bpy.data.objects["AnimPath"]
        update_curve(anim_path)
        if context.active_object in anim_path["Control Points"]:
            ptr_idx = anim_path["Control Points"].index(context.active_object)
            for obj in bpy.data.objects:
                obj.select_set(False)

            control_path_curve = anim_path.children[0]; control_path_curve.select_set(True) # Select Control Path Bezier Curve
            context.view_layer.objects.active = control_path_curve
            bpy.ops.object.mode_set(mode='OBJECT', toggle=False)
            bpy.ops.object.mode_set(mode='EDIT', toggle=False)
            bpy.ops.curve.select_all(action='DESELECT')

            bpy.context.scene.tool_settings.workspace_tool_type = 'DEFAULT'
            
            print(control_path_curve.name + " " + control_path_curve.id_type)
            control_path_curve.data.splines[0].bezier_points[ptr_idx].select_control_point = True

        return {'FINISHED'}

### Operator to evaluate the Animation on the path
#   Triggered by a button in the VPET Animation Path Panel
#   Creates a new Object that is moved along the Animation Path
#   It uses the information given by the User (as the Control Point location, orientation, frame, Ease In and Ease Out, and the tangents/handles of the Bezier Points)  
class EvaluateSpline(bpy.types.Operator):
    bl_idname = "curve.evaluate_spline"
    bl_label = "Compute Animation"
    bl_description = "Compute the animation over the selected path"

    def execute(self, context):
        if  (context.active_object and context.active_object.name == "AnimPath") or\
            (context.active_object.parent and context.active_object.parent.name == "AnimPath"):

            return {'FINISHED'}

### MODAL Operator. It is called every frame by default.
#   It executes specific functions when certain conditions are met:
#   - When DEL or X are pressed (a Control Point could have been deleted), it checks that the Animation Path is up to date. Eventually, it updates it and cleans up the data left over by the deleted point
#   - When Enter or the LMB are relesed (usually indicating that some modification has been confirmed) and Auto Update is enabled, the Path gets updated
#   - When + or shift+= are released while a Control Point Object is selected, add a new point after the selected one
#   - When ctrl+shift++ are released while a Control Point Object is selected, add a new point before the selected one
#   - When the user gets into Edit Mode while selcting a Control Point, trigger the Edit Mode on the corresponding Bezier Point of the spline
#   - When the user is into Edit Mode while selecting the Spline, record the various edits the user makes in order to be applied to later versions of the Control Path 
class InteractionListener(bpy.types.Operator):
    bl_idname = "path.interaction_listener"
    bl_label = "Interaction Listener"
    bl_description = "Listening to Interaction Events on the Animation Path Object and Sub-Objects"

    def __init__(self):
        print("Start")

    def __del__(self):
        print("End")

    def edit_handles(self, context):
        self.layout.operator(EditControlPointHandle.bl_idname, text="Edit Handles", icon='HANDLE_ALIGNED')

    def modal(self, context, event):
        
        # If the active mode is *changing to* Object
        if self.mode != 'OBJECT' and context.mode == 'OBJECT':
        
            active_cp_idx = -1

            # If the active object is the control path, check which Bezier Point was being edited (if one)
            if context.active_object and context.active_object.name == "Control Path":
                active_object = context.active_object                   # Save the current Active Object
                if active_object.data.splines:
                    for i, p in enumerate(active_object.data.splines[0].bezier_points):
                        if p.select_control_point:
                            active_cp_idx = i

            for cp in self.anim_path["Control Points"]:                 # For every Pointer Object
                bpy.context.view_layer.objects.active = cp              # Set it as the Active Object
                bpy.ops.object.mode_set(mode='OBJECT', toggle=False)    # Set its mode to Object
                cp.select_set(False)                                    # Deselect it, so that the operation is transparent to the user
            
            # If one of the Bezier Points of the Control Path was being edited, select the corresponding Control Point Object
            if active_cp_idx >= 0:
                bpy.data.objects[AddPath.default_name]["Control Points"][active_cp_idx].select_set(True)
                bpy.context.view_layer.objects.active = bpy.data.objects[AddPath.default_name]["Control Points"][active_cp_idx]
        
        # Update the current saved mode
        self.mode = context.mode

        # If the Auto Update property is active, and Enter or the Left Mouse Button are clicked, update the animation curve
        if  (event.type == 'LEFTMOUSE' or event.type == 'RET' or event.type == 'NUMPAD_ENTER') and event.value == 'RELEASE' and \
            (not context.object == None and (context.object.name == AddPath.default_name or ((not context.object.parent == None) and  context.object.parent.name == AddPath.default_name))) and \
            (not self.anim_path == None) and self.anim_path["Auto Update"]:
            update_curve(self.anim_path)
        
        # If the active object is one of the children of AnimPath, listen to 'Shift + =' or 'Ctrl + +' Release events,
        # this will trigger the addition of a new point to the animation path, right after the currently selected points
        if  (context.active_object in bpy.data.objects[AddPath.default_name].children) and \
            ((event.type == 'PLUS'and not event.ctrl and not event.shift) or (event.type == 'NUMPAD_PLUS' and event.ctrl and not event.shift) or (event.type == 'EQUAL' and event.shift and not event.ctrl)) and \
            event.value == 'RELEASE':
            bpy.ops.object.add_control_point_after()

        # If the active object is one of the children of AnimPath, listen to 'Ctrl + Shift + =' or 'Ctrl + Shift + +' Release events,
        # this will trigger the addition of a new point to the animation path, right before the currently selected points
        if  (context.active_object in bpy.data.objects[AddPath.default_name].children) and \
            ((event.type == 'PLUS' and event.ctrl and event.shift) or (event.type == 'NUMPAD_PLUS' and event.ctrl and event.shift) or (event.type == 'EQUAL' and event.shift and event.ctrl)) and \
            event.value == 'RELEASE':
            bpy.ops.object.add_control_point_before()

        # If new_cp_location.w >= 0, it means that there is one point in the Bezier Spline that has been moved (i.e. it has a new location)
        #  - therefore, we need to updater the location of the corresponding Control Point!
        #  - The index of the affected Control Point is "saved" in the w component of new_cp_location, while xyz represent the location vector to be applied to the Control Point
        #  - The update should take place when the editing of the Bezier Point is done (=> context.mode != 'EDIT')
        if context.mode != 'EDIT':
            for i, cp in enumerate(self.anim_path["Control Points"]):
                if i < len(self.new_cp_locations) and self.new_cp_locations[i].w == 1:
                    cp.location = self.new_cp_locations[i].xyz
                    self.new_cp_locations[i].w = 0     # Setting the w to -1 in order to avoid overwriting the location multiple times

        if (context.active_object) and (context.active_object.mode == 'OBJECT') and (context.active_object in self.anim_path["Control Points"]) and bpy.data.objects[AddPath.default_name]["Auto Update"]:
            # If the User is selecting a Control Point, the Object Menu will also display the possibility of jumping directly into Handles Editing
            #  - removing the entry before adding it (again) avoids duplicates
            bpy.types.VIEW3D_MT_object.remove(InteractionListener.edit_handles) # Checking whether the element is in the menu before removal takes time and does not improve the code operations
            bpy.types.VIEW3D_MT_object.append(InteractionListener.edit_handles)
        else:
            bpy.types.VIEW3D_MT_object.remove(InteractionListener.edit_handles)
            
        if context.active_object and context.active_object.mode == 'EDIT' and context.active_object in self.anim_path["Control Points"]:
            # If the User is trying to get into edit mode while selecting a pointer object redirect them to EDIT_CURVE mode while selecting the corresponding Curve Point
            #  - while in EDIT mode, blender will update the Left Handle and Right Handle properties od the Control Point object according to the User interactions with the Control Point
            bpy.ops.curve.edit_control_point_handle()
            
        if context.active_object and context.active_object.mode == 'EDIT' and context.active_object.name == "Control Path":
            # If the User is editing the Control Path Bezier Spline, save their moifications in the Properties of the various Control Points

            path = context.active_object.data.splines[0]
            self.new_cp_locations = []
            # Get the index of the (first) control point that is currently being edited
            for i in range(len(path.bezier_points)):
                if i >= len(self.new_cp_locations):
                    self.new_cp_locations.append(Vector((0, 0, 0, 0)))

                if  path.bezier_points[i].select_control_point  or\
                    path.bezier_points[i].select_left_handle    or\
                    path.bezier_points[i].select_right_handle:

                    #selected_cp_idx = i

                    selected_curve_cp = path.bezier_points[i]
                    cp_list = bpy.data.objects[AddPath.default_name]["Control Points"]
                    selected_cp = cp_list[i]

                    selected_cp["Left Handle Type"]  = selected_curve_cp.handle_left_type
                    selected_cp["Right Handle Type"] = selected_curve_cp.handle_right_type
                    selected_cp["Left Handle"]  = Vector(selected_curve_cp.handle_left - selected_curve_cp.co)
                    selected_cp["Right Handle"] = Vector(selected_curve_cp.handle_right - selected_curve_cp.co)

                    self.new_cp_locations[i].xyz = selected_curve_cp.co
                    self.new_cp_locations[i].w = 1

        return {'PASS_THROUGH'}
    
    def invoke(self, context, event):
        # Add the modal listener to the list of called handlers and save the Animation Path object
        context.window_manager.modal_handler_add(self)
        self.anim_path = bpy.data.objects[AddPath.default_name]
        self.new_cp_locations = []
        self.mode = 'OBJECT'
        return {'RUNNING_MODAL'}
    
class SendRpcCall(bpy.types.Operator):
    #TODO mod name dunctionality and txt
    bl_idname = "object.rpc"
    bl_label = "sendRPC"
    bl_description = 'send the call to generate and stream animation to animhost'
    
    def execute(self, context):
        print('rpc bep bop bep bop')
        #TODO add functionality 
        return {'FINISHED'}
       

def reset():
    close_socket_d()
    close_socket_s()
    close_socket_c()
    close_socket_u()
    cleanUp(level=2)