import bpy
from .tools import get_current_collections, switch_collection, parent_to_root, select_hierarchy, setupCollections;

def process_armature(armature):
# Function to create an empty object
    def create_empty(name, location, rotation, scale, parent):
        empty = bpy.data.objects.new(name, None)
        empty.location = location
        empty.rotation_euler = rotation.to_euler()
        empty.scale = scale
        bpy.context.collection.objects.link(empty)
        if parent:
            empty.parent = parent
        return empty

    # Get the active armature object
    armature = armature

    # Check if the active object is an armature
    if armature and armature.type == 'ARMATURE':
        bpy.ops.object.mode_set(mode='POSE')  # Switch to pose mode
        
        # List to store bone information
        bone_data_list = []
        
        # Find the root bone (typically named "Hips")
        root_bone = None
        for bone in armature.pose.bones:
            if not bone.parent:
                root_bone = bone
                break
        
        if root_bone:
            # Create empty object for the root bone
            empty_root = create_empty(root_bone.name, armature.matrix_world @ root_bone.head, root_bone.rotation_quaternion, root_bone.scale, None)
            empty_objects = {root_bone.name: empty_root}
            
            # Parent the root empty to the armature
            empty_root.parent = armature
            
            # Add root bone data to the list
            bone_data = {
                'name': root_bone.name,
                'parent': None,
                'location': armature.matrix_world @ root_bone.head,
                'rotation': root_bone.rotation_quaternion,
                'scale': root_bone.scale
            }
            bone_data_list.append(bone_data)
        
            # Iterate through each bone (excluding the root bone)
            for bone in armature.pose.bones:
                if bone != root_bone:
                    bone_matrix_global = armature.matrix_world @ bone.matrix
                    bone_location_global = bone_matrix_global.to_translation()
                    bone_rotation_global = bone_matrix_global.to_quaternion()

                    bone_data = {
                        'name': bone.name,
                        'parent': bone.parent,
                        'location': bone_location_global,
                        'rotation': bone_rotation_global,
                        'scale': bone.scale
                    }
                    bone_data_list.append(bone_data)
        
        bpy.ops.object.mode_set(mode='OBJECT')  # Switch back to object mode
        
        if root_bone:
            # Dictionary to store empty objects by bone name
            for bone_data in bone_data_list[1:]:
                parent_name = bone_data['parent'].name if bone_data['parent'] else root_bone.name
                # Create empty object for each bone
                empty = create_empty(bone_data['name'], bone_data['location'], bone_data['rotation'], bone_data['scale'], empty_objects[parent_name])
                empty_objects[bone_data['name']] = empty

            # Parent the empty objects hierarchy to the armature
            for empty in empty_objects.values():
                if empty.parent:
                    empty.parent_type = 'OBJECT'
                    empty.matrix_parent_inverse = empty.parent.matrix_world.inverted()
                    armature.select_set(True)
                    bpy.context.view_layer.objects.active = armature
                    bpy.ops.object.parent_set(type='BONE', keep_transform=True)

        collection_name = "VPET_Collection"  # Specify the collection name
        collection = bpy.data.collections.get(collection_name)
        if collection is None:
            setupCollections()

        if(get_current_collections(armature) != get_current_collections(empty_root)):
            bpy.ops.object.select_all(action='DESELECT')
            select_hierarchy(empty_root)
            switch_collection()
        else:
            bpy.ops.object.select_all(action='DESELECT')
            armature.select_set(True)
            parent_to_root()


    else:
        print("Active object is not an armature or no armature is selected.")

