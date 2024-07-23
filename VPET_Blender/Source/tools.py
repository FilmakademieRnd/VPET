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
import re
import mathutils
import blf
import bpy_extras.view3d_utils
import subprocess  # use Python executable (for pip usage)
from pathlib import Path  # Object-oriented filesystem paths since Python 3.4
from .SceneObjects import SceneCharacterObject

# Handler for drawing text
font_info = {
    "font_id": 0,
    "handler": None,
}

global auto_eval

def initialize():
    global vpet, v_prop
    vpet = bpy.context.window_manager.vpet_data
    #v_prop = bpy.context.scene.vpet_properties
    
    auto_eval = True

    # set the font drawing routine to run every frame
    font_info["handler"] = bpy.types.SpaceView3D.draw_handler_add(
        draw_pointer_numbers_callback, (None, None), 'WINDOW', 'POST_PIXEL')
    

def checkZMQ():
    try:
        import zmq
        return True
    except Exception as e:
        print(e)
        return False
    
def get_rna_ui():
    rna_ui = bpy.context.object.get('_RNA_UI')
    if rna_ui is None:
        bpy.context.object['_RNA_UI'] = {}
        rna_ui = bpy.context.object['_RNA_UI']
    return rna_ui
    
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
        if not root.name in vpetColl.objects:
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

'''
----------------------BEGIN FUNCTIONS RELATED TO THE CONTROL PATH-------------------------------
'''
def add_path(character, path_name):

    # Check whether an Animation Preview object is already present in the scene
    if path_name in bpy.data.objects:
        # If yes, save it
        print("Animation Preview object found")
        anim_path = bpy.data.objects[path_name]
    else:
        # If not, create it as an empty object 
        print("Creating new Animation Preview object")
        anim_path = bpy.data.objects.new(path_name, None)
        bpy.data.collections["Collection"].objects.link(anim_path)  # Add anim_prev to the scene

    if len(anim_path.children) == 0:
        # Create default control point in the origin 
        point_zero = make_point()
        point_zero.parent = anim_path

        anim_path["Control Points"] = [point_zero]                      # Add Control Points property and initialise it with the first "default" point. It will hold the list of all the Control Point Objects that make up the Animation Path
        anim_path["Auto Update"] = False                                # Add Auto Update property. It will hold the "mode status" for the Animation Path. It is used to enable/disable advanced editing features. 

        bpy.context.space_data.overlay.show_relationship_lines = False  # Disabling Relationship Lines to declutter scene view
        anim_path.lock_location[0] = True                                  # Locking rotation/translation of the Animation Path, as it's going to be done with its Control Points
        anim_path.lock_location[1] = True
        anim_path.lock_location[2] = True
        anim_path.lock_rotation[0] = True
        anim_path.lock_rotation[1] = True
        anim_path.lock_rotation[2] = True
        anim_path.lock_scale[0]    = True
        anim_path.lock_scale[1]    = True
        anim_path.lock_scale[2]    = True

    # Select and set as active the first point of the Path
    anim_path["Control Points"][0].select_set(True)
    bpy.context.view_layer.objects.active = anim_path["Control Points"][0]
    
    #? We could allow multiple paths in the scene (for now only one for simplifying testing)
    #? We could associate control paths to character by simply setting them as children of an armature object
    #anim_path.parent = character    # Set the selected character as the parent of the animation preview object

