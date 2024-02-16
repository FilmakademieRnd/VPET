import functools
from ..AbstractParameter import Parameter
from .SceneObject import SceneObject
from ..serverAdapter import SendParameterUpdate;

class SceneCharacterObject(SceneObject):
    def __init__(self, obj):
        super().__init__(obj)
        self.boneMap = {}
        self.bones = obj.data.bones
        for bone in self.bones:
            boneRotationParameter = Parameter(bone.rotation_quaternion, bone.name, self)
            boneRotationParameter.hasChanged.append(functools.partial(self.UpdateBoneRotation, boneRotationParameter))
            self.boneMap[boneRotationParameter._id, bone.rotation_quaternion]
        
