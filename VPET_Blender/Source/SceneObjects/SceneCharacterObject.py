import functools
import math
import bpy
from ..AbstractParameter import Parameter
from .SceneObject import SceneObject
from ..serverAdapter import SendParameterUpdate

class SceneCharacterObject(SceneObject):

    boneMap = {}
    armature_obj_pose_bones = None

    def __init__(self, obj):
        super().__init__(obj)

        SceneCharacterObject.armature_obj_pose_bones = obj.pose.bones
        self.matrix_world = obj.matrix_world
        
        for bone in SceneCharacterObject.armature_obj_pose_bones:
            bone_matrix_global = self.matrix_world @ bone.matrix
            bone_rotation_quaternion = bone_matrix_global.to_quaternion()
            localBoneRotationParameter = Parameter(bone_rotation_quaternion, bone.name, self)
            self._parameterList.append(localBoneRotationParameter)
            localBoneRotationParameter.hasChanged.append(functools.partial(self.UpdateBoneRotation, localBoneRotationParameter))
            SceneCharacterObject.boneMap[localBoneRotationParameter._id] = bone_rotation_quaternion


    def UpdateBoneRotation(self, parameter, new_value):
        id = parameter._id
        SceneCharacterObject.boneMap[id] = new_value
        name = parameter._name
        targetBone = SceneCharacterObject.armature_obj_pose_bones[name]
        targetBone.rotation_quaternion = new_value
        

        
        