### Function used to create a new Control Point. It creates the mesh geometry if it's not already present in the scene and adds and initialises the various properties
#   @param  spawn_location  Position in World Space, where the new point will be displayed
#   @returns   Reference of the created Control Point Object  
def make_point(spawn_location = (0, 0, 0)):
    # Generate new planar isosceles triangle mesh called ptr_mesh
    vertices = [(-0.0625, 0, -0.0625), (0.0625, 0, 0.0625), (0, -0.25, 0), (0.0625, 0, -0.0625), (-0.0625, 0, 0.0625)]
    edges = []
    faces = [[4, 1, 2], [0, 3, 2], [0, 4, 2], [1, 3, 2], [4, 0, 1], [1, 0, 3]]

    # Check whether a mesh called "Pointer" is already present in the blender data
    if "Pointer" in bpy.data.meshes:
        # If yes, retrieve such mesh and modify its vertices to create an isosceles triangle
        print("Pointer mesh found")
        ptr_mesh = bpy.data.meshes["Pointer"]
    else:
        # If not, create a new mesh with the geometry data defined above
        ptr_mesh = bpy.data.meshes.new("Pointer")
        ptr_mesh.from_pydata(vertices, edges, faces)
        ptr_mesh.validate(verbose = True)
        ptr_mesh.uv_layers.new()

    # Create new object ptr_obj (with UI name "Pointer") that has ptr_mesh as a mesh
    ptr_obj = bpy.data.objects.new("Pointer", ptr_mesh)
    ptr_obj.location = spawn_location                           # Placing ptr_obj at a specified location (when not specified, the default is origin)
    bpy.data.collections["Collection"].objects.link(ptr_obj)    # Add ptr_obj to the scene

    # Lock Z-axis location and XY-axes rotation
    ptr_obj.lock_location[2] = True
    ptr_obj.lock_rotation[0] = True
    ptr_obj.lock_rotation[1] = True
    
    # Adding custom property "Frame" and "Style Label"
    ptr_obj["Frame"] = 0
    ptr_obj["Ease In"] = 0
    ptr_obj["Ease Out"] = 0
    ptr_obj["Style"] = "Walking"
    ptr_obj["Left Handle Type"]  = "AUTO"
    ptr_obj["Right Handle Type"] = "AUTO"
    ptr_obj["Left Handle"]  = mathutils.Vector()
    ptr_obj["Right Handle"] = mathutils.Vector()

    # Customise shading option to highlight
    bpy.context.space_data.shading.wireframe_color_type = 'OBJECT'
    bpy.context.space_data.shading.color_type = 'OBJECT'
    ptr_obj.color = (0.9, 0.1, 0, 1)    # Setting object displaying colour (not material!)
    ptr_obj.show_wire = True
    
    return ptr_obj

### Function that adds a new point to the Animation Path
#   @param  anim_path   Reference to the Animation Path to which the point has to be added
#   @param  pos         Position in which to the new point should be inserted (default -1, i.e. at the endof the list) 
#   @param  after       Whether the point is being added before or after the selected point (only important to compute the correct offset)
def add_point(anim_path, pos=-1, after=True):
    spawn_proportional_offset = mathutils.Vector((0, -1.5, 0))

    # Calculate offset proportionally to the dimensions of the mesh of the pointer (Control Point) object and in relation to the rotation of the PREVIOUS control point
    base_rotation = anim_path["Control Points"][pos].rotation_euler
    spawn_offset = anim_path["Control Points"][pos].dimensions * spawn_proportional_offset
    spawn_offset = spawn_offset if after else spawn_offset * -1  # flipping the offset so that the point gets spawned behind the selected one (if after == False)
    spawn_offset.rotate(base_rotation)
    # Create new point, place it next to the CURRENTLY SELECTED point, and select it
    new_point = make_point(anim_path["Control Points"][pos].location + spawn_offset)
    new_point.rotation_euler = base_rotation    # Rotate the pointer so that it aligns with the previous one
    new_point.parent = anim_path                # Parent it to the selected (for now the only) path

    print("Number of Control Points " + str(len(anim_path["Control Points"])))
    if len(anim_path["Control Points"]) > 0:
        # If Control Path is already populated
        # Append it to the list of Control Points of that path
        control_points = anim_path["Control Points"]
        control_points.append(new_point)
        anim_path["Control Points"] = control_points
        # If the position is not -1 (i.e. end of list), move the point to the correct position
        if pos >= 0:
            # If inserting AFTER the current point, move to the next position (pos+1), otherwise inserting at the position of the current point, which will be moved forward as a result  
            move_point(new_point, pos+1) if after else move_point(new_point, pos)
    else:
        # If Control Points has no elements, delete the property and create it ex-novo
        del anim_path["Control Points"]
        anim_path["Control Points"] = [new_point]

    for area in bpy.context.screen.areas:
        if area.type == 'PROPERTIES':
            area.tag_redraw()

    # Deselect all selected objects
    for obj in bpy.context.selected_objects:
        obj.select_set(False)

    # Checking list of Control Points
    print("Control Points:" + str(anim_path["Control Points"]))

    # Trigger Path Updating (if the functionality is enabled)
    if anim_path["Auto Update"]:
        update_curve(anim_path)

    # Select and set as active the new point
    new_point.select_set(True)
    bpy.context.view_layer.objects.active = new_point

