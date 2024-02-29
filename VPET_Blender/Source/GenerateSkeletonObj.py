import bpy

def get_layer_collection(context, collection, layer_collection=None):
    """Recursively search for the matching LayerCollection."""
    if layer_collection is None:
        layer_collection = context.view_layer.layer_collection
    if layer_collection.collection == collection:
        return layer_collection
    for layer_child in layer_collection.children:
        found = get_layer_collection(context, collection, layer_child)
        if found:
            return found
    return None

def create_empty_for_bone(bone, parent_empty=None, armature_obj=None):
    """Create an empty object for the bone, maintaining hierarchy."""
    # Calculate the global matrix for the bone
    bone_global_matrix = armature_obj.matrix_world @ bone.matrix_local
    
    # Create an empty at the bone's global location
    bpy.ops.object.add(type='EMPTY', location=bone_global_matrix.to_translation())
    empty_obj = bpy.context.object
    empty_obj.name = bone.name
    empty_obj.empty_display_size = 0.1
    
    # Set the rotation and scale to match the bone's global transform
    empty_obj.rotation_euler = bone_global_matrix.to_euler()
    empty_obj.scale = bone_global_matrix.to_scale()

    # Parent the empty to the previously created empty if it exists
    if parent_empty:
        empty_obj.parent = parent_empty
    else:
        # If no parent, this is a root bone, parent it directly to the armature
        empty_obj.parent = armature_obj
        empty_obj.matrix_parent_inverse = armature_obj.matrix_world.inverted()
    
    # Recursively create empties for child bones
    for child_bone in bone.children:
        create_empty_for_bone(child_bone, empty_obj, armature_obj)

def process_armature(obj):
    """Process each bone in the armature to create a corresponding empty."""
    armature_obj = obj
    if armature_obj and armature_obj.type == 'ARMATURE':
        # Ensure we're in object mode
        bpy.ops.object.mode_set(mode='OBJECT')
        # Deselect all objects
        for obj in bpy.context.view_layer.objects: obj.select_set(False)
        # Activate the armature object
        bpy.context.view_layer.objects.active = armature_obj
        # Get the collection of the armature
        armature_collection = armature_obj.users_collection[0] if armature_obj.users_collection else bpy.context.scene.collection
        layer_collection = get_layer_collection(bpy.context, armature_collection)
        bpy.context.view_layer.active_layer_collection = layer_collection
        # Create empties for each root bone
        for bone in armature_obj.data.bones:
            if not bone.parent:  # Root bone
                create_empty_for_bone(bone, None, armature_obj)

