import functools
import math
from mathutils import Matrix, Quaternion
import bpy
from ..AbstractParameter import Parameter
from .SceneObject import SceneObject
from ..serverAdapter import SendParameterUpdate

class SceneCharacterObject(SceneObject):

    boneMap = {}
    local_bone_rest_transform = {}      # Stores the local resting bone space transformations in a dictionary
    local_transform_map = {}            # Stores the values updated by TRACER local bone space transformations in a dictionary (may cause issues with values updated in a TRACER non-compliant way)
    received_rot = {}
    root_bone_name = None
    armature_obj_pose_bones = None
    armature_obj_bones_rest_data = None

    def __init__(self, obj):
        super().__init__(obj)

        self.armature_obj_pose_bones = obj.pose.bones   # The pose bones (to which the rotations have to be applied)
        self.armature_obj_bones_rest_data = obj.data.bones              # The rest data of the armature bones (to compute the rest pose offsets)
        self.matrix_world = obj.matrix_world
        # self.edit_bones = obj.data.edit_bones

        # for i in self.edit_bones:
        #     print(i)

        # Saving initial/resting armature bone transforms in local **bone** space
        # Necessary for then applying animation displacements in the correct transform space
        for abone in self.armature_obj_bones_rest_data:
            if abone.parent:  # Check if the bone has a parent
                # Get the relative position of the bone to its parent
                self.local_bone_rest_transform[abone.name] = abone.parent.matrix_local.inverted() @ abone.matrix_local
            else:
                self.local_bone_rest_transform[abone.name] = abone.matrix_local
        
        for bone in self.armature_obj_pose_bones:
            # finding root bone for hierarchy traversal
            if not bone.parent:
                self.root_bone_name = bone.name

            bone_matrix_global = self.matrix_world @ bone.matrix
            bone_rotation_quaternion = bone_matrix_global.to_quaternion()
            localBoneRotationParameter = Parameter(bone_rotation_quaternion, bone.name, self)
            self._parameterList.append(localBoneRotationParameter)
            localBoneRotationParameter.hasChanged.append(functools.partial(self.UpdateBoneRotation, localBoneRotationParameter))
            self.boneMap[localBoneRotationParameter._id] = bone_rotation_quaternion

    def set_pose_matrices(self, current_pose_bone):

        if current_pose_bone.name in self.local_transform_map:
            matrix = self.local_transform_map[current_pose_bone.name]

            if current_pose_bone.parent:
                current_pose_bone.matrix_basis = current_pose_bone.bone.convert_local_to_pose(
                    matrix,
                    current_pose_bone.bone.matrix_local,
                    parent_matrix= self.local_transform_map[current_pose_bone.parent.name],
                    parent_matrix_local=current_pose_bone.parent.bone.matrix_local,
                    invert=True
                )
            else:
                current_pose_bone.matrix_basis = current_pose_bone.bone.convert_local_to_pose(
                    matrix,
                    current_pose_bone.bone.matrix_local,
                    invert=True
                )
        

    def compute_local_space_transformations(self, transforms, current_pose_bone, new_offset_quaternion):
        custom_matrix_map = {}

        t = transforms[current_pose_bone.name]

        if current_pose_bone.parent:
            new_t = self.local_transform_map[current_pose_bone.parent.name] @ Matrix.Translation(t.to_translation()) @ new_offset_quaternion.to_matrix().to_4x4()
            self.local_transform_map[current_pose_bone.name] = new_t
        else:
            new_t = Matrix.Translation(t.to_translation()) @ new_offset_quaternion.to_matrix().to_4x4()
            self.local_transform_map[current_pose_bone.name] = new_t

        return custom_matrix_map

    def UpdateBoneRotation(self, parameter, new_value):

        name = parameter._name
        self.received_rot[name] = new_value
        targetBone = self.armature_obj_pose_bones[name]

        self.compute_local_space_transformations(self.local_bone_rest_transform, targetBone, new_value)
        self.set_pose_matrices(targetBone)
        
    