### Function that builds the name of a Control Point object given the position that it should take up in the Control Path
def get_pos_name(pos):
    suffix = ""
    if pos < 0:
        print("move_point doesn't take negative positions")
        return
    elif pos == 0:
        suffix = ""
    elif pos < 10:
        suffix = (".00" + str(pos))
    elif pos < 100:
        suffix = (".0" + str(pos))
    elif pos < 1000:
        suffix = ("." + str(pos))
    return "Pointer" + suffix

### Function to move a Control Point in the Control Path, given the point to move and the position it should take up
def move_point(point, new_pos):
    print("Moving " + point.name + " to position " + str(new_pos))
    # Get the current position of the active object
    point_pos = point.parent["Control Points"].index(point)
    if new_pos == point_pos:
        # Just do a simple pass removing potential gaps in the numbering (useful after deletions)
        for i in range(len(point.parent["Control Points"])):
            point.parent["Control Points"][i].name = get_pos_name(i)
    if new_pos <  point_pos:
        # Move the elements after the new position forward by one and insert the active object at new_pos
        #for i in range(len(point.parent["Control Points"])):   #! Debug print
        #    print(point.parent["Control Points"][i].name)
        for i in range(new_pos, point_pos+1):
            print("Control Point " + point.parent["Control Points"][i].name + " to position " + str(i+1))
            if (i+1) < len(point.parent["Control Points"]):
                point.parent["Control Points"][i+1].name = "tmp"
            point.parent["Control Points"][i].name = get_pos_name(i+1)
            for i in range(len(point.parent["Control Points"])):
                print(point.parent["Control Points"][i].name)
        point.name = get_pos_name(new_pos)
        for i in range(len(point.parent["Control Points"])):
            print(point.parent["Control Points"][i].name)
    if new_pos  > point_pos:
        # Move the elements before the new position backward by one and insert the active object at new_pos
        point.name = "tmp"
        #for i in range(len(point.parent["Control Points"])):   #! Debug print
        #    print(point.parent["Control Points"][i].name)
        for i in range(point_pos+1, new_pos+1):
            #print("Control Point " + point.parent["Control Points"][i].name + " to position " + str(i-1))  #! Debug print
            point.parent["Control Points"][i].name = get_pos_name(i-1)
            #for i in range(len(point.parent["Control Points"])):   #! Debug print
            #    print(point.parent["Control Points"][i].name)
        point.name = get_pos_name(new_pos)
        #for i in range(len(point.parent["Control Points"])):   #! Debug print
        #    print(point.parent["Control Points"][i].name)
    # Evaluate the curve, given the new ordrering of the Control Points
    update_curve(point.parent)

### Update the list of Control Points given the current scene status, and remove the Control Path, which is going to be updated
def path_points_check(anim_path):
    # Check the children of the Animation Preview (or corresponding character)
    control_points = []
    cp_names = []   # Helper list containing the names of the control points left in the scene
    for child in anim_path.children:
        if re.search(r'Control Path', child.name):
            bpy.data.objects.remove(child, do_unlink=True)
        elif not child.name in bpy.context.view_layer.objects:
            bpy.data.objects.remove(child, do_unlink=True)
        else:
            control_points.append(child)
            cp_names.append(child.name)
    
    anim_path["Control Points"] = control_points

### Update Curve takes care of updating the AnimPath representation according to the modifications made by the user using the blender UI
def update_curve(anim_path):
    # Deselect all selected objects
    for obj in bpy.context.selected_objects:
        obj.select_set(False)

    #print("Number of Control Points for the spline " + str(len(control_points)))
    path_points_check(anim_path)

    # Create Control Path from control_points elements
    bezier_curve_obj = bpy.data.curves.new('Control Path', type='CURVE')            # Create new Curve Object with name Control Path
    bezier_curve_obj.dimensions = '2D'                                              # The Curve Object is a 2D curve

    bezier_spline = bezier_curve_obj.splines.new('BEZIER')                          # Create new Bezier Spline "Mesh"
    bezier_spline.bezier_points.add(len(anim_path["Control Points"])-1)             # Add points to the Spline to match the length of the control_points list
    for i, cp in enumerate(anim_path["Control Points"]):
        bezier_spline.bezier_points[i].co = cp.location                             # Assign the poistion of the elements in the list of Control Points to the Bézier Points
        bezier_spline.bezier_points[i].handle_left_type  = cp["Left Handle Type"]   # Use the handle data from the list of Control Points for the Bézier Points,
        bezier_spline.bezier_points[i].handle_left = mathutils.Vector(cp["Left Handle"].to_list()) + cp.location             #   originally the handle type is 'AUTO', but then any user-made change is saved and applied
        bezier_spline.bezier_points[i].handle_right_type = cp["Right Handle Type"]
        bezier_spline.bezier_points[i].handle_right = mathutils.Vector(cp["Right Handle"].to_list()) + cp.location

    control_path = bpy.data.objects.new('Control Path', bezier_curve_obj)           # Create a new Control Path Object with the geometry data of the Bézier Curve
    control_path.parent = anim_path                                                 # Make the Control Path a child of the Animation preview Object
    control_path.lock_location[2] = True                                               # Locking Z-component of the Control Path, as it's going to be done with its Control Points
    bpy.data.collections["Collection"].objects.link(control_path)                   # Add the Control Path Object in the scene

    for area in bpy.context.screen.areas:
        if area.type == 'PROPERTIES':
            area.tag_redraw()

### Function for drawing number labels next to the control points
def draw_pointer_numbers_callback(self, context):
    # BLF drawing routine
    anchor_3d_pos = mathutils.Vector((0,0,0))
    if "AnimPath" in bpy.context.scene.objects:
        anim_path = bpy.context.scene.objects["AnimPath"]
        # for every control point of the animation path
        for i in range(len(anim_path["Control Points"])):
            cp = anim_path["Control Points"][i]
            # cp_props = anim_path["Control Points Properties"][i]
            # If the Control Point is not hidden in the viewport
            if not (cp == None or cp.hide_get()):
                # Getting 3D position of the control point (taking in account a 3D offset, so that the label can follow the mesh orientation)
                offset_3d = mathutils.Vector((-0.1, 0, 0.1))
                offset_3d.rotate(cp.rotation_euler)
                anchor_3d_pos = cp.location + offset_3d
                # Getting the corresponding 2D viewport location of the 3D location of the control point
                txt_x, txt_y = bpy_extras.view3d_utils.location_3d_to_region_2d(
                    bpy.context.region,
                    bpy.context.space_data.region_3d,
                    anchor_3d_pos)
                font_id = font_info["font_id"]
            
            
                # Setting text position, size, colour (white)
                blf.position(font_id,
                             txt_x,
                             txt_y,
                             0)
                blf.size(font_id, 30.0)
                blf.color(font_id, 1, 1, 1, 1)
                # Writing text (the number relative to the position of the pointer in the list of control points in the path)
                blf.draw(font_id, str(i))

'''
----------------------END FUNCTIONS RELATED TO THE CONTROL PATH-------------------------------
'''

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